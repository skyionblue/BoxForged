# Run Loadout Persistence — Implementation Plan

**Goal:** Persist the player's **cardboard** and **forged weapons** across scene loads *within a run* (room → room → boss), while resetting them at the start of a brand-new run (death/restart or fresh character pick).

**Approach:** Session-only (no disk). Store state in the `DontDestroyOnLoad` `ProgressionSystem` singleton — mirroring the existing `HasRunSelection` pattern. Snapshot at room transition, restore in `GameManager.Start()`, clear on new run.

## Why this design
- Each room is its own scene; the player (`pfb_player`) is recreated every load, so on-player state (`CardboardResource.Current`, `WeaponInventory` slots/bag) resets each room today.
- `ProgressionSystem` already survives scene loads and already holds run-scoped session state (XP, IP, run selection). Natural home.
- Cardboard + forged weapons are in-run currency/loadout, not meta-progression — so **no `SaveData`/disk changes**.

## Storage (add to `ProgressionSystem`)
- `bool HasRunLoadout` — gate ("is there saved state to restore?"), like `HasRunSelection`.
- `int RunCardboard`.
- `StoredWeapon[] RunWeaponSlots` (`{ WeaponObjectSO data; WeaponTier tier; int durability; }`) + `int RunActiveSlotIndex`.
- `WeaponObjectSO[] RunMaterialBag` *(pending decision D1)*.
- API: `CaptureRunLoadout(...)`, `RestoreRunLoadout(...)`, `ClearRunLoadout()`.

## Hooks
- **Capture (snapshot):** top of `GameManager.LoadNextRoom()` (CulDeSac room→room and room→boss) and before `SceneManager.LoadScene` in `BossHallDoor` (TownSquare→boss). Via a shared `GameManager.CaptureLoadoutForTransition()` helper.
- **Restore:** end of `GameManager.Start()` (after player found and after `RunStartUI.Show()` so the character model is swapped first), gated on `HasRunLoadout`.
- **Clear (new run):** `GameManager.Restart()` and `RunStartUI.OnStartClicked()` (fresh selection). Room transitions never clear.

## Files to change (dependency order)
1. `CardboardResource.cs` — add `SetCurrent(int)` (sets value, fires `OnCardboardChanged`).
2. `WeaponInstance.cs` — ctor/overload accepting explicit `currentDurability` (current ctor forces max).
3. `WeaponInventory.cs` — add `RestoreState(slots, bag, activeIndex)` (writes arrays directly, `SetActiveSlot`, `NotifyInventoryChanged`).
4. `ProgressionSystem.cs` — fields + `CaptureRunLoadout`/`RestoreRunLoadout`/`ClearRunLoadout`.
5. `GameManager.cs` — capture in `LoadNextRoom()`, restore in `Start()`, clear in `Restart()`.
6. `BossHallDoor.cs` — capture before boss `LoadScene`.
7. `RunStartUI.cs` — clear in `OnStartClicked()`.
8. *(conditional D4)* `MetaScreen.cs`/`WorldMapScreen.cs` — clear on zone-advance if it bypasses the picker.

## Decisions (CONFIRMED)
- **D1 Material bag → NO.** Persist forged weapon slots + cardboard only. Do NOT persist the material bag (drop `RunMaterialBag`; `RestoreState` takes slots + activeIndex only). Simplifies the change.
- **D2 Durability → CARRY.** Capture/restore each weapon's `CurrentDurability` so wear carries room-to-room (needs the `WeaponInstance` explicit-durability ctor).
- **D4 Between zones → RESET.** Loadout persists through all rooms of the current zone incl. its boss, but a new zone starts fresh. Ensure `ClearRunLoadout()` fires on every new-run/zone-start path (`Restart()`, `RunStartUI.OnStartClicked()`, and any MetaScreen/WorldMap zone-advance that bypasses the picker).
- **D3 (dev verify)** — restored active weapon must equip AFTER the character model swap (`RunStartUI.Show()`); verify in play mode.

## Edge cases handled
First room (nothing to restore), death→Restart (clears), reaching boss (carries in), HUD refresh via OnChanged events, ProgressionSystem/GameManager/player Awake ordering.
