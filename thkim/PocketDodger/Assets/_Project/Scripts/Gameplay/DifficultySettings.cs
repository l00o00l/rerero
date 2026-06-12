using UnityEngine;

namespace Thkim.PocketDodger.Gameplay
{
    [CreateAssetMenu(menuName = "PocketDodger/Difficulty Settings")]
    public sealed class DifficultySettings : ScriptableObject
    {
        [SerializeField] private float startObstacleSpeed = 4.0f;
        [SerializeField] private float maxObstacleSpeed = 10.0f;
        [SerializeField] private float startSpawnInterval = 1.1f;
        [SerializeField] private float minSpawnInterval = 0.45f;
        [SerializeField] private float rampDurationSeconds = 60.0f;
        [SerializeField] private int baseScorePerSecond = 10;
        [SerializeField] private int obstacleDodgeBonus = 0;

        public int ObstacleDodgeBonus => obstacleDodgeBonus;

        public float GetObstacleSpeed(float elapsedSeconds)
        {
            return Mathf.Lerp(startObstacleSpeed, maxObstacleSpeed, GetRamp01(elapsedSeconds));
        }

        public float GetSpawnInterval(float elapsedSeconds)
        {
            return Mathf.Lerp(startSpawnInterval, minSpawnInterval, GetRamp01(elapsedSeconds));
        }

        public int GetScore(float elapsedSeconds, int bonusScore)
        {
            return Mathf.Max(0, Mathf.FloorToInt(elapsedSeconds * baseScorePerSecond) + bonusScore);
        }

        public void ConfigureForSetup(
            float startSpeed,
            float maxSpeed,
            float firstSpawnInterval,
            float finalSpawnInterval,
            float rampSeconds,
            int scorePerSecond,
            int dodgeBonus)
        {
            startObstacleSpeed = startSpeed;
            maxObstacleSpeed = maxSpeed;
            startSpawnInterval = firstSpawnInterval;
            minSpawnInterval = finalSpawnInterval;
            rampDurationSeconds = rampSeconds;
            baseScorePerSecond = scorePerSecond;
            obstacleDodgeBonus = dodgeBonus;
        }

        private float GetRamp01(float elapsedSeconds)
        {
            if (rampDurationSeconds <= 0.0f)
            {
                return 1.0f;
            }

            return Mathf.Clamp01(elapsedSeconds / rampDurationSeconds);
        }
    }
}
