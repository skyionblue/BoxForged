using UnityEngine;
using Boxhead.Core;

namespace Boxhead.Systems
{
    /// <summary>
    /// ADR-0005 (generalized from ADR-0004's WildWestCityZoneDirector): owns every
    /// scene-specific consequence of a zone change in a single-continuous-scene world —
    /// clearing props that block the boss arena, activating the pre-placed boss, and
    /// opening each zone's exit gate. All four serialized fields below are already
    /// scene-agnostic data (which props clear, which GameObject is the boss, and which
    /// gate belongs to which zone are facts about one scene's layout, not the reusable
    /// encounter system) — only the class name was ever street-specific, so this is a
    /// reusable component instantiated once per single-scene world rather than copied
    /// per world. First shipped for CulDeSac_WildWestCity (World 1); World 2's
    /// Backyard_Dojo reuses the same class as a second instance.
    ///
    /// Deliberately a single scene-local script rather than an extension of RoomManager:
    /// which props clear, which GameObject is the boss, and which barricade belongs to
    /// which zone are facts about one scene, not the reusable encounter system. RoomManager
    /// exposes OnRoomActivated (still used directly here for boss activation).
    /// Gate-opening deliberately does NOT subscribe to RoomManager.OnRoomCleared directly —
    /// see HandleZoneCleared below (docs/BACKLOG.md H4).
    /// </summary>
    public class ZoneDirector : MonoBehaviour
    {
        [Tooltip("Objects deactivated the moment the boss zone activates — e.g. props blocking the boss arena floor or a boss intro camera's dolly path (World 1: the two covered wagons and the SpinCycle intro camera's dolly path).")]
        [SerializeField] private GameObject[] _clearOnBossZone;

        [Tooltip("Pre-placed, inactive boss instance for this scene. Forced inactive in Awake regardless of the saved scene state, then activated when the boss zone becomes active.")]
        [SerializeField] private GameObject _boss;

        [Tooltip("_gateByZone[i] is the RoomGate that opens when zone i is cleared. Leave an element null if that zone has no gate (e.g. the boss zone, which is never \"cleared\" via RoomManager — the boss AI owns the win).")]
        [SerializeField] private RoomGate[] _gateByZone;

        [Tooltip("Zone index at which the boss activates and _clearOnBossZone props clear. Must match this scene's boss RoomDataSO (bossOwnedWin = true). Cross-checked against this scene's LevelBuilder.RoomData at Start() — a loud warning/error fires if it drifts out of sync.")]
        [SerializeField] private int _bossZoneIndex = 2;

        private void Awake()
        {
            // Do not rely on the saved scene flag: Play Mode state does not reliably revert
            // in this project (see .claude/agent-memory/unity-gameplay-engineer), and a boss
            // left active by a previous session would run its intro at scene load.
            if (_boss != null)
            {
                _boss.SetActive(false);
            }
            else
            {
                // H5: silent before this fix — the boss would simply never activate, giving
                // an unwinnable run with zero log output to explain why.
                Debug.LogError(
                    "[ZoneDirector] _boss is not assigned — the boss zone will " +
                    "never activate and the run is unwinnable.", this);
            }
        }

        private void Start()
        {
            // H5: cross-check _bossZoneIndex and _gateByZone against this scene's actual
            // zone data instead of trusting the hand-set Inspector values to stay in sync.
            // Diagnostic only — does not change runtime behaviour.
            var levelBuilder = FindAnyObjectByType<LevelBuilder>(FindObjectsInactive.Include);
            RoomDataSO[] roomData = levelBuilder != null ? levelBuilder.RoomData : null;

            if (roomData == null || roomData.Length == 0)
            {
                Debug.LogWarning(
                    "[ZoneDirector] Could not find this scene's LevelBuilder.RoomData " +
                    "— cannot cross-check _bossZoneIndex/_gateByZone.", this);
                return;
            }

            if (_bossZoneIndex < 0 || _bossZoneIndex >= roomData.Length)
            {
                Debug.LogError(
                    $"[ZoneDirector] _bossZoneIndex ({_bossZoneIndex}) is out of range " +
                    $"for this scene's {roomData.Length} RoomDataSO zones — the boss will never activate.", this);
            }
            else if (roomData[_bossZoneIndex] == null || !roomData[_bossZoneIndex].bossOwnedWin)
            {
                Debug.LogWarning(
                    $"[ZoneDirector] _bossZoneIndex ({_bossZoneIndex}) does not point at a " +
                    "bossOwnedWin RoomDataSO — it may be out of sync with this scene's actual zone order.", this);
            }

            // Every zone except the last needs a gate to exit it (the last zone is the boss
            // zone, which the boss AI ends directly — no gate needed). Trailing null/absent
            // entries beyond that are fine (that is how the boss zone's "no gate" slot works).
            int gatesNeeded = roomData.Length - 1;
            int gatesProvided = _gateByZone != null ? _gateByZone.Length : 0;
            if (gatesProvided < gatesNeeded)
            {
                Debug.LogWarning(
                    $"[ZoneDirector] _gateByZone has {gatesProvided} entries but this scene " +
                    $"has {roomData.Length} zones (needs {gatesNeeded} gates) — some zone clears may have " +
                    "no gate to open, permanently walling the player in.", this);
            }
        }

        private void OnEnable()
        {
            // Subscribed in OnEnable, not Start: RoomManager.Start() can call ActivateRoom(0)
            // synchronously, and a static event that is not yet subscribed misses it.
            RoomManager.OnRoomActivated += HandleZoneActivated;

            // H4: deliberately GameManager.OnRoomClearScreenShown, NOT RoomManager.OnRoomCleared.
            // RoomManager.OnRoomCleared fires immediately; GameManager.HandleRoomCleared then
            // waits _roomClearShowDelay (~1.5s) before actually showing the upgrade/shop screen.
            // Opening the gate on the earlier event left a real exploit window where the player
            // could sprint through the already-open gate into the next zone before the screen
            // froze them (Time.timeScale = 0), and the reward screen would then appear mid-fight
            // in the wrong zone. OnRoomClearScreenShown fires only once Show() has actually run
            // and frozen the player, so the gate cannot be reached unfrozen.
            GameManager.OnRoomClearScreenShown += HandleZoneCleared;
        }

        private void OnDisable()
        {
            // Mandatory — these are static events and will leak across scene reloads
            // (and re-fire into a destroyed instance) if left subscribed.
            RoomManager.OnRoomActivated -= HandleZoneActivated;
            GameManager.OnRoomClearScreenShown -= HandleZoneCleared;
        }

        private void HandleZoneActivated(int zoneIndex)
        {
            if (zoneIndex != _bossZoneIndex) return;

            // Order is load-bearing: e.g. World 1's covered_wagon_01 sits directly alongside
            // the SpinCycle intro camera's dolly path (ADR-0004 §2), so _clearOnBossZone must
            // clear before the boss (and any Awake()-created intro camera) is activated.
            if (_clearOnBossZone != null)
            {
                for (int i = 0; i < _clearOnBossZone.Length; i++)
                {
                    if (_clearOnBossZone[i] != null)
                        _clearOnBossZone[i].SetActive(false);
                }
            }

            if (_boss != null)
                _boss.SetActive(true);
        }

        private void HandleZoneCleared(int zoneIndex)
        {
            // H5: fail loudly — a silent no-op here means the player is permanently walled
            // into the current zone with no indication why.
            if (_gateByZone == null || zoneIndex < 0 || zoneIndex >= _gateByZone.Length)
            {
                Debug.LogError(
                    $"[ZoneDirector] No RoomGate configured for cleared zone {zoneIndex} " +
                    $"(_gateByZone has {(_gateByZone == null ? 0 : _gateByZone.Length)} entries) — " +
                    "the gate will never open and the player is permanently walled in.", this);
                return;
            }

            var gate = _gateByZone[zoneIndex];
            if (gate == null)
            {
                Debug.LogError(
                    $"[ZoneDirector] _gateByZone[{zoneIndex}] is null — the gate for " +
                    "this zone will never open and the player is permanently walled in.", this);
                return;
            }

            gate.Open();
        }
    }
}
