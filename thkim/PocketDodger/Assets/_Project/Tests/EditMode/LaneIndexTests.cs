using NUnit.Framework;
using Thkim.PocketDodger.Gameplay;

namespace Thkim.PocketDodger.Tests.EditMode
{
    public sealed class LaneIndexTests
    {
        [Test]
        public void Clamp_ReturnsNearestValidLane()
        {
            Assert.AreEqual(LaneIndex.Left, LaneIndexExtensions.Clamp(-10));
            Assert.AreEqual(LaneIndex.Left, LaneIndexExtensions.Clamp(0));
            Assert.AreEqual(LaneIndex.Center, LaneIndexExtensions.Clamp(1));
            Assert.AreEqual(LaneIndex.Right, LaneIndexExtensions.Clamp(2));
            Assert.AreEqual(LaneIndex.Right, LaneIndexExtensions.Clamp(99));
        }

        [Test]
        public void Move_StopsAtLaneBounds()
        {
            Assert.AreEqual(LaneIndex.Left, LaneIndex.Left.Move(-1));
            Assert.AreEqual(LaneIndex.Center, LaneIndex.Left.Move(1));
            Assert.AreEqual(LaneIndex.Right, LaneIndex.Right.Move(1));
            Assert.AreEqual(LaneIndex.Center, LaneIndex.Right.Move(-1));
        }
    }
}
