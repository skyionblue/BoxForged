# BoxForged — Asset Migration Guide

This document lists every custom asset that needs to move from the current Unity project into a new one. Scenes are excluded — they were test/verification scenes and are not being migrated.

**Source Unity project root:** `boxhead/BoxForged/`
**Custom assets live under:** `Assets/_Project/`

---

## Migration Strategy

The safest approach is to **copy `Assets/_Project/` wholesale** into the new project. This preserves all Unity GUIDs so cross-references between prefabs, materials, and ScriptableObjects remain intact. After copying:

1. Reinstall packages via `Packages/manifest.json` (see [Packages](#packages) below)
2. Reimport the Asset Store packages (see [Asset Store Packages](#asset-store-packages) below)
3. Do **not** copy the `Assets/_Project/Scenes/` folder

---

## Prefabs

### Characters (`Prefabs/Characters/`)
- `pfb_char_cowboy.prefab`
- `pfb_char_cowgirl.prefab`
- `pfb_char_ninjafemale.prefab`
- `pfb_char_ninjamale.prefab`

### Core Systems (`Prefabs/Core/`)
- `pfb_AudioManager.prefab`
- `pfb_CM_FollowCam.prefab`
- `pfb_DifficultyManager.prefab`
- `pfb_EnemySpawner.prefab`
- `pfb_GameManager.prefab`
- `pfb_Main_Camera.prefab`
- `pfb_MinimapCamera.prefab`
- `pfb_ProgressionSystem.prefab`
- `pfb_RoomManager.prefab`
- `pfb_SaveSystem.prefab`

### Enemies (`Prefabs/Enemies/`)
- `NoticePusher.prefab`
- `PermitPulper.prefab`
- `pfb_enemy_gnome_grunt.prefab`
- `pfb_enemy_milepost_marshal.prefab`
- `pfb_enemy_skeptic_grunt.prefab`
- `pfb_enemy_spincycle.prefab`
- `pfb_enemy_sprinkler_sentinel.prefab`
- `pfb_enemy_wagonwheel_roller.prefab`
- `pfb_projectile_clothesball.prefab`
- `pfb_projectile_sudsblob.prefab`
- `pfb_spincycle_export.prefab`
- `pfb_waterBurst.prefab`
- `pfb_waterBurstImpact.prefab`

### Environment (`Prefabs/ENV/`)
- `pfb_env_archery_target.prefab`
- `pfb_env_archery_target_arrows.prefab`
- `pfb_env_backyard_sign.prefab`
- `pfb_env_bld_horstiebench.prefab`
- `pfb_env_bld_porchcabin.prefab`
- `pfb_env_bld_shedwithcrate.prefab`
- `pfb_env_bld_twostoryhouse.prefab`
- `pfb_env_cardboard_sword.prefab`
- `pfb_env_cherry_blossom_tree.prefab`
- `pfb_env_command_node_birdbath.prefab`
- `pfb_env_covered_wagon.prefab`
- `pfb_env_crayon_box.prefab`
- `pfb_env_gallows_frame.prefab`
- `pfb_env_grass_bush_patches.prefab`
- `pfb_env_heroes_dog_house.prefab`
- `pfb_env_hitching_post.prefab`
- `pfb_env_lamp_post_western.prefab`
- `pfb_env_mailbox_telegraph.prefab`
- `pfb_env_pink_flower.prefab`
- `pfb_env_rain_barrel.prefab`
- `pfb_env_saloon_facade.prefab`
- `pfb_env_saloon_sign_board.prefab`
- `pfb_env_stacked_crates.prefab`
- `pfb_env_stepping_stone_tile.prefab`
- `pfb_env_stone_lantern.prefab`
- `pfb_env_target_dummy.prefab`
- `pfb_env_torii_gate.prefab`
- `pfb_env_traffic_cone.prefab`
- `pfb_env_train_sign.prefab`
- `pfb_env_treehouse_platform.prefab`
- `pfb_env_tumbleweed_static.prefab`
- `pfb_env_wanted_poster_blank.prefab`
- `pfb_env_water_trough.prefab`
- `pfb_env_weapon_rack.prefab`

### Player (`Prefabs/Player/`)
- `pfb_player.prefab`

### UI (`Prefabs/UI/`)
- `pfb_GameOverUI.prefab`
- `pfb_hud_v2.prefab`
- `pfb_MetaScreen.prefab`
- `pfb_run_end_screen.prefab`
- `pfb_shop_screen.prefab`
- `pfb_upgrade_screen.prefab`
- `pfb_world_map.prefab`

### VFX (`Prefabs/VFX/`)
- `ExplosionVFX.prefab`
- `MuzzleFlash.prefab`

### Weapons (`Prefabs/Weapons/`)
- `DynamiteProjectile.prefab`
- `pfb_cardboard_sword.prefab`
- `pfb_shuriken_projectile.prefab`
- `pfb_tumbleshot_bullet.prefab`
- `pfb_wpn_bostaff.prefab`
- `pfb_wpn_broomstick.prefab`
- `pfb_wpn_cardboardsword_v2.prefab`
- `pfb_wpn_cardboardtube.prefab`
- `pfb_wpn_dynamitebundle.prefab`
- `pfb_wpn_flashlight.prefab`
- `pfb_wpn_foamsword.prefab`
- `pfb_wpn_gardenhose.prefab`
- `pfb_wpn_katana.prefab`
- `pfb_wpn_lasso.prefab`
- `pfb_wpn_lightsaber.prefab`
- `pfb_wpn_lunchboxshield.prefab`
- `pfb_wpn_magicwand.prefab`
- `pfb_wpn_paddle.prefab`
- `pfb_wpn_pressurecannon.prefab`
- `pfb_wpn_quickdrawblade.prefab`
- `pfb_wpn_ruler.prefab`
- `pfb_wpn_shuriken.prefab`
- `pfb_wpn_sixshooter.prefab`
- `SixShooterBullet.prefab`

### Weapon Pickups (`Prefabs/Weapons/Pickups/`)
- `pfb_pickup_bostaff.prefab`
- `pfb_pickup_broomstick.prefab`
- `pfb_pickup_cardboardsword_v2.prefab`
- `pfb_pickup_cardboardtube.prefab`
- `pfb_pickup_dynamitebundle.prefab`
- `pfb_pickup_flashlight.prefab`
- `pfb_pickup_foamsword.prefab`
- `pfb_pickup_katana.prefab`
- `pfb_pickup_lasso.prefab`
- `pfb_pickup_lightsaber.prefab`
- `pfb_pickup_lunchboxshield.prefab`
- `pfb_pickup_magicwand.prefab`
- `pfb_pickup_paddle.prefab`
- `pfb_pickup_pressurecannon.prefab`
- `pfb_pickup_quickdrawblade.prefab`
- `pfb_pickup_ruler.prefab`
- `pfb_pickup_shuriken.prefab`
- `pfb_pickup_sixshooter.prefab`

---

## Scripts (`Scripts/`)

### Core
- `AudioManager.cs`
- `CameraFollowTargetInjector.cs`
- `CameraStackWirer.cs`
- `DifficultyData.cs`
- `DifficultyManager.cs`
- `FootstepReceiver.cs`
- `GameManager.cs`
- `HitStopManager.cs`
- `LevelData.cs`
- `ProgressionSystem.cs`
- `SaveData.cs`
- `SaveSystem.cs`
- `SaveTester.cs`
- `SoundData.cs`

### Player
- `AnimationEventReceiver.cs`
- `CharacterStatsSO.cs`
- `CombatController.cs`
- `FightingStyleData.cs`
- `FootstepReceiver.cs`
- `PlaceholderMover.cs`
- `PlayerController.cs`
- `PlayerStats.cs`
- `StatOverlay.cs`
- `TumbleshotBullet.cs`
- `WeaponCycler.cs`
- `WeaponEquipController.cs`
- `WeaponHolder.cs`

### Enemy
- `BasicEnemyAI.cs`
- `BossHeadBounce.cs`
- `BossIntroSequence.cs`
- `BossProjectile.cs`
- `DrumWindowRotator.cs`
- `EnemyHealthBar.cs`
- `EnemySpawner.cs`
- `EnemyStats.cs`
- `HitchingHoundAI.cs`
- `IEnemyBehavior.cs`
- `LaundryTumbler.cs`
- `MarshalBullet.cs`
- `MilepostMarshalAI.cs`
- `NoticePusherAI.cs`
- `NoticePusherPatrol.cs`
- `PermitPulperAI.cs`
- `PermitPulperBossAI.cs`
- `PermitPulperBossIntro.cs`
- `SkepticGruntAI.cs`
- `SpinCycleAI.cs`
- `SprinklerSentinelAI.cs`
- `WagonWheelRollerAI.cs`
- `WaterBurstReturn.cs`

### Systems
- `AbilityActivationContext.cs`
- `AutoDestroy.cs`
- `BossHallDoor.cs`
- `BossRoomWeaponSpawner.cs`
- `BoxData.cs`
- `BoxSystem.cs`
- `BuildingOcclusionFader.cs`
- `CameraOcclusion.cs`
- `DynamiteBundleAbilityData.cs`
- `DynamiteProjectile.cs`
- `EnemySpawnPoint.cs`
- `Inventory.cs`
- `LassoAbilityData.cs`
- `QuickdrawBladeAbilityData.cs`
- `RoomGate.cs`
- `RoomManager.cs`
- `RoomTrigger.cs`
- `SafeZone.cs`
- `ShurikenAbilityData.cs`
- `ShurikenProjectile.cs`
- `SixShooterAbilityData.cs`
- `SixShooterBullet.cs`
- `TumbleweedRoller.cs`
- `UpgradeCardData.cs`
- `WeaponAbilityData.cs`
- `WeaponData.cs`
- `WeaponPickup.cs`
- `WeaponPickupSpinner.cs`
- **StatSystem/** (7 files)
  - `IModifierOperations.cs`
  - `Modifier.cs`
  - `ModifierOperationsBase.cs`
  - `ModifiersCollection.cs`
  - `ModifierType.cs`
  - `Stat.cs`
  - `Stat.ModifierOperationsCollection.cs`
  - `ModifierOperations/ModifierOperations.cs`

### UI
- `BonusHealthBar3D.cs`
- `BossHealthBar.cs`
- `ChargeMeter3D.cs`
- `GameOverUI.cs`
- `HealthBar3D.cs`
- `HUD3DPositioner.cs`
- `HUDCameraInjector.cs`
- `HUDController.cs`
- `HUDController_V2.cs`
- `LoadingScreenController.cs`
- `MetaScreen.cs`
- `MinimapIndicator.cs`
- `OnScreenButtonFix.cs`
- `PauseMenu.cs`
- `RunEndScreen.cs`
- `RunStartUI.cs`
- `SafeAreaFitter.cs`
- `ShopScreen.cs`
- `SpinIcon.cs`
- `StyleBustSwapper.cs`
- `StyleSelectUI.cs`
- `UpgradeScreen.cs`
- `WeaponSlotUI.cs`
- `WorldMapScreen.cs`

### Editor (Editor-only, goes in `Scripts/Editor/`)
- `BuildConfigurator.cs`
- `iOSPostBuildProcessor.cs`
- `Sprint4SceneSetup.cs`
- `WeaponHolderEditor.cs`

### Root
- `BoxHeadInputActions.cs` (auto-generated from the input actions asset — will regenerate automatically once the `.inputactions` file is imported)

---

## ScriptableObjects (`ScriptableObjects/`)

### Characters
- `CharacterStats_Cowboy.asset`
- `CharacterStats_Ninja.asset`

### Abilities
- `DynamiteBundleAbility.asset`
- `ShurikenAbility.asset`
- `SixShooterAbility.asset`

### Difficulty
- `Easy.asset`
- `Medium.asset`
- `Hard.asset`

### Fighting Styles
- `FightingStyle_Cowboy.asset`
- `FightingStyle_Ninja.asset`

### Sound Data
- `SD_EnemyHit.asset`
- `SD_PlayerAttack.asset`
- `SD_PlayerDeath.asset`
- `SD_PlayerDodge.asset`
- `SD_PlayerHit.asset`
- `SD_PlayerJump.asset`

### Upgrades
- `AgileWarrior.asset`
- `CombatMedic.asset`
- `DoubleStrike.asset`
- `FieldMedic.asset`
- `Fortified.asset`
- `IronSkin.asset`
- `LuckyBreak.asset`
- `PowerStrike.asset`
- `QuickReload.asset`
- `SwiftFeet.asset`

### Weapons (base set)
- `WeaponData_CardboardSword.asset`
- `WeaponData_obj_bostaff_equipped.asset`
- `WeaponData_obj_broomstick_pickup.asset`
- `WeaponData_obj_cardboardsword_v2.asset`
- `WeaponData_obj_cardboardtube_pickup.asset`
- `WeaponData_obj_clothesball_equipped.asset`
- `WeaponData_obj_dynamitebundle_equipped.asset`
- `WeaponData_obj_flashlight_pickup.asset`
- `WeaponData_obj_foamsword_equipped.asset`
- `WeaponData_obj_gardenhose_pickup.asset`
- `WeaponData_obj_katana_equipped.asset`
- `WeaponData_obj_lasso_equipped.asset`
- `WeaponData_obj_lightsaber_equipped.asset`
- `WeaponData_obj_lunchboxshield_equipped.asset`
- `WeaponData_obj_magicwand_equipped.asset`
- `WeaponData_obj_paddle_equipped.asset`
- `WeaponData_obj_pressurecannon_equipped.asset`
- `WeaponData_obj_quickdrawblade_equipped.asset`
- `WeaponData_obj_ruler_pickup.asset`
- `WeaponData_obj_shuriken_equipped.asset`
- `WeaponData_obj_sixshooter_equipped.asset`
- `WeaponData_obj_sudsblob_equipped.asset`

### Weapons (NinjaFemale set — same names suffixed `_nf`)
19 assets mirroring the base set for NinjaFemale hold point offsets.

### Weapons (NinjaMale set — same names suffixed `_nm`)
19 assets mirroring the base set for NinjaMale hold point offsets.

### Misc
- `BoxData_Ninja.asset`
- `LevelData.asset`

---

## Animation Controllers (`Animations/Controllers/` and co-located with models)

- `Animations/Controllers/AC_Player_V2.controller`
- `Animations/Enemies/MilepostMarshal/AC_MilepostMarshal.controller`
- `Animations/Enemies/SpinCycle/AC_SpinCycle.controller`
- `Models/Characters/NoticePusher/AC_NoticePusher.controller`
- `Models/Characters/PermitPulper/AC_PermitPulper.controller`
- `Models/Enemies/GnomeGrunt/AC_GnomeGrunt.controller`
- `Models/Enemies/SkepticGrunt/AC_SkepticGrunt.controller`

---

## Input

- `Input/BoxHeadInputActions.inputactions`

---

## Materials (`Materials/`)

### Shared / Misc
- `BonusHealthBar.mat`
- `mat_cardboard_grey.mat`
- `mat_cardboard_natural.mat`
- `mat_cardboard_outline.mat`
- `mat_healthbar_fill.mat`
- `mat_marker_dark.mat`
- `mat_outline.mat`
- `MAT_ShadowDash.mat`
- `MAT_TumbleshotBullet.mat`
- `MAT_WeaponPickupBubble.mat`
- `OcclusionTransparentRef.mat`
- `SKY_DustyHaze.mat`

### Characters (`Materials/Characters/`)
- `MAT_SpinCycle_Body.mat`
- `MAT_SpinCycle_Head.mat`

### Enemies (`Materials/Enemies/`)
- `MAT_ClothesBall.mat`
- `MAT_MarshalDead.mat`
- `MAT_MarshalSlamIndicator.mat`
- `MAT_MarshalStunned.mat`
- `MAT_MilepostMarshal.mat`
- `MAT_SpinCycleBiped.mat`
- `MAT_SpinCycleBody.mat`
- `MAT_SpinCycleBodyMesh.mat`
- `MAT_SpinCycleHead.mat`
- `MAT_SprinklerBody.mat`
- `MAT_SprinklerDead.mat`
- `MAT_SprinklerLeg.mat`
- `MAT_SprinklerOverheat.mat`
- `MAT_SudsBlob.mat`
- `MAT_WaterBurst.mat`

### ENV (`Materials/ENV/`)
30 materials — one per ENV prop (e.g. `env_archery_target.mat`, `env_covered_wagon.mat`, etc.) plus 4 building materials (`HorseTieBench.mat`, `PorchCabin.mat`, `ShedWithCrate.mat`, `TwoStoryHouse.mat`).

### Props (`Materials/Props/`)
- `MAT_prop_archerytarget_v2.mat`
- `MAT_prop_backyardsign_v2.mat`
- `MAT_prop_cherryblossom_v2.mat`
- `MAT_prop_crayonbox_v2.mat`
- `MAT_prop_doghouse_v2.mat`
- `MAT_prop_grassbush_v2.mat`
- `MAT_prop_steppingstone_v2.mat`
- `MAT_prop_targetdummy_v2.mat`
- `MAT_prop_trafficcone_v2.mat`
- `MAT_prop_trainsign_v2.mat`
- `MAT_prop_treehouse_v2.mat`

### Weapons (`Materials/Weapons/`)
22 materials — one per weapon model (e.g. `MAT_obj_katana_equipped.mat`, `MAT_obj_sixshooter_equipped.mat`, etc.)

### UI (`Materials/UI/`)
Any materials in this subfolder.

### Co-located with Models
- `Models/Characters/Cowboy/MAT_Cowboy.mat`
- `Models/Characters/Cowgirl/MAT_Cowgirl.mat`
- `Models/Characters/Ninja/MAT_Ninja.mat`
- `Models/Characters/NinjaFemale/MAT_NinjaFemale.mat`
- `Models/Characters/NoticePusher/MAT_NoticePusher.mat`
- `Models/Characters/PermitPulper/MAT_PermitPulper.mat`
- `Models/Enemies/GnomeGrunt/MAT_GnomeGrunt.mat`
- `Models/Enemies/SkepticGrunt/MAT_SkepticGrunt.mat`
- `Models/Enemies/WagonWheelRoller/MAT_WagonWheelRoller.mat`
- `Models/HUD/ui_hud_chargemeter_frame/Mat_HUDChargeMeter.mat`
- `Models/HUD/ui_hud_healthbar_frame/Mat_HUDHealthbar.mat`
- `Models/HUD/ui_hud_ipcounter_frame/Mat_HUDIPCounter.mat`
- `Models/HUD/ui_hud_ipcounter/Mat_HUDIPTally.mat`
- `Models/HUD/ui_hud_styleicon_frame/Mat_HUDStyleIcon.mat`
- `Models/Weapons/CardboardSword/MAT_CardboardSword.mat`
- `Prefabs/VFX/MAT_MuzzleFlash.mat`
- `Prefabs/VFX/MAT_Smoke.mat`
- `Prefabs/VFX/MAT_Spark.mat`
- `Prefabs/Weapons/MAT_SixShooterBullet.mat`

---

## Models (`Models/`)

### Characters
- `Characters/Cowboy/` — FBX + textures
- `Characters/Cowgirl/` — FBX + textures
- `Characters/Ninja/` — FBX + textures
- `Characters/NinjaFemale/` — FBX + textures
- `Characters/NoticePusher/` — FBX + textures + `AC_NoticePusher.controller`
- `Characters/PermitPulper/` — FBX + textures + `AC_PermitPulper.controller`

### Enemies
- `Enemies/GnomeGrunt/` — FBX + textures + `AC_GnomeGrunt.controller`
- `Enemies/MilepostMarshal/` — FBX + textures
- `Enemies/SkepticGrunt/` — FBX + textures + `AC_SkepticGrunt.controller`
- `Enemies/SpinCycle/` — FBX + textures
- `Enemies/SprinklerSentinel/` — FBX + textures
- `Enemies/WagonWheelRoller/` — FBX + textures

### ENV Props (`Models/ENV/`)
30 prop folders — one per environmental asset (archery target, backyard sign, cherry blossom tree, command node birdbath, covered wagon, crayon box, gallows frame, grass/bush patches, dog house, hitching post, lamp post, mailbox/telegraph, pink flower, rain barrel, saloon facade, saloon sign, stacked crates, stepping stone tile, stone lantern, target dummy, torii gate, traffic cone, train sign, treehouse platform, tumbleweed, wanted poster, water trough, weapon rack, plus the cardboard sword and building models).

### HUD Models (`Models/HUD/`)
- `ui_hud_chargemeter_frame/`
- `ui_hud_healthbar_frame/`
- `ui_hud_ipcounter/`
- `ui_hud_ipcounter_frame/`
- `ui_hud_styleicon_frame/`

### Props (`Models/Props/`)
11 prop folders (v2 versions): archery target, backyard sign, cherry blossom, crayon box, dog house, grass/bush, stepping stone, target dummy, traffic cone, train sign, treehouse.

### Weapons (`Models/Weapons/`)
22 weapon model folders — one per weapon (bo staff, broomstick, cardboard sword, cardboard tube, clothes ball, dynamite bundle, flashlight, foam sword, garden hose, katana, lasso, lightsaber, lunchbox shield, magic wand, paddle, pressure cannon, quickdraw blade, ruler, shuriken, six shooter, suds blob, plus the SixShooterBullet folder).

---

## UI Assets (`UI/`)

- `UI/Icons/HUD/` — all HUD icon sprites
- `UI/Sprites/` — all UI sprites
- `UI/Textures/` — all UI textures

---

## Art Assets (`Art/`)

- `Art/Sprites/Weapons/` — weapon icon sprites
- `Art/UI/` — UI art assets

---

## Audio (`Audio/`)

- `Audio/SFX/Player/` — player sound effect clips

---

## Fonts (`Fonts/`)

- `PermanentMarker.asset` — TextMeshPro font asset

---

## Packages

Copy `Packages/manifest.json` to the new project, or manually add these via the Package Manager:

| Package | Version |
|---|---|
| `com.unity.cinemachine` | 3.1.7 |
| `com.unity.ai.navigation` | 2.0.14 |
| `com.unity.inputsystem` | 1.19.0 |
| `com.unity.render-pipelines.universal` | 17.5.0 |
| `com.unity.timeline` | 1.8.12 |
| `com.unity.ugui` | 2.5.0 |
| `com.unity.test-framework` | 1.7.0 |
| `com.unity.visualscripting` | 1.9.12 |

> **Note:** `com.coplaydev.unity-mcp` is the MCP for Unity dev tool — only needed if you want Unity MCP in the new project. It installs from a local path: `file:/Users/jcelli/Documents/tools/unity-mcp/MCPForUnity`

---

## Asset Store Packages

These are not in `_Project/` — they live in their own top-level folders under `Assets/`. Reimport them from the Unity Asset Store into the new project:

| Package | Folder |
|---|---|
| RPG Character Mecanim Animation Pack FREE (ExplosiveLLC) | `Assets/ExplosiveLLC/RPG Character Mecanim Animation Pack FREE/` |
| SuperCharacterController (ExplosiveLLC) | `Assets/ExplosiveLLC/SuperCharacterController/` |
| Low Poly Mega Pack — Polyworks (Off Axis Studios) | `Assets/Off Axis Studios/Polyworks/` |
| Polylised — Medieval Desert City | `Assets/Polylised - Medieval Desert City/` |
| SimpleTown | `Assets/SimpleTown/` |
| TextMesh Pro | Via Package Manager (built-in) |

---

## What to Exclude

- `Assets/_Project/Scenes/` — all scenes (test/verification only, not migrating)
- `Assets/Screenshots/` — editor screenshots
- `Assets/Scenes/` — any root-level scenes
- `Library/`, `Logs/`, `UserSettings/`, `build/` — auto-generated, do not copy
