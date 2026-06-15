using NUnit.Framework;
using Thkim.DreamLaundromat.Levels;
using Thkim.DreamLaundromat.Rules;
using Thkim.DreamLaundromat.UI;
using UnityEditor;

namespace Thkim.DreamLaundromat.Tests.EditMode
{
    public sealed class LevelValidatorTests
    {
        [Test]
        public void PrototypeLevels_AllPassValidation()
        {
            LevelCatalog catalog = AssetDatabase.LoadAssetAtPath<LevelCatalog>("Assets/_Project/ScriptableObjects/LevelCatalog.asset");
            Assert.That(catalog, Is.Not.Null);
            Assert.That(catalog.Levels, Has.Length.EqualTo(10));

            for (int i = 0; i < catalog.Levels.Length; i++)
            {
                ValidationResult result = LevelValidator.Validate(catalog.Levels[i]);
                Assert.That(result.Errors, Is.Empty, catalog.Levels[i].LevelId);
            }
        }

        [Test]
        public void PrototypeLevels_IntroduceCoreRulesInFirstFourLevels()
        {
            LevelCatalog catalog = AssetDatabase.LoadAssetAtPath<LevelCatalog>("Assets/_Project/ScriptableObjects/LevelCatalog.asset");
            Assert.That(catalog, Is.Not.Null);
            Assert.That(catalog.Levels, Has.Length.GreaterThanOrEqualTo(4));

            Assert.That(catalog.Levels[0].Machines, Is.Empty, "DL-001 should only teach direct submit.");
            Assert.That(catalog.Levels[1].Machines, Has.Length.EqualTo(1), "DL-002 should introduce washing.");
            Assert.That(catalog.Levels[1].Machines[0].Type, Is.EqualTo(MachineType.Washer));

            Assert.That(catalog.Levels[2].Machines, Has.Length.EqualTo(2), "DL-003 should introduce washer + dryer chaining.");
            Assert.That(catalog.Levels[2].Machines[0].Type, Is.EqualTo(MachineType.Washer));
            Assert.That(catalog.Levels[2].Machines[1].Type, Is.EqualTo(MachineType.Dryer));

            Assert.That(catalog.Levels[3].Baskets[0].Capacity, Is.EqualTo(1), "DL-004 should make storage pressure visible.");
            Assert.That(catalog.Levels[3].Baskets[1].Capacity, Is.EqualTo(1));
        }

        [Test]
        public void UiIconCatalog_IsComplete()
        {
            UiIconCatalog catalog = AssetDatabase.LoadAssetAtPath<UiIconCatalog>("Assets/_Project/ScriptableObjects/UiIconCatalog.asset");

            Assert.That(catalog, Is.Not.Null);
            Assert.That(catalog.IsComplete, Is.True);
        }
    }
}
