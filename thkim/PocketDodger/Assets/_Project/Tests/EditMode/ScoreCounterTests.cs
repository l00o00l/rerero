using NUnit.Framework;
using Thkim.PocketDodger.Gameplay;
using UnityEngine;

namespace Thkim.PocketDodger.Tests.EditMode
{
    public sealed class ScoreCounterTests
    {
        [Test]
        public void TickAndReset_UpdateScoreDeterministically()
        {
            DifficultySettings settings = ScriptableObject.CreateInstance<DifficultySettings>();
            settings.ConfigureForSetup(4.0f, 10.0f, 1.1f, 0.45f, 60.0f, 10, 5);
            ScoreCounter counter = new ScoreCounter(settings);

            counter.Tick(1.2f);
            counter.AddBonus(5);

            Assert.AreEqual(1.2f, counter.ElapsedSeconds, 0.001f);
            Assert.AreEqual(17, counter.Score);

            counter.Reset();

            Assert.AreEqual(0.0f, counter.ElapsedSeconds, 0.001f);
            Assert.AreEqual(0, counter.Score);

            Object.DestroyImmediate(settings);
        }
    }
}
