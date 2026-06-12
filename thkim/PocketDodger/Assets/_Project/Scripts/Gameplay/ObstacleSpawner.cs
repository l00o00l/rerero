using UnityEngine;

namespace Thkim.PocketDodger.Gameplay
{
    public sealed class ObstacleSpawner : MonoBehaviour
    {
        [SerializeField] private LaneLayout laneLayout;
        [SerializeField] private ObstaclePool obstaclePool;
        [SerializeField] private DifficultySettings difficultySettings;

        private float _elapsedSeconds;
        private float _spawnTimer;
        private bool _isRunning;

        public System.Action PlayerHit;
        public System.Action ObstacleDodged;

        private void Update()
        {
            if (!_isRunning)
            {
                return;
            }

            _elapsedSeconds += Time.deltaTime;
            _spawnTimer -= Time.deltaTime;

            if (_spawnTimer <= 0.0f)
            {
                SpawnObstacle();
                _spawnTimer += difficultySettings.GetSpawnInterval(_elapsedSeconds);
            }
        }

        public void Configure(LaneLayout layout, ObstaclePool pool, DifficultySettings settings)
        {
            laneLayout = layout;
            obstaclePool = pool;
            difficultySettings = settings;
        }

        public void ResetSpawner()
        {
            _elapsedSeconds = 0.0f;
            _spawnTimer = 0.0f;
            _isRunning = false;
        }

        public void StartSpawning()
        {
            ResetSpawner();
            _isRunning = true;
        }

        public void StopSpawning()
        {
            _isRunning = false;
        }

        private void SpawnObstacle()
        {
            if (laneLayout == null || obstaclePool == null || difficultySettings == null)
            {
                return;
            }

            LaneIndex lane = LaneIndexExtensions.Clamp(Random.Range(0, LaneIndexExtensions.LaneCount));
            Obstacle obstacle = obstaclePool.Rent();
            obstacle.Despawned = HandleObstacleDespawned;
            obstacle.PlayerHit = HandlePlayerHit;
            obstacle.Activate(
                lane,
                laneLayout.GetLanePosition(lane, laneLayout.SpawnY),
                difficultySettings.GetObstacleSpeed(_elapsedSeconds),
                laneLayout.DespawnY);
        }

        private void HandleObstacleDespawned(Obstacle obstacle)
        {
            obstaclePool.Return(obstacle);
            ObstacleDodged?.Invoke();
        }

        private void HandlePlayerHit(Obstacle obstacle)
        {
            obstaclePool.Return(obstacle);
            PlayerHit?.Invoke();
        }
    }
}
