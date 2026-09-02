using System.Collections;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Rendering;
using Boxhead.Core;
using Boxhead.Player;

namespace Boxhead.Enemy
{
    /// <summary>
    /// The Grasscutter — World 2 (Backyard/Dojo) boss. Two-phase state machine, deliberately
    /// modeled after <see cref="SpinCycleAI"/> (state names, telegraph-invocation pattern,
    /// wind-up/attack/stagger/defeat structure, unscaled-time defeat sequence) per
    /// docs/v4/levels/World2/backyard-dojo/gdd.md §5 and docs/story/enemies/grasscutter-boss.md.
    ///
    /// Phase 1 "Kata" (100%→50% HP): an honorable blade-master — Blade Combo, Reel Guard-Break,
    /// Petal Toss, cycling in that order.
    /// Phase 2 "Rev" (50%→0% HP): the reel becomes a continuous whirlwind — Spin-Dash and
    /// Whirlwind Pull, cycling in that order. Cut-Grass Trail is not a separate chosen attack;
    /// it is a pooled, zero-per-frame-allocation hazard laid automatically behind every Spin-Dash.
    ///
    /// ADR-0005 §4 / ADR-0006 §1.2 (mandatory, not optional): every committed movement that
    /// relocates the boss (Spin-Dash's charge, this AI's equivalent of SpinCycleAI's
    /// JumpBack/SpinCharge/JumpCharge) resolves its landing point through
    /// NavMesh.SamplePosition before the move commits — see <see cref="ClampToNavMesh"/>. This is
    /// the exact defect ADR-0005 flagged in SpinCycleAI's raw-transform moves; it must not ship a
    /// second time. Whirlwind Pull does not reposition the boss (only pulls the player), so it has
    /// no landing point to clamp.
    /// </summary>
    [RequireComponent(typeof(EnemyStats))]
    [RequireComponent(typeof(NavMeshAgent))]
    public class GrasscutterAI : MonoBehaviour
    {
        // ── Inspector ─────────────────────────────────────────────────────────

        [Header("Movement")]
        [SerializeField] private float walkSpeed   = 2f;
        [SerializeField] private float runSpeed    = 4.5f;
        [SerializeField] private float chaseRange  = 14f;
        [SerializeField] private float meleeRange  = 3.5f;
        [SerializeField] private float rangedRange = 8f;

        [Header("Attack Timing")]
        [SerializeField] private float windUpDuration       = 1.0f;
        [SerializeField] private float attackActiveDuration = 0.4f;
        [SerializeField] private float attackCooldown       = 2.5f;
        [SerializeField] private float phase2CooldownMult   = 0.75f;

        [Header("Stagger")]
        [SerializeField] private float staggerDuration = 2.5f;

        [Header("Blade Combo — Phase 1")]
        [Tooltip("Pause between the first and second overhead beat. Both beats are independently parryable (GDD §5).")]
        [SerializeField] private float bladeComboBeatGap = 0.5f;

        [Header("Reel Guard-Break — Phase 1")]
        [Tooltip("Deliberately longer than the standard wind-up so the wide flare reads as slow. Never parryable — GDD: \"parrying this breaks guard, don't let the player parry it.\" There is no separate guard-break punish mechanic in CombatController, so this is enforced simply by never passing parryable:true.")]
        [SerializeField] private float guardBreakWindUpDuration = 1.5f;
        [SerializeField] private float guardBreakRange = 4.5f;

        [Header("Petal Toss — Phase 1 (retreat then fire)")]
        [SerializeField] private float jumpBackDistance = 3.5f;
        [SerializeField] private float jumpBackHeight   = 1.4f;
        [SerializeField] private float jumpBackDuration = 0.4f;
        [Tooltip("Fan-of-blades projectile. Optional — a melee-range fallback fires if unassigned (same convention as SpinCycleAI's ClothesToss/SudsBurst). Reuses BossProjectile; set its _isParryable=true and a reasonable _damage on the prefab.")]
        [SerializeField] private GameObject _petalTossPrefab;
        [SerializeField] private float _petalTossSpeed   = 9f;
        [SerializeField] private int   _petalCount       = 3;
        [SerializeField] private float _petalSpreadAngle = 20f;

        [Header("Spin-Dash — Phase 2")]
        [Tooltip("ADR-0007 §4 hard floor: 0.75 s — proven by the escape arithmetic (0.38 s to clear 1.9 m laterally on foot + ~0.35 s recognition) to be the minimum wind-up that leaves the ground-plane lane telegraph dodgeable. Clamped up to the floor in OnValidate/Awake if set lower; do not reduce below it even for tuning, and do not widen the dash lane without lengthening this to match.")]
        [SerializeField] private float spinDashRevDuration = 0.9f;
        [SerializeField] private float spinDashSpeed        = 13f;
        [Tooltip("ADR-0006 §1.2 hard cap — not a tuning knob. Ratio-derived (0.325 of the 20 m arena) against SpinCycle's playtested 0.284; do not raise without a corresponding arena-size decision.")]
        [SerializeField] private float spinDashMaxDistance = 6.5f;
        [Tooltip("ADR-0007 §4: the dash's own hit-test radius. The ground-plane lane telegraph's drawn width is derived from this (width = 2 × _dashContactRadius = 3.0 m by default) so the visible band and the actual hitbox cannot drift apart — never hardcode a second copy of this number at either call site.")]
        [SerializeField] private float _dashContactRadius = 1.5f;
        [Tooltip("ADR-0007 §4: fallback half-length for the full-chord ground-plane lane on a side where the Building-layer raycast finds no wall (should not happen in the built arena, but a missing wall must not silently collapse the lane to zero length). Not a dash-travel tuning knob — that stays spinDashMaxDistance.")]
        [SerializeField] private float _dashLaneFallbackLength = 20.0f;

        [Header("Cut-Grass Trail — pooled (Phase 2, laid by Spin-Dash)")]
        [Tooltip("Optional visual material for the pooled hazard quads. Left unassigned they still function (damage + pooling), just with Unity's default material — see class remarks on B32's shader-reference lesson: do not runtime Shader.Find here.")]
        [SerializeField] private Material _trailHazardMaterial;
        [SerializeField] private int   _trailPoolSize      = 14;
        [SerializeField] private float _trailSpawnInterval = 0.12f;
        [SerializeField] private float _trailHazardDuration = 3f;
        [SerializeField] private float _trailHazardRadius   = 0.9f;
        [SerializeField] private int   _trailHazardDamage   = 10;

        [Header("Whirlwind Pull — Phase 2")]
        [SerializeField] private float whirlwindDuration      = 2.2f;
        [Tooltip("Meters/second the player is dragged toward the boss while in range.")]
        [SerializeField] private float whirlwindPullStrength  = 3.5f;
        [SerializeField] private float whirlwindContactRange  = 1.6f;
        [SerializeField] private float whirlwindContactTickInterval = 0.6f;

        [Header("Phase Transition")]
        [SerializeField] private float phaseTransitionPause = 1.5f;

        [Header("Defeat")]
        [SerializeField] private float defeatHoldDuration = 2.5f;
        [Tooltip("Particle burst at the moment the Grasscutter starts to vanish. GDD: \"a final gust of cherry blossoms\" — no such VFX asset exists yet (flagged gap); this stays optional/nullable exactly like SpinCycleAI's _deathBurstVFX.")]
        [SerializeField] private ParticleSystem _deathBurstVFX;
        [SerializeField] private float _defeatStumbleDuration = 0.4f;
        [SerializeField] private float _defeatWobbleDuration  = 0.5f;

        [Header("Counter Strike")]
        [SerializeField] private int counterStrikeDamage = 40;

        [Header("Screen Shake")]
        [SerializeField] private CinemachineImpulseSource _impulseSource;

        [Header("Imagination Restore")]
        [Tooltip("Same shared mechanism World 1's SpinCycle defeat uses — coordinate through this, do not invent a parallel system for \"Gold Imagination Restore blooms.\" The GDD's other defeat beat, the cherry tree bursting into full flower, is presentation/VFX with no hook in this project yet — flagged gap, not built here.")]
        [SerializeField] private Volume _imaginationVolume;
        [SerializeField] private float _imaginationLerpDuration = 1.5f;

        [Header("Intro")]
        [Tooltip("Reel_Root child — spun continuously in LateUpdate as the boss's visual \"heart\" (GDD: Phase 1 spinning heart -> Phase 2 continuous whirlwind).")]
        [SerializeField] private Transform _reelRoot;
        [Tooltip("Direct scene reference to the cherry tree, for the intro's first camera cut. Assign the CherryTree_TrunkCollider (or its parent) instance already authored in Backyard_Dojo.unity. Falls back to a name search (same defensive convention as SpinCycleAI.FindSaloon) if left unassigned.")]
        [SerializeField] private Transform _cherryTreeLookTarget;
        [SerializeField] private string    _cherryTreeNameContains = "cherry";
        [Tooltip("Absolute world Y the intro's Phase B shot aims at on the tree's XZ position — NOT an offset from the tree transform's own Y (ADR-0008 Amendment 2 §4/#7). 4.50 m is the canopy mesh's triangle-area-weighted centroid, re-measured live; Renderer.bounds.center.y (3.810) sits in the trunk and is not usable here.")]
        [SerializeField] private float _introTreeLookHeight = 4.50f;
        [SerializeField] private float _introTreeHoldDuration = 1.4f;
        [SerializeField] private float _introRiseDuration     = 1.6f;
        [SerializeField] private float _introSpinUpDuration   = 2.2f;
        [SerializeField] private float _introPostRisePause    = 0.5f;
        [Tooltip("How far \"sunk in the grass\" the boss starts before rising to its authored dormancy position.")]
        [SerializeField] private float _buriedYOffset = -4.45f;
        [Tooltip("Height above standPos (the boss's feet) that the intro's Phase C shot aims at, instead of the bare feet position — ADR-0008 Amendment 2 §4/#6. 2.13 = 50.1% of the measured 4.250 m boss height; restores SpinCycleAI._introCamLookHeight, which this port originally dropped.")]
        [SerializeField] private float _introBossLookHeight = 2.13f;

        [Header("Intro — Cinematic Camera")]
        [Tooltip("ADR-0008 Amendment 2 (\"authored vantage\"): a hand-placed scene Transform (GrasscutterIntroCamAnchor in Backyard_Dojo.unity) whose .position is used directly by ComputeIntroCamPosition when assigned. This replaces the old player-axis-derived camera position, which could never guarantee wall/tree clearance because its distance from the arena centre depended on the player's live position. When null, falls back to the legacy player-axis computation using _introCamDistance/_introCamHeight below — see OnValidate/Awake for the missing-reference warning.")]
        [SerializeField] private Transform _introCamAnchor;
        [Tooltip("World 1 boss-intro pattern reused: a dedicated priority-overridden vcam, created in Awake so frame 1 never shows the top-down gameplay view. Unlike SpinCycleAI's continuous dolly, the GDD calls for hard cuts (\"camera cuts to the cherry tree, then to the mower\"), so this vcam is repositioned instantly between fixed framings rather than lerped. Both hard-cut framings (FrameIntroCamera's Phase B/C calls) share the SAME camera position — see that method's remarks — so this height/distance pair only needs to work for one vantage point, not two independently. Only used as part of the ComputeIntroCamPosition fallback when _introCamAnchor is unassigned.")]
        [SerializeField] private float _introCamHeight   = 1.8f;
        [Tooltip("Fallback only, used when _introCamAnchor is unassigned (ADR-0008 Amendment 2). Previously the boss's real intro-camera distance, re-derived twice as the boss's true height and the arena's real geometry were measured (14 -> 9 -> 7.9): first against a mis-measured 4.70 m boss height (an inflated SkinnedMeshRenderer.bounds artifact — see B119/ADR-0008 Amendment 2 Fact 1), then against the boss's actual 4.250 m height and the third-derivation staging. 7.9 m is not a live camera distance any more once the anchor is assigned; it only matters if the anchor reference is ever lost.")]
        [SerializeField] private float _introCamDistance = 7.9f;
        [SerializeField] private float _introCamFoV      = 45f;
        // ADR-0001: must match pfb_CM_FollowCam's Lens.FieldOfView (45) — see SpinCycleAI's
        // identical field/comment; kept here for the same reason (no live camera reference wired).
        [SerializeField] private float _normalCameraFoV  = 45f;
        [SerializeField] private int   _introCamPriority = 100;

        [Header("Reel Spin")]
        [SerializeField] private float _reelKataRPM     = 45f;
        [SerializeField] private float _reelRevRPM      = 220f;
        [SerializeField] private float _reelRpmLerpSpeed = 60f;
        [Tooltip("Deceleration rate used only while Dead — GDD defeat beat: \"the reel jams, grinds, stops.\"")]
        [SerializeField] private float _reelStopRate = 90f;

        [Tooltip("Height above the boss the overhead telegraph indicator floats at (ADR-0003). Was 2.6 m (smaller than AttackTelegraphService's project-wide 3.2 m default) back when this boss's silhouette was roughly human-plus-mower scale, shorter than SpinCycle. B27's fix brought this boss's visual height up to roughly match SpinCycle's, so it now uses the same 3.2 m project-wide default SpinCycle relies on instead of a bespoke smaller value. (The boss's actual measured height is 4.250 m, not the previously-cited 4.70 m — see B119/ADR-0008 Amendment 2 Fact 1, an inflated SkinnedMeshRenderer.bounds artifact from a rotated root bone.)")]
        [SerializeField] private float _telegraphHeightOffset = 3.2f;

        // ── State ─────────────────────────────────────────────────────────────

        private enum BossState { Idle, Approaching, WindUp, Attacking, Staggered, PhaseTransition, Dead }
        private enum Phase { Kata, Rev }

        // ADR-0007 §4: proven floor for spinDashRevDuration — see that field's tooltip.
        private const float SpinDashRevDurationFloor = 0.75f;

        // ADR-0007 §4 code-review Must-Fix 2: a zero/negative _dashContactRadius makes
        // IsPlayerWithinRange(_dashContactRadius) unhittable and collapses the lane's drawn
        // width (2 × radius) toward zero; a zero/negative _dashLaneFallbackLength collapses the
        // fallback-side lane length to zero when a wall raycast finds nothing. Same
        // floor-clamp convention as SpinDashRevDurationFloor above (OnValidate + Awake).
        private const float DashContactRadiusFloor      = 0.1f;
        private const float DashLaneFallbackLengthFloor = 0.1f;

        // ADR-0007 §4: the sample radius ClampToNavMesh's initial NavMesh.SamplePosition call
        // uses; shared with SpinDash's post-clamp sanity check on finalEnd so both call sites
        // agree on what "close enough to the NavMesh" means.
        private const float NavMeshSampleDistance = 2f;

        private BossState _state = BossState.Idle;
        private Phase _phase = Phase.Kata;

        private int  _attackIndex;
        private bool _phaseTransitioned;
        private float _attackCooldownTimer;
        private bool _introComplete;

        private Unity.Cinemachine.CinemachineCamera _introVcam;
        private GameObject _introVcamGO;

        private float _reelTargetRPM;
        private float _reelCurrentRPM;

        // ── References ────────────────────────────────────────────────────────

        private Transform _player;
        private CombatController _playerCombat;
        private PlayerController _playerController;
        private EnemyStats _stats;
        private Animator _animator;
        private Renderer _renderer;
        private Material _material;
        private Color _baseColor;
        private Coroutine _activeRoutine;
        private Coroutine _defeatRoutine;

        private NavMeshAgent _agent;
        private static readonly float PathUpdateInterval = 0.25f;
        private float _pathUpdateTimer;

        // ADR-0007 §4: the Spin-Dash's heading, committed once at rev start (before WindUp) and
        // never re-aimed — see SpinDash's remarks for why this field exists at all (the fix for
        // Fact 2's undodgeable re-aim-on-launch bug). Exposed only for the reflection-based
        // commit-timing verification this ADR requires; nothing else should read it.
        private Vector3 _committedDashDir;
        private AttackTelegraphHandle _spinDashLaneHandle = AttackTelegraphHandle.None;

        // Zero-allocation chord measurement for the ground-plane lane (ADR-0007 §4) — buffer and
        // layer mask resolved once in Awake, matching CameraOcclusion/LevelBuilder's convention
        // of caching LayerMask.GetMask("Building") rather than re-resolving it per call.
        private readonly RaycastHit[] _dashLaneRaycastBuffer = new RaycastHit[8];
        private int _buildingLayerMask;

        // Cut-Grass Trail pool — pre-warmed once in Awake, Activate()/Deactivate()-recycled.
        // Never Instantiate()/Destroy()'d during gameplay (TDD §3.2 steady-state GC budget).
        private GrasscutterTrailHazard[] _trailPool;
        private int _trailNextIndex;

        private static readonly int AnimSpeed   = Animator.StringToHash("Speed");
        private static readonly int AnimAttack  = Animator.StringToHash("AttackTrigger");
        private static readonly int AnimStagger = Animator.StringToHash("StaggerTrigger");
        private static readonly int AnimIsDead  = Animator.StringToHash("IsDead");

        // Flagged asset gap: AC_Grasscutter.controller currently only defines Idle,
        // AttackTrigger, and IsDead — no Speed float or StaggerTrigger, unlike SpinCycleAI's
        // controller which has all four this script calls. Animator.SetFloat/SetTrigger throws
        // a "Parameter '<hash>' does not exist" console error (not a silent no-op) for a hash
        // with no matching declared parameter, so calling AnimSpeed/AnimStagger unconditionally
        // would spam the console every frame/attack until an animator pass adds them. Cached
        // once here and checked before each call so this script is correct today (no errors)
        // and needs zero changes once those parameters are added.
        private System.Collections.Generic.HashSet<int> _animatorParamHashes;

        // Pre-allocated overlap buffer, reserved for future AoE-style checks (none currently
        // use Physics.OverlapSphereNonAlloc, but this matches SpinCycleAI's convention of never
        // allocating a Collider[] per call — kept for parity/future attacks rather than unused).

        private WaitForSeconds _waitAttackActive;
        private WaitForSeconds _waitStagger;
        private WaitForSeconds _waitBladeGap;
        private WaitForSeconds _waitWindUp;
        private WaitForSeconds _waitGuardBreakWindUp;
        private WaitForSeconds _waitSpinDashRev;
        private WaitForSeconds _waitDefeatHold;
        // Realtime (unscaled) — see DefeatSequence's remarks: this must always reach TriggerWin()
        // even if something elsewhere has left Time.timeScale at 0. Matches SpinCycleAI exactly
        // (docs/BACKLOG.md World 1 boss-win bug) — this is a project-wide pattern, not a one-off.
        private WaitForSecondsRealtime _waitDefeatStumble;

        // ── Unity lifecycle ───────────────────────────────────────────────────

        private void Awake()
        {
            _stats    = GetComponent<EnemyStats>();
            _animator = GetComponentInChildren<Animator>();
            CacheAnimatorParameters();

            // ADR-0008 Amendment 2 §5: runtime defense in depth alongside OnValidate below —
            // OnValidate is Editor-only, so a build (or a prefab instance whose override predates
            // OnValidate ever running) must still be caught here. An unassigned anchor silently
            // falls back to the old player-axis computation, which cannot guarantee wall/tree
            // clearance — this is a staging regression, not a crash, so it warns rather than throws.
            if (_introCamAnchor == null)
            {
                Debug.LogWarning("[GrasscutterAI] _introCamAnchor is unassigned — falling back to the player-axis-derived intro camera position, which is not guaranteed clear of walls or the cherry tree (ADR-0008 Amendment 2). Assign the scene's GrasscutterIntroCamAnchor Transform.", this);
            }

            CreateIntroCamera();

            _renderer = GetComponentInChildren<Renderer>();
            if (_renderer != null)
            {
                _material  = _renderer.material;
                _baseColor = _material.GetColor("_BaseColor");
            }

            // ADR-0007 §4: runtime defense in depth alongside OnValidate — OnValidate is
            // Editor-only, so a build (or a prefab instance whose override predates OnValidate
            // ever running) must still be caught here, not silently ship an undodgeable rev.
            if (spinDashRevDuration < SpinDashRevDurationFloor)
            {
                Debug.LogWarning($"[GrasscutterAI] spinDashRevDuration ({spinDashRevDuration}) is below the ADR-0007 escape-window floor of {SpinDashRevDurationFloor} s — clamping. The ground-plane lane telegraph cannot make Spin-Dash dodgeable if the wind-up itself doesn't leave time to clear it.", this);
                spinDashRevDuration = SpinDashRevDurationFloor;
            }

            if (_dashContactRadius < DashContactRadiusFloor)
            {
                Debug.LogWarning($"[GrasscutterAI] _dashContactRadius ({_dashContactRadius}) is below the floor of {DashContactRadiusFloor} m — clamping. A zero/negative radius makes Spin-Dash's own hit test unhittable and collapses the ground-plane lane's drawn width toward zero.", this);
                _dashContactRadius = DashContactRadiusFloor;
            }

            if (_dashLaneFallbackLength < DashLaneFallbackLengthFloor)
            {
                Debug.LogWarning($"[GrasscutterAI] _dashLaneFallbackLength ({_dashLaneFallbackLength}) is below the floor of {DashLaneFallbackLengthFloor} m — clamping. A missing wall on one side must not collapse the ground-plane lane to zero length.", this);
                _dashLaneFallbackLength = DashLaneFallbackLengthFloor;
            }

            _buildingLayerMask = LayerMask.GetMask("Building");

            _waitAttackActive     = new WaitForSeconds(attackActiveDuration);
            _waitStagger          = new WaitForSeconds(staggerDuration);
            _waitBladeGap         = new WaitForSeconds(bladeComboBeatGap);
            _waitWindUp           = new WaitForSeconds(windUpDuration);
            _waitGuardBreakWindUp = new WaitForSeconds(guardBreakWindUpDuration);
            _waitSpinDashRev      = new WaitForSeconds(spinDashRevDuration);
            _waitDefeatHold       = new WaitForSeconds(defeatHoldDuration);
            _waitDefeatStumble    = new WaitForSecondsRealtime(_defeatStumbleDuration);

            _agent = GetComponent<NavMeshAgent>();
            if (_agent != null)
            {
                _agent.speed            = walkSpeed;
                _agent.stoppingDistance = meleeRange - 0.5f;
                _agent.updateRotation   = false;
                _agent.isStopped        = true;
            }

            if (_impulseSource != null)
                _impulseSource.ImpulseDefinition.AmplitudeGain = 3f;

            WarmTrailPool();
        }

#if UNITY_EDITOR
        // ADR-0007 §4: catches the floor in the Inspector at edit time; Awake's runtime clamp
        // above is the build-safe backstop since OnValidate never runs outside the Editor.
        private void OnValidate()
        {
            if (spinDashRevDuration < SpinDashRevDurationFloor)
                spinDashRevDuration = SpinDashRevDurationFloor;

            if (_dashContactRadius < DashContactRadiusFloor)
                _dashContactRadius = DashContactRadiusFloor;

            if (_dashLaneFallbackLength < DashLaneFallbackLengthFloor)
                _dashLaneFallbackLength = DashLaneFallbackLengthFloor;

            // ADR-0008 Amendment 2 §5: catches the missing reference in the Inspector at edit
            // time; Awake's runtime warning above is the build-safe backstop since OnValidate
            // never runs outside the Editor. Skipped on the prefab ASSET itself — a prefab asset
            // can never hold a scene Transform reference, so this would warn permanently every
            // time the asset is opened/edited with no way to resolve it. Only the scene instance
            // (which can and must have this wired) is checked here.
            if (_introCamAnchor == null && !UnityEditor.PrefabUtility.IsPartOfPrefabAsset(this))
            {
                Debug.LogWarning("[GrasscutterAI] _introCamAnchor is unassigned — the intro camera will fall back to the player-axis-derived position, which is not guaranteed clear of walls or the cherry tree (ADR-0008 Amendment 2). Assign the scene's GrasscutterIntroCamAnchor Transform.", this);
            }
        }
#endif

        private void WarmTrailPool()
        {
            _trailPool = new GrasscutterTrailHazard[Mathf.Max(1, _trailPoolSize)];
            var holder = new GameObject("CutGrassTrailPool").transform;
            holder.SetParent(transform, false);

            for (int i = 0; i < _trailPool.Length; i++)
            {
                var go = new GameObject($"CutGrassTrail_{i}");
                go.transform.SetParent(holder, false);
                var hazard = go.AddComponent<GrasscutterTrailHazard>();
                hazard.Initialize(_trailHazardMaterial);
                go.SetActive(false);
                _trailPool[i] = hazard;
            }
        }

        // Builds the dedicated cinematic intro vcam, enabled at highest priority before the
        // first Brain update so frame 1 never shows the top-down gameplay view — identical
        // rationale/technique to SpinCycleAI.CreateIntroCamera.
        //
        // As of the 2026-09-02 activation-gated rework (see BossIntro's header comment), this
        // boss now matches SpinCycleAI's own trigger timing: BossIntro's Phase B cut fires
        // immediately once Start() runs, essentially the same frame as this method. So the frame
        // this call produces is only ever a single-frame placeholder covering the brief Awake->
        // Start gap — a vcam enabled-but-never-positioned would default to world (0,0,0)/identity,
        // and this avoids the Brain ever cutting to that empty-space framing, however briefly.
        // Framed on the boss's own (not-yet-buried) authored position since _player isn't
        // resolved yet at Awake time anyway (see the position-fallback note below) — Phase B
        // reframes this same vcam to the tree immediately after.
        private void CreateIntroCamera()
        {
            if (_introVcamGO != null) return;
            _introVcamGO = new GameObject("CM_GrasscutterIntroCam");
            _introVcam   = _introVcamGO.AddComponent<Unity.Cinemachine.CinemachineCamera>();

            var lens = _introVcam.Lens;
            lens.FieldOfView = _normalCameraFoV;
            _introVcam.Lens  = lens;

            var prio = _introVcam.Priority;
            prio.Value          = _introCamPriority;
            _introVcam.Priority = prio;
            _introVcam.enabled  = true;

            // Camera position: ComputeIntroCamPosition() returns _introCamAnchor.position when
            // assigned (the shipping case — see ADR-0008 Amendment 2), independent of _player.
            // Only the unassigned-anchor fallback needs a player reference for its direction, and
            // even then _player is not yet resolved here (Awake runs before Start) — that path
            // falls back further to -transform.forward, deterministic and good enough since
            // BossIntro's own Phase B/C calls reframe this vcam correctly once the player exists.
            //
            // Look target: bare transform.position (feet), not standPos + _introBossLookHeight
            // like Phase C — doesn't matter which now that this is only a single-frame
            // placeholder (see method header above), but kept simple since there's nothing
            // meaningful to frame yet at this point in Awake regardless.
            FrameIntroCamera(transform.position);
        }

        private void Start()
        {
            var playerObj = GameObject.FindWithTag("Player");
            if (playerObj != null)
            {
                _player           = playerObj.transform;
                _playerCombat     = playerObj.GetComponent<CombatController>();
                _playerController = playerObj.GetComponent<PlayerController>();

                if (_playerCombat != null)
                    _playerCombat.OnCounterStrike += OnCounterStrikeLanded;
            }

            _stats.OnDeath += HandleDeath;

            StartCoroutine(BossIntro());
        }

        private void Update()
        {
            if (_state == BossState.Dead) return;
            if (_player == null) return;

            if (_attackCooldownTimer > 0f)
                _attackCooldownTimer -= Time.deltaTime;

            switch (_state)
            {
                case BossState.Idle:
                    if (_introComplete && Vector3.Distance(transform.position, _player.position) <= chaseRange)
                        _state = BossState.Approaching;
                    break;

                case BossState.Approaching:
                    Approach();
                    break;
            }

            float speed = _state == BossState.Approaching
                ? (_agent != null ? _agent.velocity.magnitude : (_phase == Phase.Kata ? walkSpeed : runSpeed))
                : 0f;
            SafeSetFloat(AnimSpeed, speed);
        }

        // The reel is the boss's continuous "spinning heart" — GDD: Phase 1 lesson, Phase 2
        // whirlwind, Defeat "jams, grinds, stops." Driven independently of the Animator in
        // LateUpdate, same technique as DrumWindowRotator (SpinCycleAI's equivalent), so it
        // survives whatever pose the Animator writes each frame.
        private void LateUpdate()
        {
            if (_reelRoot == null) return;

            float target = _state == BossState.Dead ? 0f : _reelTargetRPM;
            float rate   = _state == BossState.Dead ? _reelStopRate : _reelRpmLerpSpeed;
            _reelCurrentRPM = Mathf.MoveTowards(_reelCurrentRPM, target, rate * Time.deltaTime);

            if (Mathf.Abs(_reelCurrentRPM) > 0.01f)
            {
                // NOTE (asset-orientation caution, per project convention): rotating about local
                // +X assumes a push-reel-mower's blade drum spins about the axis perpendicular to
                // its travel direction (i.e. the transverse roller axis), NOT the vertical Y axis
                // SpinCycleAI's drum-head uses. This has not been visually verified against the
                // actual Grasscutter.fbx rig orientation — check via screenshot once wired and
                // flip to Vector3.forward/Vector3.up if the reel visibly spins the wrong way.
                _reelRoot.Rotate(Vector3.right, _reelCurrentRPM * 6f * Time.deltaTime, Space.Self);
            }
        }

        // ── Boss intro ─────────────────────────────────────────────────────────

        // GDD §5: "Dormant in the tall grass at the arena's far end. As Kid approaches, the reel
        // ticks over, grass and petals kick up, and it rises. Camera cuts to the cherry tree,
        // then to the mower spinning up — mirrors the World 1 boss-intro cadence." Reuses
        // SpinCycleAI's cadence (dedicated priority-overridden vcam, invulnerable throughout,
        // hand back to gameplay at the end) — and, as of an owner decision on 2026-09-02, ALSO
        // matches SpinCycleAI on trigger timing: activation-gated, firing immediately on zone
        // activation, not proximity-gated. (A proximity-gated version shipped earlier the same
        // day and was reverted: waiting for the player to approach closer, with nothing visible
        // in the meantime, read as "nothing happens when I enter the boss area" — worse, a
        // separate same-day input-lock fix froze the player before they could ever close that
        // distance, a hard soft-lock. Cutting straight to the cherry tree on activation removes
        // both problems at once.) The camera still hard-cuts between framings rather than
        // dollying, per the GDD's "camera cuts to..." language.
        private IEnumerator BossIntro()
        {
            _stats?.SetInvulnerable(true);
            _state = BossState.Idle;
            if (_agent != null) _agent.enabled = false;

            try
            {
                Vector3 standPos  = transform.position; // authored dormancy position
                Vector3 buriedPos = standPos + Vector3.up * _buriedYOffset;
                transform.position = buriedPos;

                // Input lock: the cinematic starts immediately below, with no approach phase to
                // protect, so there's no reason to leave this until later — lock right away.
                // Same PermitPulperBossIntro precedent as before (disable PlayerController for
                // the duration; _playerController cached once in Start(), same reference
                // WhirlwindPull uses). No separate manual camera-input script exists in this
                // project to lock alongside it (grepped _Project/Scripts: only automatic
                // follow/injector/occlusion/framing cameras — CameraFollowTargetInjector,
                // CameraStackWirer, AspectAdaptiveCameraFraming, MinimapCameraFollow, CameraOcclusion —
                // none read touch/drag/pinch input, so there is nothing else to disable here).
                if (_playerController != null) _playerController.enabled = false;

                // ── Phase B: camera cuts to the cherry tree ──
                Vector3 treeLook = ResolveCherryTreeLookPoint();
                FrameIntroCamera(treeLook);
                float holdTimer = 0f;
                while (holdTimer < _introTreeHoldDuration) { holdTimer += Time.deltaTime; yield return null; }

                // ── Phase C: hard cut to the mower — it rises out of the grass, reel spins up ──
                // ADR-0008 Amendment 2 §4/#6: aim above standPos (the boss's feet), not at the bare
                // feet position, or the boss reads as cropped/cut off at the bottom of frame.
                FrameIntroCamera(standPos + Vector3.up * _introBossLookHeight);

                float riseTimer = 0f;
                while (riseTimer < _introRiseDuration)
                {
                    riseTimer += Time.deltaTime;
                    float t = Mathf.Clamp01(riseTimer / _introRiseDuration);
                    transform.position = Vector3.Lerp(buriedPos, standPos, t);
                    yield return null;
                }
                transform.position = standPos;

                float spinTimer = 0f;
                while (spinTimer < _introSpinUpDuration)
                {
                    spinTimer += Time.deltaTime;
                    float t = spinTimer / _introSpinUpDuration;
                    _reelTargetRPM = Mathf.Lerp(0f, _reelRevRPM, t * t); // quadratic ease-in
                    if (spinTimer >= _introSpinUpDuration * 0.5f && spinTimer < _introSpinUpDuration * 0.5f + Time.deltaTime)
                        _impulseSource?.GenerateImpulse(0.2f);
                    yield return null;
                }
                _reelTargetRPM = _reelKataRPM;

                // ── Phase D: hand control back to the gameplay camera, combat begins ──
                float pauseTimer = 0f;
                while (pauseTimer < _introPostRisePause) { pauseTimer += Time.deltaTime; yield return null; }

                if (_introVcam != null) _introVcam.enabled = false;

                if (_agent != null)
                {
                    _agent.enabled   = true;
                    _agent.Warp(transform.position);
                    _agent.isStopped = true;
                }

                if (_player != null)
                {
                    Vector3 toPlayer = _player.position - transform.position;
                    toPlayer.y = 0f;
                    if (toPlayer.sqrMagnitude > 0.01f)
                        transform.rotation = Quaternion.LookRotation(toPlayer.normalized);
                }

                _introComplete = true;
                _state         = BossState.Approaching;

                _stats?.SetInvulnerable(false);
            }
            finally
            {
                // Covers normal completion and any in-body early exit (yield break/exception)
                // inside the try above — always restores the player's controls exactly once.
                //
                // Does NOT by itself cover an external stop: verified directly in this Unity
                // version (isolated try/finally coroutine test, StartCoroutine + external
                // StopAllCoroutines()) that Unity's StopCoroutine()/StopAllCoroutines() do NOT call
                // Dispose() on the stopped enumerator, so this finally block never runs when
                // something else stops this coroutine — the same behavior HandleDeath's
                // AttackTelegraphService.Hide() comment already documents for SpinDash's lane
                // telegraph (ADR-0007). The real guarantee for that path is the explicit
                // `_playerController.enabled = true` at both actual external-stop call sites
                // (HandleDeath and OnDestroy, next to their own StopAllCoroutines() calls) — this
                // finally is a correctness nicety for the normal path, not the safety net.
                //
                // _agent.enabled and _stats.SetInvulnerable(false) are deliberately NOT restored
                // here: HandleDeath() already explicitly disables the agent as part of its own
                // death handling, and invulnerability doesn't need clearing for a boss about to be
                // destroyed — unconditionally redoing either here would fight HandleDeath's intent.
                if (_playerController != null) _playerController.enabled = true;
            }
        }

        // Hard-cuts the intro vcam to a fixed shot looking at lookPoint, from a SINGLE shared
        // vantage point (see ComputeIntroCamPosition) — an instant reframe rather than a dolly,
        // matching the GDD's "camera cuts to..." language (SpinCycleAI's continuous dolly does
        // not apply here since there is no doorway to walk out of).
        //
        // Bug fix (owner playtest, post-B27): the camera position used to be re-derived from
        // `lookPoint` itself (`camPos = lookPoint + towardCam * _introCamDistance`), with
        // `towardCam` always the boss<->player direction regardless of what lookPoint was. That
        // was coherent for Phase C (looking at the boss — offset direction and look target are
        // both boss-relative) but not for Phase B (looking at the cherry tree): the tree's
        // position has no geometric relationship to the boss<->player axis, so depending on
        // which side the player approached from, the offset could land the camera on the wrong
        // side of the tree, or too close to/behind it — "goes behind the tree and sits there."
        // Fix: BOTH phases now share the exact same camera position (see ComputeIntroCamPosition —
        // an authored anchor Transform when assigned, never re-derived per-phase) and only the
        // look target changes between calls — a true "hard cut between two framings" of the same
        // vantage point, matching the class comment above BossIntro(). Tree and boss dormancy are NOT close together in the
        // authored Backyard_Dojo.unity scene (~9.6 m apart at the current staging — a claim of
        // ~0.5 m here was stale and has been measured wrong; see ADR-0008 Amendment 2 §4/#14).
        // One vantage point still frames both correctly, but not because the two subjects are
        // near each other: it works because _introCamAnchor (ADR-0008 Amendment 2 §5) is a single
        // hand-authored Transform whose position was derived and verified (convex-polygon wall
        // clearance, exact ray-triangle canopy tests) to see both subjects cleanly, combined with
        // two independent look heights — _introTreeLookHeight for Phase B, _introBossLookHeight
        // for Phase C — that each aim correctly at their own subject from that one shared point.
        private void FrameIntroCamera(Vector3 lookPoint)
        {
            if (_introVcamGO == null) return;

            Vector3 camPos = ComputeIntroCamPosition();
            _introVcamGO.transform.position = camPos;
            _introVcamGO.transform.rotation = Quaternion.LookRotation((lookPoint - camPos).normalized);

            var lens = _introVcam.Lens;
            lens.FieldOfView = _introCamFoV;
            _introVcam.Lens  = lens;
        }

        // The single shared vantage point both BossIntro Phase B and Phase C frame from. Primary
        // source (ADR-0008 Amendment 2 §5): _introCamAnchor, a hand-authored Transform, returned
        // as-is — never re-derived from lookPoint or the player, so the position itself never
        // depends on which target is being looked at or where the player is standing. Fallback
        // only (unassigned/lost anchor reference, warned about in OnValidate/Awake): offset back
        // from the boss's own position along the boss<->player axis, at _introCamHeight — this is
        // also what's used for the very first (pre-trigger) framing in CreateIntroCamera before
        // _player is resolved, if no anchor is assigned, via the -transform.forward fallback below.
        private Vector3 ComputeIntroCamPosition()
        {
            // ADR-0008 Amendment 2 §5: an authored anchor Transform is now the primary source —
            // see _introCamAnchor's tooltip. The player-axis computation below only remains as a
            // fallback for a lost/unassigned reference (warned about in OnValidate/Awake), since
            // it can never itself guarantee wall/tree clearance (its distance from the arena
            // centre depends on the player's live position at trigger time).
            if (_introCamAnchor != null)
                return _introCamAnchor.position;

            Vector3 towardCam = _player != null
                ? (transform.position - _player.position)
                : -transform.forward;
            towardCam.y = 0f;
            if (towardCam.sqrMagnitude < 0.0001f) towardCam = Vector3.forward;
            towardCam.Normalize();

            Vector3 camPos = transform.position + towardCam * _introCamDistance;
            camPos.y = _introCamHeight;
            return camPos;
        }

        private Vector3 ResolveCherryTreeLookPoint()
        {
            // ADR-0008 Amendment 2 §4/#7: aim at the tree's XZ position raised to an ABSOLUTE
            // world height, not an offset from the tree transform's own (trunk-base) Y — the two
            // are not interchangeable once the anchor/framing math depends on the exact look
            // height reaching the canopy's real visual centroid rather than a fixed offset above
            // wherever the trunk collider's pivot happens to sit.
            if (_cherryTreeLookTarget != null)
                return new Vector3(_cherryTreeLookTarget.position.x, _introTreeLookHeight, _cherryTreeLookTarget.position.z);

            // Fallback: locate by name, same defensive convention as SpinCycleAI.FindSaloon —
            // scans root objects once, not a per-frame call.
            var scene = UnityEngine.SceneManagement.SceneManager.GetActiveScene();
            var roots = scene.GetRootGameObjects();
            for (int i = 0; i < roots.Length; i++)
            {
                Transform hit = SearchByNameContains(roots[i].transform, _cherryTreeNameContains);
                if (hit != null) return new Vector3(hit.position.x, _introTreeLookHeight, hit.position.z);
            }

            Debug.LogWarning("[GrasscutterAI] Could not resolve the cherry tree for the intro's first camera cut (assign _cherryTreeLookTarget or ensure a scene object name contains \"" + _cherryTreeNameContains + "\") — falling back to the boss's own XZ position at the tree look height.", this);
            // Same absolute-height treatment as the two paths above — NOT bare transform.position,
            // which since ADR-0008 Amendment 2's _buriedYOffset (-4.45) sits well underground
            // during Phase A and would aim the camera into the dirt.
            return new Vector3(transform.position.x, _introTreeLookHeight, transform.position.z);
        }

        private Transform SearchByNameContains(Transform t, string contains)
        {
            if (t.name.IndexOf(contains, System.StringComparison.OrdinalIgnoreCase) >= 0)
                return t;
            for (int i = 0; i < t.childCount; i++)
            {
                Transform hit = SearchByNameContains(t.GetChild(i), contains);
                if (hit != null) return hit;
            }
            return null;
        }

        // ── Movement ──────────────────────────────────────────────────────────

        // PetalToss (Phase 1, slot 2) and SpinDash (Phase 2, slot 0 — wants distance to charge
        // across the arena rather than closing to melee) use the ranged approach distance.
        private bool NextAttackIsRanged()
        {
            if (_phase == Phase.Kata) return (_attackIndex % 3) == 2;
            return (_attackIndex % 2) == 0;
        }

        private void Approach()
        {
            float dist = Vector3.Distance(transform.position, _player.position);

            if (dist > chaseRange)
            {
                if (_agent != null) { _agent.isStopped = true; _agent.ResetPath(); }
                _state = BossState.Idle;
                return;
            }

            float attackRange = NextAttackIsRanged() ? rangedRange : meleeRange;
            if (dist <= attackRange && _attackCooldownTimer <= 0f)
            {
                float cooldown = _phase == Phase.Kata
                    ? attackCooldown
                    : attackCooldown * phase2CooldownMult;
                _attackCooldownTimer = cooldown;
                if (_agent != null) _agent.isStopped = true;
                StopActive();
                StartActive(AttackRoutine());
                return;
            }

            _reelTargetRPM = _phase == Phase.Kata ? _reelKataRPM : _reelRevRPM;

            if (_agent != null && _agent.isOnNavMesh)
            {
                _agent.speed     = _phase == Phase.Kata ? walkSpeed : runSpeed;
                _agent.isStopped = false;

                _pathUpdateTimer -= Time.deltaTime;
                if (_pathUpdateTimer <= 0f)
                {
                    _pathUpdateTimer = PathUpdateInterval;
                    _agent.SetDestination(_player.position);
                }

                if (_agent.velocity.sqrMagnitude > 0.01f)
                    transform.rotation = Quaternion.LookRotation(_agent.velocity.normalized);
            }
            else
            {
                if (_agent != null && !_agent.isOnNavMesh)
                    _agent.Warp(transform.position);

                Vector3 dir = (_player.position - transform.position).normalized;
                dir.y = 0f;
                float speed = _phase == Phase.Kata ? walkSpeed : runSpeed;
                transform.position += dir * speed * Time.deltaTime;
                if (dir != Vector3.zero)
                    transform.rotation = Quaternion.LookRotation(dir);
            }
        }

        // ── Attack dispatch ────────────────────────────────────────────────────

        private IEnumerator AttackRoutine()
        {
            if (_phase == Phase.Kata)
                yield return StartCoroutine(KataAttack());
            else
                yield return StartCoroutine(RevAttack());
        }

        // Phase 1 "Kata" pool: BladeCombo → ReelGuardBreak → PetalToss (repeating), exactly the
        // three attacks in GDD §5's Phase 1 table — no invented fourth attack.
        private IEnumerator KataAttack()
        {
            switch (_attackIndex % 3)
            {
                case 0: yield return StartCoroutine(BladeCombo());     break;
                case 1: yield return StartCoroutine(ReelGuardBreak()); break;
                case 2: yield return StartCoroutine(PetalToss());      break;
            }
            _attackIndex++;

            if (!_phaseTransitioned && _stats.CurrentHealth <= _stats.MaxHealth * 0.5f)
                yield return StartCoroutine(PhaseTransitionRoutine());

            if (_state != BossState.Dead && _state != BossState.Staggered)
                _state = BossState.Approaching;
        }

        // Phase 2 "Rev" pool: SpinDash → WhirlwindPull (repeating). Cut-Grass Trail is laid
        // automatically inside SpinDash, not cycled as its own attack (GDD §5).
        private IEnumerator RevAttack()
        {
            switch (_attackIndex % 2)
            {
                case 0: yield return StartCoroutine(SpinDash());      break;
                case 1: yield return StartCoroutine(WhirlwindPull()); break;
            }
            _attackIndex++;

            if (_state != BossState.Dead)
                _state = BossState.Approaching;
        }

        // ── Individual attacks — Phase 1 "Kata" ───────────────────────────────

        // Two-beat overhead reel swing. Both beats are independently parryable — GDD: "parryable
        // both beats." A successful parry on either beat staggers the boss and aborts the combo,
        // matching SpinCycleAI.DoubleHaymaker's structure (which differs only in that its second
        // beat is deliberately un-parryable — Grasscutter's Kata phase teaches the opposite tell).
        private IEnumerator BladeCombo()
        {
            yield return StartCoroutine(WindUp(Color.cyan, AttackTelegraphKind.MeleeParryable, _waitWindUp));
            _state = BossState.Attacking;
            SetColor(_baseColor);
            SafeSetTrigger(AnimAttack);

            if (IsPlayerWithinRange(meleeRange + 0.5f) && _playerCombat != null)
            {
                AttackResult result = _playerCombat.TryReceiveAttack(_stats.AttackDamage, parryable: true, attacker: gameObject);
                if (result == AttackResult.Hit)
                {
                    AudioManager.Instance?.Play(SoundEvent.EnemyHit);
                    _impulseSource?.GenerateImpulse();
                }
                if (result == AttackResult.Parried)
                {
                    yield return StartCoroutine(StaggerRoutine());
                    yield break;
                }
            }
            yield return _waitAttackActive;
            if (_state == BossState.Dead) yield break;

            yield return _waitBladeGap;
            if (_state == BossState.Dead) yield break;

            yield return StartCoroutine(WindUp(Color.cyan, AttackTelegraphKind.MeleeParryable, _waitWindUp));
            _state = BossState.Attacking;
            SetColor(_baseColor);
            SafeSetTrigger(AnimAttack);

            if (IsPlayerWithinRange(meleeRange + 0.5f) && _playerCombat != null)
            {
                AttackResult result = _playerCombat.TryReceiveAttack(_stats.AttackDamage, parryable: true, attacker: gameObject);
                if (result == AttackResult.Hit)
                {
                    AudioManager.Instance?.Play(SoundEvent.EnemyHit);
                    _impulseSource?.GenerateImpulse();
                }
                if (result == AttackResult.Parried)
                {
                    yield return StartCoroutine(StaggerRoutine());
                    yield break;
                }
            }
            yield return _waitAttackActive;
        }

        // Reel flares wide and slow. Never parryable — the player must dodge back instead.
        private IEnumerator ReelGuardBreak()
        {
            yield return StartCoroutine(WindUp(new Color(1f, 0.45f, 0f), AttackTelegraphKind.MeleeUnparryable, _waitGuardBreakWindUp));
            _state = BossState.Attacking;
            SetColor(_baseColor);
            SafeSetTrigger(AnimAttack);

            if (IsPlayerWithinRange(guardBreakRange) && _playerCombat != null)
            {
                AttackResult result = _playerCombat.TryReceiveAttack(_stats.AttackDamage, parryable: false);
                if (result == AttackResult.Hit)
                {
                    AudioManager.Instance?.Play(SoundEvent.EnemyHit);
                    _impulseSource?.GenerateImpulse();
                }
            }
            yield return _waitAttackActive;
        }

        // Retreats to range, then flicks a fan of cut blades. Dodge lateral or block (parryable).
        private IEnumerator PetalToss()
        {
            yield return StartCoroutine(JumpBack(AttackTelegraphKind.ProjectileParryable));
            _state = BossState.Attacking;
            SetColor(_baseColor);
            SafeSetTrigger(AnimAttack);

            if (_petalTossPrefab != null && _player != null)
            {
                Vector3 spawnPos = transform.position + Vector3.up * 1.2f + transform.forward * 0.6f;
                Vector3 forward  = _player.position - transform.position;
                forward.y = 0f;
                if (forward == Vector3.zero) forward = transform.forward;
                forward.Normalize();

                float startAngle = -_petalSpreadAngle * 0.5f * (_petalCount - 1);
                for (int i = 0; i < _petalCount; i++)
                {
                    float angle = startAngle + i * _petalSpreadAngle;
                    Vector3 dir = Quaternion.Euler(0f, angle, 0f) * forward;
                    GameObject petal = Instantiate(_petalTossPrefab, spawnPos, Quaternion.LookRotation(dir));
                    if (petal.TryGetComponent<BossProjectile>(out var proj))
                        proj.Initialize(_playerCombat);
                    if (petal.TryGetComponent<Rigidbody>(out var rb))
                        rb.linearVelocity = dir * _petalTossSpeed;
                }
            }
            else if (IsPlayerWithinRange(rangedRange) && _playerCombat != null)
            {
                // Fallback when no prefab is assigned — same convention as SpinCycleAI's
                // ClothesToss/SudsBurst. Flagged asset gap: no petal/cut-blade projectile
                // prefab exists yet in this project.
                AttackResult result = _playerCombat.TryReceiveAttack(_stats.AttackDamage, parryable: true, attacker: gameObject);
                if (result == AttackResult.Hit)
                    AudioManager.Instance?.Play(SoundEvent.EnemyHit);
            }

            yield return _waitAttackActive;
        }

        // ── Individual attacks — Phase 2 "Rev" ────────────────────────────────

        // Revs, aims, then charges in a straight line, laying a Cut-Grass Trail behind it.
        // Never parryable — dodge perpendicular to the lane. ADR-0006 §1.2: travel capped at
        // ≤ 6.5 m, hard constant. ADR-0005 §4 (mandatory): the landing point is
        // NavMesh-clamped before the move commits — see ClampToNavMesh.
        //
        // ADR-0007 §4 (load-bearing fix): the heading is computed and committed to
        // _committedDashDir HERE, before WindUp is entered — not after, against the player's
        // live position at launch. At 13 m/s against the player's 5 m/s, an attack that re-aims
        // after its own tell finishes is not dodgeable by movement at all; the comment above
        // ("dodge perpendicular to the lane") is only true once the heading is committed before
        // the player has to react to it. The ground-plane lane telegraph is raised on this same
        // committed heading, in the same frame, so the tell and the eventual attack always agree.
        private IEnumerator SpinDash()
        {
            Vector3 startPos  = transform.position;
            Vector3 aimTarget = _player != null ? _player.position : transform.position + transform.forward;
            Vector3 dir       = aimTarget - startPos;
            dir.y = 0f;
            if (dir.sqrMagnitude < 0.01f) dir = transform.forward;
            dir.Normalize();
            _committedDashDir = dir;

            // ADR-0007 §4: full-chord ground-plane lane — wall inner face to wall inner face
            // along the committed heading, not just the dash's own travel distance. A 6.5 m
            // segment starting at a north-rim boss would be entirely off-frame for a south-rim
            // player; a chord to both walls always has its near end inside the frustum.
            float laneWidth    = _dashContactRadius * 2f;
            float laneDuration = spinDashRevDuration + spinDashMaxDistance / Mathf.Max(0.1f, spinDashSpeed);
            (Vector3 laneStart, float laneLength) = ComputeDashLaneChord(startPos, _committedDashDir);
            _spinDashLaneHandle = AttackTelegraphService.ShowGroundLane(
                laneStart, _committedDashDir, laneLength, laneWidth,
                AttackTelegraphKind.AreaUnparryable, laneDuration);

            yield return StartCoroutine(WindUp(Color.red, AttackTelegraphKind.MeleeUnparryable, _waitSpinDashRev));
            if (_state == BossState.Dead)
            {
                AttackTelegraphService.Hide(_spinDashLaneHandle);
                yield break;
            }
            _state = BossState.Attacking;
            SetColor(_baseColor);
            SafeSetTrigger(AnimAttack);

            Vector3 rawEnd     = startPos + _committedDashDir * spinDashMaxDistance;
            Vector3 clampedEnd = ClampToNavMesh(rawEnd, startPos);

            // ADR-0007 §4 code-review Must-Fix 1: clamp only the DISTANCE traveled along the
            // already-committed heading — never re-derive a new heading from the clamped point.
            // NavMesh.SamplePosition's search radius (NavMeshSampleDistance, 2 m) means
            // clampedEnd can sit up to ~2 m laterally off the _committedDashDir ray; the
            // previous implementation re-derived `dirActual` from that clamped travel vector,
            // which silently let the dash veer up to ~2 m away from the heading the ground-plane
            // lane telegraph actually showed the player. That is not merely "over-stating" the
            // danger zone (the old comment's claim) — a lateral snap toward the clamped point
            // UNDER-states the danger exactly along the axis the player's dodge depends on,
            // defeating B118's entire "telegraph the real attack, don't let the boss cheat"
            // guarantee. Projecting the clamp onto the committed ray guarantees the dash always
            // travels the heading that was telegraphed; it can only ever be shortened, never
            // redirected.
            float travelDist = Mathf.Clamp(Vector3.Dot(clampedEnd - startPos, _committedDashDir), 0f, spinDashMaxDistance);

            if (travelDist < 0.05f)
            {
                // Nowhere safe to go (e.g. NavMesh sampling failed near the start point too) —
                // abort the charge gracefully rather than commit to an unclamped position.
                AttackTelegraphService.Hide(_spinDashLaneHandle);
                yield return _waitAttackActive;
                yield break;
            }

            Vector3 dirActual = _committedDashDir; // never re-derived — the dash always travels the advertised heading.
            Vector3 finalEnd  = startPos + dirActual * travelDist;

            // finalEnd is a point on the committed ray, not necessarily a point ClampToNavMesh
            // itself validated — that call searched around rawEnd, not this (generally shorter)
            // point. Re-validate finalEnd directly before committing to it; treat a failure the
            // same as the travelDist abort above rather than dash to an unvalidated position.
            if (!NavMesh.SamplePosition(finalEnd, out _, NavMeshSampleDistance, NavMesh.AllAreas))
            {
                AttackTelegraphService.Hide(_spinDashLaneHandle);
                yield return _waitAttackActive;
                yield break;
            }

            transform.rotation = Quaternion.LookRotation(dirActual);

            if (_agent != null) _agent.enabled = false;

            float duration = travelDist / Mathf.Max(0.1f, spinDashSpeed);
            float elapsed  = 0f;
            float trailTimer = 0f;
            bool hitLanded = false;

            while (elapsed < duration)
            {
                float dt = Time.deltaTime;
                transform.position = Vector3.Lerp(startPos, finalEnd, elapsed / duration);
                elapsed += dt;

                trailTimer -= dt;
                if (trailTimer <= 0f)
                {
                    trailTimer = _trailSpawnInterval;
                    SpawnTrailSegment(transform.position);
                }

                // ADR-0007 §4: the dash's own hit-test radius, promoted from a hardcoded 1.5f —
                // this is the same value the ground-plane lane's width is derived from above, so
                // the drawn band and the actual hitbox cannot drift apart.
                if (!hitLanded && IsPlayerWithinRange(_dashContactRadius) && _playerCombat != null)
                {
                    AttackResult result = _playerCombat.TryReceiveAttack(_stats.AttackDamage, parryable: false);
                    if (result == AttackResult.Hit)
                    {
                        AudioManager.Instance?.Play(SoundEvent.EnemyHit);
                        _impulseSource?.GenerateImpulse(dirActual * 1.5f);
                    }
                    hitLanded = true; // one hit per dash, same convention as SpinCycleAI.SpinCharge
                }

                yield return null;
            }

            transform.position = finalEnd;
            if (_agent != null) { _agent.enabled = true; _agent.Warp(transform.position); }
        }

        // ADR-0007 §4: measures the full chord of the arena along dir, wall inner face to wall
        // inner face, by raycasting outward from origin in both directions on the Building
        // layer and taking the FARTHEST hit each way. Farthest (not first) is what makes an
        // interior obstruction irrelevant without a name/tag special-case — verified against
        // Backyard_Dojo.unity that CherryTree_TrunkCollider is, in fact, also on the Building
        // layer (layer 8), same as every BD01_WallModule; taking the farthest hit means this is
        // correct regardless of whether the tree shares the wall's layer, since the wall is
        // always farther from the boss than the central tree. Zero allocation: reuses
        // _dashLaneRaycastBuffer via RaycastNonAlloc.
        private (Vector3 start, float length) ComputeDashLaneChord(Vector3 origin, Vector3 dir)
        {
            const float castHeight  = 0.5f;
            const float maxDistance = 24f;
            Vector3 rayOrigin = origin + Vector3.up * castHeight;
            float fallbackHalf = _dashLaneFallbackLength * 0.5f;

            float forwardDist  = FarthestHitDistance(rayOrigin, dir, maxDistance, fallbackHalf);
            float backwardDist = FarthestHitDistance(rayOrigin, -dir, maxDistance, fallbackHalf);

            Vector3 laneStart = origin - dir * backwardDist;
            float laneLength  = forwardDist + backwardDist;
            return (laneStart, laneLength);
        }

        private float FarthestHitDistance(Vector3 origin, Vector3 dir, float maxDistance, float fallback)
        {
            int count = Physics.RaycastNonAlloc(origin, dir, _dashLaneRaycastBuffer, maxDistance, _buildingLayerMask);
            if (count <= 0)
            {
                Debug.LogWarning($"[GrasscutterAI] ComputeDashLaneChord found no Building-layer wall along {dir} from {origin} — falling back to half of _dashLaneFallbackLength ({fallback} m) on this side. A missing wall must not silently collapse the lane to zero length.", this);
                return fallback;
            }

            float farthest = 0f;
            for (int i = 0; i < count; i++)
                if (_dashLaneRaycastBuffer[i].distance > farthest)
                    farthest = _dashLaneRaycastBuffer[i].distance;
            return farthest;
        }

        // Sustained spin drags the player inward. Never parryable — dodge outward against the
        // pull. Does not reposition the boss (only pulls the player via
        // PlayerController.ApplyExternalDisplacement), so there is no boss landing point to
        // NavMesh-clamp here — see class remarks.
        private IEnumerator WhirlwindPull()
        {
            yield return StartCoroutine(WindUp(new Color(0.6f, 0.2f, 0.85f), AttackTelegraphKind.AreaUnparryable, _waitWindUp));
            if (_state == BossState.Dead) yield break;
            _state = BossState.Attacking;
            SetColor(_baseColor);
            SafeSetTrigger(AnimAttack);

            float elapsed  = 0f;
            float hitTimer = 0f;

            while (elapsed < whirlwindDuration)
            {
                float dt = Time.deltaTime;
                elapsed += dt;

                if (_player != null)
                {
                    Vector3 toBoss = transform.position - _player.position;
                    toBoss.y = 0f;
                    float dist = toBoss.magnitude;

                    if (dist > 0.05f && dist <= rangedRange && _playerController != null)
                    {
                        Vector3 pull = toBoss.normalized * whirlwindPullStrength * dt;
                        if (pull.magnitude > dist) pull = toBoss; // never overshoot past the boss
                        _playerController.ApplyExternalDisplacement(pull);
                    }

                    hitTimer -= dt;
                    if (hitTimer <= 0f && dist <= whirlwindContactRange && _playerCombat != null)
                    {
                        AttackResult result = _playerCombat.TryReceiveAttack(_stats.AttackDamage, parryable: false);
                        if (result == AttackResult.Hit)
                            AudioManager.Instance?.Play(SoundEvent.EnemyHit);
                        hitTimer = whirlwindContactTickInterval;
                    }
                }

                yield return null;
            }
        }

        private void SpawnTrailSegment(Vector3 position)
        {
            if (_trailPool == null || _trailPool.Length == 0) return;
            int idx = _trailNextIndex;
            _trailNextIndex = (_trailNextIndex + 1) % _trailPool.Length;
            _trailPool[idx].Activate(position, _trailHazardRadius, _trailHazardDuration, _trailHazardDamage);
        }

        // ── Shared helpers ────────────────────────────────────────────────────

        // ADR-0003: raises the occlusion-independent overhead telegraph from the same seam every
        // attack tell in this codebase goes through — additive to the existing body-tint colour.
        private IEnumerator WindUp(Color color, AttackTelegraphKind kind, WaitForSeconds cachedWait)
        {
            _state = BossState.WindUp;
            SetColor(color);
            AttackTelegraphService.Show(transform, kind, WaitDuration(cachedWait), _telegraphHeightOffset);
            yield return cachedWait;
        }

        // WaitForSeconds does not expose its duration; the cached wait objects are constructed
        // 1:1 from a known field above, so this small lookup keeps WindUp() generic without a
        // second parameter that could drift out of sync with the WaitForSeconds it is paired with.
        private float WaitDuration(WaitForSeconds wait)
        {
            if (wait == _waitWindUp)           return windUpDuration;
            if (wait == _waitGuardBreakWindUp) return guardBreakWindUpDuration;
            if (wait == _waitSpinDashRev)      return spinDashRevDuration;
            return windUpDuration;
        }

        private IEnumerator JumpBack(AttackTelegraphKind kind)
        {
            _state = BossState.WindUp;
            SetColor(Color.cyan);
            AttackTelegraphService.Show(transform, kind, jumpBackDuration, _telegraphHeightOffset);

            Vector3 startPos = transform.position;
            Vector3 awayDir = _player != null
                ? (transform.position - _player.position).normalized
                : -transform.forward;
            awayDir.y = 0f;
            if (awayDir == Vector3.zero) awayDir = -transform.forward;

            Vector3 landPos = startPos + awayDir * jumpBackDistance;
            landPos.y = startPos.y;
            // ADR-0005 §4 (mandatory): clamp before the move commits.
            landPos = ClampToNavMesh(landPos, startPos);

            if (_agent != null) _agent.enabled = false;

            float elapsed = 0f;
            while (elapsed < jumpBackDuration)
            {
                float t = elapsed / jumpBackDuration;
                Vector3 flatPos = Vector3.Lerp(startPos, landPos, t);
                flatPos.y = startPos.y + jumpBackHeight * Mathf.Sin(t * Mathf.PI);
                transform.position = flatPos;
                elapsed += Time.deltaTime;
                yield return null;
            }

            transform.position = landPos;
            if (_agent != null) { _agent.enabled = true; _agent.Warp(transform.position); }

            if (_player != null)
            {
                Vector3 toPlayer = _player.position - transform.position;
                toPlayer.y = 0f;
                if (toPlayer.sqrMagnitude > 0.01f)
                    transform.rotation = Quaternion.LookRotation(toPlayer.normalized);
            }

            SetColor(_baseColor);
        }

        // ADR-0005 §4 / ADR-0006 §1.2 (mandatory, not optional): resolves a committed movement's
        // landing point through NavMesh.SamplePosition before the move commits. This is the exact
        // fix for the bug SpinCycleAI shipped (raw transform moves with no bounds check can land
        // the boss ~3 m inside a building) — it must not repeat here. Backs off toward `fallback`
        // in steps if the desired point does not sample cleanly, and only returns `fallback`
        // itself (never an unclamped raw position) if every step fails.
        private Vector3 ClampToNavMesh(Vector3 desired, Vector3 fallback)
        {
            if (NavMesh.SamplePosition(desired, out NavMeshHit hit, NavMeshSampleDistance, NavMesh.AllAreas))
                return hit.position;

            for (int i = 1; i <= 8; i++)
            {
                float t = i / 8f;
                Vector3 candidate = Vector3.Lerp(desired, fallback, t);
                if (NavMesh.SamplePosition(candidate, out NavMeshHit backoffHit, NavMeshSampleDistance, NavMesh.AllAreas))
                    return backoffHit.position;
            }

            Debug.LogWarning("[GrasscutterAI] ClampToNavMesh found no valid NavMesh position between the desired landing point and the fallback — holding at the fallback position.", this);
            return fallback;
        }

        private bool IsPlayerWithinRange(float range)
        {
            if (_player == null) return false;
            return Vector3.Distance(transform.position, _player.position) <= range;
        }

        // ── Phase transition ──────────────────────────────────────────────────

        private IEnumerator PhaseTransitionRoutine()
        {
            _phaseTransitioned = true;
            _state = BossState.PhaseTransition;
            SafeSetTrigger(AnimStagger);
            SetColor(_baseColor);
            _impulseSource?.GenerateImpulse();

            float t = 0f;
            while (t < phaseTransitionPause) { t += Time.deltaTime; yield return null; }

            _phase         = Phase.Rev;
            _attackIndex   = 0;
            _reelTargetRPM = _reelRevRPM;
            _state         = BossState.Approaching;
        }

        // ── Stagger ───────────────────────────────────────────────────────────

        private IEnumerator StaggerRoutine()
        {
            _state = BossState.Staggered;
            SafeSetTrigger(AnimStagger);
            _impulseSource?.GenerateImpulse(Vector3.up * 0.5f);
            SetColor(Color.yellow);
            yield return _waitStagger;
            SetColor(_baseColor);
            if (_state == BossState.Staggered)
                _state = BossState.Approaching;
        }

        // ── Event handlers ────────────────────────────────────────────────────

        private void OnCounterStrikeLanded(GameObject target)
        {
            if (target != null && target != gameObject) return;
            if (_state != BossState.Staggered) return;
            _stats.TakeDamage(counterStrikeDamage);
        }

        private void HandleDeath()
        {
            _state = BossState.Dead;

            if (_agent != null) { _agent.isStopped = true; _agent.enabled = false; }

            // ADR-0007: StopAllCoroutines() below discards SpinDash's IEnumerator mid-flight
            // without running any of its own cleanup past the current yield — a Unity coroutine
            // stopped externally never resumes to hit a later Hide() call, so an active
            // ground-plane lane must be explicitly hidden here rather than relying on SpinDash's
            // own Dead-state check (which only fires if the coroutine resumes on its own).
            AttackTelegraphService.Hide(_spinDashLaneHandle);

            // Same lesson applies to BossIntro's player-input lock: StopAllCoroutines() below
            // discards that coroutine's IEnumerator without ever resuming it, and — verified
            // directly in this Unity version with an isolated try/finally coroutine test — Unity's
            // StopAllCoroutines()/StopCoroutine() do NOT call Dispose() on the stopped enumerator,
            // so a finally block inside BossIntro's try never runs on an external stop either. In
            // practice this path is unreachable while _stats stays invulnerable for BossIntro's
            // entire duration (TakeDamage() no-ops under _invulnerable, so HandleDeath can't fire
            // mid-intro today), but re-enabling here — not just relying on BossIntro's own
            // try/finally — is the same defense-in-depth this method already uses for the dash
            // lane above, and costs nothing if _playerController is already enabled.
            if (_playerController != null) _playerController.enabled = true;

            StopAllCoroutines();
            _activeRoutine = null;

            if (_animator != null)
            {
                _animator.speed = 1f;
                SafeSetFloat(AnimSpeed, 0f);
                SafeSetBool(AnimIsDead, true);
            }

            SetColor(Color.gray);

            // Destroy any in-flight petal projectiles so they cannot kill the player during the
            // defeat sequence — same convention as SpinCycleAI.
            foreach (var proj in FindObjectsByType<BossProjectile>(FindObjectsInactive.Exclude, FindObjectsSortMode.None))
                Destroy(proj.gameObject);

            _defeatRoutine = StartCoroutine(DefeatSequence());
        }

        // Entirely unscaled/real time — see SpinCycleAI.DefeatSequence's class-level remarks for
        // the full rationale (docs/BACKLOG.md World 1 boss-win bug): if anything else in the
        // scene has left Time.timeScale at 0 at the moment of the killing blow, a sequence that
        // waits on scaled time can silently stall forever and TriggerWin() never fires. This
        // guarantees the win condition always reaches TriggerWin() a few real seconds after
        // death regardless of Time.timeScale.
        private IEnumerator DefeatSequence()
        {
            // Reel deceleration to zero (GDD: "the reel jams, grinds, stops") happens
            // automatically via LateUpdate's Dead-state branch once _state == Dead, set above.
            SetColor(Color.gray);
            yield return _waitDefeatStumble;

            yield return StartCoroutine(WobbleRoutine(_defeatWobbleDuration));

            if (_deathBurstVFX != null)
            {
                ParticleSystem burst = Instantiate(_deathBurstVFX, transform.position, Quaternion.identity);
                burst.Play();
                Destroy(burst.gameObject, burst.main.duration + burst.main.startLifetime.constantMax + 0.5f);
            }

            yield return StartCoroutine(ShrinkAndFade(defeatHoldDuration));

            if (_imaginationVolume == null)
                _imaginationVolume = GameObject.Find("ImaginationRestore_Volume")
                    ?.GetComponent<UnityEngine.Rendering.Volume>();

            if (_imaginationVolume != null)
                yield return StartCoroutine(LerpImagination(_imaginationLerpDuration));
            else
                GameManager.Instance?.TriggerWin();

            Destroy(gameObject);
        }

        private IEnumerator WobbleRoutine(float duration)
        {
            float elapsed     = 0f;
            float startAngleY = transform.eulerAngles.y;
            const float wobbleAmplitude = 15f;
            const float wobbleFrequency = 12f;

            while (elapsed < duration)
            {
                elapsed += Time.unscaledDeltaTime;
                float angle = Mathf.Sin(elapsed * wobbleFrequency) * wobbleAmplitude * (1f - elapsed / duration);
                transform.rotation = Quaternion.Euler(0f, startAngleY + angle, 0f);
                yield return null;
            }

            transform.rotation = Quaternion.Euler(0f, startAngleY, 0f);
        }

        private IEnumerator ShrinkAndFade(float duration)
        {
            Vector3 startScale = transform.localScale;
            float elapsed = 0f;

            while (elapsed < duration)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(elapsed / duration));
                transform.localScale = Vector3.Lerp(startScale, Vector3.zero, t);
                yield return null;
            }

            transform.localScale = Vector3.zero;
        }

        private IEnumerator LerpImagination(float duration)
        {
            float elapsed = 0f;
            while (elapsed < duration)
            {
                if (_imaginationVolume == null) yield break;
                elapsed += Time.unscaledDeltaTime;
                _imaginationVolume.weight = Mathf.Clamp01(elapsed / duration);
                yield return null;
            }
            if (_imaginationVolume != null)
                _imaginationVolume.weight = 1f;
            GameManager.Instance?.TriggerWin();
        }

        // ── Helpers ───────────────────────────────────────────────────────────

        private void SetColor(Color color)
        {
            if (_material != null)
                _material.color = color;
        }

        // See the class-level remark on _animatorParamHashes: AC_Grasscutter.controller does not
        // yet define Speed or StaggerTrigger, and Animator.SetFloat/SetTrigger logs a real
        // console error (not a silent no-op) for a hash with no matching parameter. Cached once
        // here instead of querying _animator.parameters on every call.
        private void CacheAnimatorParameters()
        {
            _animatorParamHashes = new System.Collections.Generic.HashSet<int>();
            if (_animator == null) return;
            foreach (var p in _animator.parameters)
                _animatorParamHashes.Add(p.nameHash);
        }

        private void SafeSetFloat(int hash, float value)
        {
            if (_animator != null && _animatorParamHashes != null && _animatorParamHashes.Contains(hash))
                _animator.SetFloat(hash, value);
        }

        private void SafeSetTrigger(int hash)
        {
            if (_animator != null && _animatorParamHashes != null && _animatorParamHashes.Contains(hash))
                _animator.SetTrigger(hash);
        }

        private void SafeSetBool(int hash, bool value)
        {
            if (_animator != null && _animatorParamHashes != null && _animatorParamHashes.Contains(hash))
                _animator.SetBool(hash, value);
        }

        private void StartActive(IEnumerator routine)
        {
            _activeRoutine = StartCoroutine(routine);
        }

        private void StopActive()
        {
            if (_activeRoutine != null)
            {
                StopCoroutine(_activeRoutine);
                _activeRoutine = null;
            }
        }

        private void OnDestroy()
        {
            if (_introVcamGO != null)
            {
                Destroy(_introVcamGO);
                _introVcamGO = null;
                _introVcam   = null;
            }

            // Same defense-in-depth as HandleDeath's identical line (see its comment for the
            // verified reason a BossIntro-only try/finally isn't enough): this covers a BossIntro
            // interrupted by scene teardown/GameObject destruction WITHOUT going through
            // HandleDeath first (e.g. the player leaves Backyard_Dojo mid-cinematic). A player
            // permanently frozen because this coroutine was cut off would be a worse bug than the
            // missing input lock this exists to fix — null-check only, since the player object may
            // already be mid-teardown itself in the same scene unload.
            if (_playerController != null) _playerController.enabled = true;

            StopAllCoroutines();
            _activeRoutine = null;
            _defeatRoutine = null;

            if (_playerCombat != null)
                _playerCombat.OnCounterStrike -= OnCounterStrikeLanded;

            if (_stats != null)
                _stats.OnDeath -= HandleDeath;

            if (_material != null)
                Destroy(_material);
        }
    }
}
