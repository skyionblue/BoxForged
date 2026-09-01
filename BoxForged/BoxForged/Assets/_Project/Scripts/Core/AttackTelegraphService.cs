using UnityEngine;

namespace Boxhead.Core
{
    /// <summary>
    /// ADR-0003: broad classification of an incoming attack. Drives both the telegraph shape
    /// (parryable silhouette vs un-parryable silhouette — the ADR requires shape, not hue, to
    /// carry this bit) and the audio cue class. Deliberately not one enum member per named
    /// attack (DrumSlam, Haymaker, ClawSwipe, ...) across every enemy — the ADR asks for "a
    /// distinct audio cue per attack class," not one per individual move.
    /// </summary>
    public enum AttackTelegraphKind
    {
        MeleeParryable,
        MeleeUnparryable,
        AreaUnparryable,
        ProjectileParryable,
        ProjectileUnparryable,
    }

    /// <summary>Opaque handle to an active telegraph indicator, for early cancellation via Hide().</summary>
    public readonly struct AttackTelegraphHandle
    {
        internal readonly int PoolIndex;
        internal readonly int Generation;
        // ADR-0007: which pool this handle belongs to (0 = billboard, 1 = ground-plane lane) so
        // Hide() routes to the right array. Defaults to 0 (billboard) for source compatibility
        // with every existing Show() call site, none of which pass this parameter.
        internal readonly byte PoolId;

        internal AttackTelegraphHandle(int poolIndex, int generation, byte poolId = 0)
        {
            PoolIndex  = poolIndex;
            Generation = generation;
            PoolId     = poolId;
        }

        public static readonly AttackTelegraphHandle None = new AttackTelegraphHandle(-1, 0);
        public bool IsValid => PoolIndex >= 0;
    }

    /// <summary>
    /// ADR-0003: pooled, occlusion-independent overhead telegraph for enemy attack wind-ups.
    ///
    /// Persistent singleton, same lifecycle pattern as AudioManager — pre-warms a fixed pool of
    /// indicators at Awake and never instantiates one per wind-up. Concurrent indicators are
    /// capped at _poolSize; once exhausted, the oldest active indicator is recycled early rather
    /// than growing the pool or silently dropping the newest request — a full room should
    /// degrade gracefully, not hide the newest (most time-critical) tell.
    ///
    /// Raise this from the same seam every enemy AI already uses to centralise its wind-up tell
    /// (the WindUp(Color) coroutine on the two bosses, or the equivalent EnterWindUp-style state
    /// entry point elsewhere) — see ADR-0003 decision 5. This is additive: the existing
    /// whole-body material tint stays exactly as-is.
    /// </summary>
    public sealed class AttackTelegraphService : MonoBehaviour
    {
        public static AttackTelegraphService Instance { get; private set; }

        [Tooltip("Max simultaneous telegraph indicators. ADR-0003 validation calls for testing at maxConcurrentEnemies with every enemy winding up at once — keep this comfortably above the highest per-room cap (see RoomManager.maxConcurrentEnemies). Must be positive — a zero/negative value would divide-by-zero in FindSlot's modulo and is corrected to 8 with a logged error if misconfigured.")]
        [SerializeField] private int _poolSize = 8;

        [Tooltip("Height above each target's root transform the indicator floats at. A single project-wide default — not tuned per enemy silhouette height yet.")]
        [SerializeField] private float _defaultHeightOffset = 3.2f;

        [Tooltip("Source material using the BoxForged/TelegraphOverlayUnlit shader (mat_TelegraphOverlay.mat). Referencing the shader through this real asset is what makes Unity include it in a build — see B32: a runtime Shader.Find(\"BoxForged/TelegraphOverlayUnlit\") string lookup does not count as a reference, so the shader was being stripped from real builds despite working fine in the Editor. Two tinted instances (parryable / un-parryable) are derived from this once in Awake and shared by every pooled indicator.")]
        [SerializeField] private Material _overlaySourceMaterial;

        [Tooltip("ADR-0007: source material for the ground-plane lane geometry, using the same shader at ZTest LEqual (mat_TelegraphLane.mat) instead of the billboard's ZTest Always. One shared, UnparryableColor-tinted instance is derived from this once in Awake for every pooled lane — reference the asset directly here; never runtime Shader.Find (B32).")]
        [SerializeField] private Material _laneSourceMaterial;

        // Redundant reinforcement only — shape is the load-bearing parryable/un-parryable signal
        // (see AttackTelegraphIndicator's class remarks). Baked into the two shared materials
        // below rather than set per-activation, since indicators no longer own their own material.
        private static readonly Color ParryableColor   = new Color(0.25f, 0.85f, 1f, 0.95f);
        private static readonly Color UnparryableColor = new Color(1f, 0.25f, 0.2f, 0.95f);
        private static readonly int   BaseColorId      = Shader.PropertyToID("_BaseColor");

        // ADR-0007: which AttackTelegraphHandle.PoolId maps to which pool.
        private const byte BillboardPoolId = 0;
        private const byte LanePoolId      = 1;

        // ADR-0007 §3: a fixed, small, separate pool — not the billboard pool's FindSlot cursor,
        // which evicts at its round-robin position when full. A boss lane is raised first and
        // held for ~1.4 s while ordinary wind-ups come and go; sharing the billboard pool would
        // make a fairness-critical indicator recyclable by a grunt's wind-up.
        private const int LanePoolSize = 2;

        private AttackTelegraphIndicator[] _pool;
        private int[] _generation;
        private int _nextIndex;

        private AttackTelegraphLane[] _lanePool;
        private int[] _laneGeneration;
        private int _laneNextIndex;

        private Material _parryableMaterial;
        private Material _unparryableMaterial;
        private Material _laneMaterial;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);

            if (_poolSize <= 0)
            {
                Debug.LogError($"[AttackTelegraphService] _poolSize must be positive (was {_poolSize}) — falling back to 8 to avoid a divide-by-zero in slot selection.", this);
                _poolSize = 8;
            }

            CreateSharedMaterials();

            _pool       = new AttackTelegraphIndicator[_poolSize];
            _generation = new int[_poolSize];

            for (int i = 0; i < _poolSize; i++)
            {
                var go = new GameObject($"AttackTelegraph_{i}");
                go.transform.SetParent(transform, false);
                _pool[i] = go.AddComponent<AttackTelegraphIndicator>();
                _pool[i].Initialize(_parryableMaterial, _unparryableMaterial);
                _pool[i].gameObject.SetActive(false);
            }

            // ADR-0007: a separate, fixed-size pool for ground-plane lanes — see LanePoolSize's
            // remarks for why this must not share _pool/FindSlot.
            _lanePool       = new AttackTelegraphLane[LanePoolSize];
            _laneGeneration = new int[LanePoolSize];

            for (int i = 0; i < LanePoolSize; i++)
            {
                var go = new GameObject($"AttackTelegraphLane_{i}");
                go.transform.SetParent(transform, false);
                _lanePool[i] = go.AddComponent<AttackTelegraphLane>();
                _lanePool[i].Initialize(_laneMaterial);
                _lanePool[i].gameObject.SetActive(false);
            }
        }

        // Derives the 2 shared, pre-tinted material instances every pooled indicator switches
        // between (see B32) — collapses what used to be 8 per-instance runtime materials (one
        // "new Material(...)" per pooled indicator in AttackTelegraphIndicator.Awake) down to 2,
        // owned and destroyed here rather than by each indicator.
        private void CreateSharedMaterials()
        {
            if (_overlaySourceMaterial == null)
            {
                Debug.LogError("[AttackTelegraphService] _overlaySourceMaterial is not assigned — telegraph indicators will render with no material. Assign Assets/_Project/Materials/mat_TelegraphOverlay.mat in the Inspector.", this);
                return;
            }

            _parryableMaterial = new Material(_overlaySourceMaterial) { name = "TelegraphOverlay_Parryable" };
            _parryableMaterial.SetColor(BaseColorId, ParryableColor);

            _unparryableMaterial = new Material(_overlaySourceMaterial) { name = "TelegraphOverlay_Unparryable" };
            _unparryableMaterial.SetColor(BaseColorId, UnparryableColor);

            if (_laneSourceMaterial == null)
            {
                Debug.LogError("[AttackTelegraphService] _laneSourceMaterial is not assigned — ground-plane lane telegraphs will render with no material. Assign Assets/_Project/Materials/mat_TelegraphLane.mat in the Inspector.", this);
            }
            else
            {
                // Lanes are only ever un-parryable geometry (ADR-0007 §1 — kind carries
                // parryability/audio only, and the current single caller passes
                // AreaUnparryable), so one tint suffices, unlike the billboard's two.
                _laneMaterial = new Material(_laneSourceMaterial) { name = "TelegraphLane_Unparryable" };
                _laneMaterial.SetColor(BaseColorId, UnparryableColor);
            }
        }

        private void OnDestroy()
        {
            if (_parryableMaterial   != null) Destroy(_parryableMaterial);
            if (_unparryableMaterial != null) Destroy(_unparryableMaterial);
            if (_laneMaterial        != null) Destroy(_laneMaterial);
            if (Instance == this) Instance = null;
        }

        /// <summary>
        /// Activates a pooled indicator above <paramref name="target"/> for
        /// <paramref name="duration"/> seconds and plays the matching audio cue once. Returns a
        /// handle usable with Hide() to cancel early (e.g. the wind-up gets interrupted by a
        /// counter-stagger before its natural duration elapses).
        /// </summary>
        public static AttackTelegraphHandle Show(
            Transform target, AttackTelegraphKind kind, float duration, float heightOffset = -1f)
        {
            EnsureInstance();
            if (Instance == null || target == null) return AttackTelegraphHandle.None;
            return Instance.ShowInternal(target, kind, duration, heightOffset);
        }

        // Self-bootstrapping rather than requiring a placed pfb_AttackTelegraphService in every
        // scene (the way pfb_AudioManager is placed today). The existing Cul-de-Sac scenes are
        // being superseded (ADR-0002) and are explicitly out of scope to edit for this ADR, so
        // this avoids needing to touch them while still working correctly if a scene never
        // places the prefab. An explicit prefab instance can still be added later (e.g. to tune
        // _poolSize/_defaultHeightOffset per scene in the Inspector) — if present, its Awake()
        // runs first and this becomes a no-op.
        private static void EnsureInstance()
        {
            if (Instance != null) return;
            var go = new GameObject("AttackTelegraphService (auto-created)");
            go.AddComponent<AttackTelegraphService>();
        }

        /// <summary>
        /// ADR-0007: raises a pooled, world-space ground-plane lane from <paramref name="start"/>,
        /// extending <paramref name="length"/> meters along <paramref name="direction"/>,
        /// <paramref name="width"/> meters wide, for <paramref name="duration"/> seconds. Unlike
        /// <see cref="Show"/>, this does not take or track a Transform — the lane is committed
        /// geometry, anchored once at cast time. <paramref name="direction"/> is normalized on
        /// the XZ plane by this method; a degenerate (near-zero, or purely vertical) direction
        /// returns <see cref="AttackTelegraphHandle.None"/> and logs once. <paramref name="kind"/>
        /// continues to carry parryability and the audio-cue class only — it does not select
        /// geometry (see AttackTelegraphKind's remarks); the current convention is to call this
        /// with <see cref="AttackTelegraphKind.AreaUnparryable"/>.
        /// </summary>
        public static AttackTelegraphHandle ShowGroundLane(
            Vector3 start, Vector3 direction, float length, float width,
            AttackTelegraphKind kind, float duration, float groundY = 0f)
        {
            EnsureInstance();
            if (Instance == null) return AttackTelegraphHandle.None;

            Vector3 flatDir = direction;
            flatDir.y = 0f;
            if (flatDir.sqrMagnitude < 0.0001f)
            {
                Debug.LogWarning("[AttackTelegraphService] ShowGroundLane called with a degenerate direction (near-zero, or purely vertical) — ignoring.", Instance);
                return AttackTelegraphHandle.None;
            }
            flatDir.Normalize();

            return Instance.ShowGroundLaneInternal(start, flatDir, length, width, kind, duration, groundY);
        }

        /// <summary>Cancels an active indicator or lane early. Safe to call with a stale/expired handle.</summary>
        public static void Hide(AttackTelegraphHandle handle)
        {
            if (Instance == null || !handle.IsValid) return;
            Instance.HideInternal(handle);
        }

        private AttackTelegraphHandle ShowInternal(
            Transform target, AttackTelegraphKind kind, float duration, float heightOffset)
        {
            int index = FindSlot();
            _generation[index]++;

            float height = heightOffset >= 0f ? heightOffset : _defaultHeightOffset;
            // Explicit null check rather than ?. — AudioManager.Instance can be a Unity "fake
            // null" destroyed object in some teardown orderings, and ?. on a fake-null UnityEngine.Object
            // still calls into native code and can throw MissingReferenceException instead of
            // short-circuiting the way a real C# null would.
            if (AudioManager.Instance != null)
                AudioManager.Instance.Play(MapAudioCue(kind));

            _pool[index].Activate(target, kind, height, duration > 0f ? duration : 1f);
            return new AttackTelegraphHandle(index, _generation[index], BillboardPoolId);
        }

        private AttackTelegraphHandle ShowGroundLaneInternal(
            Vector3 start, Vector3 direction, float length, float width,
            AttackTelegraphKind kind, float duration, float groundY)
        {
            int index = FindLaneSlot();
            _laneGeneration[index]++;

            if (AudioManager.Instance != null)
                AudioManager.Instance.Play(MapAudioCue(kind));

            _lanePool[index].Activate(
                start, direction,
                Mathf.Max(0.01f, length), Mathf.Max(0.01f, width),
                groundY, duration > 0f ? duration : 1f);
            return new AttackTelegraphHandle(index, _laneGeneration[index], LanePoolId);
        }

        private void HideInternal(AttackTelegraphHandle handle)
        {
            if (handle.PoolId == LanePoolId)
            {
                if (handle.PoolIndex < 0 || handle.PoolIndex >= _lanePool.Length) return;
                if (_laneGeneration[handle.PoolIndex] != handle.Generation) return; // stale — slot was reused
                _lanePool[handle.PoolIndex].Deactivate();
                return;
            }

            if (handle.PoolIndex < 0 || handle.PoolIndex >= _pool.Length) return;
            if (_generation[handle.PoolIndex] != handle.Generation) return; // stale — slot was reused
            _pool[handle.PoolIndex].Deactivate();
        }

        // Prefers an idle slot (round-robin scan); falls back to recycling the oldest active
        // indicator so a full pool degrades gracefully instead of growing or rejecting requests.
        private int FindSlot()
        {
            for (int i = 0; i < _pool.Length; i++)
            {
                int idx = (_nextIndex + i) % _pool.Length;
                if (!_pool[idx].IsActive)
                {
                    _nextIndex = (idx + 1) % _pool.Length;
                    return idx;
                }
            }

            int evict = _nextIndex;
            _nextIndex = (_nextIndex + 1) % _pool.Length;
            return evict;
        }

        // ADR-0007: same round-robin-with-eviction policy as FindSlot, over the separate
        // 2-slot lane pool.
        private int FindLaneSlot()
        {
            for (int i = 0; i < _lanePool.Length; i++)
            {
                int idx = (_laneNextIndex + i) % _lanePool.Length;
                if (!_lanePool[idx].IsActive)
                {
                    _laneNextIndex = (idx + 1) % _lanePool.Length;
                    return idx;
                }
            }

            int evict = _laneNextIndex;
            _laneNextIndex = (_laneNextIndex + 1) % _lanePool.Length;
            return evict;
        }

        private static SoundEvent MapAudioCue(AttackTelegraphKind kind)
        {
            switch (kind)
            {
                case AttackTelegraphKind.MeleeParryable:        return SoundEvent.TelegraphMeleeParryable;
                case AttackTelegraphKind.MeleeUnparryable:      return SoundEvent.TelegraphMeleeUnparryable;
                case AttackTelegraphKind.AreaUnparryable:       return SoundEvent.TelegraphAreaUnparryable;
                case AttackTelegraphKind.ProjectileParryable:
                case AttackTelegraphKind.ProjectileUnparryable: return SoundEvent.TelegraphProjectile;
                default:                                        return SoundEvent.TelegraphMeleeUnparryable;
            }
        }
    }
}
