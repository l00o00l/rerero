using UnityEngine;

namespace Thkim.PocketDodger.Gameplay
{
    public sealed class LaneLayout : MonoBehaviour
    {
        [SerializeField] private float laneSpacing = 2.4f;
        [SerializeField] private float playerY = -3.6f;
        [SerializeField] private float spawnY = 5.5f;
        [SerializeField] private float despawnY = -6.2f;

        public float PlayerY => playerY;
        public float SpawnY => spawnY;
        public float DespawnY => despawnY;

        public Vector3 GetLanePosition(LaneIndex lane, float y)
        {
            return new Vector3(GetLaneX(lane), y, 0.0f);
        }

        public float GetLaneX(LaneIndex lane)
        {
            return ((int)lane - 1) * laneSpacing;
        }

        public void Configure(float spacing, float playerPositionY, float spawnPositionY, float despawnPositionY)
        {
            laneSpacing = spacing;
            playerY = playerPositionY;
            spawnY = spawnPositionY;
            despawnY = despawnPositionY;
        }
    }
}
