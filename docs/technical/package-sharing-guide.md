# Unity Shared Package Creation Plan

## Context

You want to share HUD components, Animation Controllers, and reusable scripts between **Unboxed Heroes** and a future project (**Hard Knocks Workshop**). Unity's proper solution for this is creating a **Unity Package Manager (UPM) custom package** — think of it like creating an npm package for Node.js or a Python package for pip, but for Unity.

**Why UPM is the right approach:**
- Native Unity integration (built-in package management system)
- Git-based distribution (no Asset Store overhead)
- Automatic dependency management
- Clean separation between shared code and project-specific content
- Version control with semantic versioning
- Live development mode with local `file:` references

**What we'll share:**
- Core systems: AudioManager, HitStopManager, SoundData, StatSystem
- UI components: HUD prefabs, HUDController_V2, HealthBar3D
- Animation controllers (generic player/enemy state machines)
- ScriptableObject definitions (WeaponData, WeaponAbilityData, BoxData)
- Input Action assets

**What stays project-specific:**
- GameManager, PlayerController, specific enemy AI
- Character models, environment prefabs, scenes
- ScriptableObject **instances** (e.g., `so_BoxData_Cowboy.asset`)
- Audio clips, textures, VFX

---

## Phase 1: Create Package Repository Structure

### 1.1 Create Git Repository
```bash
cd ~/Documents/personal
mkdir unboxed-heroes-shared
cd unboxed-heroes-shared
git init
```

### 1.2 Create Package Folder Structure
```
unboxed-heroes-shared/
├── package.json                          # Package manifest (metadata, dependencies)
├── README.md                             # Package documentation
├── CHANGELOG.md                          # Version history
├── Runtime/                              # Runtime code & assets
│   ├── Scripts/
│   │   ├── Core/                        # AudioManager, SoundData, HitStopManager
│   │   ├── UI/                          # HUDController_V2, HealthBar3D, BossHealthBar
│   │   ├── Systems/                     # StatSystem, WeaponData, WeaponAbilityData
│   │   ├── Player/                      # PlayerStats, WeaponHolder (reusable player components)
│   │   └── Enemy/                       # EnemyStats (generic enemy stats)
│   ├── Prefabs/
│   │   └── UI/                          # pfb_hud_v2.prefab and dependencies
│   ├── Animations/
│   │   └── Controllers/                 # AC_Player_V2.controller and generic controllers
│   ├── Input/                           # BoxHeadInputActions.inputactions
│   ├── Models/
│   │   └── HUD/                         # HUD 3D frame models (healthbar, chargemeter, etc.)
│   ├── Materials/                       # Shared materials (mat_healthbar_fill, HUD materials)
│   ├── Art/
│   │   └── UI/                          # UI textures and sprites
│   └── UnboxedHeroes.Shared.Runtime.asmdef  # Assembly definition
├── Editor/                              # Editor-only scripts
│   ├── Scripts/
│   │   └── WeaponHolderEditor.cs        # Custom inspector
│   └── UnboxedHeroes.Shared.Editor.asmdef
└── .gitignore                           # Ignore .meta files during development
```

### 1.3 Create `package.json`
```json
{
  "name": "com.skyionblue.unboxedheroes.shared",
  "version": "0.1.0",
  "displayName": "Unboxed Heroes - Shared Components",
  "description": "Shared systems, UI components, and gameplay scripts for Unboxed Heroes and related projects. Includes AudioManager, StatSystem, HUD prefabs, animation controllers, and ScriptableObject definitions.",
  "unity": "6000.0",
  "keywords": [
    "unboxed-heroes",
    "shared",
    "core-systems",
    "ui",
    "animation",
    "audio"
  ],
  "author": {
    "name": "Louie Celli",
    "url": "https://github.com/skyionblue"
  },
  "dependencies": {
    "com.unity.inputsystem": "1.11.0",
    "com.unity.ugui": "2.0.0",
    "com.unity.textmeshpro": "3.0.0",
    "com.unity.modules.physics": "1.0.0",
    "com.unity.modules.animation": "1.0.0"
  }
}
```

### 1.4 Create Assembly Definition Files

**`Runtime/UnboxedHeroes.Shared.Runtime.asmdef`:**
```json
{
    "name": "UnboxedHeroes.Shared.Runtime",
    "rootNamespace": "Boxhead",
    "references": [
        "Unity.InputSystem",
        "Unity.TextMeshPro"
    ],
    "includePlatforms": [],
    "excludePlatforms": [],
    "allowUnsafeCode": false,
    "overrideReferences": false,
    "precompiledReferences": [],
    "autoReferenced": true,
    "defineConstraints": [],
    "versionDefines": [],
    "noEngineReferences": false
}
```

**`Editor/UnboxedHeroes.Shared.Editor.asmdef`:**
```json
{
    "name": "UnboxedHeroes.Shared.Editor",
    "rootNamespace": "Boxhead.Editor",
    "references": [
        "UnboxedHeroes.Shared.Runtime"
    ],
    "includePlatforms": [
        "Editor"
    ],
    "excludePlatforms": [],
    "allowUnsafeCode": false,
    "overrideReferences": false,
    "precompiledReferences": [],
    "autoReferenced": true,
    "defineConstraints": [],
    "versionDefines": [],
    "noEngineReferences": false
}
```

### 1.5 Create Documentation Files

**`README.md`:**
```markdown
# Unboxed Heroes - Shared Components

Shared systems, UI components, and gameplay scripts for Unboxed Heroes and related projects.

## Features

- **Audio System**: AudioManager singleton with event-driven sound playback
- **Stat System**: Generic modifier-based stat system for gameplay attributes
- **HUD Components**: 3D world-space HUD with health bars, special ability UI
- **Animation Controllers**: Generic player and enemy animation state machines
- **Input System**: Configured Input Actions for mobile and gamepad
- **ScriptableObject Definitions**: WeaponData, BoxData, SoundData

## Installation

Add to your project's `Packages/manifest.json`:

```json
{
  "dependencies": {
    "com.skyionblue.unboxedheroes.shared": "https://github.com/skyionblue/unboxed-heroes-shared.git#main"
  }
}
```

## Requirements

- Unity 6000.0 or later
- Unity Input System package
- TextMesh Pro
- URP (Universal Render Pipeline)

## Usage

See individual script documentation for API details.
```

**`CHANGELOG.md`:**
```markdown
# Changelog

All notable changes to this package will be documented in this file.

## [0.1.0] - 2026-07-22

### Added
- Initial package structure
- Core systems: AudioManager, SoundData, HitStopManager, CameraStackWirer
- Stat system (Stat, Modifier, ModifierOperations)
- UI components: HUDController_V2, HealthBar3D, BossHealthBar
- Player components: PlayerStats, WeaponHolder
- Enemy components: EnemyStats
- ScriptableObject definitions: WeaponData, WeaponAbilityData, BoxData
- Animation controllers: AC_Player_V2
- Input Action asset: BoxHeadInputActions
- HUD prefab: pfb_hud_v2
- Editor utilities: WeaponHolderEditor
```

### 1.6 Create `.gitignore`
```gitignore
# Unity-generated meta files will be handled per-project
# Package itself should not commit .meta files during development
*.meta

# OS files
.DS_Store
Thumbs.db
```

**Why ignore `.meta` files during development:** Unity generates `.meta` files with GUIDs when it imports assets. For packages, Unity regenerates these on import into each project, so we don't commit them in the package repo itself. Once the package is stable, we'll commit `.meta` files for GUID stability.

---

## Phase 2: Extract Core Systems from Unboxed Heroes

**Extraction strategy:** Copy files from `Assets/_Project/` into the package's `Runtime/` folder. We'll use `file:` protocol for live development, so changes in the package immediately reflect in Unity.

### 2.1 Copy Core Systems Scripts

**From Unboxed Heroes → To Package:**

```
Assets/_Project/Scripts/Core/AudioManager.cs → Runtime/Scripts/Core/AudioManager.cs
Assets/_Project/Scripts/Core/SoundData.cs → Runtime/Scripts/Core/SoundData.cs
Assets/_Project/Scripts/Core/HitStopManager.cs → Runtime/Scripts/Core/HitStopManager.cs
Assets/_Project/Scripts/Core/CameraStackWirer.cs → Runtime/Scripts/Core/CameraStackWirer.cs

Assets/_Project/Scripts/Systems/StatSystem/ (entire folder) → Runtime/Scripts/Systems/StatSystem/
Assets/_Project/Scripts/Systems/WeaponData.cs → Runtime/Scripts/Systems/WeaponData.cs
Assets/_Project/Scripts/Systems/WeaponAbilityData.cs → Runtime/Scripts/Systems/WeaponAbilityData.cs
Assets/_Project/Scripts/Systems/BoxData.cs → Runtime/Scripts/Systems/BoxData.cs
Assets/_Project/Scripts/Systems/AbilityActivationContext.cs → Runtime/Scripts/Systems/AbilityActivationContext.cs

Assets/_Project/Scripts/Player/PlayerStats.cs → Runtime/Scripts/Player/PlayerStats.cs
Assets/_Project/Scripts/Player/WeaponHolder.cs → Runtime/Scripts/Player/WeaponHolder.cs

Assets/_Project/Scripts/Enemy/EnemyStats.cs → Runtime/Scripts/Enemy/EnemyStats.cs

Assets/_Project/Scripts/UI/HUDController_V2.cs → Runtime/Scripts/UI/HUDController_V2.cs
Assets/_Project/Scripts/UI/HealthBar3D.cs → Runtime/Scripts/UI/HealthBar3D.cs
Assets/_Project/Scripts/UI/BossHealthBar.cs → Runtime/Scripts/UI/BossHealthBar.cs
Assets/_Project/Scripts/UI/HUDCameraInjector.cs → Runtime/Scripts/UI/HUDCameraInjector.cs

Assets/_Project/Scripts/Editor/WeaponHolderEditor.cs → Editor/Scripts/WeaponHolderEditor.cs
```

**Total files: ~25 C# scripts**

### 2.2 Copy Prefabs and Assets

```
Assets/_Project/Prefabs/UI/pfb_hud_v2.prefab → Runtime/Prefabs/UI/pfb_hud_v2.prefab

Assets/_Project/Animations/Controllers/AC_Player_V2.controller → Runtime/Animations/Controllers/AC_Player_V2.controller
Assets/_Project/Animations/UpperBodyAttackMask.asset → Runtime/Animations/UpperBodyAttackMask.asset

Assets/_Project/Input/BoxHeadInputActions.inputactions → Runtime/Input/BoxHeadInputActions.inputactions
Assets/_Project/Input/BoxHeadInputActions.cs → Runtime/Input/BoxHeadInputActions.cs

Assets/_Project/Models/HUD/ (entire folder) → Runtime/Models/HUD/
  - ui_hud_healthbar_frame/
  - ui_hud_chargemeter_frame/
  - ui_hud_ipcounter_frame/
  - ui_hud_ipcounter/
  - ui_hud_styleicon_frame/

Assets/_Project/Materials/mat_healthbar_fill.mat → Runtime/Materials/mat_healthbar_fill.mat

Assets/_Project/Art/UI/ (entire folder) → Runtime/Art/UI/
  - cardboard_box_switch_bg.png
  - cardboard_pause_bg.png
  - cardboard_strip.png
  - crayon_fill.png
  - portrait_cowboy.png
  - portrait_ninja.png
  - rounded_button.png

Assets/_Project/UI/Icons/ (entire folder) → Runtime/UI/Icons/
Assets/_Project/UI/Sprites/ (entire folder) → Runtime/UI/Sprites/
Assets/_Project/UI/Textures/ (entire folder) → Runtime/UI/Textures/
```

**Important:** Copy prefabs and their dependencies together. Unity prefabs reference GUIDs of scripts/materials/textures — if dependencies are missing, prefabs will have pink "Missing Script" warnings.

### 2.3 Generate `.meta` Files in Package

After copying files to the package folder:
1. Open Unity Editor with Unboxed Heroes project
2. In Project window, right-click `Assets/` → Reimport All
3. Unity will generate `.meta` files for all copied assets in the package
4. Copy those `.meta` files from Unity's `Library/PackageCache/` back to the package repo (if using local `file:` reference, they're already there)

---

## Phase 3: Configure Unboxed Heroes to Use Package

### 3.1 Add Package Reference

**Edit `UnboxedHeroes/UnboxedHeroes/Packages/manifest.json`:**

Add this line to the `dependencies` object:
```json
{
  "dependencies": {
    "com.skyionblue.unboxedheroes.shared": "file:../../../unboxed-heroes-shared",
    "com.unity.collab-proxy": "2.6.0",
    ...other existing packages...
  }
}
```

**Path explanation:** Relative to `UnboxedHeroes/UnboxedHeroes/Packages/manifest.json`:
- `../` = up to `UnboxedHeroes/UnboxedHeroes/`
- `../../` = up to `UnboxedHeroes/`
- `../../../` = up to `boxhead/`
- `../../../unboxed-heroes-shared` = `boxhead/../unboxed-heroes-shared`

This creates a **live link** — edits in the package folder immediately reflect in Unity without Git commits.

### 3.2 Unity Reimport

1. Save `manifest.json`
2. Unity will auto-detect the change and reimport
3. Check Unity Console for errors
4. The package should appear in Window → Package Manager under "In Project"

### 3.3 Fix Broken References

**Potential issues:**
- **Missing scripts on prefabs:** Delete the missing script component, re-add from package
- **ScriptableObject instances:** Don't move SO instances (e.g., `so_BoxData_Cowboy.asset`) — only the class definitions
- **Material references:** Ensure materials in the package reference textures also in the package

**How to fix:**
1. Search Project window for "Missing Script" or pink objects
2. Select each broken prefab
3. In Inspector, delete missing script references
4. Re-add scripts from package (they'll be under `Packages/com.skyionblue.unboxedheroes.shared/Runtime/Scripts/`)
5. Reconfigure serialized fields if needed

### 3.4 Update Assembly References (if needed)

If your project has custom `.asmdef` files (e.g., `Boxhead.Runtime.asmdef`), add a reference to the package assembly:

```json
{
    "name": "Boxhead.Runtime",
    "references": [
        "UnboxedHeroes.Shared.Runtime"
    ],
    ...
}
```

This tells Unity that project scripts can use package scripts.

---

## Phase 4: Verify Shared Package Works in Unboxed Heroes

### 4.1 Manual Testing Checklist

**Test in Play Mode:**
- [ ] HUD displays correctly (health bar, IP counter, special ability button)
- [ ] AudioManager plays sounds (test attack/hit sounds)
- [ ] Player animation controller transitions work (idle → walk → run → attack)
- [ ] HealthBar3D renders on enemies
- [ ] Input Actions respond (movement, attack, dodge, parry)
- [ ] StatSystem modifiers apply correctly (test damage calculation)
- [ ] WeaponHolder equips/unequips weapons visually

**Test in Editor:**
- [ ] No console errors about missing scripts
- [ ] Prefabs show no pink "Missing" components
- [ ] Custom inspectors work (WeaponHolderEditor)
- [ ] ScriptableObject assets reference package classes correctly

### 4.2 Compile Check

Run these Unity menu commands to verify compilation:
- **Assets → Reimport All** (force full re-import)
- **Assets → Refresh** (quick reimport)
- Check Console for any red compilation errors

---

## Phase 5: Publish Package to Git Repository

### 5.1 Commit Package Files

```bash
cd ~/Documents/personal/unboxed-heroes-shared
git add .
git commit -m "Initial package structure with core systems, UI, and animation controllers"
```

### 5.2 Create GitHub Repository

```bash
# On GitHub, create new repo: unboxed-heroes-shared (private or public)
git remote add origin git@github.com:skyionblue/unboxed-heroes-shared.git
git branch -M main
git push -u origin main
```

### 5.3 Tag Initial Release

```bash
git tag v0.1.0 -m "Initial release: core systems, HUD, animations"
git push --tags
```

### 5.4 Update Unboxed Heroes to Use Git Package

**Edit `UnboxedHeroes/UnboxedHeroes/Packages/manifest.json`:**

Change the package reference from `file:` to GitHub URL:
```json
{
  "dependencies": {
    "com.skyionblue.unboxedheroes.shared": "https://github.com/skyionblue/unboxed-heroes-shared.git#v0.1.0",
    ...
  }
}
```

**Git URL formats:**
- `#main` — always uses latest main branch (for active development)
- `#v0.1.0` — locks to specific version tag (for production)
- `#develop` — tracks develop branch (for pre-release testing)

Unity will clone the repo into `Library/PackageCache/` and import it like any other package.

---

## Phase 6: Use Package in Hard Knocks Workshop (New Project)

### 6.1 Create New Unity Project

1. Open Unity Hub
2. Create new project: "Hard Knocks Workshop"
3. Template: 3D URP
4. Unity version: 6000.0.x (same as Unboxed Heroes)

### 6.2 Add Shared Package

**Edit `HardKnocksWorkshop/Packages/manifest.json`:**

```json
{
  "dependencies": {
    "com.skyionblue.unboxedheroes.shared": "https://github.com/skyionblue/unboxed-heroes-shared.git#v0.1.0",
    "com.unity.inputsystem": "1.11.0",
    "com.unity.textmeshpro": "3.0.0",
    ...
  }
}
```

Unity will automatically:
1. Clone the package repo
2. Read `package.json` dependencies
3. Install required packages (Input System, TextMesh Pro)
4. Compile scripts

### 6.3 Test Shared Systems

**Create test scene:**
1. Drag `pfb_hud_v2.prefab` into scene
2. Create empty GameObject → Add `AudioManager` component
3. Create test character → Add `PlayerStats` component
4. Wire HUD to PlayerStats events
5. Enter Play Mode → verify HUD updates

---

## Versioning Strategy

### Semantic Versioning

Use `MAJOR.MINOR.PATCH` format:
- **PATCH** (0.1.0 → 0.1.1): Bug fixes, no API changes
- **MINOR** (0.1.0 → 0.2.0): New features, backward compatible
- **MAJOR** (0.1.0 → 1.0.0): Breaking changes (API changes, namespace changes)

### Update Workflow

**When making changes to the package:**

1. **Local development** (use `file:` reference):
   ```json
   "com.skyionblue.unboxedheroes.shared": "file:../../unboxed-heroes-shared"
   ```
   Edit package files → Unity auto-reimports → test immediately

2. **Commit and tag release:**
   ```bash
   cd ~/Documents/personal/unboxed-heroes-shared
   git add .
   git commit -m "Add new feature X"
   # Bump version in package.json first!
   git tag v0.2.0 -m "Add feature X"
   git push && git push --tags
   ```

3. **Update projects to new version:**
   ```json
   "com.skyionblue.unboxedheroes.shared": "https://github.com/skyionblue/unboxed-heroes-shared.git#v0.2.0"
   ```

### Version Locking Strategy

**For production projects (Unboxed Heroes on TestFlight):**
- Lock to specific tag: `#v1.0.0`
- Only update when explicitly ready (prevents surprise breakage)

**For active development (Hard Knocks Workshop):**
- Track `main` branch: `#main`
- Get latest features automatically

---

## File Checklist

### Files to Create (in package repo)

- [ ] `package.json` (package manifest)
- [ ] `README.md` (documentation)
- [ ] `CHANGELOG.md` (version history)
- [ ] `Runtime/UnboxedHeroes.Shared.Runtime.asmdef` (assembly definition)
- [ ] `Editor/UnboxedHeroes.Shared.Editor.asmdef` (editor assembly)
- [ ] `.gitignore` (ignore .meta during development)

### Files to Copy (from Unboxed Heroes)

**Scripts (25 files):**
- [ ] Core: AudioManager, SoundData, HitStopManager, CameraStackWirer
- [ ] Systems: StatSystem folder, WeaponData, WeaponAbilityData, BoxData, AbilityActivationContext
- [ ] Player: PlayerStats, WeaponHolder
- [ ] Enemy: EnemyStats
- [ ] UI: HUDController_V2, HealthBar3D, BossHealthBar, HUDCameraInjector
- [ ] Editor: WeaponHolderEditor

**Prefabs (1 file):**
- [ ] pfb_hud_v2.prefab

**Animations (2 files):**
- [ ] AC_Player_V2.controller
- [ ] UpperBodyAttackMask.asset

**Input (2 files):**
- [ ] BoxHeadInputActions.inputactions
- [ ] BoxHeadInputActions.cs

**Models (5 folders):**
- [ ] HUD/ui_hud_healthbar_frame/
- [ ] HUD/ui_hud_chargemeter_frame/
- [ ] HUD/ui_hud_ipcounter_frame/
- [ ] HUD/ui_hud_ipcounter/
- [ ] HUD/ui_hud_styleicon_frame/

**Materials (1 file):**
- [ ] mat_healthbar_fill.mat

**Art (7 files):**
- [ ] cardboard_box_switch_bg.png
- [ ] cardboard_pause_bg.png
- [ ] cardboard_strip.png
- [ ] crayon_fill.png
- [ ] portrait_cowboy.png
- [ ] portrait_ninja.png
- [ ] rounded_button.png

**UI Assets (3 folders):**
- [ ] UI/Icons/ (HUD action icons)
- [ ] UI/Sprites/ (loading screen, special button)
- [ ] UI/Textures/ (main screen art)

### Files to Edit (in Unboxed Heroes)

- [ ] `Packages/manifest.json` (add package reference)
- [ ] Fix broken prefab references (if any)
- [ ] Update assembly references (if custom .asmdef exists)

---

## Verification Steps

### After Phase 3 (Local Package in Unboxed Heroes)

1. **Open Unboxed Heroes in Unity**
2. **Check Package Manager:** Window → Package Manager → In Project → "Unboxed Heroes - Shared Components" should appear
3. **Check Console:** No red compilation errors
4. **Play Mode Test:** Run main scene, verify HUD/audio/combat work
5. **Inspector Test:** Open prefabs, verify no pink "Missing Script" warnings

### After Phase 5 (Git Package)

1. **Clone Unboxed Heroes to new location** (simulate fresh checkout)
2. **Open in Unity** → package should auto-install from GitHub
3. **Play Mode Test:** Verify everything still works
4. **Build Test:** Try building for iOS/Android — package code should compile into build

### After Phase 6 (New Project)

1. **Create minimal test scene** in Hard Knocks Workshop
2. **Drag HUD prefab into scene**
3. **Create test character with PlayerStats**
4. **Play Mode:** Verify HUD displays and updates
5. **Audio Test:** Play sound via AudioManager.Instance.Play(SoundEvent)

---

## Common Issues and Solutions

### Issue: "The type or namespace 'Boxhead' could not be found"
**Cause:** Assembly reference missing
**Fix:** Add `"UnboxedHeroes.Shared.Runtime"` to project's `.asmdef` references

### Issue: Prefab shows pink "Missing Script"
**Cause:** Script moved to package but GUID changed
**Fix:** Delete missing script, re-add from package, reconfigure fields

### Issue: Package changes not updating in Unity
**Cause:** Unity caches package contents
**Fix:** Assets → Reimport All, or restart Unity
**For Git packages:** Unity caches at specific commit — must push changes and update `manifest.json`

### Issue: ScriptableObject instances show as missing
**Cause:** Moved SO instance file to package (should only move class definition)
**Fix:** Keep SO **instances** in project (`so_BoxData_Cowboy.asset`), only move class (`BoxData.cs`) to package

### Issue: Circular dependency error
**Cause:** Package references project-specific code
**Fix:** Package should **never** reference project code — one-way dependency only (Project → Package)

---

## Next Steps After Implementation

### Short Term (v0.2.0)
- Add XML doc comments to public APIs for IntelliSense
- Create example scene showing how to wire HUD to a character
- Add unit tests for StatSystem

### Medium Term (v0.5.0)
- Split into multiple packages if it grows large (`core`, `ui`, `audio`)
- Add more generic animation controllers (enemy state machines)
- Create editor tools for quick HUD setup

### Long Term (v1.0.0)
- Publish to Asset Store (optional, if you want public distribution)
- Generate API documentation from XML comments
- Add CI/CD pipeline for automated testing

---

## Summary

**What we're building:**
A Unity Package Manager (UPM) custom package containing all reusable components from Unboxed Heroes — HUD, animation controllers, core systems (AudioManager, StatSystem), ScriptableObject definitions, and Input Actions.

**Why this approach:**
- Native Unity package management (like npm for Node.js)
- Git-based distribution (no Asset Store overhead)
- Live development with `file:` protocol
- Version control with semantic versioning
- Clean separation: shared code in package, project-specific content in Assets/

**Critical files:**
- `package.json` — package manifest (metadata, dependencies)
- `.asmdef` files — Unity assembly definitions (compilation control)
- `Packages/manifest.json` (in Unboxed Heroes) — declares package dependency

**Workflow:**
1. Create package repo with folder structure
2. Copy scripts/prefabs/assets from Unboxed Heroes
3. Configure Unboxed Heroes to use package via `file:` reference
4. Test thoroughly in Play Mode
5. Commit package to Git, tag release
6. Update Unboxed Heroes to use GitHub URL
7. Use same package in Hard Knocks Workshop

**Result:**
Both projects share the same HUD, animation controllers, and core systems — update the package once, both projects get the fix. No more copy-paste, no divergence, proper version control.
