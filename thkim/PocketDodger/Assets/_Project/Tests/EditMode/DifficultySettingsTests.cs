using NUnit.Framework;
using Thkim.PocketDodger.Gameplay;
using UnityEngine;

namespace Thkim.PocketDodger.Tests.EditMode
{
    public sealed class DifficultySettingsTests
    {
        [Test]
        public void DifficultyValues_InterpolateAndClampByElapsedTime()
        {
            DifficultySettings settings = ScriptableObject.CreateInstance<DifficultySettings>();
            settings.ConfigureForSetup(4.0f, 10.0f, 1.1f, 0.45f, 60.0f, 10, 0);

            Assert.AreEqual(4.0f, settings.GetObstacleSpeed(0.0f), 0.001f);
            Assert.AreEqual(7.0f, settings.GetObstacleSpeed(30.0f), 0.001f);
            Assert.AreEqual(10.0f, settings.GetObstacleSpeed(999.0f), 0.001f);

            Assert.AreEqual(1.1f, settings.GetSpawnInterval(0.0f), 0.001f);
            Assert.AreEqual(0.775f, settings.GetSpawnInterval(30.0f), 0.001f);
            Assert.AreEqual(0.45f, settings.GetSpawnInterval(999.0f), 0.001f);

            Object.DestroyImmediate(settings);
        }

        [Test]
        public void Score_UsesElapsedSecondsAndBonus()
        {
            DifficultySettings settings = ScriptableObject.CreateInstance<DifficultySettings>();
            settings.ConfigureForSetup(4.0f, 10.0f, 1.1f, 0.45f, 60.0f, 10, 25);

            Assert.AreEqual(0, settings.GetScore(-1.0f, 0));
            Assert.AreEqual(12, settings.GetScore(1.25f, 0));
            Assert.AreEqual(37, settings.GetScore(1.25f, 25));

            Object.DestroyImmediate(settings);
        }
    }
}
