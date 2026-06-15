using System;
using System.IO;
using Thkim.DreamLaundromat.Levels;
using Thkim.DreamLaundromat.Rules;
using Thkim.DreamLaundromat.UI;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Thkim.DreamLaundromat.Editor.Setup
{
    public static class DreamLaundromatProjectSetup
    {
        private const int IconSize = 128;
        private const string ScenePath = "Assets/_Project/Scenes/MainGame.unity";
        private const string IconDirectory = "Assets/_Project/Art/UI";
        private const string LevelDirectory = "Assets/_Project/ScriptableObjects/Levels";
        private const string CatalogPath = "Assets/_Project/ScriptableObjects/LevelCatalog.asset";
        private const string IconCatalogPath = "Assets/_Project/ScriptableObjects/UiIconCatalog.asset";

        [MenuItem("DreamLaundromat/Setup/Setup All")]
        public static void SetupAll()
        {
            CreateFolders();
            ConfigureProjectSettings();
            LevelCatalog catalog = CreateLevelAssets();
            UiIconCatalog iconCatalog = CreateUiIconCatalog();
            CreateMainScene(catalog, iconCatalog);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        [MenuItem("DreamLaundromat/Setup/Verify Playable Scene")]
        public static void VerifyPlayableScene()
        {
            if (!File.Exists(ScenePath))
            {
                throw new FileNotFoundException("MainGame scene is missing.", ScenePath);
            }

            Scene scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
            if (!scene.IsValid())
            {
                throw new InvalidOperationException("MainGame scene is invalid.");
            }

            DreamLaundromatGame game = UnityEngine.Object.FindAnyObjectByType<DreamLaundromatGame>();
            if (game == null)
            {
                throw new InvalidOperationException("DreamLaundromatGame is missing in MainGame scene.");
            }

            LevelCatalog catalog = AssetDatabase.LoadAssetAtPath<LevelCatalog>(CatalogPath);
            if (catalog == null || catalog.Levels.Length != 10)
            {
                throw new InvalidOperationException("Level catalog must contain 10 prototype levels.");
            }

            UiIconCatalog iconCatalog = AssetDatabase.LoadAssetAtPath<UiIconCatalog>(IconCatalogPath);
            if (iconCatalog == null || !iconCatalog.IsComplete)
            {
                throw new InvalidOperationException("UI icon catalog is missing or incomplete.");
            }

            for (int i = 0; i < catalog.Levels.Length; i++)
            {
                ValidationResult result = LevelValidator.Validate(catalog.Levels[i]);
                if (!result.IsValid)
                {
                    throw new InvalidOperationException($"Level validation failed for {catalog.Levels[i].LevelId}: {string.Join(", ", result.Errors)}");
                }
            }
        }

        private static void CreateFolders()
        {
            string[] folders =
            {
                "Assets/_Project",
                "Assets/_Project/Art",
                IconDirectory,
                "Assets/_Project/Audio",
                "Assets/_Project/Editor",
                "Assets/_Project/Editor/BuildPipeline",
                "Assets/_Project/Editor/Setup",
                "Assets/_Project/Materials",
                "Assets/_Project/Prefabs",
                "Assets/_Project/Scenes",
                "Assets/_Project/ScriptableObjects",
                LevelDirectory,
                "Assets/_Project/Scripts",
                "Assets/_Project/Scripts/Rules",
                "Assets/_Project/Scripts/Levels",
                "Assets/_Project/Scripts/Gameplay",
                "Assets/_Project/Scripts/Input",
                "Assets/_Project/Scripts/UI",
                "Assets/_Project/Scripts/Infrastructure",
                "Assets/_Project/Settings",
                "Assets/_Project/Tests",
                "Assets/_Project/Tests/EditMode",
                "Assets/_Project/Tests/PlayMode",
                "Assets/_Project/UI"
            };

            foreach (string folder in folders)
            {
                if (!AssetDatabase.IsValidFolder(folder))
                {
                    Directory.CreateDirectory(folder);
                }
            }
        }

        private static void ConfigureProjectSettings()
        {
            PlayerSettings.productName = "DreamLaundromat";
            PlayerSettings.companyName = "rerero";
            PlayerSettings.SetApplicationIdentifier(NamedBuildTarget.Android, "com.rerero.dreamlaundromat");
            PlayerSettings.defaultInterfaceOrientation = UIOrientation.Portrait;
            PlayerSettings.allowedAutorotateToPortrait = true;
            PlayerSettings.allowedAutorotateToPortraitUpsideDown = false;
            PlayerSettings.allowedAutorotateToLandscapeLeft = false;
            PlayerSettings.allowedAutorotateToLandscapeRight = false;
            PlayerSettings.SetUseDefaultGraphicsAPIs(BuildTarget.Android, false);
            PlayerSettings.SetGraphicsAPIs(BuildTarget.Android, new[] { UnityEngine.Rendering.GraphicsDeviceType.OpenGLES3 });
            EditorSettings.serializationMode = SerializationMode.ForceText;
            EditorSettings.externalVersionControl = "Visible Meta Files";
        }

        private static LevelCatalog CreateLevelAssets()
        {
            LevelDefinition[] levels = new LevelDefinition[10];

            levels[0] = CreateLevel(
                "DL-001",
                4,
                new[] { Dream("dream-a", "Dream A", DreamStain.None, DreamMoisture.Dry) },
                Array.Empty<MachineDefinition>(),
                Baskets(2, 2),
                new[] { Order("order-a", "Clean Dry", Req(1, true, DreamStain.None, true, DreamMoisture.Dry)) },
                "주문과 맞는 꿈을 제출한다.");

            levels[1] = CreateLevel(
                "DL-002",
                5,
                new[] { Dream("dream-a", "Nightmare A", DreamStain.Nightmare, DreamMoisture.Dry) },
                WasherOnly(),
                Baskets(2, 2),
                new[] { Order("order-a", "Clean Any", Req(1, true, DreamStain.None, false, DreamMoisture.Dry)) },
                "세탁기로 악몽을 깨끗하게 만든 뒤 보관함을 거쳐 제출한다.");

            levels[2] = CreateLevel(
                "DL-003",
                7,
                new[] { Dream("dream-a", "Nightmare A", DreamStain.Nightmare, DreamMoisture.Dry) },
                WasherAndDryer(),
                Baskets(2, 2),
                new[] { Order("order-a", "Clean Dry", Req(1, true, DreamStain.None, true, DreamMoisture.Dry)) },
                "세탁 후 젖은 꿈을 건조해서 제출한다.");

            levels[3] = CreateLevel(
                "DL-004",
                7,
                new[]
                {
                    Dream("dream-a", "Nightmare A", DreamStain.Nightmare, DreamMoisture.Dry),
                    Dream("dream-b", "Nightmare B", DreamStain.Nightmare, DreamMoisture.Dry)
                },
                WasherOnly(),
                Baskets(1, 1),
                new[] { Order("order-a", "Two Clean Dreams", Req(2, true, DreamStain.None, false, DreamMoisture.Dry)) },
                "보관함이 작을 때 세탁, 보관, 제출 순서를 관리한다.");

            levels[4] = CreateLevel(
                "DL-005",
                8,
                new[]
                {
                    Dream("dream-a", "Nightmare A", DreamStain.Nightmare, DreamMoisture.Dry),
                    Dream("dream-b", "Clean B", DreamStain.None, DreamMoisture.Dry)
                },
                WasherOnly(),
                Baskets(1, 2),
                new[]
                {
                    Order("order-a", "Any Clean", Req(1, true, DreamStain.None, false, DreamMoisture.Dry)),
                    Order("order-b", "Clean Dry", Req(1, true, DreamStain.None, true, DreamMoisture.Dry))
                },
                "이미 깨끗한 꿈과 세탁한 꿈을 서로 다른 주문에 배정한다.");

            levels[5] = CreateLevel(
                "DL-006",
                5,
                new[] { Dream("dream-a", "Wet A", DreamStain.None, DreamMoisture.Wet) },
                DryerOnly(),
                Baskets(2, 2),
                new[] { Order("order-a", "Dry Dream", Req(1, true, DreamStain.None, true, DreamMoisture.Dry)) },
                "젖은 꿈은 건조해야 제출할 수 있다.");

            levels[6] = CreateLevel(
                "DL-007",
                8,
                new[] { Dream("dream-a", "Nightmare A", DreamStain.Nightmare, DreamMoisture.Dry) },
                WasherAndDryer(),
                Baskets(2, 2),
                new[] { Order("order-a", "Clean Dry", Req(1, true, DreamStain.None, true, DreamMoisture.Dry)) },
                "세탁 후 건조해서 제출한다.");

            levels[7] = CreateLevel(
                "DL-008",
                9,
                new[]
                {
                    Dream("dream-a", "Wet A", DreamStain.None, DreamMoisture.Wet),
                    Dream("dream-b", "Nightmare B", DreamStain.Nightmare, DreamMoisture.Dry)
                },
                WasherAndDryer(),
                Baskets(2, 2),
                new[]
                {
                    Order("order-a", "Wet Customer", Req(1, true, DreamStain.None, true, DreamMoisture.Wet)),
                    Order("order-b", "Dry Customer", Req(1, true, DreamStain.None, true, DreamMoisture.Dry))
                },
                "젖은 주문과 마른 주문을 구분한다.");

            levels[8] = CreateLevel(
                "DL-009",
                12,
                new[]
                {
                    Dream("dream-a", "Nightmare A", DreamStain.Nightmare, DreamMoisture.Dry),
                    Dream("dream-b", "Wet B", DreamStain.None, DreamMoisture.Wet),
                    Dream("dream-c", "Clean C", DreamStain.None, DreamMoisture.Dry)
                },
                WasherAndDryer(),
                Baskets(2, 2),
                new[]
                {
                    Order("order-a", "Dry Clean 1", Req(1, true, DreamStain.None, true, DreamMoisture.Dry)),
                    Order("order-b", "Dry Clean 2", Req(1, true, DreamStain.None, true, DreamMoisture.Dry)),
                    Order("order-c", "Any Clean", Req(1, true, DreamStain.None, false, DreamMoisture.Dry))
                },
                "세탁/건조 순서를 바구니와 함께 사용한다.");

            levels[9] = CreateLevel(
                "DL-010",
                14,
                new[]
                {
                    Dream("dream-a", "Nightmare A", DreamStain.Nightmare, DreamMoisture.Dry),
                    Dream("dream-b", "Nightmare B", DreamStain.Nightmare, DreamMoisture.Dry),
                    Dream("dream-c", "Wet C", DreamStain.None, DreamMoisture.Wet),
                    Dream("dream-d", "Clean D", DreamStain.None, DreamMoisture.Dry)
                },
                WasherAndDryer(),
                Baskets(1, 2),
                new[]
                {
                    Order("order-a", "Dry Clean 1", Req(1, true, DreamStain.None, true, DreamMoisture.Dry)),
                    Order("order-b", "Dry Clean 2", Req(1, true, DreamStain.None, true, DreamMoisture.Dry)),
                    Order("order-c", "Wet Clean", Req(1, true, DreamStain.None, true, DreamMoisture.Wet))
                },
                "모든 기본 규칙과 공간 압박을 함께 검증한다.");

            LevelCatalog catalog = AssetDatabase.LoadAssetAtPath<LevelCatalog>(CatalogPath);
            if (catalog == null)
            {
                catalog = ScriptableObject.CreateInstance<LevelCatalog>();
                AssetDatabase.CreateAsset(catalog, CatalogPath);
            }

            catalog.Configure(levels);
            EditorUtility.SetDirty(catalog);
            return catalog;
        }

        private static LevelDefinition CreateLevel(
            string id,
            int moveLimit,
            DreamDefinition[] dreams,
            MachineDefinition[] machines,
            BasketDefinition[] baskets,
            OrderDefinition[] orders,
            string hint)
        {
            string path = $"{LevelDirectory}/{id}.asset";
            LevelDefinition level = AssetDatabase.LoadAssetAtPath<LevelDefinition>(path);
            if (level == null)
            {
                level = ScriptableObject.CreateInstance<LevelDefinition>();
                AssetDatabase.CreateAsset(level, path);
            }

            level.Configure(id, moveLimit, dreams, machines, baskets, orders, hint);
            EditorUtility.SetDirty(level);
            return level;
        }

        private static DreamDefinition Dream(string id, string name, DreamStain stain, DreamMoisture moisture)
        {
            return new DreamDefinition
            {
                Id = id,
                DisplayName = name,
                InitialAttributes = new DreamAttributes(stain, moisture),
                CapacityCost = 1
            };
        }

        private static OrderRequirement Req(
            int count,
            bool requiresStain,
            DreamStain stain,
            bool requiresMoisture,
            DreamMoisture moisture)
        {
            return new OrderRequirement(count, requiresStain, stain, requiresMoisture, moisture);
        }

        private static OrderDefinition Order(string id, string name, params OrderRequirement[] requirements)
        {
            return new OrderDefinition
            {
                Id = id,
                DisplayName = name,
                Requirements = requirements
            };
        }

        private static BasketDefinition[] Baskets(int capacityA, int capacityB)
        {
            return new[]
            {
                new BasketDefinition { Id = "basket-a", DisplayName = "Basket A", Capacity = capacityA },
                new BasketDefinition { Id = "basket-b", DisplayName = "Basket B", Capacity = capacityB }
            };
        }

        private static MachineDefinition[] WasherOnly()
        {
            return new[] { new MachineDefinition { Id = "washer", DisplayName = "Washer", Type = MachineType.Washer, Capacity = 1 } };
        }

        private static MachineDefinition[] DryerOnly()
        {
            return new[] { new MachineDefinition { Id = "dryer", DisplayName = "Dryer", Type = MachineType.Dryer, Capacity = 1 } };
        }

        private static MachineDefinition[] WasherAndDryer()
        {
            return new[]
            {
                new MachineDefinition { Id = "washer", DisplayName = "Washer", Type = MachineType.Washer, Capacity = 1 },
                new MachineDefinition { Id = "dryer", DisplayName = "Dryer", Type = MachineType.Dryer, Capacity = 1 }
            };
        }

        private static UiIconCatalog CreateUiIconCatalog()
        {
            Sprite cleanDream = CreateIconSprite("icon-dream-clean.png", PaintCleanDream);
            Sprite nightmareDream = CreateIconSprite("icon-dream-nightmare.png", PaintNightmareDream);
            Sprite wetState = CreateIconSprite("icon-state-wet.png", PaintWetState);
            Sprite dryState = CreateIconSprite("icon-state-dry.png", PaintDryState);
            Sprite washerMachine = CreateIconSprite("icon-machine-washer.png", PaintWasherMachine);
            Sprite dryerMachine = CreateIconSprite("icon-machine-dryer.png", PaintDryerMachine);
            Sprite submitOrder = CreateIconSprite("icon-submit-order.png", PaintSubmitOrder);
            Sprite storageBasket = CreateIconSprite("icon-storage-basket.png", PaintStorageBasket);

            UiIconCatalog catalog = AssetDatabase.LoadAssetAtPath<UiIconCatalog>(IconCatalogPath);
            if (catalog == null)
            {
                catalog = ScriptableObject.CreateInstance<UiIconCatalog>();
                AssetDatabase.CreateAsset(catalog, IconCatalogPath);
            }

            catalog.Configure(
                cleanDream,
                nightmareDream,
                wetState,
                dryState,
                washerMachine,
                dryerMachine,
                submitOrder,
                storageBasket);
            EditorUtility.SetDirty(catalog);
            return catalog;
        }

        private static Sprite CreateIconSprite(string fileName, Func<float, float, Color> painter)
        {
            string assetPath = $"{IconDirectory}/{fileName}";
            string relativePath = assetPath.Substring("Assets/".Length).Replace('/', Path.DirectorySeparatorChar);
            string fullPath = Path.Combine(Application.dataPath, relativePath);
            Directory.CreateDirectory(Path.GetDirectoryName(fullPath));

            var texture = new Texture2D(IconSize, IconSize, TextureFormat.RGBA32, false);
            var pixels = new Color32[IconSize * IconSize];

            for (int y = 0; y < IconSize; y++)
            {
                for (int x = 0; x < IconSize; x++)
                {
                    float u = (x + 0.5f) / IconSize;
                    float v = (y + 0.5f) / IconSize;
                    pixels[y * IconSize + x] = painter(u, v);
                }
            }

            texture.SetPixels32(pixels);
            texture.Apply(false, false);
            File.WriteAllBytes(fullPath, texture.EncodeToPNG());
            UnityEngine.Object.DestroyImmediate(texture);

            AssetDatabase.ImportAsset(assetPath, ImportAssetOptions.ForceUpdate);
            TextureImporter importer = AssetImporter.GetAtPath(assetPath) as TextureImporter;
            if (importer == null)
            {
                throw new InvalidOperationException($"Texture importer is missing for {assetPath}.");
            }

            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Single;
            importer.spritePixelsPerUnit = IconSize;
            importer.mipmapEnabled = false;
            importer.alphaIsTransparency = true;
            importer.textureCompression = TextureImporterCompression.Uncompressed;
            importer.wrapMode = TextureWrapMode.Clamp;
            importer.filterMode = FilterMode.Bilinear;
            importer.SaveAndReimport();

            Sprite sprite = AssetDatabase.LoadAssetAtPath<Sprite>(assetPath);
            if (sprite == null)
            {
                throw new InvalidOperationException($"Sprite import failed for {assetPath}.");
            }

            return sprite;
        }

        private static Color PaintCleanDream(float u, float v)
        {
            Color badge = PaintBadge(u, v, new Color(0.14f, 0.62f, 0.72f, 1f), new Color(0.72f, 0.96f, 1f, 1f));
            if (badge.a <= 0f)
            {
                return badge;
            }

            float x = u - 0.5f;
            float y = v - 0.5f;
            float diamond = Mathf.Abs(x) + Mathf.Abs(y);
            if (diamond < 0.24f)
            {
                return new Color(0.96f, 1f, 0.78f, 1f);
            }

            if (diamond < 0.32f)
            {
                return new Color(0.36f, 0.88f, 0.96f, 1f);
            }

            return badge;
        }

        private static Color PaintNightmareDream(float u, float v)
        {
            Color badge = PaintBadge(u, v, new Color(0.29f, 0.18f, 0.45f, 1f), new Color(0.72f, 0.52f, 0.92f, 1f));
            if (badge.a <= 0f)
            {
                return badge;
            }

            float x = u - 0.5f;
            float y = v - 0.5f;
            float wave = Mathf.Sin((u * 19f) + (v * 12f)) * 0.04f;
            if (Mathf.Abs(x) < 0.19f && y < 0.26f + wave && y > -0.32f)
            {
                return new Color(0.08f, 0.05f, 0.13f, 1f);
            }

            if (Mathf.Abs(x + 0.2f) + Mathf.Abs(y - 0.12f) < 0.12f)
            {
                return new Color(0.1f, 0.04f, 0.16f, 1f);
            }

            return badge;
        }

        private static Color PaintWetState(float u, float v)
        {
            Color badge = PaintBadge(u, v, new Color(0.08f, 0.36f, 0.78f, 1f), new Color(0.5f, 0.84f, 1f, 1f));
            if (badge.a <= 0f)
            {
                return badge;
            }

            float x = u - 0.5f;
            float y = v - 0.38f;
            bool dropBody = (x * x / 0.055f) + (y * y / 0.09f) < 1f;
            bool dropTip = v > 0.48f && v < 0.82f && Mathf.Abs(x) < (0.82f - v) * 0.34f;
            if (dropBody || dropTip)
            {
                return new Color(0.82f, 0.96f, 1f, 1f);
            }

            return badge;
        }

        private static Color PaintDryState(float u, float v)
        {
            Color badge = PaintBadge(u, v, new Color(0.75f, 0.44f, 0.12f, 1f), new Color(1f, 0.9f, 0.45f, 1f));
            if (badge.a <= 0f)
            {
                return badge;
            }

            float x = u - 0.5f;
            float y = v - 0.5f;
            float distance = Mathf.Sqrt((x * x) + (y * y));
            float angle = Mathf.Atan2(y, x);
            bool ray = distance > 0.21f && distance < 0.37f && Mathf.Abs(Mathf.Sin(angle * 4f)) < 0.24f;
            if (distance < 0.19f || ray)
            {
                return new Color(1f, 0.95f, 0.55f, 1f);
            }

            return badge;
        }

        private static Color PaintWasherMachine(float u, float v)
        {
            Color badge = PaintBadge(u, v, new Color(0.12f, 0.31f, 0.54f, 1f), new Color(0.52f, 0.78f, 1f, 1f));
            if (badge.a <= 0f)
            {
                return badge;
            }

            float x = Mathf.Abs(u - 0.5f);
            float y = Mathf.Abs(v - 0.5f);
            if (x < 0.29f && y < 0.32f)
            {
                float drumX = u - 0.5f;
                float drumY = v - 0.44f;
                float drum = Mathf.Sqrt((drumX * drumX) + (drumY * drumY));
                if (drum < 0.16f)
                {
                    return new Color(0.72f, 0.93f, 1f, 1f);
                }

                if (v > 0.68f && Mathf.Abs(u - 0.32f) < 0.045f)
                {
                    return new Color(0.72f, 0.93f, 1f, 1f);
                }

                return new Color(0.2f, 0.47f, 0.72f, 1f);
            }

            return badge;
        }

        private static Color PaintDryerMachine(float u, float v)
        {
            Color badge = PaintBadge(u, v, new Color(0.62f, 0.29f, 0.12f, 1f), new Color(1f, 0.72f, 0.38f, 1f));
            if (badge.a <= 0f)
            {
                return badge;
            }

            float x = u - 0.5f;
            float y = v - 0.5f;
            float distance = Mathf.Sqrt((x * x) + (y * y));
            if (distance < 0.28f && Mathf.Abs(Mathf.Sin((u * 18f) + (v * 10f))) < 0.4f)
            {
                return new Color(1f, 0.86f, 0.54f, 1f);
            }

            if (Mathf.Abs(y) < 0.05f && u > 0.28f && u < 0.76f)
            {
                return new Color(1f, 0.86f, 0.54f, 1f);
            }

            return badge;
        }

        private static Color PaintSubmitOrder(float u, float v)
        {
            Color badge = PaintBadge(u, v, new Color(0.13f, 0.46f, 0.3f, 1f), new Color(0.62f, 0.92f, 0.7f, 1f));
            if (badge.a <= 0f)
            {
                return badge;
            }

            bool paper = u > 0.3f && u < 0.7f && v > 0.24f && v < 0.78f;
            if (paper)
            {
                float checkA = Mathf.Abs((v - 0.38f) - ((u - 0.36f) * 0.8f));
                float checkB = Mathf.Abs((v - 0.38f) + ((u - 0.56f) * 1.15f));
                if ((u > 0.36f && u < 0.5f && checkA < 0.035f) || (u >= 0.48f && u < 0.68f && checkB < 0.035f))
                {
                    return new Color(0.08f, 0.45f, 0.22f, 1f);
                }

                return new Color(0.92f, 0.98f, 0.9f, 1f);
            }

            return badge;
        }

        private static Color PaintStorageBasket(float u, float v)
        {
            Color badge = PaintBadge(u, v, new Color(0.34f, 0.34f, 0.38f, 1f), new Color(0.82f, 0.82f, 0.78f, 1f));
            if (badge.a <= 0f)
            {
                return badge;
            }

            bool basketBody = v > 0.28f && v < 0.58f && u > 0.24f + ((v - 0.28f) * 0.2f) && u < 0.76f - ((v - 0.28f) * 0.2f);
            bool rim = v > 0.57f && v < 0.64f && u > 0.25f && u < 0.75f;
            bool handle = v > 0.61f && v < 0.78f && Mathf.Abs(Mathf.Sqrt(Mathf.Pow(u - 0.5f, 2f) + Mathf.Pow((v - 0.61f) * 1.3f, 2f)) - 0.24f) < 0.035f;
            if (rim || handle)
            {
                return new Color(0.94f, 0.9f, 0.78f, 1f);
            }

            if (basketBody)
            {
                float stripe = Mathf.Abs(Mathf.Sin(u * 28f));
                return stripe > 0.58f ? new Color(0.82f, 0.75f, 0.58f, 1f) : new Color(0.66f, 0.58f, 0.42f, 1f);
            }

            return badge;
        }

        private static Color PaintBadge(float u, float v, Color fill, Color border)
        {
            float x = u - 0.5f;
            float y = v - 0.5f;
            float distance = Mathf.Sqrt((x * x) + (y * y));
            if (distance > 0.48f)
            {
                return new Color(0f, 0f, 0f, 0f);
            }

            if (distance > 0.4f)
            {
                return border;
            }

            float highlight = Mathf.Clamp01(1f - (distance / 0.4f));
            return Color.Lerp(fill, border, highlight * 0.28f);
        }

        private static void CreateMainScene(LevelCatalog catalog, UiIconCatalog iconCatalog)
        {
            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            scene.name = "MainGame";

            GameObject cameraObject = new GameObject("Main Camera", typeof(Camera));
            cameraObject.tag = "MainCamera";
            Camera camera = cameraObject.GetComponent<Camera>();
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = new Color(0.08f, 0.09f, 0.12f, 1f);
            camera.orthographic = true;
            camera.orthographicSize = 5f;

            GameObject gameObject = new GameObject("DreamLaundromatGame", typeof(DreamLaundromatGame));
            DreamLaundromatGame game = gameObject.GetComponent<DreamLaundromatGame>();
            game.Configure(catalog, iconCatalog);

            EditorSceneManager.SaveScene(scene, ScenePath);
            EditorBuildSettings.scenes = new[] { new EditorBuildSettingsScene(ScenePath, true) };
        }
    }
}
