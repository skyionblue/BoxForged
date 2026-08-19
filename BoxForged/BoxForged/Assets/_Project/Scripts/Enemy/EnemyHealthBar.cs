using UnityEngine;

namespace Boxhead.Enemy
{
    [RequireComponent(typeof(EnemyStats))]
    public class EnemyHealthBar : MonoBehaviour
    {
        [Header("Bar Layout")]
        [SerializeField] private Vector3 _offset    = new Vector3(0f, 2.5f, 0f);
        // ADR-0001 consequence: these are world-space quad dimensions. Halving camera distance
        // (16.8 m -> 9.36 m) roughly doubles their apparent screen size relative to how much of
        // the frame the rest of the scene occupies. Scaled down by the approximate distance
        // ratio (9.36/16.8 ~= 0.56) as a first pass — re-check visually against the new rig and
        // adjust per art direction; this is an estimate, not a measured value.
        [SerializeField] private float   _barWidth  = 0.8f;
        [SerializeField] private float   _barHeight = 0.17f;

        private static Shader   _healthBarShader;
        private static readonly int BaseColorId = Shader.PropertyToID("_BaseColor");

        private EnemyStats   _stats;
        private Transform    _barRoot;
        private Transform    _fillTransform;
        private MeshRenderer _bgRenderer;
        private MeshRenderer _fillRenderer;
        private Material     _bgMat;
        private Material     _fillMat;
        private Camera       _mainCamera;
        private Vector3      _lastCameraPos;
        private Vector3      _lastEnemyPos;
        private Quaternion   _lastEnemyRot;

#if UNITY_EDITOR
        [UnityEditor.InitializeOnLoadMethod]
        private static void RegisterEditorCleanup()
        {
            UnityEditor.EditorApplication.playModeStateChanged += state =>
            {
                if (state == UnityEditor.PlayModeStateChange.ExitingPlayMode)
                    _healthBarShader = null;
            };
        }
#endif

        private void Awake()
        {
            _stats      = GetComponent<EnemyStats>();
            _mainCamera = Camera.main;
            EnsureShader();
            BuildBar();
        }

        // Start() catches the case where Awake() ran too early for pre-placed enemies
        // (Shader.Find returned null → new Material(null) → error shader, not the URP/Unlit we want).
        // By Start() time all shaders are guaranteed loaded. We compare _bgMat.shader against
        // the freshly-confirmed _healthBarShader; a mismatch means BuildBar used the wrong shader.
        private void Start()
        {
            if (_mainCamera == null) _mainCamera = Camera.main;
            EnsureShader();

            bool wrongShader = _healthBarShader != null && _bgMat != null &&
                               _bgMat.shader != _healthBarShader;

            if (wrongShader || _barRoot == null)
            {
                RebuildBar();
                RefreshDisplay();
            }
        }

        private void OnEnable()
        {
            if (_stats == null) _stats = GetComponent<EnemyStats>();
            if (_mainCamera == null) _mainCamera = Camera.main;

            // Same wrong-shader guard as Start() — catches enable/disable cycles after Start runs.
            EnsureShader();
            bool wrongShader = _healthBarShader != null && _bgMat != null &&
                               _bgMat.shader != _healthBarShader;
            if (wrongShader || _barRoot == null)
                RebuildBar();

            if (_mainCamera == null)
            {
                Debug.LogWarning("[EnemyHealthBar] Camera.main is null in OnEnable — bar will be set up in Start().", this);
                return;
            }

            if (_stats == null) return;
            _stats.OnHealthChanged += HandleHealthChanged;
            _stats.OnDeath         += HandleDeath;
            RefreshDisplay();
        }

        private void OnDisable()
        {
            if (_stats == null) return;
            _stats.OnHealthChanged -= HandleHealthChanged;
            _stats.OnDeath         -= HandleDeath;
        }

        private void OnDestroy()
        {
            if (_stats != null)
            {
                _stats.OnHealthChanged -= HandleHealthChanged;
                _stats.OnDeath         -= HandleDeath;
            }
            if (_bgMat   != null) Destroy(_bgMat);
            if (_fillMat != null) Destroy(_fillMat);
        }

        // Refreshing only on camera movement was invisible at the old ~40.8° near-top-down
        // pitch, where per-enemy view angle barely varied across the screen. At the new 36°
        // pitch (ADR-0001) an enemy that moves or turns while the camera holds still visibly
        // skews out of billboard alignment, so the bar must also re-orient on enemy motion.
        private void LateUpdate()
        {
            if (_barRoot == null || !_barRoot.gameObject.activeInHierarchy) return;
            if (_mainCamera == null) return;

            var camPos = _mainCamera.transform.position;
            var enemyPos = transform.position;
            var enemyRot = transform.rotation;

            bool cameraMoved = (camPos - _lastCameraPos).sqrMagnitude > 0.0001f;
            bool enemyMoved   = (enemyPos - _lastEnemyPos).sqrMagnitude > 0.0001f;
            bool enemyRotated = Quaternion.Angle(enemyRot, _lastEnemyRot) > 0.1f;
            if (!cameraMoved && !enemyMoved && !enemyRotated) return;

            _lastCameraPos = camPos;
            _lastEnemyPos  = enemyPos;
            _lastEnemyRot  = enemyRot;
            ApplyBillboard();
        }

        // Called after a build or rebuild to show correct fill colour and activate the bar.
        private void RefreshDisplay()
        {
            if (_barRoot != null) _barRoot.gameObject.SetActive(true);
            if (_stats != null)
            {
                float fraction = _stats.MaxHealth > 0 ? (float)_stats.CurrentHealth / _stats.MaxHealth : 1f;
                SetFill(fraction);
                SetFillColor(fraction);
            }
            ApplyBillboard();
        }

        private void ApplyBillboard()
        {
            if (_mainCamera == null || _barRoot == null) return;
            var dir = _mainCamera.transform.position - _barRoot.position;
            if (dir.sqrMagnitude > 0.001f)
                _barRoot.forward = -dir;
        }

        private void RebuildBar()
        {
            for (int i = transform.childCount - 1; i >= 0; i--)
            {
                var child = transform.GetChild(i);
                if (child.name == "HealthBar")
                    DestroyImmediate(child.gameObject);
            }
            if (_bgMat   != null) { Destroy(_bgMat);   _bgMat   = null; }
            if (_fillMat != null) { Destroy(_fillMat); _fillMat = null; }
            _barRoot       = null;
            _fillTransform = null;
            _bgRenderer    = null;
            _fillRenderer  = null;
            BuildBar();
        }

        private static void EnsureShader()
        {
            if (_healthBarShader != null) return;
            _healthBarShader = Shader.Find("Universal Render Pipeline/Unlit")
                            ?? Shader.Find("Unlit/Color");
            if (_healthBarShader == null)
                Debug.LogError("[EnemyHealthBar] Health bar shader not found. Add URP/Unlit to Graphics Settings.");
        }

        private void BuildBar()
        {
            if (_healthBarShader == null) return; // shader not ready — Start() will retry

            var rootGO = new GameObject("HealthBar");
            _barRoot = rootGO.transform;
            _barRoot.SetParent(transform);
            _barRoot.localPosition = _offset;
            _barRoot.localRotation = Quaternion.identity;

            Vector3 ls = transform.lossyScale;
            _barRoot.localScale = new Vector3(
                ls.x != 0f ? 1f / ls.x : 1f,
                ls.y != 0f ? 1f / ls.y : 1f,
                ls.z != 0f ? 1f / ls.z : 1f);

            _bgMat = new Material(_healthBarShader);
            _bgMat.SetTexture("_BaseMap", Texture2D.whiteTexture);
            _bgMat.SetColor(BaseColorId, new Color(0.05f, 0.05f, 0.05f, 1f));

            var bgGO = GameObject.CreatePrimitive(PrimitiveType.Quad);
            bgGO.name = "BG";
            DestroyImmediate(bgGO.GetComponent<MeshCollider>());
            bgGO.transform.SetParent(_barRoot, false);
            bgGO.transform.localPosition = Vector3.zero;
            bgGO.transform.localScale    = new Vector3(_barWidth, _barHeight, 1f);
            _bgRenderer = bgGO.GetComponent<MeshRenderer>();
            _bgRenderer.sharedMaterial    = _bgMat;
            _bgRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            _bgRenderer.receiveShadows    = false;

            _fillMat = new Material(_healthBarShader);
            _fillMat.SetTexture("_BaseMap", Texture2D.whiteTexture);
            _fillMat.SetColor(BaseColorId, Color.green);

            var fillGO = GameObject.CreatePrimitive(PrimitiveType.Quad);
            fillGO.name = "Fill";
            DestroyImmediate(fillGO.GetComponent<MeshCollider>());
            _fillTransform = fillGO.transform;
            _fillTransform.SetParent(_barRoot, false);
            _fillRenderer = fillGO.GetComponent<MeshRenderer>();
            _fillRenderer.sharedMaterial    = _fillMat;
            _fillRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            _fillRenderer.receiveShadows    = false;
            SetFill(1f);
        }

        private void SetFill(float amount)
        {
            if (_fillTransform == null) return;
            float fill  = Mathf.Clamp01(amount);
            float inset = _barHeight * 0.15f;
            float fillW = (_barWidth  - inset * 2f) * fill;
            float fillH =  _barHeight - inset * 2f;
            _fillTransform.localScale    = new Vector3(fillW, fillH, 1f);
            float xOffset = -(_barWidth - inset * 2f) * (1f - fill) * 0.5f;
            _fillTransform.localPosition = new Vector3(xOffset, 0f, -0.005f);
        }

        private void HandleHealthChanged(int current, int max)
        {
            float fraction = max > 0 ? (float)current / max : 0f;
            SetFill(fraction);
            SetFillColor(fraction);
        }

        private void SetFillColor(float fraction)
        {
            if (_fillMat == null) return;
            Color color;
            if (fraction > 0.5f)
                color = Color.Lerp(Color.yellow, Color.green, (fraction - 0.5f) * 2f);
            else
                color = Color.Lerp(Color.red, Color.yellow, fraction * 2f);
            _fillMat.SetColor(BaseColorId, color);
        }

        private void HandleDeath()
        {
            if (_barRoot != null)
                _barRoot.gameObject.SetActive(false);
        }
    }
}
