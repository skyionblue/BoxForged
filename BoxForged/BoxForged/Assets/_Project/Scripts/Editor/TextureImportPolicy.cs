using System;
using UnityEditor;
using UnityEngine;

namespace Boxhead.Editor
{
    /// <summary>
    /// Enforces the project's per-category texture import policy
    /// (<c>docs/TECHNICAL_DESIGN.md</c> §3.4, required by <c>docs/adr/0005-world2-single-continuous-scene.md</c> §3
    /// as a prerequisite for new World 2 art). Runs automatically on every texture
    /// (re)import via <see cref="OnPreprocessTexture"/>.
    ///
    /// The defect this fixes: essentially every project texture imports at
    /// <c>maxTextureSize 2048</c> with no Android/iOS platform overrides
    /// (<c>textureFormat Auto</c>). At 2048² a compressed mipmapped texture costs
    /// ~2.5–5.6 MB; a scene drawing 40–60 distinct textures can land at
    /// 100–150 MB of texture memory, which on a 3–4-year-old device presents as
    /// thermal throttling partway through a 10–15 minute run rather than a crash.
    ///
    /// This is intentionally reversible and non-retroactive: it only affects
    /// textures that are newly imported or explicitly reimported. Existing textures
    /// keep whatever settings they already have until something triggers their
    /// reimport — mass-reimporting the ~350 existing project textures is a separate,
    /// larger, owner-reviewed decision and is explicitly not part of this change.
    ///
    /// Scope: only textures under <c>Assets/_Project/</c> are touched. Vendor/
    /// third-party content (e.g. <c>Assets/ExplosiveLLC</c>, <c>Assets/Hayq Art</c>,
    /// <c>Assets/TextMesh Pro</c>) and any package textures are left completely
    /// alone — this policy encodes BoxForged's own art categories and folder
    /// layout, not a project-wide default.
    ///
    /// IMPORTANT — Unity caveat discovered while building this: simply having an
    /// <c>AssetPostprocessor</c> that implements <see cref="OnPreprocessTexture"/>
    /// present in the project causes Unity to invalidate its cached import for
    /// EVERY texture of the affected type on the next asset-database refresh —
    /// there is no engine-level way to scope that invalidation to "only textures
    /// imported from now on." Left unconditional, that would silently reimport all
    /// ~350 existing project textures the moment this script compiles, which is
    /// exactly the mass-retroactive change this task was told NOT to make as a
    /// silent side effect. <see cref="Enabled"/> exists to prevent that: the policy
    /// only *applies* its settings when explicitly turned on (see the
    /// <c>BoxForged/Textures/</c> menu, mirroring <see cref="BuildConfigurator"/>'s
    /// convention). Off by default, a reimport pass is a no-op — importer values
    /// are left untouched, so existing <c>.meta</c> files do not change. Turning it
    /// on and running Reimport All is the deliberate, visible action that performs
    /// the larger retroactive pass, whenever the owner decides to do it.
    /// </summary>
    public class TextureImportPolicy : AssetPostprocessor
    {
        private const string EnabledPrefKey = "BoxForged.TextureImportPolicy.Enabled";

        /// <summary>
        /// Whether the policy actively applies caps/overrides on import. Off by
        /// default so adding this script does not retroactively touch existing
        /// textures — see the class remarks above. Toggle via the
        /// <c>BoxForged/Textures/</c> menu items below.
        /// </summary>
        public static bool Enabled
        {
            get => EditorPrefs.GetBool(EnabledPrefKey, false);
            private set => EditorPrefs.SetBool(EnabledPrefKey, value);
        }

        private const string MenuRoot = "BoxForged/Textures/";

        [MenuItem(MenuRoot + "Enable Texture Import Policy")]
        public static void EnablePolicy()
        {
            Enabled = true;
            Debug.Log("[TextureImportPolicy] Enabled. New/reimported textures under Assets/_Project/ " +
                       "will now get category caps + Android/iOS ASTC overrides on import. " +
                       "This does NOT reimport anything by itself — trigger Reimport as a separate, " +
                       "deliberate action (e.g. Assets → Reimport All) when ready.");
        }

        [MenuItem(MenuRoot + "Disable Texture Import Policy")]
        public static void DisablePolicy()
        {
            Enabled = false;
            Debug.Log("[TextureImportPolicy] Disabled. Texture imports are unaffected by this policy " +
                       "until re-enabled.");
        }

        [MenuItem(MenuRoot + "Show Policy Status")]
        public static void ShowStatus()
        {
            Debug.Log($"[TextureImportPolicy] Enabled = {Enabled}");
        }

        // ── Category caps (TDD §3.4) ────────────────────────────────────────────
        private const int CharacterMaxSize = 1024;   // Characters & bosses
        private const int WeaponMaxSize = 512;       // Weapons
        private const int EnvironmentMaxSize = 512;  // Environment props
        private const int UIMaxSize = 256;           // UI

        // ASTC block size per category. Smaller block = higher visual fidelity at a
        // larger file size; larger block = smaller file at lower fidelity. Characters,
        // weapons, and environment props are viewed in 3D gameplay at typical camera
        // distance and keep ASTC 6x6, Unity's general-purpose balanced default — 4x4
        // would double file size for a fidelity gain that isn't perceivable at this
        // project's camera distance (ADR-0001: ~9.4 m). UI art is flat/iconographic
        // (HUD frames, buttons, sprite icons) and tolerates the more aggressive 8x8
        // block without visible banding.
        private const TextureImporterFormat CharacterFormat = TextureImporterFormat.ASTC_6x6;
        private const TextureImporterFormat WeaponFormat = TextureImporterFormat.ASTC_6x6;
        private const TextureImporterFormat EnvironmentFormat = TextureImporterFormat.ASTC_6x6;
        private const TextureImporterFormat UIFormat = TextureImporterFormat.ASTC_8x8;

        // Matches the project's existing convention (every current texture .meta
        // already uses compressionQuality 50 / "Normal").
        private const int CompressionQuality = 50;

        private const string ProjectRoot = "Assets/_Project/";

        private enum Category
        {
            Character,
            Weapon,
            Environment,
            UI
        }

        private void OnPreprocessTexture()
        {
            // Off by default — see the class remarks on why this gate exists. When
            // disabled this is a true no-op: nothing on the importer is touched, so a
            // reimport pass forced by Unity (e.g. after this script itself compiles)
            // does not change any existing texture's settings.
            if (!Enabled) return;

            string path = assetPath.Replace('\\', '/');

            // Out of scope entirely — vendor/package content keeps whatever settings
            // it already has. Never globalize this policy onto content this project
            // does not own.
            if (!path.StartsWith(ProjectRoot, StringComparison.OrdinalIgnoreCase)) return;

            var importer = (TextureImporter)assetImporter;

            if (!TryClassify(path, out Category category))
            {
                Debug.LogWarning(
                    $"[TextureImportPolicy] Unrecognized texture path — defaulting to " +
                    $"Environment props (512, ASTC 6x6): {path}. Add a rule to " +
                    $"TryClassify if this is a real new category.");
                category = Category.Environment;
            }

            (int maxSize, TextureImporterFormat format) = category switch
            {
                Category.Character => (CharacterMaxSize, CharacterFormat),
                Category.Weapon => (WeaponMaxSize, WeaponFormat),
                Category.UI => (UIMaxSize, UIFormat),
                _ => (EnvironmentMaxSize, EnvironmentFormat),
            };

            // Default/fallback platform (also covers Standalone/WebGL/editor use).
            importer.maxTextureSize = maxSize;
            importer.textureCompression = TextureImporterCompression.Compressed;

            ApplyPlatformOverride(importer, "Android", maxSize, format);
            ApplyPlatformOverride(importer, "iOS", maxSize, format);
        }

        /// <summary>
        /// Classifies a texture by its import path against the categories established
        /// in <c>Assets/_Project/</c>'s actual folder layout. Order matters — rules are
        /// evaluated most-specific-first so an unambiguous 3D-model-category match
        /// always wins over a broader UI/environment catch-all.
        /// </summary>
        private static bool TryClassify(string path, out Category category)
        {
            // 1. Characters & bosses — player characters and enemies/bosses share a
            //    cap because both are rendered close, animated, and are the primary
            //    per-frame visual read.
            if (Contains(path, "/Models/Characters/") || Contains(path, "/Models/Enemies/"))
            {
                category = Category.Character;
                return true;
            }

            // 2. Weapons — 3D weapon model textures (equipped + pickup variants).
            if (Contains(path, "/Models/Weapons/"))
            {
                category = Category.Weapon;
                return true;
            }

            // 3. UI — anything under a folder literally named "UI" anywhere in the
            //    path (Art/UI, UI/Icons, UI/Sprites, UI/Textures, Materials/UI), the
            //    top-level 2D art folder (Art/Sprites/* — flat icon/sprite art, not
            //    3D model textures, even where a subfolder happens to be named after
            //    a weapon), Models/HUD (3D geometry authored for the HUD overlay —
            //    "ui_hud_*_frame" meshes — not world geometry, despite living under
            //    Models/), and Resources (currently only the fullscreen loading
            //    background). Checked before the Environment rule so Materials/UI/
            //    resolves here rather than falling into the generic Materials/ case.
            if (Contains(path, "/UI/") ||
                path.StartsWith(ProjectRoot + "Art/", StringComparison.OrdinalIgnoreCase) ||
                Contains(path, "/Models/HUD/") ||
                path.StartsWith(ProjectRoot + "Resources/", StringComparison.OrdinalIgnoreCase))
            {
                category = Category.UI;
                return true;
            }

            // 4. Environment props — world geometry, dressing, and ground/world
            //    materials (e.g. Materials/Tex_WesternDirt_Ground.png).
            if (Contains(path, "/Models/ENV/") ||
                Contains(path, "/Models/Environment/") ||
                Contains(path, "/Models/Props/") ||
                Contains(path, "/Materials/"))
            {
                category = Category.Environment;
                return true;
            }

            category = Category.Environment; // conservative default — see caller's warning
            return false;
        }

        private static void ApplyPlatformOverride(
            TextureImporter importer, string platform, int maxSize, TextureImporterFormat format)
        {
            TextureImporterPlatformSettings settings = importer.GetPlatformTextureSettings(platform);
            settings.overridden = true;
            settings.maxTextureSize = maxSize;
            settings.format = format;
            settings.textureCompression = TextureImporterCompression.Compressed;
            settings.compressionQuality = CompressionQuality;
            importer.SetPlatformTextureSettings(settings);
        }

        private static bool Contains(string path, string segment) =>
            path.IndexOf(segment, StringComparison.OrdinalIgnoreCase) >= 0;
    }
}
