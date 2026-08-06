using UnityEngine;
using UnityEngine.SceneManagement;
using Boxhead.Enemy;

namespace Boxhead.Systems
{
    /// <summary>
    /// Trigger collider on the CommunityHall south entrance. Locks the door until all
    /// outdoor enemies in TownSquare_Room1 are dead, then loads the boss scene on contact.
    ///
    /// Architecture: subscribes once to EnemyStats.OnAnyEnemyDeath (static event) so it
    /// hears every enemy kill without polling FindGameObjectsWithTag every frame.
    /// FindGameObjectsWithTag is called only inside CheckUnlock(), which fires at most
    /// once per enemy death — not per frame.
    /// </summary>
    [RequireComponent(typeof(BoxCollider))]
    public class BossHallDoor : MonoBehaviour
    {
        [SerializeField] private string     _bossSceneName    = "TownSquare_BossHall";
        [SerializeField] private GameObject _lockedIndicator;
        [SerializeField] private GameObject _openIndicator;
        [SerializeField] private string     _lockedMessage    = "Defeat all enemies first";

        private bool _unlocked;
        private int  _killCount;
        private BoxCollider _collider;

        private void Awake()
        {
            _collider = GetComponent<BoxCollider>();
            _collider.isTrigger = true;
        }

        private void Start()
        {
            EnemyStats.OnAnyEnemyDeath += CheckUnlock;
            // Do NOT call CheckUnlock on Start — the spawner hasn't fired yet so
            // living == 0 would unlock the door immediately before any enemies appear.
        }

        private void OnDestroy()
        {
            EnemyStats.OnAnyEnemyDeath -= CheckUnlock;
        }

        // Called each time any enemy dies. Requires at least one kill before unlocking
        // so the door never opens at scene start before the spawner has fired.
        private void CheckUnlock()
        {
            if (_unlocked) return;

            _killCount++;

            var enemies = GameObject.FindGameObjectsWithTag("Enemy");
            int living = 0;
            for (int i = 0; i < enemies.Length; i++)
            {
                if (enemies[i] == gameObject) continue;
                var stats = enemies[i].GetComponent<EnemyStats>();
                if (stats != null && !stats.IsDead)
                    living++;
            }

            if (living == 0)
                Unlock();
        }

        private void Unlock()
        {
            _unlocked = true;

            if (_lockedIndicator != null)
                _lockedIndicator.SetActive(false);

            if (_openIndicator != null)
                _openIndicator.SetActive(true);

            Debug.Log("[BossHallDoor] All enemies defeated — door is now open.");
        }

        private void OnTriggerEnter(Collider other)
        {
            if (!other.CompareTag("Player")) return;

            if (!_unlocked)
            {
                Debug.Log($"[BossHallDoor] {_lockedMessage}");
                return;
            }

            // Snapshot cardboard + forged weapons so the loadout carries into the boss hall.
            Boxhead.Core.GameManager.Instance?.CaptureLoadoutForTransition();

            // Ensure timeScale is normal before the scene loads (it may have been paused)
            Time.timeScale = 1f;
            SceneManager.LoadScene(_bossSceneName);
        }
    }
}
