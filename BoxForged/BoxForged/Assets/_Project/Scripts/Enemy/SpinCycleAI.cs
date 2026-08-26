using System.Collections;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Rendering;
using Boxhead.Core;
using Boxhead.Player;

namespace Boxhead.Enemy
{
    [RequireComponent(typeof(EnemyStats))]
    [RequireComponent(typeof(NavMeshAgent))]
    public class SpinCycleAI : MonoBehaviour
    {
        // ── Inspector ─────────────────────────────────────────────────────────

        [Header("Movement")]
        [SerializeField] private float walkSpeed          = 2f;
        [SerializeField] private float runSpeed           = 4f;
        [SerializeField] private float chaseRange         = 12f;
        [SerializeField] private float meleeRange         = 4f;
        [SerializeField] private float rangedRange        = 8f;

        [Header("Attack Timing")]
        [SerializeField] private float windUpDuration        = 1.0f;
        [SerializeField] private float attackActiveDuration  = 0.4f;
        [SerializeField] private float attackCooldown        = 2.5f;
        [SerializeField] private float phase2CooldownMult    = 0.7f;

        [Header("Stagger")]
        [SerializeField] private float staggerDuration = 2.5f;

        [Header("SpinCharge")]
        [SerializeField] private float spinChargeSpeed    = 8f;
        [SerializeField] private float spinChargeDuration = 0.6f;

        [Header("JumpBack")]
        [SerializeField] private float jumpBackDistance = 4f;
        [SerializeField] private float jumpBackHeight   = 1.8f;
        [SerializeField] private float jumpBackDuration = 0.45f;

        [Header("JumpCharge")]
        [SerializeField] private float jumpChargeDuration = 0.8f;
        [SerializeField] private float jumpChargeHeight   = 2f;

        [Header("FullSpin")]
        [SerializeField] private float fullSpinRadius = 3f;

        [Header("Phase Transition")]
        [SerializeField] private float phaseTransitionPause = 1.5f;

        [Header("Defeat")]
        [SerializeField] private float defeatHoldDuration = 2.5f;
        [Tooltip("Particle burst spawned at SpinCycle's position the moment he starts to vanish. Assign ExplosionVFX or any burst prefab.")]
        [SerializeField] private ParticleSystem _deathBurstVFX;
        [SerializeField] private float _defeatStumbleDuration = 0.4f;
        [SerializeField] private float _defeatWobbleDuration  = 0.5f;

        [Header("Counter Strike")]
        [SerializeField] private int counterStrikeDamage = 40;

        [Header("Projectiles")]
        [SerializeField] private GameObject _clothesTossPrefab;
        [SerializeField] private GameObject _sudsBlobPrefab;
        [SerializeField] private float _clothesTossSpeed      = 8f;
        [SerializeField] private float _sudsBlobSpeed         = 6f;
        [SerializeField] private float _sudsBlobArcVelocity   = 1.5f;

        [Header("Held Props")]
        [Tooltip("Visual-only ClothesBall parented to the left hand hold point.")]
        [SerializeField] private GameObject _heldClothesBall;
        [Tooltip("Visual-only SudsBlob parented to the right hand hold point.")]
        [SerializeField] private GameObject _heldSudsBlob;

        [Header("Screen Shake")]
        [SerializeField] private CinemachineImpulseSource _impulseSource;

        [Header("Imagination Restore")]
        [SerializeField] private Volume _imaginationVolume;
        [SerializeField] private float _imaginationLerpDuration = 1.5f;

        [Header("Intro")]
        [SerializeField] private Transform _drumHead;               // assign: SpinCycle_Head child
        [SerializeField] private float     _introWalkTarget_X     = -4f;
        [SerializeField] private float     _introWalkTarget_Z     = 4f;
        [SerializeField] private float     _introCameraFoV        = 30f;  // FoV at peak zoom during spin-up
        // ADR-0001: must match pfb_CM_FollowCam's Lens.FieldOfView (45) or the boss-intro ->
        // gameplay handoff pops. No live camera reference is wired here, so this stays a
        // hardcoded duplicate — keep it in sync if the rig's FOV changes again.
        [SerializeField] private float     _normalCameraFoV       = 45f;  // restored after intro
        [SerializeField] private float     _introPanDuration      = 1f;    // how long to hold on the doorway before he emerges
        [SerializeField] private float     _introRunDuration      = 2.5f;  // how long SpinCycle takes to walk out
        [SerializeField] private float     _introWalkInDistance   = 5f;    // how far back inside the saloon the boss starts, behind the doorway
        [SerializeField] private float     _introPostWalkPause    = 0.6f;  // pause after he is fully out, before the head spins up

        [Header("Intro — Saloon Geometry")]
        [Tooltip("Substring used to locate the runtime-spawned saloon facade instance. Derives the doorway from its live transform.")]
        [SerializeField] private string    _saloonNameContains    = "saloon_facade";
        [Tooltip("Height (world Y) of the raised porch deck the boss stands on before stepping down. Derived from the saloon mesh (~0.5).")]
        [SerializeField] private float     _porchDeckHeight       = 0.5f;
        [Tooltip("Distance from the saloon center out to the front wall / doorway plane, along the facade's outward forward. Derived from the mesh (~2.2).")]
        [SerializeField] private float     _doorDepthInset        = 2.2f;
        [Tooltip("Fallback saloon world position if the runtime instance cannot be found by name.")]
        [SerializeField] private Vector3   _saloonFallbackPos     = new Vector3(-8f, 0f, 8f);
        [Tooltip("Fallback saloon Y rotation (degrees) if the runtime instance cannot be found by name.")]
        [SerializeField] private float     _saloonFallbackYaw     = 135f;

        [Header("Intro — Cinematic Camera")]
        [Tooltip("Distance out from the doorway (along the walk-out direction) at the START of the emergence — a tighter establishing shot on the doorway.")]
        [SerializeField] private float     _introCamStartDistance = 8f;
        [Tooltip("Distance out from the doorway at the END of the emergence — the camera dollies BACK to this as SpinCycle walks out so his full body stays framed.")]
        [SerializeField] private float     _introCamEndDistance   = 14f;
        [Tooltip("World height of the intro camera — near the boss's chest/face for a level, non-top-down angle.")]
        [SerializeField] private float     _introCamHeight        = 1.8f;
        [Tooltip("World height the intro camera looks at (boss chest height).")]
        [SerializeField] private float     _introCamLookHeight    = 2.0f;
        [Tooltip("Priority given to the dedicated intro vcam while it is active (must exceed the gameplay cam's).")]
        [SerializeField] private int       _introCamPriority      = 100;

        [Header("References")]
        [SerializeField] private DrumWindowRotator drumWindow;

        // ── State ─────────────────────────────────────────────────────────────

        private enum BossState
        {
            Idle, Approaching, WindUp, Attacking, Staggered, PhaseTransition, Dead
        }

        private enum Phase { One, Two }

        private BossState _state = BossState.Idle;
        private Phase _phase = Phase.One;

        // Attack pools cycle in order; index wraps on each completion.
        private int _attackIndex;
        private bool _phaseTransitioned;
        private float _attackCooldownTimer;
        private bool       _introComplete;

        // Dedicated cinematic intro camera — created in Awake so it frames the doorway on
        // frame 1 (no top-down flash), torn down in Phase 4 when gameplay resumes.
        private Unity.Cinemachine.CinemachineCamera _introVcam;
        private GameObject _introVcamGO;

        // Doorway geometry, derived from the saloon's runtime transform in Awake.
        private Vector3 _doorwayGround;   // world XZ of the doorway threshold at ground level
        private Vector3 _saloonOutward;   // facade forward (points from saloon out into the arena)

        // ── References ────────────────────────────────────────────────────────

        private Transform _player;
        private CombatController _playerCombat;
        private EnemyStats _stats;
        private Animator _animator;
        private Renderer _renderer;
        private Material _material;
        private Color _baseColor;
        private Coroutine _activeRoutine;
        private Coroutine _defeatRoutine;

        // Pre-allocated overlap buffer — FullSpin hits up to 4 colliders.
        private readonly Collider[] _overlapBuffer = new Collider[4];

        // ── Animator param hashes ─────────────────────────────────────────────

        private static readonly int AnimSpeed   = Animator.StringToHash("Speed");
        private static readonly int AnimAttack  = Animator.StringToHash("AttackTrigger");
        private static readonly int AnimStagger = Animator.StringToHash("StaggerTrigger");
        private static readonly int AnimIsDead  = Animator.StringToHash("IsDead");

        // Suds burst fires 3 blobs: center, left -25°, right +25°.
        private static readonly float[] _sudsBurstAngles = { 0f, -25f, 25f };

        // ── NavMeshAgent ──────────────────────────────────────────────────────

        private NavMeshAgent _agent;
        private static readonly float PathUpdateInterval = 0.25f;
        private float _pathUpdateTimer;

        // ── Cached yields ─────────────────────────────────────────────────────

        private WaitForSeconds _waitWindUp;
        private WaitForSeconds _waitAttackActive;
        private WaitForSeconds _waitStagger;
        private WaitForSeconds _waitPhaseTransition;
        private WaitForSeconds _waitDefeatHold;
        // Realtime (unscaled), not WaitForSeconds: DefeatSequence must always reach TriggerWin()
        // even if something elsewhere in the scene has left Time.timeScale at 0 (e.g. a UI panel
        // that failed to restore it) — see the unscaled-time fix throughout DefeatSequence below.
        private WaitForSecondsRealtime _waitDefeatStumble;

        // ── Unity lifecycle ───────────────────────────────────────────────────

        private void Awake()
        {
            _stats    = GetComponent<EnemyStats>();
            _animator = GetComponentInChildren<Animator>();

            // Derive the doorway geometry from the saloon's runtime transform (source of truth),
            // then stand up a dedicated cinematic intro camera framing that doorway from the
            // front — created here, before any Start() runs, so frame 1 shows the intro framing
            // and never the top-down gameplay view of the arena.
            DeriveDoorwayGeometry();
            CreateIntroCamera();

            _renderer = GetComponentInChildren<Renderer>();
            if (_renderer != null)
            {
                _material  = _renderer.material;
                _baseColor = _material.GetColor("_BaseColor");
            }

            _waitWindUp          = new WaitForSeconds(windUpDuration);
            _waitAttackActive    = new WaitForSeconds(attackActiveDuration);
            _waitStagger         = new WaitForSeconds(staggerDuration);
            _waitPhaseTransition = new WaitForSeconds(phaseTransitionPause);
            _waitDefeatHold      = new WaitForSeconds(defeatHoldDuration);
            _waitDefeatStumble   = new WaitForSecondsRealtime(_defeatStumbleDuration);

            _agent = GetComponent<NavMeshAgent>();
            if (_agent != null)
            {
                _agent.speed            = walkSpeed;
                _agent.stoppingDistance = meleeRange - 0.5f;
                _agent.updateRotation   = false;
                _agent.isStopped        = true;
            }

            // Bump amplitude so shake is noticeable on mobile
            if (_impulseSource != null)
                _impulseSource.ImpulseDefinition.AmplitudeGain = 3f;
        }

        // Locates the runtime-spawned saloon facade by name and computes the doorway threshold
        // position (front-face center, at ground level) and the facade's outward direction.
        // Falls back to the serialized saloon transform if the instance is not found yet.
        private void DeriveDoorwayGeometry()
        {
            Vector3 saloonPos;
            Vector3 outward;

            Transform saloon = FindSaloon();
            if (saloon != null)
            {
                saloonPos = saloon.position;
                outward   = Vector3.ProjectOnPlane(saloon.forward, Vector3.up).normalized;
            }
            else
            {
                saloonPos = _saloonFallbackPos;
                outward   = Quaternion.Euler(0f, _saloonFallbackYaw, 0f) * Vector3.forward;
            }

            if (outward.sqrMagnitude < 0.0001f) outward = Vector3.forward;

            _saloonOutward = outward;
            // Doorway threshold: push out from the saloon center to the front wall plane,
            // horizontally centered on the facade, at ground level.
            _doorwayGround = new Vector3(
                saloonPos.x + outward.x * _doorDepthInset,
                0f,
                saloonPos.z + outward.z * _doorDepthInset);
        }

        // Finds the saloon facade instance in the loaded scene by name substring.
        // Avoids FindObjectsOfTypeAll (which returns prefab assets) — scans root objects only,
        // once, in Awake; not a per-frame call.
        private Transform FindSaloon()
        {
            var scene = UnityEngine.SceneManagement.SceneManager.GetActiveScene();
            var roots = scene.GetRootGameObjects();
            for (int i = 0; i < roots.Length; i++)
            {
                Transform hit = SearchByName(roots[i].transform);
                if (hit != null) return hit;
            }
            return null;
        }

        private Transform SearchByName(Transform t)
        {
            if (t.name.IndexOf(_saloonNameContains, System.StringComparison.OrdinalIgnoreCase) >= 0)
                return t;
            for (int i = 0; i < t.childCount; i++)
            {
                Transform hit = SearchByName(t.GetChild(i));
                if (hit != null) return hit;
            }
            return null;
        }

        // Builds the dedicated cinematic intro vcam: a front view of the doorway at chest
        // height, pulled back along the walk-out direction so the whole boss + doorway frame.
        // The gameplay cam is left untouched; we simply out-prioritize it, then hand back in
        // Phase 4 by disabling this vcam.
        private void CreateIntroCamera()
        {
            _introVcamGO = new GameObject("CM_BossIntroCam");
            _introVcam   = _introVcamGO.AddComponent<Unity.Cinemachine.CinemachineCamera>();

            // Position in FRONT of the doorway (arena side) at the START distance, looking back
            // toward the saloon. BossIntro dollies the camera back to _introCamEndDistance and
            // pans the look point to follow the boss as he emerges.
            PositionIntroCamera(_introCamStartDistance, _doorwayGround);

            var lens = _introVcam.Lens;
            lens.FieldOfView = _normalCameraFoV;
            _introVcam.Lens  = lens;

            // No Follow/LookAt targets — this is a fixed cinematic shot, driven by transform only.
            // Priority struct in Cinemachine 3: set the value so the Brain prefers this vcam,
            // then toggle `enabled` (not priority math) to hand control back later.
            var prio = _introVcam.Priority;
            prio.Value       = _introCamPriority;
            _introVcam.Priority = prio;
            _introVcam.enabled  = true;

            // The vcam is enabled with the highest priority before the first Brain update, so
            // the Brain selects it on frame 1 — no top-down flash of the arena. (We intentionally
            // do NOT call ManualUpdate here: the Brain is in its normal update mode, so that call
            // is a no-op that only logs a warning.)
        }

        // Places the intro vcam `distance` metres out in front of the doorway (arena side),
        // at _introCamHeight, aiming at `lookTargetXZ` raised to _introCamLookHeight. Used both
        // for the initial placement and for the per-frame dolly-back during the emergence.
        private void PositionIntroCamera(float distance, Vector3 lookTargetXZ)
        {
            if (_introVcamGO == null) return;

            Vector3 camPos = _doorwayGround + _saloonOutward * distance;
            camPos.y = _introCamHeight;
            _introVcamGO.transform.position = camPos;

            Vector3 lookAt = lookTargetXZ;
            lookAt.y = _introCamLookHeight;
            _introVcamGO.transform.rotation = Quaternion.LookRotation((lookAt - camPos).normalized);
        }

        private void Start()
        {
            var playerObj = GameObject.FindWithTag("Player");
            if (playerObj != null)
            {
                _player       = playerObj.transform;
                _playerCombat = playerObj.GetComponent<CombatController>();

                if (_playerCombat != null)
                    _playerCombat.OnCounterStrike += OnCounterStrikeLanded;
            }

            _stats.OnDeath += HandleDeath;

            // Play the pre-rendered standoff cutscene first (every encounter, skippable), then
            // run the in-engine emergence intro. The video overlay hides the scene, so the boss
            // stays hidden behind it until the in-engine BossIntro takes over on the callback.
            var cutscene = Boxhead.Core.CutscenePlayer.Instance;
            if (cutscene != null)
                cutscene.Play(
                    Boxhead.Core.CutsceneCatalog.SpinCycleStandoff,
                    onFinished: () => StartCoroutine(BossIntro()),
                    skippable: true);
            else
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
                    // Do not transition to Approaching until the intro coroutine completes.
                    if (_introComplete && Vector3.Distance(transform.position, _player.position) <= chaseRange)
                        _state = BossState.Approaching;
                    break;

                case BossState.Approaching:
                    Approach();
                    break;
            }

            float speed = _state == BossState.Approaching
                ? (_agent != null ? _agent.velocity.magnitude : (_phase == Phase.One ? walkSpeed : runSpeed))
                : 0f;
            _animator?.SetFloat(AnimSpeed, speed);
        }

        // ── Boss intro ─────────────────────────────────────────────────────────

        // Boss intro: SpinCycle starts tiny on the saloon porch deck behind the swinging doors,
        // walks OUT through the doorway growing to full size while STEPPING DOWN off the porch
        // to ground level, pauses, then spins up before combat begins. A dedicated cinematic
        // camera frames the doorway head-on for the whole emergence.
        private IEnumerator BossIntro()
        {
            _stats?.SetInvulnerable(true);
            _state = BossState.Idle;
            drumWindow?.ResetToForward();

            // Cache the designed full size so we restore it correctly at the end.
            Vector3 fullScale = transform.localScale;

            // ── Compute intro positions from the DERIVED doorway (source of truth) ──
            // _doorwayGround / _saloonOutward were computed in Awake from the saloon's runtime
            // transform. Outward points from the saloon out into the arena.
            Vector3 outward = _saloonOutward;

            // Start: horizontally centered on the doorway, pushed BACK into the building against
            // outward, standing UP on the porch deck (deck height) so he begins tiny inside.
            Vector3 insideStart = _doorwayGround - outward * _introWalkInDistance;
            insideStart.y = _porchDeckHeight;

            // Doorway threshold, at deck height — the point he passes through as he emerges.
            Vector3 doorwayTop = _doorwayGround;
            doorwayTop.y = _porchDeckHeight;

            // End: the walk-out target out in the arena, at GROUND level (steps down off porch).
            Vector3 walkOutTarget = new Vector3(_introWalkTarget_X, 0f, _introWalkTarget_Z);

            // Teleport the boss onto the deck inside the saloon and shrink to a tiny fraction
            // BEFORE the first rendered frame, so the intro cam sees him start tiny in the doorway.
            transform.position   = insideStart;
            transform.rotation   = Quaternion.LookRotation(outward);
            transform.localScale = fullScale * 0.02f;

            // ── Phase 1: Hold on the doorway ──
            // The dedicated intro vcam (created in Awake) is already framing the doorway, so we
            // simply hold here while the shot settles and the boss stands tiny in the opening.
            float panTimer = 0f;
            while (panTimer < _introPanDuration) { panTimer += Time.deltaTime; yield return null; }

            // ── Phase 2: Walk out through the doorway + grow + step down off the porch ──
            // The start point is on the deck, off the NavMesh — the agent cannot move him from
            // there. Disable it and drive the walk with a direct transform lerp, then re-enable
            // + Warp once he is out in the arena at ground level.
            if (_agent != null) _agent.enabled = false;

            float runDuration = Mathf.Max(0.0001f, _introRunDuration);
            // Fraction of the walk spent crossing the deck (inside → doorway). During this leg
            // he stays centered on the doorway and at deck height; afterward he steps down.
            const float doorwayFrac = 0.45f;
            float walkTimer = 0f;
            while (walkTimer < runDuration)
            {
                walkTimer += Time.deltaTime;
                float t = Mathf.Clamp01(walkTimer / runDuration);

                Vector3 pos;
                if (t <= doorwayFrac)
                {
                    // Leg 1 — cross the deck to the doorway, centered on the doorway line, deck height.
                    float u = t / doorwayFrac;
                    pos   = Vector3.Lerp(insideStart, doorwayTop, u);
                    pos.y = _porchDeckHeight;
                }
                else
                {
                    // Leg 2 — leave the doorway, step down off the porch to ground level in the arena.
                    float u = (t - doorwayFrac) / (1f - doorwayFrac);
                    pos   = Vector3.Lerp(doorwayTop, walkOutTarget, u);
                    pos.y = Mathf.Lerp(_porchDeckHeight, 0f, u); // smooth descent off the deck
                }
                transform.position = pos;

                // Grow from tiny to full size, easing in so he "swells" as he approaches.
                transform.localScale = fullScale * Mathf.Lerp(0.02f, 1f, t * t);
                // Face the walk-out direction throughout.
                transform.rotation = Quaternion.LookRotation(outward);
                _animator?.SetFloat(AnimSpeed, walkSpeed * 1.5f);

                // Dolly the camera BACK as he emerges (start → end distance) and pan its look
                // point to follow him, so his growing body stays framed and he walks toward us.
                float dollyDist = Mathf.Lerp(_introCamStartDistance, _introCamEndDistance, t);
                Vector3 lookXZ = new Vector3(pos.x, 0f, pos.z);
                PositionIntroCamera(dollyDist, lookXZ);

                yield return null;
            }

            // Snap to the exact arena target at ground level, full size; stop the walk animation.
            transform.position   = walkOutTarget;
            transform.localScale = fullScale;
            _animator?.SetFloat(AnimSpeed, 0f);

            // Re-enable the agent and Warp it onto the NavMesh at the arena target so
            // Approach() can path immediately once _introComplete is set.
            if (_agent != null)
            {
                _agent.enabled   = true;
                _agent.Warp(transform.position);
                _agent.isStopped = true;
            }

            // Face the player after stopping.
            if (_player != null)
            {
                Vector3 toPlayer = _player.position - transform.position;
                toPlayer.y = 0f;
                if (toPlayer.sqrMagnitude > 0.01f)
                    transform.rotation = Quaternion.LookRotation(toPlayer.normalized);
            }

            // Settle the camera at its final pulled-back distance, aimed at the boss, for the
            // pause and spin-up.
            PositionIntroCamera(_introCamEndDistance, new Vector3(transform.position.x, 0f, transform.position.z));

            // ── Phase 2.5: Post-walk pause ──
            // Boss stands at full size, fully out in the arena, before the head spins up.
            float pauseTimer = 0f;
            while (pauseTimer < _introPostWalkPause) { pauseTimer += Time.deltaTime; yield return null; }

            // ── Phase 3: Head spins up — intro cam zooms in for the reveal ──
            drumWindow?.StartIntroBuildUp();

            float startFoV     = _introVcam != null ? _introVcam.Lens.FieldOfView : _normalCameraFoV;
            float spinTimer    = 0f;
            float spinDuration = 3.0f;
            while (spinTimer < spinDuration)
            {
                spinTimer += Time.deltaTime;
                float t = spinTimer / spinDuration;
                float degPerSec = Mathf.Lerp(30f, 240f, t);
                _drumHead?.Rotate(0f, degPerSec * Time.deltaTime, 0f, Space.Self);
                // Zoom the INTRO cam in during the first 60% of spin-up.
                if (_introVcam != null)
                {
                    float zoomT = Mathf.Clamp01(t / 0.6f);
                    var lens = _introVcam.Lens;
                    lens.FieldOfView = Mathf.Lerp(startFoV, _introCameraFoV, zoomT * zoomT);
                    _introVcam.Lens = lens;
                }
                if (spinTimer >= spinDuration * 0.5f && spinTimer < spinDuration * 0.5f + Time.deltaTime)
                    _impulseSource?.GenerateImpulse(0.2f);
                yield return null;
            }

            // ── Phase 4: Hand control back to the gameplay camera, combat begins ──
            drumWindow?.SetSlowPhase(); // settle to slow idle spin during combat

            // Disabling the intro vcam lets the untouched gameplay cam (pfb_CM_FollowCam) win
            // the Brain again; Cinemachine blends from the intro shot to the fixed low-angle
            // follow camera (36° pitch, no rotation — ADR-0001).
            if (_introVcam != null) _introVcam.enabled = false;

            _introComplete = true;
            _state         = BossState.Approaching;

            // Drop invulnerability only after the intro is fully torn down (camera handed back,
            // agent warped, state set) to close a death-race on camera/agent state.
            _stats?.SetInvulnerable(false);
        }

        // ── Movement ──────────────────────────────────────────────────────────

        // ClothesToss (Phase 1, slot 3) and SudsBurst (Phase 2, slot 1) fire at range.
        private bool NextAttackIsRanged()
        {
            if (_phase == Phase.One) return (_attackIndex % 4) == 3;
            return (_attackIndex % 4) == 1;
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
                float cooldown = _phase == Phase.One
                    ? attackCooldown
                    : attackCooldown * phase2CooldownMult;
                _attackCooldownTimer = cooldown;
                if (_agent != null) { _agent.isStopped = true; }
                StopActive();
                StartActive(AttackRoutine());
                return;
            }

            if (_agent != null && _agent.isOnNavMesh)
            {
                _agent.speed     = _phase == Phase.One ? walkSpeed : runSpeed;
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
                // Fallback: direct movement when no NavMeshAgent OR agent not yet on NavMesh.
                // Attempt a Warp each frame until the agent snaps to the mesh.
                if (_agent != null && !_agent.isOnNavMesh)
                    _agent.Warp(transform.position);

                Vector3 dir = (_player.position - transform.position).normalized;
                dir.y = 0f;
                float speed = _phase == Phase.One ? walkSpeed : runSpeed;
                transform.position += dir * speed * Time.deltaTime;
                if (dir != Vector3.zero)
                    transform.rotation = Quaternion.LookRotation(dir);
            }
        }

        // ── Attack dispatch ────────────────────────────────────────────────────

        private IEnumerator AttackRoutine()
        {
            if (_phase == Phase.One)
                yield return StartCoroutine(Phase1Attack());
            else
                yield return StartCoroutine(Phase2Attack());
        }

        // Phase 1 pool: DrumSlam → Haymaker → SpinCharge → ClothesToss (repeating)
        private IEnumerator Phase1Attack()
        {
            switch (_attackIndex % 4)
            {
                case 0: yield return StartCoroutine(DrumSlam());       break;
                case 1: yield return StartCoroutine(Haymaker());       break;
                case 2: yield return StartCoroutine(SpinCharge());     break;
                case 3: yield return StartCoroutine(ClothesToss());    break;
            }
            _attackIndex++;
            drumWindow?.EndPendulum();

            // Check for phase transition after each attack completes.
            if (!_phaseTransitioned && _stats.CurrentHealth <= _stats.MaxHealth * 0.5f)
                yield return StartCoroutine(PhaseTransitionRoutine());

            if (_state != BossState.Dead && _state != BossState.Staggered)
                _state = BossState.Approaching;
        }

        // Phase 2 pool: FullSpin → SudsBurst → DoubleHaymaker → JumpCharge (repeating)
        private IEnumerator Phase2Attack()
        {
            switch (_attackIndex % 4)
            {
                case 0: yield return StartCoroutine(FullSpin());          break;
                case 1: yield return StartCoroutine(SudsBurst());         break;
                case 2: yield return StartCoroutine(DoubleHaymaker());    break;
                case 3: yield return StartCoroutine(JumpCharge());        break;
            }
            _attackIndex++;
            drumWindow?.EndPendulum();

            if (_state != BossState.Dead)
                _state = BossState.Approaching;
        }

        // ── Individual attacks ────────────────────────────────────────────────

        private IEnumerator DrumSlam()
        {
            yield return StartCoroutine(WindUp(Color.red, AttackTelegraphKind.MeleeUnparryable));
            _state = BossState.Attacking;
            SetColor(_baseColor);
            _animator?.SetTrigger(AnimAttack);

            if (IsPlayerWithinRange(meleeRange + 0.5f) && _playerCombat != null)
            {
                // DrumSlam is never parryable — the drum face blocks the player's counter.
                AttackResult result = _playerCombat.TryReceiveAttack(_stats.AttackDamage, parryable: false);
                if (result == AttackResult.Hit)
                {
                    AudioManager.Instance?.Play(SoundEvent.EnemyHit);
                    _impulseSource?.GenerateImpulse(Vector3.down * 2f);
                }
            }

            yield return _waitAttackActive;
        }

        private IEnumerator Haymaker()
        {
            yield return StartCoroutine(WindUp(Color.yellow, AttackTelegraphKind.MeleeUnparryable));
            _state = BossState.Attacking;
            SetColor(_baseColor);
            _animator?.SetTrigger(AnimAttack);

            if (IsPlayerWithinRange(meleeRange + 0.5f) && _playerCombat != null)
            {
                // Haymaker is only parryable when the drum window is facing the player.
                bool canParry = drumWindow != null && drumWindow.IsParryWindowOpen;
                AttackResult result = _playerCombat.TryReceiveAttack(_stats.AttackDamage, parryable: canParry, attacker: gameObject);

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

        private IEnumerator SpinCharge()
        {
            yield return StartCoroutine(WindUp(new Color(1f, 0.5f, 0f), AttackTelegraphKind.MeleeUnparryable)); // orange
            _state = BossState.Attacking;
            SetColor(_baseColor);
            _animator?.SetTrigger(AnimAttack);

            // SpinCharge uses a frame-by-frame loop so contact detection runs every frame.
            // A WaitForSeconds here would skip frames and miss short contact windows.
            // Capture the player's position at charge start so the rush has a fixed target.
            Vector3 chargeTarget = _player != null ? _player.position : transform.position;
            Vector3 chargeDir    = (chargeTarget - transform.position).normalized;
            chargeDir.y = 0f;
            if (chargeDir != Vector3.zero)
                transform.rotation = Quaternion.LookRotation(chargeDir);

            // Yield one frame before contact detection so the boss visibly moves before hitting.
            yield return null;

            float elapsed = 0f;
            bool hitLanded = false;

            if (_agent != null) _agent.enabled = false;

            while (elapsed < spinChargeDuration)
            {
                transform.position += chargeDir * spinChargeSpeed * Time.deltaTime;
                elapsed += Time.deltaTime;

                if (!hitLanded && IsPlayerWithinRange(1.2f) && _playerCombat != null)
                {
                    // SpinCharge is a moving tackle — not parryable.
                    AttackResult result = _playerCombat.TryReceiveAttack(_stats.AttackDamage, parryable: false);
                    if (result == AttackResult.Hit)
                    {
                        AudioManager.Instance?.Play(SoundEvent.EnemyHit);
                        _impulseSource?.GenerateImpulse(chargeDir * 1.5f);
                    }
                    hitLanded = true;
                    break; // End charge early on contact.
                }

                yield return null;
            }

            if (_agent != null) { _agent.enabled = true; _agent.Warp(transform.position); }
        }

        private IEnumerator ClothesToss()
        {
            yield return StartCoroutine(JumpBack(AttackTelegraphKind.ProjectileUnparryable));
            _state = BossState.Attacking;
            SetColor(_baseColor);
            _animator?.SetTrigger(AnimAttack);

            if (_heldClothesBall != null) _heldClothesBall.SetActive(false);

            if (_clothesTossPrefab != null && _player != null)
            {
                // Use held prop world position as spawn origin if available, else default offset.
                Vector3 spawnPos = _heldClothesBall != null
                    ? _heldClothesBall.transform.position
                    : transform.position + Vector3.up * 1.5f;
                GameObject ball = Instantiate(_clothesTossPrefab, spawnPos, Quaternion.identity);
                if (ball.TryGetComponent<BossProjectile>(out var proj))
                    proj.Initialize(_playerCombat);
                if (ball.TryGetComponent<Rigidbody>(out var rb))
                {
                    // Solve velocity to land at player's current position in flightTime seconds.
                    Vector3 toTarget = _player.position - spawnPos;
                    float horizDist  = new Vector3(toTarget.x, 0f, toTarget.z).magnitude;
                    float T          = Mathf.Max(0.5f, horizDist / _clothesTossSpeed);
                    Vector3 vel      = toTarget / T;
                    // Gravity compensation: add what gravity removes over the flight.
                    vel.y -= 0.5f * Physics.gravity.y * T;
                    rb.linearVelocity = vel;
                }
            }
            else if (IsPlayerWithinRange(meleeRange + 1f) && _playerCombat != null)
            {
                // Fallback when no prefab is assigned — melee hit as placeholder.
                AttackResult result = _playerCombat.TryReceiveAttack(_stats.AttackDamage, parryable: false);
                if (result == AttackResult.Hit)
                    AudioManager.Instance?.Play(SoundEvent.EnemyHit);
            }

            yield return _waitAttackActive;
            if (_heldClothesBall != null) _heldClothesBall.SetActive(true);
        }

        private IEnumerator FullSpin()
        {
            yield return StartCoroutine(WindUp(Color.magenta, AttackTelegraphKind.AreaUnparryable));
            _state = BossState.Attacking;
            SetColor(_baseColor);
            _animator?.SetTrigger(AnimAttack);

            int count = Physics.OverlapSphereNonAlloc(transform.position, fullSpinRadius, _overlapBuffer);
            for (int i = 0; i < count; i++)
            {
                if (!_overlapBuffer[i].CompareTag("Player")) continue;
                if (_playerCombat == null) break;

                // AoE full rotation — no safe angle, not parryable.
                AttackResult result = _playerCombat.TryReceiveAttack(_stats.AttackDamage, parryable: false);
                if (result == AttackResult.Hit)
                {
                    AudioManager.Instance?.Play(SoundEvent.EnemyHit);
                    _impulseSource?.GenerateImpulse();
                }
                break;
            }

            yield return _waitAttackActive;
        }

        private IEnumerator SudsBurst()
        {
            yield return StartCoroutine(JumpBack(AttackTelegraphKind.ProjectileUnparryable));
            _state = BossState.Attacking;
            SetColor(_baseColor);
            _animator?.SetTrigger(AnimAttack);

            if (_heldSudsBlob != null) _heldSudsBlob.SetActive(false);

            if (_sudsBlobPrefab != null && _player != null)
            {
                // Use held prop world position as spawn origin if available, else default offset.
                Vector3 spawnPos = _heldSudsBlob != null
                    ? _heldSudsBlob.transform.position
                    : transform.position + Vector3.up * 0.5f;
                Vector3 forward  = (_player.position - transform.position);
                forward.y = 0f;
                if (forward == Vector3.zero) forward = transform.forward;
                forward.Normalize();

                for (int i = 0; i < _sudsBurstAngles.Length; i++)
                {
                    Vector3 dir    = Quaternion.Euler(0f, _sudsBurstAngles[i], 0f) * forward;
                    GameObject blob = Instantiate(_sudsBlobPrefab, spawnPos, Quaternion.identity);
                    if (blob.TryGetComponent<BossProjectile>(out var proj))
                        proj.Initialize(_playerCombat);
                    if (blob.TryGetComponent<Rigidbody>(out var rb))
                        rb.linearVelocity = dir * _sudsBlobSpeed + Vector3.up * _sudsBlobArcVelocity;
                }
            }

            yield return _waitAttackActive;
            if (_heldSudsBlob != null) _heldSudsBlob.SetActive(true);
        }

        private IEnumerator DoubleHaymaker()
        {
            // First swing: parryable via drum window.
            yield return StartCoroutine(WindUp(Color.yellow, AttackTelegraphKind.MeleeUnparryable));
            _state = BossState.Attacking;
            SetColor(_baseColor);
            _animator?.SetTrigger(AnimAttack);

            if (IsPlayerWithinRange(meleeRange + 0.5f) && _playerCombat != null)
            {
                bool canParry  = drumWindow != null && drumWindow.IsParryWindowOpen;
                AttackResult r = _playerCombat.TryReceiveAttack(_stats.AttackDamage, parryable: canParry, attacker: gameObject);

                if (r == AttackResult.Hit)
                {
                    AudioManager.Instance?.Play(SoundEvent.EnemyHit);
                    _impulseSource?.GenerateImpulse();
                }

                if (r == AttackResult.Parried)
                {
                    // Parrying the first arm aborts the combo.
                    yield return StartCoroutine(StaggerRoutine());
                    yield break;
                }
            }

            yield return _waitAttackActive;

            if (_state == BossState.Dead) yield break;

            // Second swing: always un-parryable — SpinCycle commits with the off-hand.
            yield return StartCoroutine(WindUp(Color.yellow, AttackTelegraphKind.MeleeUnparryable));
            _state = BossState.Attacking;
            SetColor(_baseColor);
            _animator?.SetTrigger(AnimAttack);

            if (IsPlayerWithinRange(meleeRange + 0.5f) && _playerCombat != null)
            {
                AttackResult r = _playerCombat.TryReceiveAttack(_stats.AttackDamage, parryable: false);
                if (r == AttackResult.Hit)
                {
                    AudioManager.Instance?.Play(SoundEvent.EnemyHit);
                    _impulseSource?.GenerateImpulse();
                }
            }

            yield return _waitAttackActive;
        }

        private IEnumerator JumpCharge()
        {
            yield return StartCoroutine(WindUp(new Color(0.5f, 0f, 1f), AttackTelegraphKind.AreaUnparryable)); // purple
            _state = BossState.Attacking;
            SetColor(_baseColor);
            _animator?.SetTrigger(AnimAttack);

            // Capture landing target at jump start.
            Vector3 startPos  = transform.position;
            Vector3 targetPos = _player != null ? _player.position : transform.position;
            targetPos.y = startPos.y; // stay on ground plane until landing

            if (_agent != null) _agent.enabled = false;

            float elapsed = 0f;
            while (elapsed < jumpChargeDuration)
            {
                float t = elapsed / jumpChargeDuration;

                // Linear XZ interpolation with a Sin arc for height.
                Vector3 flatPos = Vector3.Lerp(startPos, targetPos, t);
                flatPos.y = startPos.y + jumpChargeHeight * Mathf.Sin(t * Mathf.PI);
                transform.position = flatPos;

                elapsed += Time.deltaTime;
                yield return null;
            }

            // Snap to ground on landing.
            transform.position = targetPos;
            if (_agent != null) { _agent.enabled = true; _agent.Warp(transform.position); }
            _impulseSource?.GenerateImpulse(Vector3.down * 3f);

            // Landing hit — not parryable, boss falls from above.
            if (IsPlayerWithinRange(meleeRange + 1f) && _playerCombat != null)
            {
                AttackResult result = _playerCombat.TryReceiveAttack(_stats.AttackDamage, parryable: false);
                if (result == AttackResult.Hit)
                    AudioManager.Instance?.Play(SoundEvent.EnemyHit);
            }
        }

        // ── Shared helpers ────────────────────────────────────────────────────

        // ADR-0003: raises the occlusion-independent overhead telegraph from the same seam that
        // already centralises every melee tell's body-tint colour. Additive — the tint is
        // unchanged. `kind` selects the telegraph's shape/audio class; see AttackTelegraphKind.
        private IEnumerator WindUp(Color color, AttackTelegraphKind kind)
        {
            _state = BossState.WindUp;
            SetColor(color);
            AttackTelegraphService.Show(transform, kind, windUpDuration);
            drumWindow?.BeginPendulum();
            yield return _waitWindUp;
        }

        private IEnumerator JumpBack(AttackTelegraphKind kind)
        {
            _state = BossState.WindUp;

            // Fire a brief color flash to telegraph the jump.
            SetColor(Color.cyan);
            AttackTelegraphService.Show(transform, kind, jumpBackDuration);
            drumWindow?.BeginPendulum();

            Vector3 startPos = transform.position;

            // Jump directly away from the player.
            Vector3 awayDir = _player != null
                ? (transform.position - _player.position).normalized
                : -transform.forward;
            awayDir.y = 0f;
            if (awayDir == Vector3.zero) awayDir = -transform.forward;

            Vector3 landPos = startPos + awayDir * jumpBackDistance;
            landPos.y = startPos.y;

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

            // Snap to landing position and face the player before firing.
            transform.position = landPos;
            if (_agent != null) { _agent.enabled = true; _agent.Warp(transform.position); }
            if (_player != null)
            {
                Vector3 toPlayer = (_player.position - transform.position).normalized;
                toPlayer.y = 0f;
                if (toPlayer != Vector3.zero)
                    transform.rotation = Quaternion.LookRotation(toPlayer);
            }

            SetColor(_baseColor);
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
            _animator?.SetTrigger(AnimStagger);
            SetColor(_baseColor);

            _impulseSource?.GenerateImpulse();

            yield return _waitPhaseTransition;

            drumWindow?.SetFastPhase();

            _phase       = Phase.Two;
            _attackIndex = 0;
            _state       = BossState.Approaching;
        }

        // ── Stagger ───────────────────────────────────────────────────────────

        private IEnumerator StaggerRoutine()
        {
            _state = BossState.Staggered;
            drumWindow?.EndPendulum();
            _animator?.SetTrigger(AnimStagger);
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
            // Counter strike only deals damage during stagger, matching BasicEnemyAI convention.
            if (_state != BossState.Staggered) return;
            _stats.TakeDamage(counterStrikeDamage);
        }

        private void HandleDeath()
        {
            _state = BossState.Dead;

            if (_agent != null) { _agent.isStopped = true; _agent.enabled = false; }

            // StopAllCoroutines kills nested attack routines (DrumSlam, Haymaker, etc.)
            // that are started via yield return StartCoroutine() — StopCoroutine on the
            // outer routine alone leaves those inner routines running.
            StopAllCoroutines();
            _activeRoutine = null;

            if (_animator != null)
            {
                _animator.speed = 1f;           // restore speed in case wind-up changed it
                _animator.SetFloat(AnimSpeed, 0f);
                _animator.SetBool(AnimIsDead, true);
            }

            SetColor(Color.gray);
            if (_heldClothesBall != null) _heldClothesBall.SetActive(false);
            if (_heldSudsBlob    != null) _heldSudsBlob.SetActive(false);

            // Destroy any in-flight projectiles so they cannot kill the player
            // during the defeat sequence.
            foreach (var proj in FindObjectsByType<BossProjectile>(FindObjectsInactive.Exclude, FindObjectsSortMode.None))
                Destroy(proj.gameObject);

            _defeatRoutine = StartCoroutine(DefeatSequence());
        }

        // Runs entirely on unscaled/real time (see WobbleRoutine/ShrinkAndFade/LerpImagination
        // below, plus _waitDefeatStumble being a WaitForSecondsRealtime) rather than the scaled
        // Time.deltaTime the rest of this AI otherwise uses. Root cause (docs/BACKLOG.md, World1
        // WildWestCity boss-win bug): once the boss is confirmed dead, this sequence's only job
        // is to play a short animation and then unconditionally call GameManager.TriggerWin() —
        // but every step previously waited on SCALED time, so if anything else in the scene left
        // Time.timeScale at 0 at the moment of the killing blow (confirmed reproducible: a UI
        // panel that pauses the game via Time.timeScale = 0 but whose own "unpause" path never
        // ran), the whole sequence silently stalled forever and TriggerWin() was never reached —
        // the boss died with no error, but the run never ended. Converting these waits to
        // unscaled time guarantees the win condition always fires a few real seconds after death,
        // regardless of what any other system has done to Time.timeScale. Matches the same
        // pattern this codebase already uses for the identical class of problem elsewhere
        // (ForgePanel.WaitForCutsceneThenClose, GameManager._waitRoomClearShow/_cutsceneStartDelay).
        private IEnumerator DefeatSequence()
        {
            drumWindow?.BeginStopDrum();

            // ── Step 1: Stumble pause — let the player see him "fail" before he disappears ──
            // The drum spins down (BeginStopDrum above), SpinCycle freezes mid-motion.
            SetColor(Color.gray);
            yield return _waitDefeatStumble;

            // ── Step 2: Wobble — brief shake so the defeat feels physical, not clean ──
            yield return StartCoroutine(WobbleRoutine(_defeatWobbleDuration));

            // ── Step 3: Burst VFX at the moment he starts to vanish ──
            if (_deathBurstVFX != null)
            {
                ParticleSystem burst = Instantiate(_deathBurstVFX, transform.position, Quaternion.identity);
                burst.Play();
                // Auto-destroy the VFX after its duration so it does not leak.
                Destroy(burst.gameObject, burst.main.duration + burst.main.startLifetime.constantMax + 0.5f);
            }

            // ── Step 4: Shrink to zero AND fade alpha simultaneously over defeatHoldDuration ──
            // MaterialPropertyBlock drives the alpha without touching shader keywords — the
            // material asset stays opaque, so no URP surface-type switch can reset _BaseColor.
            yield return StartCoroutine(ShrinkAndFade(defeatHoldDuration));

            // ── Step 5: ImaginationRestore effect, then TriggerWin ──
            // If _imaginationVolume wasn't assigned in the Inspector, find it by name at runtime.
            if (_imaginationVolume == null)
                _imaginationVolume = GameObject.Find("ImaginationRestore_Volume")
                    ?.GetComponent<UnityEngine.Rendering.Volume>();

            // Lerp the imagination-restore volume in, then TriggerWin (called inside LerpImagination).
            // If the volume is still null, fall back to TriggerWin directly.
            if (_imaginationVolume != null)
                yield return StartCoroutine(LerpImagination(_imaginationLerpDuration));
            else
                GameManager.Instance?.TriggerWin();

            Destroy(gameObject);
        }

        /// <summary>
        /// Oscillates SpinCycle's Y rotation back and forth — simulates a stumble/wobble
        /// without DOTween. Rotation resets to the pre-wobble value when done.
        /// Unscaled time — see DefeatSequence's class-level comment on why every step of the
        /// defeat animation must be immune to Time.timeScale.
        /// </summary>
        private IEnumerator WobbleRoutine(float duration)
        {
            float elapsed     = 0f;
            float startAngleY = transform.eulerAngles.y;
            const float wobbleAmplitude = 15f;
            const float wobbleFrequency = 12f;

            while (elapsed < duration)
            {
                elapsed += Time.unscaledDeltaTime;
                float angle = Mathf.Sin(elapsed * wobbleFrequency) * wobbleAmplitude
                              * (1f - elapsed / duration); // dampen toward zero
                transform.rotation = Quaternion.Euler(0f, startAngleY + angle, 0f);
                yield return null;
            }

            transform.rotation = Quaternion.Euler(0f, startAngleY, 0f);
        }

        /// <summary>
        /// Shrinks scale to zero over <paramref name="duration"/> seconds using a smooth-step
        /// curve. Alpha fade via MaterialPropertyBlock was removed because the SpinCycle material
        /// uses URP Surface Type = Opaque, which ignores _BaseColor.a entirely — the geometry
        /// stayed fully opaque regardless of the alpha value written. The shrink alone (wobble
        /// + particle burst + scale-to-zero) provides sufficient visual payoff.
        /// Unscaled time — see DefeatSequence's class-level comment on why every step of the
        /// defeat animation must be immune to Time.timeScale.
        /// </summary>
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

            // Guarantee exact zero — floating-point lerp may not land exactly.
            transform.localScale = Vector3.zero;

            // Do NOT call Destroy here — DefeatSequence must invoke TriggerWin first.
        }

        // Unscaled time — see DefeatSequence's class-level comment on why every step of the
        // defeat animation must be immune to Time.timeScale (this is the step that ultimately
        // calls GameManager.TriggerWin(), so it matters most here).
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
            // Safety net: if the intro was interrupted before Phase 4 disabled the intro vcam,
            // tear the whole cinematic camera GameObject down here so it cannot leak or keep
            // out-prioritizing the gameplay camera after the boss is gone.
            if (_introVcamGO != null)
            {
                Destroy(_introVcamGO);
                _introVcamGO = null;
                _introVcam   = null;
            }

            // HandleDeath calls StopAllCoroutines; if destroyed without dying, clean up here.
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
