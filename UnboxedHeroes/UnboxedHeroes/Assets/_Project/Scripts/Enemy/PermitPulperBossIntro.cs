using System.Collections;
using UnityEngine;
using Boxhead.UI;

namespace Boxhead.Enemy
{
    /// <summary>
    /// Boss intro for the Permit Pulper. Boss is already at full scale when the scene loads.
    /// On trigger: freeze player, cut camera to the boss, hold, cut back, begin combat.
    /// </summary>
    public class PermitPulperBossIntro : MonoBehaviour
    {
        [Header("Trigger")]
        [SerializeField] private bool _waitForAllEnemiesDead = true;
        [SerializeField] private int  _minKillsRequired = 4;

        [Header("Spawner")]
        [SerializeField] private EnemySpawner _spawnerToDisable;

        [Header("Camera")]
        [Tooltip("How long the camera holds on the boss before cutting back to the player.")]
        [SerializeField] private float _bossShowDuration = 8f;
        [Tooltip("Slow-motion scale while the boss is revealed. 1 = real time, 0.25 = quarter speed.")]
        [SerializeField] private float _slowMoScale = 0.3f;

        [Header("VFX")]
        [Tooltip("Unscaled seconds after camera cut before the camera shake fires.")]
        [SerializeField] private float _shakeDelay  = 1.2f;
        [Tooltip("Strength of the camera shake impulse.")]
        [SerializeField] private float _shakeForce  = 2.5f;
        [Tooltip("How far the directional light colour shifts toward red during the reveal.")]
        [SerializeField] private float _lightPulseAmount = 0.6f;

        [Header("HUD")]
        [SerializeField] private BossHealthBar _bossHealthBar;

        private PermitPulperBossAI _ai;
        private Transform          _player;
        private bool               _triggered;
        private int                _killCount;

        private void Start()
        {
            _ai     = GetComponent<PermitPulperBossAI>();
            _player = GameObject.FindWithTag("Player")?.transform;

            if (_ai != null) _ai.enabled = false;

            if (_waitForAllEnemiesDead)
            {
                int totalSpawnerCapacity = 0;
                var spawners = FindObjectsByType<EnemySpawner>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);
                foreach (var s in spawners)
                {
                    var f = s.GetType().GetField("maxActiveEnemies",
                        System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                    if (f != null) totalSpawnerCapacity += (int)f.GetValue(s);
                }

                var staticEnemies = GameObject.FindGameObjectsWithTag("Enemy");
                int staticCount = 0;
                foreach (var e in staticEnemies)
                {
                    if (e == gameObject) continue;
                    if (e.GetComponent<EnemyStats>() != null) staticCount++;
                }

                int totalRequired = totalSpawnerCapacity + staticCount;
                Debug.Log($"[PermitPulperIntro] Kill threshold = {totalRequired}");

                if (totalRequired == 0)
                {
                    StartCoroutine(IntroRoutine());
                    return;
                }

                _minKillsRequired = totalRequired;
                EnemyStats.OnAnyEnemyDeath += CheckAllEnemiesDead;
            }
        }

        private void OnDestroy()
        {
            EnemyStats.OnAnyEnemyDeath -= CheckAllEnemiesDead;
            if (Mathf.Abs(Time.timeScale - _slowMoScale) < 0.01f)
                Time.timeScale = 1f;
        }

        private void CheckAllEnemiesDead()
        {
            if (_triggered) return;
            _killCount++;
            if (_killCount >= _minKillsRequired)
                TryTrigger();
        }

        private void TryTrigger()
        {
            if (_triggered) return;
            var allEnemies = GameObject.FindGameObjectsWithTag("Enemy");
            int livingNonBoss = 0;
            for (int i = 0; i < allEnemies.Length; i++)
            {
                if (allEnemies[i] == gameObject) continue;
                var s = allEnemies[i].GetComponent<EnemyStats>();
                if (s != null && !s.IsDead) livingNonBoss++;
            }
            if (livingNonBoss == 0)
                StartCoroutine(IntroRoutine());
        }

        // ── Camera helpers ────────────────────────────────────────────────────

        // Disable the Cinemachine Brain so we can drive the Main Camera transform directly.
        private Unity.Cinemachine.CinemachineBrain _brain;
        private Camera _mainCam;

        private void TakeoverCamera()
        {
            _mainCam = Camera.main;
            if (_mainCam == null) return;
            _brain = _mainCam.GetComponent<Unity.Cinemachine.CinemachineBrain>();
            if (_brain != null) _brain.enabled = false;
        }

        private void RestoreCamera()
        {
            if (_brain != null) _brain.enabled = true;
        }

        // ── Intro sequence ────────────────────────────────────────────────────

        private IEnumerator IntroRoutine()
        {
            _triggered = true;

            if (_spawnerToDisable != null)
                _spawnerToDisable.enabled = false;

            // Freeze player
            var playerCtrl = _player != null
                ? _player.GetComponent<Boxhead.Player.PlayerController>()
                : null;
            if (playerCtrl != null) playerCtrl.enabled = false;

            // Boss invulnerable until combat begins
            var bossStats = GetComponent<EnemyStats>();
            if (bossStats != null) bossStats.SetInvulnerable(true);

            // Cache VFX references
            var impulse = GetComponent("CinemachineImpulseSource") as MonoBehaviour;
            var dirLight = UnityEngine.Object.FindAnyObjectByType<Light>();
            Color originalLightColor = dirLight != null ? dirLight.color : Color.white;

            // Take over the Main Camera — disable Cinemachine Brain so we drive it directly
            TakeoverCamera();
            Time.timeScale = _slowMoScale;

            // Camera pan: knee level → full body reveal, deliberately slow
            // Boss faces -Z; camera is on the entrance side (lower Z values)
            Vector3 bossBase  = transform.position;
            Vector3 camStart  = bossBase + new Vector3(0.5f,  0.5f,  -2.5f); // knee height, 2.5m in front
            Vector3 camEnd    = bossBase + new Vector3(2.0f,  2.5f,  -8.0f); // pulled back, mid-chest height
            Vector3 lookStart = bossBase + new Vector3(0f,    0.8f,   0f);    // looking at lower legs
            Vector3 lookEnd   = bossBase + new Vector3(0f,    2.8f,   0f);    // looking at upper chest

            Transform introCamTF = _mainCam != null ? _mainCam.transform : null;

            // Freeze boss animator so it holds a standing pose — not the idle breathing cycle
            var bossAnimator = GetComponentInChildren<Animator>(true);
            if (bossAnimator != null)
            {
                bossAnimator.Play("Walk", 0, 0.13f); // freeze just past the standing phase
                bossAnimator.speed = 0f;
            }

            float elapsed = 0f;
            bool shookCamera = false;
            bool stamped     = false;
            bool lightPulsed = false;

            while (elapsed < _bossShowDuration)
            {
                elapsed += Time.unscaledDeltaTime;
                float rawT = Mathf.Clamp01(elapsed / _bossShowDuration);
                // Ease-in slow at start (feet close-up), then smooth pan upward
                float t = Mathf.SmoothStep(0f, 1f, rawT);

                // Animate the intro cam position + look each frame
                if (introCamTF != null)
                {
                    introCamTF.position = Vector3.Lerp(camStart, camEnd, t);
                    introCamTF.LookAt(Vector3.Lerp(lookStart, lookEnd, t));
                }

                // At 30% through: light pulse + camera shake
                if (!lightPulsed && rawT >= 0.30f && dirLight != null)
                {
                    lightPulsed = true;
                    dirLight.color = Color.Lerp(originalLightColor,
                        new Color(1f, 0.35f, 0.1f), _lightPulseAmount);
                    StartCoroutine(RestoreLight(dirLight, originalLightColor, 0.8f));
                }

                if (!shookCamera && rawT >= 0.30f)
                {
                    shookCamera = true;
                    if (impulse != null)
                    {
                        var method = impulse.GetType().GetMethod("GenerateImpulse",
                            new[] { typeof(float) });
                        method?.Invoke(impulse, new object[] { _shakeForce });
                    }
                }

                // At 50%: boss stamp animation
                if (!stamped && rawT >= 0.50f && bossAnimator != null)
                {
                    stamped = true;
                    bossAnimator.SetTrigger("AttackTrigger");
                }

                yield return null;
            }

            Time.timeScale = 1f;

            // Restore Cinemachine — follow-cam takes back over automatically
            RestoreCamera();

            // Brief pause so Cinemachine can snap back to the follow position
            float blendBack = 0f;
            while (blendBack < 0.5f)
            {
                blendBack += Time.deltaTime;
                yield return null;
            }

            if (playerCtrl != null) playerCtrl.enabled = true;
            if (bossStats  != null) bossStats.SetInvulnerable(false);
            if (bossAnimator != null) bossAnimator.speed = 1f; // resume animations for combat
            _bossHealthBar?.Activate();
            if (_ai != null) _ai.enabled = true;
        }

        private IEnumerator RestoreLight(Light light, Color original, float duration)
        {
            float t = 0f;
            Color pulsed = light.color;
            while (t < duration)
            {
                t += Time.unscaledDeltaTime;
                light.color = Color.Lerp(pulsed, original, t / duration);
                yield return null;
            }
            light.color = original;
        }
    }
}
