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

        internal AttackTelegraphHandle(int poolIndex, int generation)
        {
            PoolIndex  = poolIndex;
            Generation = generation;
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

        // Redundant reinforcement only — shape is the load-bearing parryable/un-parryable signal
        // (see AttackTelegraphIndicator's class remarks). Baked into the two shared materials
        // below rather than set per-activation, since indicators no longer own their own material.
        private static readonly Color ParryableColor   = new Color(0.25f, 0.85f, 1f, 0.95f);
        private static readonly Color UnparryableColor = new Color(1f, 0.25f, 0.2f, 0.95f);
        private static readonly int   BaseColorId      = Shader.PropertyToID("_BaseColor");

        private AttackTelegraphIndicator[] _pool;
        private int[] _generation;
        private int _nextIndex;

        private Material _parryableMaterial;
        private Material _unparryableMaterial;

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
        }

        private void OnDestroy()
        {
            if (_parryableMaterial   != null) Destroy(_parryableMaterial);
            if (_unparryableMaterial != null) Destroy(_unparryableMaterial);
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

        /// <summary>Cancels an active indicator early. Safe to call with a stale/expired handle.</summary>
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
            return new AttackTelegraphHandle(index, _generation[index]);
        }

        private void HideInternal(AttackTelegraphHandle handle)
        {
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
