using System.Collections;
using UnityEngine;

namespace Boxhead.Enemy
{
    /// <summary>
    /// Plays the SpinCycle boss intro: boss walks out of the Saloon while scaling up from
    /// small to full size. Arena wagons simultaneously shrink and disappear.
    /// Fires a camera shake impulse and slow-motion swell at the moment of emergence.
    /// Disables SpinCycleAI until the intro completes.
    /// </summary>
    public class BossIntroSequence : MonoBehaviour
    {
        [Header("Trigger")]
        [Tooltip("If true, intro fires when all non-boss enemies die instead of player proximity.")]
        [SerializeField] private bool  _waitForAllEnemiesDead = true;
        [Tooltip("Minimum total enemy kills required before the boss intro can trigger.")]
        [SerializeField] private int   _minKillsRequired = 8;
        [SerializeField] private float _triggerRange = 14f;

        [Header("Boss Walk-Out")]
        [SerializeField] private float   _walkInDistance    = 8f;   // how far inside the Saloon the boss starts
        [SerializeField] private float   _doorOutsideOffset = 1f;   // how far outside the door he stops
        [SerializeField] private float   _introDuration     = 3f;

        [Header("Boss Scale")]
        [SerializeField] private Vector3 _startScale = new Vector3(0.3f, 0.3f, 0.3f);
        [SerializeField] private Vector3 _fullScale  = new Vector3(2f, 2f, 2f);

        [Header("Held Props (walk-out display only)")]
        [SerializeField] private GameObject _clothesBallPropPrefab;
        [SerializeField] private GameObject _sudsBlobPropPrefab;
        [SerializeField] private Material   _clothesBallMaterial;
        [SerializeField] private Material   _sudsBlobMaterial;

        [Header("Wagon Clearing")]
        [SerializeField] private Transform[] _wagonsToRemove;
        [SerializeField] private float        _wagonShrinkDuration = 1.5f;

        [Header("Dramatic Entrance")]
        [SerializeField] private float _slowMoScale       = 0.25f;
        [SerializeField] private float _slowMoDuration    = 1.8f;
        [SerializeField] private float _cameraPanDuration = 3.5f;   // seconds camera holds on boss during drum spin-up

        private SpinCycleAI     _ai;
        private EnemyStats      _stats;
        private BossHeadBounce  _headBounce;
        private DrumWindowRotator _drumWindow;
        private Renderer[]      _bossRenderers;   // cached for hide/show during intro
        private Animator        _bossAnimator;
        private Component       _impulse;
        private Component       _introCam;
        private Transform       _player;
        private GameObject      _heldClothesBall;   // runtime instance parented to LeftHandHold
        private GameObject      _heldSudsBlob;      // runtime instance parented to RightHandHold
        private Transform       _leftHandHold;
        private Transform       _rightHandHold;
        private bool        _triggered;
        private int         _killCount;

        private Vector3 _doorPosition;
        private Vector3 _startPosition;

        private void Start()
        {
            _ai           = GetComponent<SpinCycleAI>();
            _stats        = GetComponent<EnemyStats>();
            _headBounce   = GetComponent<BossHeadBounce>();
            _drumWindow   = GetComponentInChildren<DrumWindowRotator>(true);
            _bossAnimator = GetComponentInChildren<Animator>(true);
            _impulse      = GetComponent("CinemachineImpulseSource") as Component;

            // Cache hand hold transform references for prop instantiation
            foreach (var t in GetComponentsInChildren<Transform>(true))
            {
                if (t.name == "LeftHandHold")  _leftHandHold  = t;
                if (t.name == "RightHandHold") _rightHandHold = t;
            }
            var introCamGO = GameObject.Find("CM_BossIntroCam");
            if (introCamGO != null)
                _introCam = introCamGO.GetComponent("CinemachineCamera") as Component;

            if (_ai != null) _ai.enabled = false;

            // Hide all renderers — boss is invisible inside the Saloon until the cutscene reveals him.
            _bossRenderers = GetComponentsInChildren<Renderer>(true);
            SetBossVisible(false);

            // Disable NavMeshAgent for the entire intro — the boss starts inside the Saloon
            // which is OFF the NavMesh. An active agent snaps to the nearest valid mesh
            // position (outside the building) and fights the direct transform.position moves,
            // causing the boss to get stuck in the wall on Android.
            var agent = GetComponent<UnityEngine.AI.NavMeshAgent>();
            if (agent != null) agent.enabled = false;

            // End position: just outside the Saloon door (_doorOutsideOffset units in front)
            // Start position: _walkInDistance units further inside the Saloon
            _doorPosition  = transform.position + transform.forward * _doorOutsideOffset;
            _startPosition = _doorPosition - transform.forward * _walkInDistance;

            transform.position   = _startPosition;
            transform.localScale = _startScale;

            _player = GameObject.FindWithTag("Player")?.transform;

            if (_waitForAllEnemiesDead)
            {
                // Use maxActiveEnemies (not maxTotalSpawns) so the threshold is the
                // number of enemies alive at peak, not the total that could ever spawn.
                // Player just needs to clear the arena once — not kill every possible spawn.
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
                    if (e != gameObject && e.GetComponent<EnemyStats>() != null) staticCount++;
                _minKillsRequired = Mathf.Max(1, totalSpawnerCapacity + staticCount);
                UnityEngine.Debug.Log($"[BossIntro] Kill threshold = {_minKillsRequired} (activeSlots={totalSpawnerCapacity} static={staticCount})");
                EnemyStats.OnAnyEnemyDeath += CheckAllEnemiesDead;
            }
        }

        private void OnDestroy()
        {
            EnemyStats.OnAnyEnemyDeath -= CheckAllEnemiesDead;
            // Restore timeScale if destroyed mid-intro
            if (Mathf.Abs(Time.timeScale - _slowMoScale) < 0.01f)
                Time.timeScale = 1f;
        }

        private void CheckAllEnemiesDead()
        {
            if (_triggered) return;
            _killCount++;
            // Once threshold met, check after each subsequent kill whether arena is clear.
            // Spawners replenish so there's no single moment all enemies die — we keep checking.
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
            Debug.LogWarning($"[BossIntro] TryTrigger — kills={_killCount}/{_minKillsRequired} livingNonBoss={livingNonBoss}");
            if (livingNonBoss == 0)
                StartCoroutine(IntroRoutine());
        }

        private void Update()
        {
            if (_triggered || _waitForAllEnemiesDead || _player == null) return;
            if ((_player.position - _doorPosition).sqrMagnitude <= _triggerRange * _triggerRange)
                StartCoroutine(IntroRoutine());
        }

        // In Cinemachine 3, Priority is a PrioritySettings struct — property reflection fails.
        // Instead: enable the intro cam and disable the follow-cam (or vice-versa).
        // This gives clean exclusive camera control without any priority math.
        private Behaviour _followCamBehaviour;

        private void SetIntroCamActive(bool introActive)
        {
            // Enable/disable intro camera
            if (_introCam is Behaviour introBehaviour)
                introBehaviour.enabled = introActive;

            // Find and toggle the follow-cam
            if (_followCamBehaviour == null)
            {
                var followGO = GameObject.Find("CM_FollowCam");
                if (followGO != null)
                    foreach (var c in followGO.GetComponents<Component>())
                        if (c.GetType().Name == "CinemachineCamera" && c is Behaviour b)
                        { _followCamBehaviour = b; break; }
            }
            if (_followCamBehaviour != null)
                _followCamBehaviour.enabled = !introActive;
        }

        private void SetBossVisible(bool visible)
        {
            if (_bossRenderers == null) return;
            for (int i = 0; i < _bossRenderers.Length; i++)
                if (_bossRenderers[i] != null) _bossRenderers[i].enabled = visible;
        }

        private IEnumerator IntroRoutine()
        {
            _triggered = true;

            // Boss is invulnerable during the cutscene — player cannot skip it by killing the boss.
            _stats?.SetInvulnerable(true);

            // Reveal the boss now that the cutscene is starting.
            SetBossVisible(true);

            // Camera cuts to Saloon. Wagons shrink. Boss walks out growing. Camera holds.
            SetIntroCamActive(true);

            for (int i = 0; i < _wagonsToRemove.Length; i++)
                if (_wagonsToRemove[i] != null)
                    StartCoroutine(ShrinkWagon(_wagonsToRemove[i]));

            // Reset drum to face forward and hold still during the walk-out.
            if (_drumWindow != null) _drumWindow.ResetToForward();

            // Extend slow-mo by 1 extra second for more drama
            float totalSlowDuration = _introDuration + 1f;
            Time.timeScale = _slowMoScale;

            // Walk out AND grow simultaneously.
            // Uses ease-in only (SmoothStep start) then linear, tapering Speed at the end
            // so the legs slow naturally rather than snapping to idle.
            float elapsed = 0f;
            while (elapsed < totalSlowDuration)
            {
                elapsed += Time.unscaledDeltaTime;
                float rawT = Mathf.Clamp01(elapsed / totalSlowDuration);

                // Ease-in only: smooth acceleration at start, linear until 80%, ease-out last 20%
                float t = rawT < 0.2f
                    ? Mathf.SmoothStep(0f, 0.2f, rawT / 0.2f) * (0.2f / 0.2f)
                    : rawT;
                t = Mathf.Clamp01(t);

                transform.position   = Vector3.Lerp(_startPosition, _doorPosition, t);
                transform.localScale = Vector3.Lerp(_startScale, _fullScale, t);

                // Taper walk animation speed in the final 20% so legs decelerate smoothly
                float speed = rawT > 0.8f ? Mathf.SmoothStep(1f, 0f, (rawT - 0.8f) / 0.2f) : 1f;
                _bossAnimator?.SetFloat("Speed", speed, 0.1f, Time.unscaledDeltaTime);

                yield return null;
            }

            _bossAnimator?.SetFloat("Speed", 0f);
            transform.position   = _doorPosition;
            transform.localScale = _fullScale;
            Time.timeScale = 1f;

            // Boss has stopped at the door — start drum spin-up with smoke and sparks.
            // ResetToForward() was called at walk-out start so the porthole faces forward here.
            _drumWindow?.StartIntroBuildUp();
            _headBounce?.StartVFX();

            // Hold camera for the full build-up duration plus a brief settling beat.
            float spinHold = 0f;
            while (spinHold < _cameraPanDuration)
            {
                spinHold += Time.unscaledDeltaTime;
                yield return null;
            }

            // Spin-up complete — stop VFX, return drum to standard idle speed.
            _headBounce?.StopVFX();
            _drumWindow?.SetSlowPhase();

            // Hand back to follow-cam and enable AI.
            // Re-enable NavMeshAgent and warp it to the door position so the agent
            // starts on the NavMesh surface rather than searching from inside the Saloon.
            SetIntroCamActive(false);
            var agentEnd = GetComponent<UnityEngine.AI.NavMeshAgent>();
            if (agentEnd != null)
            {
                agentEnd.enabled = true;
                agentEnd.Warp(transform.position);
            }
            if (_ai != null) _ai.enabled = true;

            // Cutscene finished — boss is now vulnerable.
            _stats?.SetInvulnerable(false);
        }

        private IEnumerator ShrinkWagon(Transform wagon)
        {
            Vector3 originalScale = wagon.localScale;
            float elapsed = 0f;
            // Use unscaled time so the shrink plays at the same real-world speed
            // regardless of whether we're in slow-mo or not.
            while (elapsed < _wagonShrinkDuration)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(elapsed / _wagonShrinkDuration));
                wagon.localScale = Vector3.Lerp(originalScale, Vector3.zero, t);
                yield return null;
            }
            wagon.gameObject.SetActive(false);
        }
    }
}
