using System;
using System.IO;
using System.Text;
using Thkim.DreamLaundromat.Gameplay.ReleaseSlice;
using UnityEditor;
using UnityEngine;

namespace Thkim.DreamLaundromat.Editor.ReleaseSlice
{
    public static class ReleaseUiArtGenerator
    {
        public const string CatalogPath = "Assets/_Project/ScriptableObjects/ReleaseUiArtCatalog.asset";
        public const string BackgroundDirectory = "Assets/_Project/Art/UI/Backgrounds";
        public const string CardDirectory = "Assets/_Project/Art/UI/Cards";
        public const string IconDirectory = "Assets/_Project/Art/UI/Icons";
        public const string EffectDirectory = "Assets/_Project/Art/UI/Effects";
        public const string SourceNotesDirectory = "Assets/_Project/Art/UI/SourceNotes";

        private const int IconSize = 256;
        private const int CardSize = 512;
        private const int BackgroundWidth = 1080;
        private const int BackgroundHeight = 1920;

        [MenuItem("DreamLaundromat/Release Slice/Generate UI Art Assets")]
        public static void GenerateFromMenu()
        {
            GenerateReleaseUiArtAssets();
        }

        public static void RunFromCommandLine()
        {
            GenerateReleaseUiArtAssets();
        }

        public static ReleaseUiArtCatalog GenerateReleaseUiArtAssets()
        {
            CreateFolders();

            Sprite gameplayBackground = LoadOrCreateSpriteAsset(
                $"{BackgroundDirectory}/release-gameplay-background.png",
                BackgroundWidth,
                BackgroundHeight,
                PaintGameplayBackground,
                100f);
            Sprite titleBackground = LoadOrCreateSpriteAsset(
                $"{BackgroundDirectory}/release-title-night-laundromat.png",
                BackgroundWidth,
                BackgroundHeight,
                PaintTitleBackground,
                100f);
            Sprite levelSelectBackground = LoadOrCreateSpriteAsset(
                $"{BackgroundDirectory}/release-level-select-order-board.png",
                BackgroundWidth,
                BackgroundHeight,
                PaintLevelSelectBackground,
                100f);
            Sprite dreamCardFrame = LoadOrCreateSpriteAsset(
                $"{CardDirectory}/release-dream-card-frame.png",
                CardSize,
                CardSize,
                (u, v) => PaintFrame(u, v, new Color(0.13f, 0.18f, 0.22f, 0.92f), new Color(0.48f, 0.74f, 0.82f, 1f), new Color(0.74f, 0.95f, 1f, 1f)),
                128f);
            Sprite orderSheetFrame = LoadOrCreateSpriteAsset(
                $"{CardDirectory}/release-order-sheet-frame.png",
                CardSize,
                CardSize,
                (u, v) => PaintFrame(u, v, new Color(0.17f, 0.22f, 0.18f, 0.94f), new Color(0.68f, 0.84f, 0.58f, 1f), new Color(0.95f, 1f, 0.8f, 1f)),
                128f);
            Sprite storageShelfFrame = LoadOrCreateSpriteAsset(
                $"{CardDirectory}/release-storage-shelf-frame.png",
                CardSize,
                CardSize,
                (u, v) => PaintFrame(u, v, new Color(0.16f, 0.17f, 0.22f, 0.94f), new Color(0.72f, 0.66f, 0.52f, 1f), new Color(1f, 0.86f, 0.58f, 1f)),
                128f);
            Sprite operationButtonFrame = LoadOrCreateSpriteAsset(
                $"{CardDirectory}/release-operation-button-frame.png",
                CardSize,
                CardSize,
                (u, v) => PaintFrame(u, v, new Color(0.08f, 0.11f, 0.15f, 0.96f), new Color(0.42f, 0.78f, 0.88f, 1f), new Color(0.74f, 0.9f, 1f, 1f)),
                128f);
            Sprite submitButtonFrame = LoadOrCreateSpriteAsset(
                $"{CardDirectory}/release-submit-button-frame.png",
                CardSize,
                CardSize,
                (u, v) => PaintFrame(u, v, new Color(0.09f, 0.15f, 0.12f, 0.96f), new Color(0.52f, 0.82f, 0.5f, 1f), new Color(0.9f, 1f, 0.68f, 1f)),
                128f);
            Sprite storageActionFrame = LoadOrCreateSpriteAsset(
                $"{CardDirectory}/release-storage-action-frame.png",
                CardSize,
                CardSize,
                (u, v) => PaintFrame(u, v, new Color(0.12f, 0.12f, 0.16f, 0.96f), new Color(0.74f, 0.62f, 0.42f, 1f), new Color(1f, 0.82f, 0.48f, 1f)),
                128f);
            Sprite navigationButtonFrame = LoadOrCreateSpriteAsset(
                $"{CardDirectory}/release-navigation-button-frame.png",
                CardSize,
                CardSize,
                (u, v) => PaintFrame(u, v, new Color(0.08f, 0.1f, 0.13f, 0.96f), new Color(0.36f, 0.48f, 0.66f, 1f), new Color(0.74f, 0.84f, 1f, 1f)),
                128f);

            Sprite stateTaintClean = CreateIconSprite("release-state-taint-clean.png", ReleaseGlyph.Clean, new Color(0.12f, 0.43f, 0.48f, 1f), new Color(0.62f, 0.96f, 1f, 1f));
            Sprite stateTaintNightmare = CreateIconSprite("release-state-taint-nightmare.png", ReleaseGlyph.Nightmare, new Color(0.28f, 0.15f, 0.44f, 1f), new Color(0.75f, 0.55f, 0.95f, 1f));
            Sprite stateMoodCalm = CreateIconSprite("release-state-mood-calm.png", ReleaseGlyph.Calm, new Color(0.12f, 0.38f, 0.26f, 1f), new Color(0.64f, 0.94f, 0.72f, 1f));
            Sprite stateMoodAnxious = CreateIconSprite("release-state-mood-anxious.png", ReleaseGlyph.Anxious, new Color(0.45f, 0.23f, 0.17f, 1f), new Color(1f, 0.72f, 0.48f, 1f));
            Sprite stateClarityVivid = CreateIconSprite("release-state-clarity-vivid.png", ReleaseGlyph.Vivid, new Color(0.16f, 0.28f, 0.55f, 1f), new Color(0.66f, 0.82f, 1f, 1f));
            Sprite stateClarityBlurry = CreateIconSprite("release-state-clarity-blurry.png", ReleaseGlyph.Blurry, new Color(0.32f, 0.34f, 0.39f, 1f), new Color(0.86f, 0.88f, 0.9f, 1f));
            Sprite stateStabilityStable = CreateIconSprite("release-state-stability-stable.png", ReleaseGlyph.Stable, new Color(0.24f, 0.38f, 0.18f, 1f), new Color(0.82f, 0.95f, 0.62f, 1f));
            Sprite stateStabilityUnsettled = CreateIconSprite("release-state-stability-unsettled.png", ReleaseGlyph.Unsettled, new Color(0.48f, 0.24f, 0.2f, 1f), new Color(1f, 0.72f, 0.56f, 1f));
            Sprite operationWash = CreateIconSprite("release-operation-wash.png", ReleaseGlyph.Wash, new Color(0.1f, 0.35f, 0.5f, 1f), new Color(0.58f, 0.9f, 1f, 1f));
            Sprite operationSoothe = CreateIconSprite("release-operation-soothe.png", ReleaseGlyph.Soothe, new Color(0.12f, 0.36f, 0.24f, 1f), new Color(0.68f, 0.96f, 0.7f, 1f));
            Sprite operationClarify = CreateIconSprite("release-operation-clarify.png", ReleaseGlyph.Clarify, new Color(0.22f, 0.26f, 0.54f, 1f), new Color(0.78f, 0.86f, 1f, 1f));
            Sprite operationSettle = CreateIconSprite("release-operation-settle.png", ReleaseGlyph.Settle, new Color(0.43f, 0.34f, 0.16f, 1f), new Color(1f, 0.84f, 0.44f, 1f));
            Sprite toolPreviewSwap = CreateIconSprite("release-tool-preview-swap.png", ReleaseGlyph.PreviewSwap, new Color(0.25f, 0.25f, 0.52f, 1f), new Color(0.78f, 0.78f, 1f, 1f));
            Sprite toolDreamRefresh = CreateIconSprite("release-tool-dream-refresh.png", ReleaseGlyph.DreamRefresh, new Color(0.22f, 0.32f, 0.52f, 1f), new Color(0.72f, 0.88f, 1f, 1f));
            Sprite obstacleLockedSlot = CreateIconSprite("release-obstacle-locked-slot.png", ReleaseGlyph.Lock, new Color(0.5f, 0.24f, 0.18f, 1f), new Color(1f, 0.7f, 0.54f, 1f));
            Sprite obstacleOrderPin = CreateIconSprite("release-obstacle-order-pin.png", ReleaseGlyph.OrderPin, new Color(0.46f, 0.3f, 0.16f, 1f), new Color(1f, 0.78f, 0.46f, 1f));
            Sprite obstacleSoftBlock = CreateIconSprite("release-obstacle-soft-block.png", ReleaseGlyph.SoftBlock, new Color(0.44f, 0.2f, 0.24f, 1f), new Color(1f, 0.66f, 0.72f, 1f));
            Sprite effectClearGlow = CreateEffectSprite("release-effect-clear-glow.png", ReleaseGlyph.ClearGlow, new Color(0.18f, 0.42f, 0.26f, 1f), new Color(0.82f, 1f, 0.62f, 1f));
            Sprite effectFailWarning = CreateEffectSprite("release-effect-fail-warning.png", ReleaseGlyph.FailWarning, new Color(0.52f, 0.22f, 0.14f, 1f), new Color(1f, 0.72f, 0.42f, 1f));

            ReleaseUiArtCatalog catalog = AssetDatabase.LoadAssetAtPath<ReleaseUiArtCatalog>(CatalogPath);
            if (catalog == null)
            {
                catalog = ScriptableObject.CreateInstance<ReleaseUiArtCatalog>();
                AssetDatabase.CreateAsset(catalog, CatalogPath);
            }

            catalog.Configure(
                gameplayBackground,
                titleBackground,
                levelSelectBackground,
                dreamCardFrame,
                orderSheetFrame,
                storageShelfFrame,
                operationButtonFrame,
                submitButtonFrame,
                storageActionFrame,
                navigationButtonFrame,
                stateTaintClean,
                stateTaintNightmare,
                stateMoodCalm,
                stateMoodAnxious,
                stateClarityVivid,
                stateClarityBlurry,
                stateStabilityStable,
                stateStabilityUnsettled,
                operationWash,
                operationSoothe,
                operationClarify,
                operationSettle,
                toolPreviewSwap,
                toolDreamRefresh,
                obstacleLockedSlot,
                obstacleOrderPin,
                obstacleSoftBlock,
                effectClearGlow,
                effectFailWarning);
            EditorUtility.SetDirty(catalog);
            WriteSourceNotes();
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            if (!catalog.IsComplete)
            {
                throw new InvalidOperationException("Release UI art catalog generation produced an incomplete catalog.");
            }

            Debug.Log($"Release UI art assets generated: {CatalogPath}");
            return catalog;
        }

        private static Sprite CreateIconSprite(string fileName, ReleaseGlyph glyph, Color fill, Color border)
        {
            return LoadOrCreateSpriteAsset(
                $"{IconDirectory}/{fileName}",
                IconSize,
                IconSize,
                (u, v) => PaintIcon(u, v, glyph, fill, border),
                IconSize);
        }

        private static Sprite CreateEffectSprite(string fileName, ReleaseGlyph glyph, Color fill, Color border)
        {
            return LoadOrCreateSpriteAsset(
                $"{EffectDirectory}/{fileName}",
                IconSize,
                IconSize,
                (u, v) => PaintIcon(u, v, glyph, fill, border),
                IconSize);
        }

        private static Sprite LoadOrCreateSpriteAsset(
            string assetPath,
            int width,
            int height,
            Func<float, float, Color> painter,
            float pixelsPerUnit)
        {
            string fullPath = ToFullPath(assetPath);
            if (!File.Exists(fullPath))
            {
                return CreateSpriteAsset(assetPath, width, height, painter, pixelsPerUnit);
            }

            AssetDatabase.ImportAsset(assetPath, ImportAssetOptions.ForceUpdate);
            ConfigureSpriteImporter(assetPath, width, height, pixelsPerUnit);
            Sprite sprite = AssetDatabase.LoadAssetAtPath<Sprite>(assetPath);
            if (sprite == null)
            {
                throw new InvalidOperationException($"Existing sprite import failed for {assetPath}.");
            }

            return sprite;
        }

        private static Sprite CreateSpriteAsset(
            string assetPath,
            int width,
            int height,
            Func<float, float, Color> painter,
            float pixelsPerUnit)
        {
            string fullPath = ToFullPath(assetPath);
            Directory.CreateDirectory(Path.GetDirectoryName(fullPath));

            var texture = new Texture2D(width, height, TextureFormat.RGBA32, false);
            var pixels = new Color32[width * height];
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    float u = (x + 0.5f) / width;
                    float v = (y + 0.5f) / height;
                    pixels[(y * width) + x] = painter(u, v);
                }
            }

            texture.SetPixels32(pixels);
            texture.Apply(false, false);
            File.WriteAllBytes(fullPath, texture.EncodeToPNG());
            UnityEngine.Object.DestroyImmediate(texture);

            AssetDatabase.ImportAsset(assetPath, ImportAssetOptions.ForceUpdate);
            ConfigureSpriteImporter(assetPath, width, height, pixelsPerUnit);

            Sprite sprite = AssetDatabase.LoadAssetAtPath<Sprite>(assetPath);
            if (sprite == null)
            {
                throw new InvalidOperationException($"Sprite import failed for {assetPath}.");
            }

            return sprite;
        }

        private static void ConfigureSpriteImporter(string assetPath, int width, int height, float pixelsPerUnit)
        {
            TextureImporter importer = AssetImporter.GetAtPath(assetPath) as TextureImporter;
            if (importer == null)
            {
                throw new InvalidOperationException($"Texture importer is missing for {assetPath}.");
            }

            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Single;
            importer.spritePixelsPerUnit = pixelsPerUnit;
            importer.mipmapEnabled = false;
            importer.alphaIsTransparency = true;
            importer.sRGBTexture = true;
            importer.textureCompression = TextureImporterCompression.Uncompressed;
            importer.wrapMode = TextureWrapMode.Clamp;
            importer.filterMode = FilterMode.Bilinear;
            importer.maxTextureSize = Mathf.NextPowerOfTwo(Mathf.Max(width, height));
            importer.SaveAndReimport();
        }

        private static void CreateFolders()
        {
            string[] folders =
            {
                "Assets/_Project/Art",
                "Assets/_Project/Art/UI",
                BackgroundDirectory,
                CardDirectory,
                IconDirectory,
                EffectDirectory,
                SourceNotesDirectory,
                "Assets/_Project/ScriptableObjects"
            };

            for (int i = 0; i < folders.Length; i++)
            {
                if (!Directory.Exists(ToFullPath(folders[i])))
                {
                    Directory.CreateDirectory(ToFullPath(folders[i]));
                }
            }
        }

        private static void WriteSourceNotes()
        {
            string notePath = ToFullPath($"{SourceNotesDirectory}/generated-release-ui-assets.md");
            Directory.CreateDirectory(Path.GetDirectoryName(notePath));
            string body = string.Join(
                Environment.NewLine,
                "# Generated Release UI Assets",
                "",
                "이 폴더의 Release UI PNG는 코드 기반 임시 아트다.",
                "최종 출시 아트로 교체하기 전까지 카탈로그 구조, 파일명, import 설정, 화면 배치 검증을 고정하기 위한 목적이다.",
                "`ReleaseUiArtGenerator`는 기존 PNG를 보존하고, 파일이 없을 때만 기본 fallback 에셋을 생성한다.",
                "",
                "- 원본 생성기: `Assets/_Project/Editor/ReleaseSlice/ReleaseUiArtGenerator.cs`",
                "- 연결 카탈로그: `Assets/_Project/ScriptableObjects/ReleaseUiArtCatalog.asset`",
                "- 적용 화면: `Assets/_Project/Scenes/ReleaseGameplaySlice.unity`",
                "- 교체 원칙: 같은 역할의 에셋은 파일명을 안정적으로 유지하고, 대체 후 `.meta`와 카탈로그 참조를 함께 검증한다.",
                "",
                "현재 에셋은 외부 무료/유료 에셋 라이선스에 의존하지 않는다.");
            File.WriteAllText(notePath, body, new UTF8Encoding(false));
            AssetDatabase.ImportAsset($"{SourceNotesDirectory}/generated-release-ui-assets.md", ImportAssetOptions.ForceUpdate);
        }

        private static string ToFullPath(string assetPath)
        {
            if (!assetPath.StartsWith("Assets/", StringComparison.Ordinal))
            {
                throw new ArgumentException($"Asset path must start with Assets/: {assetPath}", nameof(assetPath));
            }

            string relativePath = assetPath.Substring("Assets/".Length).Replace('/', Path.DirectorySeparatorChar);
            return Path.Combine(Application.dataPath, relativePath);
        }

        private static Color PaintGameplayBackground(float u, float v)
        {
            Color color = Color.Lerp(new Color(0.025f, 0.031f, 0.047f, 1f), new Color(0.11f, 0.13f, 0.16f, 1f), v);
            color = AddNoise(color, u, v, 0.024f);
            color = DrawFloor(color, u, v, 0.32f, new Color(0.035f, 0.045f, 0.06f, 1f), new Color(0.15f, 0.2f, 0.24f, 1f));
            color = DrawWallTiles(color, u, v, 0.36f, 0.9f, new Color(0.075f, 0.092f, 0.105f, 1f), 0.18f);
            color = DrawMachineBank(color, u, v, 0.18f, 0.13f, 0.28f, 0.26f, 0.68f);
            color = DrawMachineBank(color, u, v, 0.57f, 0.16f, 0.28f, 0.26f, 0.58f);
            color = DrawCounter(color, u, v, 0.04f, 0.28f, 0.96f, 0.36f, 0.7f);
            color = DrawFoldedCloth(color, u, v, 0.08f, 0.38f, 0.24f, 0.46f, new Color(0.74f, 0.54f, 0.86f, 1f));
            color = DrawFoldedCloth(color, u, v, 0.72f, 0.39f, 0.92f, 0.47f, new Color(0.76f, 0.86f, 0.86f, 1f));
            color = DrawHangingLaundry(color, u, v, 0.07f, 0.7f, 0.32f, 0.88f, 0.42f);
            color = DrawHangingLaundry(color, u, v, 0.69f, 0.69f, 0.94f, 0.86f, 0.32f);
            color = DrawDreamBubble(color, u, v, 0.19f, 0.68f, 0.09f, new Color(0.58f, 0.88f, 1f, 0.52f));
            color = DrawDreamBubble(color, u, v, 0.82f, 0.62f, 0.075f, new Color(0.86f, 0.66f, 1f, 0.45f));
            color = DrawDreamBubble(color, u, v, 0.52f, 0.82f, 0.055f, new Color(1f, 0.82f, 0.42f, 0.34f));
            color = DrawSparkleField(color, u, v, 0.78f);
            color = DrawUiReadabilityWash(color, u, v, 0.52f);
            return ApplyVignette(color, u, v, 1.15f);
        }

        private static Color PaintTitleBackground(float u, float v)
        {
            Color color = Color.Lerp(new Color(0.018f, 0.023f, 0.038f, 1f), new Color(0.095f, 0.105f, 0.145f, 1f), v);
            color = AddNoise(color, u, v, 0.02f);
            color = DrawFloor(color, u, v, 0.26f, new Color(0.025f, 0.03f, 0.044f, 1f), new Color(0.13f, 0.16f, 0.19f, 1f));
            color = DrawStorefrontWindow(color, u, v);
            color = DrawSoftRect(color, u, v, 0.13f, 0.76f, 0.87f, 0.84f, 0.035f, new Color(0.88f, 0.64f, 0.24f, 1f), 0.28f);
            color = DrawLine(color, u, v, 0.18f, 0.8f, 0.82f, 0.8f, 0.012f, new Color(0.65f, 0.95f, 1f, 1f), 0.34f);
            color = DrawMachineBank(color, u, v, 0.2f, 0.2f, 0.26f, 0.24f, 0.72f);
            color = DrawMachineBank(color, u, v, 0.56f, 0.2f, 0.26f, 0.24f, 0.6f);
            color = DrawCounter(color, u, v, 0.12f, 0.33f, 0.88f, 0.42f, 0.62f);
            color = DrawDreamBubble(color, u, v, 0.5f, 0.64f, 0.17f, new Color(0.55f, 0.9f, 1f, 0.42f));
            color = DrawDreamBubble(color, u, v, 0.26f, 0.83f, 0.07f, new Color(0.88f, 0.68f, 1f, 0.34f));
            color = DrawDreamBubble(color, u, v, 0.78f, 0.86f, 0.055f, new Color(1f, 0.74f, 0.42f, 0.28f));
            color = DrawSparkleField(color, u, v, 0.92f);
            color = DrawTitleSafeSpotlight(color, u, v);
            return ApplyVignette(color, u, v, 1.05f);
        }

        private static Color PaintLevelSelectBackground(float u, float v)
        {
            Color color = Color.Lerp(new Color(0.045f, 0.043f, 0.047f, 1f), new Color(0.15f, 0.125f, 0.085f, 1f), v);
            color = AddNoise(color, u, v, 0.035f);
            color = DrawSoftRect(color, u, v, 0.04f, 0.18f, 0.96f, 0.9f, 0.025f, new Color(0.22f, 0.16f, 0.095f, 1f), 0.72f);
            color = DrawSoftRect(color, u, v, 0.06f, 0.21f, 0.94f, 0.87f, 0.018f, new Color(0.34f, 0.24f, 0.13f, 1f), 0.52f);
            color = DrawBoardGrain(color, u, v);
            color = DrawPinnedTicket(color, u, v, 0.1f, 0.66f, 0.34f, 0.82f, 0.54f, new Color(0.72f, 0.9f, 0.94f, 1f));
            color = DrawPinnedTicket(color, u, v, 0.66f, 0.64f, 0.9f, 0.8f, 0.42f, new Color(0.9f, 0.72f, 0.86f, 1f));
            color = DrawPinnedTicket(color, u, v, 0.1f, 0.28f, 0.32f, 0.43f, 0.44f, new Color(0.9f, 0.82f, 0.58f, 1f));
            color = DrawJar(color, u, v, 0.73f, 0.25f, 0.14f, 0.18f, new Color(0.58f, 0.88f, 1f, 1f));
            color = DrawJar(color, u, v, 0.85f, 0.36f, 0.11f, 0.15f, new Color(0.88f, 0.66f, 1f, 1f));
            color = DrawLine(color, u, v, 0.08f, 0.58f, 0.92f, 0.58f, 0.007f, new Color(0.96f, 0.74f, 0.35f, 1f), 0.42f);
            color = DrawLine(color, u, v, 0.08f, 0.38f, 0.92f, 0.38f, 0.007f, new Color(0.96f, 0.74f, 0.35f, 1f), 0.36f);
            color = DrawSparkleField(color, u, v, 0.68f);
            color = DrawUiReadabilityWash(color, u, v, 0.4f);
            return ApplyVignette(color, u, v, 1.2f);
        }

        private static Color PaintFrame(float u, float v, Color fill, Color border, Color accent)
        {
            float outer = RoundedRectMask(u, v, 0.04f, 0.04f, 0.96f, 0.96f, 0.075f);
            if (outer <= 0f)
            {
                return Color.clear;
            }

            float inner = RoundedRectMask(u, v, 0.095f, 0.095f, 0.905f, 0.905f, 0.045f);
            if (inner < 1f)
            {
                return Color.Lerp(border, accent, Mathf.Clamp01((v - 0.04f) / 0.92f) * 0.25f);
            }

            float thread = Mathf.Abs(Mathf.Sin((u + v) * 80f));
            Color body = Color.Lerp(fill, accent, thread > 0.96f ? 0.14f : 0.02f);
            if (v > 0.78f && u > 0.12f && u < 0.88f)
            {
                body = Color.Lerp(body, accent, 0.16f);
            }

            return body;
        }

        private static Color PaintIcon(float u, float v, ReleaseGlyph glyph, Color fill, Color border)
        {
            Color badge = PaintBadge(u, v, fill, border);
            if (badge.a <= 0f)
            {
                return badge;
            }

            Color ink = new Color(0.96f, 0.98f, 0.9f, 1f);
            float x = u - 0.5f;
            float y = v - 0.5f;
            if (PaintGlyph(glyph, u, v, x, y))
            {
                return ink;
            }

            return badge;
        }

        private static Color AddNoise(Color color, float u, float v, float amount)
        {
            float grain = (Hash(u * 157.3f, v * 241.7f) - 0.5f) * amount;
            return ClampColor(new Color(color.r + grain, color.g + grain, color.b + grain, color.a));
        }

        private static Color DrawFloor(Color baseColor, float u, float v, float horizon, Color near, Color far)
        {
            if (v >= horizon)
            {
                return baseColor;
            }

            float depth = Mathf.Clamp01(1f - (v / horizon));
            Color floor = Color.Lerp(far, near, depth);
            float perspectiveLines = Mathf.Abs(Mathf.Sin((u - 0.5f) * 20f / Mathf.Max(0.18f, v + 0.12f)));
            float tileLines = Mathf.Abs(Mathf.Sin((v + 0.04f) * 64f));
            float lineMask = Mathf.Clamp01((0.05f - Mathf.Min(perspectiveLines, tileLines)) * 10f);
            floor = Blend(floor, new Color(0.32f, 0.42f, 0.48f, 1f), lineMask * 0.12f);

            float reflection = Mathf.Exp(-Mathf.Abs(u - 0.5f) * 5f) * Mathf.Clamp01((0.27f - v) * 2.7f);
            floor = Blend(floor, new Color(0.28f, 0.72f, 0.86f, 1f), reflection * 0.16f);
            return Blend(baseColor, floor, 0.95f);
        }

        private static Color DrawWallTiles(Color baseColor, float u, float v, float minY, float maxY, Color tileColor, float opacity)
        {
            if (v < minY || v > maxY)
            {
                return baseColor;
            }

            Color color = Blend(baseColor, tileColor, opacity);
            float vertical = Mathf.Abs(Mathf.Sin(u * 12f * Mathf.PI));
            float horizontal = Mathf.Abs(Mathf.Sin((v - minY) * 16f * Mathf.PI));
            float grout = Mathf.Clamp01((0.05f - Mathf.Min(vertical, horizontal)) * 8f);
            return Blend(color, new Color(0.32f, 0.4f, 0.42f, 1f), grout * 0.14f);
        }

        private static Color DrawMachineBank(Color baseColor, float u, float v, float left, float bottom, float width, float height, float opacity)
        {
            Color color = DrawSoftRect(baseColor, u, v, left, bottom, left + width, bottom + height, 0.03f, new Color(0.16f, 0.2f, 0.23f, 1f), opacity);
            color = DrawSoftRect(color, u, v, left + 0.012f, bottom + height - 0.038f, left + width - 0.012f, bottom + height - 0.01f, 0.012f, new Color(0.55f, 0.7f, 0.75f, 1f), opacity * 0.34f);

            const int MachineCount = 3;
            float gap = width * 0.045f;
            float machineWidth = (width - (gap * (MachineCount + 1))) / MachineCount;
            for (int i = 0; i < MachineCount; i++)
            {
                float machineLeft = left + gap + (i * (machineWidth + gap));
                float machineRight = machineLeft + machineWidth;
                float doorCenterX = (machineLeft + machineRight) * 0.5f;
                float doorCenterY = bottom + (height * 0.43f);
                float radius = machineWidth * 0.34f;

                color = DrawSoftRect(color, u, v, machineLeft, bottom + 0.035f, machineRight, bottom + height - 0.052f, 0.026f, new Color(0.38f, 0.48f, 0.52f, 1f), opacity * 0.42f);
                color = DrawSoftCircle(color, u, v, doorCenterX, doorCenterY, radius * 1.18f, new Color(0.72f, 0.84f, 0.88f, 1f), opacity * 0.34f);
                color = DrawSoftCircle(color, u, v, doorCenterX, doorCenterY, radius, new Color(0.05f, 0.08f, 0.11f, 1f), opacity * 0.68f);
                color = DrawSoftCircle(color, u, v, doorCenterX - (radius * 0.24f), doorCenterY + (radius * 0.22f), radius * 0.42f, new Color(0.35f, 0.85f, 0.95f, 1f), opacity * 0.22f);
                color = DrawLine(color, u, v, machineLeft + 0.012f, bottom + height - 0.07f, machineRight - 0.012f, bottom + height - 0.07f, 0.004f, new Color(0.95f, 0.74f, 0.32f, 1f), opacity * 0.22f);
            }

            return color;
        }

        private static Color DrawCounter(Color baseColor, float u, float v, float left, float bottom, float right, float top, float opacity)
        {
            Color color = DrawSoftRect(baseColor, u, v, left, bottom, right, top, 0.018f, new Color(0.18f, 0.13f, 0.09f, 1f), opacity);
            color = DrawSoftRect(color, u, v, left, top - 0.025f, right, top + 0.005f, 0.012f, new Color(0.74f, 0.55f, 0.28f, 1f), opacity * 0.62f);
            color = DrawLine(color, u, v, left + 0.03f, bottom + 0.02f, right - 0.03f, bottom + 0.02f, 0.004f, new Color(0.35f, 0.22f, 0.12f, 1f), opacity * 0.5f);
            return color;
        }

        private static Color DrawFoldedCloth(Color baseColor, float u, float v, float left, float bottom, float right, float top, Color tint)
        {
            Color color = baseColor;
            float layerHeight = (top - bottom) / 3f;
            for (int i = 0; i < 3; i++)
            {
                float offset = i * layerHeight * 0.82f;
                Color layer = Color.Lerp(tint, new Color(0.95f, 0.98f, 0.96f, 1f), i * 0.12f);
                color = DrawSoftRect(color, u, v, left + (i * 0.012f), bottom + offset, right - (i * 0.006f), bottom + offset + layerHeight, 0.012f, layer, 0.58f);
                color = DrawLine(color, u, v, left + 0.025f, bottom + offset + (layerHeight * 0.5f), right - 0.025f, bottom + offset + (layerHeight * 0.5f), 0.003f, new Color(0.18f, 0.2f, 0.24f, 1f), 0.22f);
            }

            return color;
        }

        private static Color DrawHangingLaundry(Color baseColor, float u, float v, float left, float bottom, float right, float top, float opacity)
        {
            Color color = DrawLine(baseColor, u, v, left, top, right, top - 0.02f, 0.004f, new Color(0.78f, 0.7f, 0.55f, 1f), opacity);
            float width = (right - left) / 3.4f;
            color = DrawSoftRect(color, u, v, left + 0.01f, bottom + 0.03f, left + width, top - 0.02f, 0.018f, new Color(0.62f, 0.82f, 0.86f, 1f), opacity * 0.55f);
            color = DrawSoftRect(color, u, v, left + width + 0.03f, bottom, left + (width * 2.05f), top - 0.04f, 0.02f, new Color(0.68f, 0.54f, 0.82f, 1f), opacity * 0.46f);
            color = DrawSoftRect(color, u, v, right - width, bottom + 0.02f, right - 0.01f, top - 0.03f, 0.018f, new Color(0.86f, 0.74f, 0.48f, 1f), opacity * 0.42f);
            return color;
        }

        private static Color DrawStorefrontWindow(Color baseColor, float u, float v)
        {
            Color color = DrawSoftRect(baseColor, u, v, 0.08f, 0.32f, 0.92f, 0.76f, 0.035f, new Color(0.055f, 0.075f, 0.095f, 1f), 0.82f);
            color = DrawSoftRect(color, u, v, 0.1f, 0.34f, 0.9f, 0.74f, 0.026f, new Color(0.16f, 0.25f, 0.28f, 1f), 0.32f);
            color = DrawLine(color, u, v, 0.5f, 0.34f, 0.5f, 0.74f, 0.006f, new Color(0.74f, 0.86f, 0.82f, 1f), 0.28f);
            color = DrawLine(color, u, v, 0.1f, 0.54f, 0.9f, 0.54f, 0.005f, new Color(0.74f, 0.86f, 0.82f, 1f), 0.24f);
            color = DrawSoftCircle(color, u, v, 0.28f, 0.58f, 0.12f, new Color(0.36f, 0.9f, 1f, 1f), 0.13f);
            color = DrawSoftCircle(color, u, v, 0.72f, 0.55f, 0.13f, new Color(0.96f, 0.72f, 0.36f, 1f), 0.11f);
            return color;
        }

        private static Color DrawBoardGrain(Color baseColor, float u, float v)
        {
            if (u < 0.06f || u > 0.94f || v < 0.21f || v > 0.87f)
            {
                return baseColor;
            }

            float streak = Mathf.Abs(Mathf.Sin((u * 52f) + (Hash(v * 11f, u * 9f) * 4f)));
            float knot = SoftCircleMask(u, v, 0.36f, 0.74f, 0.07f, 0.08f) + SoftCircleMask(u, v, 0.82f, 0.48f, 0.05f, 0.07f);
            Color color = Blend(baseColor, new Color(0.52f, 0.34f, 0.16f, 1f), (1f - streak) * 0.055f);
            return Blend(color, new Color(0.16f, 0.1f, 0.055f, 1f), Mathf.Clamp01(knot) * 0.2f);
        }

        private static Color DrawPinnedTicket(Color baseColor, float u, float v, float left, float bottom, float right, float top, float opacity, Color accent)
        {
            Color paper = new Color(0.88f, 0.83f, 0.66f, 1f);
            Color color = DrawSoftRect(baseColor, u, v, left, bottom, right, top, 0.014f, paper, opacity);
            color = DrawLine(color, u, v, left + 0.03f, top - 0.04f, right - 0.04f, top - 0.055f, 0.004f, accent, opacity * 0.55f);
            color = DrawLine(color, u, v, left + 0.03f, top - 0.08f, right - 0.06f, top - 0.096f, 0.003f, new Color(0.22f, 0.19f, 0.15f, 1f), opacity * 0.2f);
            color = DrawLine(color, u, v, left + 0.03f, top - 0.115f, right - 0.09f, top - 0.126f, 0.003f, new Color(0.22f, 0.19f, 0.15f, 1f), opacity * 0.16f);
            color = DrawSoftCircle(color, u, v, (left + right) * 0.5f, top - 0.018f, 0.012f, new Color(0.92f, 0.32f, 0.28f, 1f), opacity * 0.78f);
            return color;
        }

        private static Color DrawJar(Color baseColor, float u, float v, float centerX, float bottom, float width, float height, Color glow)
        {
            float left = centerX - (width * 0.5f);
            float right = centerX + (width * 0.5f);
            Color color = DrawSoftRect(baseColor, u, v, left, bottom, right, bottom + height, 0.036f, new Color(0.16f, 0.22f, 0.25f, 1f), 0.5f);
            color = DrawSoftRect(color, u, v, left + 0.018f, bottom + 0.02f, right - 0.018f, bottom + height - 0.028f, 0.028f, glow, 0.18f);
            color = DrawSoftRect(color, u, v, left + 0.026f, bottom + height - 0.01f, right - 0.026f, bottom + height + 0.024f, 0.014f, new Color(0.8f, 0.66f, 0.38f, 1f), 0.5f);
            color = DrawSoftCircle(color, u, v, centerX, bottom + (height * 0.5f), width * 0.26f, glow, 0.28f);
            return color;
        }

        private static Color DrawSparkleField(Color baseColor, float u, float v, float opacity)
        {
            Color color = baseColor;
            for (int i = 0; i < 20; i++)
            {
                float x = Hash(i * 13.17f, 2.93f);
                float y = Mathf.Lerp(0.42f, 0.94f, Hash(i * 5.7f, 8.31f));
                float radius = Mathf.Lerp(0.004f, 0.012f, Hash(i * 3.1f, 1.9f));
                float mask = SoftCircleMask(u, v, x, y, radius, radius * 1.8f);
                Color sparkle = i % 3 == 0
                    ? new Color(1f, 0.82f, 0.44f, 1f)
                    : new Color(0.62f, 0.9f, 1f, 1f);
                color = Blend(color, sparkle, mask * opacity * 0.42f);
            }

            return color;
        }

        private static Color DrawUiReadabilityWash(Color baseColor, float u, float v, float opacity)
        {
            float x = (u - 0.5f) / 0.42f;
            float y = (v - 0.49f) / 0.46f;
            float mask = Mathf.Clamp01(1f - ((x * x) + (y * y)));
            return Blend(baseColor, new Color(0.016f, 0.02f, 0.032f, 1f), mask * opacity);
        }

        private static Color DrawTitleSafeSpotlight(Color baseColor, float u, float v)
        {
            float titleMask = SoftRectMask(u, v, 0.16f, 0.46f, 0.84f, 0.74f, 0.08f, 0.14f);
            float buttonMask = SoftRectMask(u, v, 0.18f, 0.08f, 0.82f, 0.25f, 0.05f, 0.08f);
            Color color = Blend(baseColor, new Color(0.02f, 0.024f, 0.038f, 1f), titleMask * 0.35f);
            return Blend(color, new Color(0.02f, 0.024f, 0.038f, 1f), buttonMask * 0.28f);
        }

        private static Color DrawSoftRect(Color baseColor, float u, float v, float minX, float minY, float maxX, float maxY, float radius, Color color, float opacity)
        {
            float mask = SoftRectMask(u, v, minX, minY, maxX, maxY, radius, 0.012f);
            return Blend(baseColor, color, mask * opacity);
        }

        private static Color DrawSoftCircle(Color baseColor, float u, float v, float centerX, float centerY, float radius, Color color, float opacity)
        {
            float mask = SoftCircleMask(u, v, centerX, centerY, radius, radius * 0.35f);
            return Blend(baseColor, color, mask * opacity);
        }

        private static Color DrawLine(Color baseColor, float u, float v, float ax, float ay, float bx, float by, float thickness, Color color, float opacity)
        {
            float dx = bx - ax;
            float dy = by - ay;
            float lengthSquared = (dx * dx) + (dy * dy);
            if (lengthSquared <= 0.0001f)
            {
                return baseColor;
            }

            float t = Mathf.Clamp01((((u - ax) * dx) + ((v - ay) * dy)) / lengthSquared);
            float closestX = ax + (dx * t);
            float closestY = ay + (dy * t);
            float distanceX = u - closestX;
            float distanceY = v - closestY;
            float distance = Mathf.Sqrt((distanceX * distanceX) + (distanceY * distanceY));
            float mask = 1f - Mathf.SmoothStep(thickness, thickness + 0.008f, distance);
            return Blend(baseColor, color, mask * opacity);
        }

        private static float SoftRectMask(float u, float v, float minX, float minY, float maxX, float maxY, float radius, float feather)
        {
            float centerX = (minX + maxX) * 0.5f;
            float centerY = (minY + maxY) * 0.5f;
            float halfWidth = (maxX - minX) * 0.5f;
            float halfHeight = (maxY - minY) * 0.5f;
            float dx = Mathf.Abs(u - centerX) - halfWidth + radius;
            float dy = Mathf.Abs(v - centerY) - halfHeight + radius;
            float outsideX = Mathf.Max(dx, 0f);
            float outsideY = Mathf.Max(dy, 0f);
            float outsideDistance = Mathf.Sqrt((outsideX * outsideX) + (outsideY * outsideY));
            float insideDistance = Mathf.Min(Mathf.Max(dx, dy), 0f);
            float signedDistance = outsideDistance + insideDistance - radius;
            return 1f - Mathf.SmoothStep(0f, feather, signedDistance);
        }

        private static float SoftCircleMask(float u, float v, float centerX, float centerY, float radius, float feather)
        {
            float dx = u - centerX;
            float dy = v - centerY;
            float distance = Mathf.Sqrt((dx * dx) + (dy * dy));
            return 1f - Mathf.SmoothStep(radius, radius + feather, distance);
        }

        private static Color Blend(Color baseColor, Color overlay, float opacity)
        {
            float amount = Mathf.Clamp01(opacity * overlay.a);
            Color color = Color.Lerp(baseColor, new Color(overlay.r, overlay.g, overlay.b, 1f), amount);
            color.a = Mathf.Lerp(baseColor.a, 1f, amount);
            return ClampColor(color);
        }

        private static Color ClampColor(Color color)
        {
            return new Color(
                Mathf.Clamp01(color.r),
                Mathf.Clamp01(color.g),
                Mathf.Clamp01(color.b),
                Mathf.Clamp01(color.a));
        }

        private static float Hash(float x, float y)
        {
            return Mathf.Repeat(Mathf.Sin((x * 12.9898f) + (y * 78.233f)) * 43758.5453f, 1f);
        }

        private static bool PaintGlyph(ReleaseGlyph glyph, float u, float v, float x, float y)
        {
            switch (glyph)
            {
                case ReleaseGlyph.Clean:
                    return Mathf.Abs(x) + Mathf.Abs(y) < 0.26f;
                case ReleaseGlyph.Nightmare:
                    return Mathf.Abs(x) < 0.18f && y > -0.28f && y < 0.24f + (Mathf.Sin(u * 22f) * 0.04f);
                case ReleaseGlyph.Calm:
                    return Line(u, v, 0.24f, 0.46f, 0.76f, 0.46f + (Mathf.Sin(u * 12f) * 0.04f), 0.032f)
                        || Line(u, v, 0.28f, 0.58f, 0.72f, 0.58f + (Mathf.Sin(u * 10f) * 0.03f), 0.025f);
                case ReleaseGlyph.Anxious:
                    return Line(u, v, 0.27f, 0.65f, 0.4f, 0.38f, 0.03f)
                        || Line(u, v, 0.4f, 0.38f, 0.53f, 0.62f, 0.03f)
                        || Line(u, v, 0.53f, 0.62f, 0.72f, 0.33f, 0.03f);
                case ReleaseGlyph.Vivid:
                    return Mathf.Abs(x) < 0.035f && Mathf.Abs(y) < 0.29f
                        || Mathf.Abs(y) < 0.035f && Mathf.Abs(x) < 0.29f
                        || Mathf.Abs(x + y) < 0.032f && Mathf.Abs(x - y) < 0.24f;
                case ReleaseGlyph.Blurry:
                    return Ring(x + 0.08f, y + 0.02f, 0.13f, 0.028f)
                        || Ring(x - 0.12f, y - 0.06f, 0.16f, 0.025f)
                        || Circle(x, y + 0.18f, 0.06f);
                case ReleaseGlyph.Stable:
                    return Line(u, v, 0.5f, 0.22f, 0.5f, 0.72f, 0.035f)
                        || Line(u, v, 0.3f, 0.34f, 0.7f, 0.34f, 0.04f)
                        || Line(u, v, 0.34f, 0.2f, 0.66f, 0.2f, 0.045f);
                case ReleaseGlyph.Unsettled:
                    return Mathf.Abs(y - (Mathf.Sin((u * 18f) + 0.4f) * 0.12f)) < 0.035f && u > 0.18f && u < 0.82f;
                case ReleaseGlyph.Wash:
                    return RoundedRectMask(u, v, 0.29f, 0.22f, 0.71f, 0.77f, 0.05f) > 0f
                        && (!Circle(x, y - 0.03f, 0.2f) || Ring(x, y - 0.03f, 0.2f, 0.045f));
                case ReleaseGlyph.Soothe:
                    return Line(u, v, 0.28f, 0.38f, 0.48f, 0.58f, 0.045f)
                        || Line(u, v, 0.48f, 0.58f, 0.74f, 0.46f, 0.045f)
                        || Circle(x + 0.02f, y - 0.16f, 0.08f);
                case ReleaseGlyph.Clarify:
                    return Ring(x - 0.05f, y + 0.06f, 0.19f, 0.04f)
                        || Line(u, v, 0.58f, 0.38f, 0.74f, 0.22f, 0.04f);
                case ReleaseGlyph.Settle:
                    return Line(u, v, 0.31f, 0.34f, 0.69f, 0.34f, 0.055f)
                        || Line(u, v, 0.38f, 0.48f, 0.62f, 0.48f, 0.055f)
                        || Line(u, v, 0.45f, 0.62f, 0.55f, 0.62f, 0.055f);
                case ReleaseGlyph.PreviewSwap:
                    return Line(u, v, 0.25f, 0.61f, 0.72f, 0.61f, 0.032f)
                        || Line(u, v, 0.75f, 0.61f, 0.63f, 0.73f, 0.032f)
                        || Line(u, v, 0.75f, 0.61f, 0.63f, 0.49f, 0.032f)
                        || Line(u, v, 0.75f, 0.36f, 0.28f, 0.36f, 0.032f)
                        || Line(u, v, 0.25f, 0.36f, 0.37f, 0.48f, 0.032f)
                        || Line(u, v, 0.25f, 0.36f, 0.37f, 0.24f, 0.032f);
                case ReleaseGlyph.DreamRefresh:
                    return Ring(x, y, 0.22f, 0.035f)
                        || Line(u, v, 0.66f, 0.62f, 0.79f, 0.66f, 0.035f)
                        || Line(u, v, 0.66f, 0.62f, 0.69f, 0.76f, 0.035f);
                case ReleaseGlyph.Lock:
                    return RoundedRectMask(u, v, 0.31f, 0.3f, 0.69f, 0.58f, 0.035f) > 0f
                        || (Ring(x, y + 0.04f, 0.18f, 0.035f) && v > 0.52f);
                case ReleaseGlyph.OrderPin:
                    return Circle(x, y - 0.18f, 0.1f)
                        || Line(u, v, 0.5f, 0.38f, 0.5f, 0.73f, 0.035f)
                        || Line(u, v, 0.38f, 0.73f, 0.62f, 0.73f, 0.035f);
                case ReleaseGlyph.SoftBlock:
                    return RoundedRectMask(u, v, 0.24f, 0.42f, 0.76f, 0.58f, 0.04f) > 0f
                        || Line(u, v, 0.33f, 0.28f, 0.67f, 0.72f, 0.035f);
                case ReleaseGlyph.ClearGlow:
                    return Mathf.Abs(x) < 0.03f && Mathf.Abs(y) < 0.31f
                        || Mathf.Abs(y) < 0.03f && Mathf.Abs(x) < 0.31f
                        || (Mathf.Abs(x) + Mathf.Abs(y) < 0.22f);
                case ReleaseGlyph.FailWarning:
                    return Triangle(u, v, 0.5f, 0.78f, 0.24f, 0.25f, 0.76f, 0.25f)
                        && !(Mathf.Abs(x) < 0.035f && v > 0.38f && v < 0.62f)
                        && !Circle(x, y + 0.22f, 0.035f);
                default:
                    return false;
            }
        }

        private static Color PaintBadge(float u, float v, Color fill, Color border)
        {
            float x = u - 0.5f;
            float y = v - 0.5f;
            float distance = Mathf.Sqrt((x * x) + (y * y));
            if (distance > 0.48f)
            {
                return Color.clear;
            }

            if (distance > 0.4f)
            {
                return border;
            }

            float highlight = Mathf.Clamp01(1f - (distance / 0.4f));
            return Color.Lerp(fill, border, highlight * 0.28f);
        }

        private static Color DrawWasherRow(Color baseColor, float u, float v, float y, float radius, float opacity)
        {
            Color color = baseColor;
            color = DrawWasher(color, u, v, 0.22f, y, radius, opacity);
            color = DrawWasher(color, u, v, 0.5f, y + 0.02f, radius * 1.05f, opacity);
            color = DrawWasher(color, u, v, 0.78f, y, radius, opacity);
            return color;
        }

        private static Color DrawWasher(Color baseColor, float u, float v, float centerX, float centerY, float radius, float opacity)
        {
            float dx = u - centerX;
            float dy = v - centerY;
            float distance = Mathf.Sqrt((dx * dx) + (dy * dy));
            if (distance > radius)
            {
                return baseColor;
            }

            Color ring = distance > radius * 0.72f
                ? new Color(0.55f, 0.75f, 0.85f, 1f)
                : new Color(0.14f, 0.22f, 0.28f, 1f);
            return Color.Lerp(baseColor, ring, opacity);
        }

        private static Color DrawDreamBubble(Color baseColor, float u, float v, float centerX, float centerY, float radius, Color bubble)
        {
            float dx = u - centerX;
            float dy = v - centerY;
            float distance = Mathf.Sqrt((dx * dx) + (dy * dy));
            if (distance > radius)
            {
                return baseColor;
            }

            float edge = Mathf.Clamp01(1f - (distance / radius));
            return Color.Lerp(baseColor, bubble, bubble.a * edge);
        }

        private static Color ApplyVignette(Color color, float u, float v, float strength = 1f)
        {
            float dx = u - 0.5f;
            float dy = v - 0.5f;
            float distance = Mathf.Sqrt((dx * dx) + (dy * dy));
            return Color.Lerp(color, new Color(0.02f, 0.024f, 0.036f, 1f), Mathf.Clamp01((distance - 0.34f) * 1.3f * strength));
        }

        private static float RoundedRectMask(float u, float v, float minX, float minY, float maxX, float maxY, float radius)
        {
            float centerX = (minX + maxX) * 0.5f;
            float centerY = (minY + maxY) * 0.5f;
            float halfWidth = (maxX - minX) * 0.5f;
            float halfHeight = (maxY - minY) * 0.5f;
            float dx = Mathf.Abs(u - centerX) - halfWidth + radius;
            float dy = Mathf.Abs(v - centerY) - halfHeight + radius;
            float outsideX = Mathf.Max(dx, 0f);
            float outsideY = Mathf.Max(dy, 0f);
            float outsideDistance = Mathf.Sqrt((outsideX * outsideX) + (outsideY * outsideY));
            float insideDistance = Mathf.Min(Mathf.Max(dx, dy), 0f);
            return outsideDistance + insideDistance <= radius ? 1f : 0f;
        }

        private static bool Circle(float x, float y, float radius)
        {
            return (x * x) + (y * y) <= radius * radius;
        }

        private static bool Ring(float x, float y, float radius, float thickness)
        {
            float distance = Mathf.Sqrt((x * x) + (y * y));
            return Mathf.Abs(distance - radius) <= thickness;
        }

        private static bool Line(float u, float v, float ax, float ay, float bx, float by, float thickness)
        {
            float dx = bx - ax;
            float dy = by - ay;
            float lengthSquared = (dx * dx) + (dy * dy);
            if (lengthSquared <= 0.0001f)
            {
                return false;
            }

            float t = Mathf.Clamp01((((u - ax) * dx) + ((v - ay) * dy)) / lengthSquared);
            float closestX = ax + (dx * t);
            float closestY = ay + (dy * t);
            float distanceX = u - closestX;
            float distanceY = v - closestY;
            return (distanceX * distanceX) + (distanceY * distanceY) <= thickness * thickness;
        }

        private static bool Triangle(float u, float v, float ax, float ay, float bx, float by, float cx, float cy)
        {
            float d1 = Sign(u, v, ax, ay, bx, by);
            float d2 = Sign(u, v, bx, by, cx, cy);
            float d3 = Sign(u, v, cx, cy, ax, ay);
            bool hasNegative = d1 < 0f || d2 < 0f || d3 < 0f;
            bool hasPositive = d1 > 0f || d2 > 0f || d3 > 0f;
            return !(hasNegative && hasPositive);
        }

        private static float Sign(float px, float py, float ax, float ay, float bx, float by)
        {
            return ((px - bx) * (ay - by)) - ((ax - bx) * (py - by));
        }

        private enum ReleaseGlyph
        {
            Clean,
            Nightmare,
            Calm,
            Anxious,
            Vivid,
            Blurry,
            Stable,
            Unsettled,
            Wash,
            Soothe,
            Clarify,
            Settle,
            PreviewSwap,
            DreamRefresh,
            Lock,
            OrderPin,
            SoftBlock,
            ClearGlow,
            FailWarning
        }
    }
}
