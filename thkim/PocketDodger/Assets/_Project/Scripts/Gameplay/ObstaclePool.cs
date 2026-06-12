using System.Collections.Generic;
using UnityEngine;

namespace Thkim.PocketDodger.Gameplay
{
    public sealed class ObstaclePool : MonoBehaviour
    {
        [SerializeField] private Obstacle obstaclePrefab;
        [SerializeField] private Transform poolRoot;
        [SerializeField] private int initialSize = 8;

        private readonly List<Obstacle> _obstacles = new List<Obstacle>();

        private void Awake()
        {
            WarmUp();
        }

        public void Configure(Obstacle prefab, Transform root, int size)
        {
            obstaclePrefab = prefab;
            poolRoot = root;
            initialSize = size;
        }

        public Obstacle Rent()
        {
            for (int i = 0; i < _obstacles.Count; i++)
            {
                if (!_obstacles[i].gameObject.activeSelf)
                {
                    return _obstacles[i];
                }
            }

            return CreateObstacle();
        }

        public void Return(Obstacle obstacle)
        {
            if (obstacle != null)
            {
                obstacle.Deactivate();
            }
        }

        public void ReturnAll()
        {
            for (int i = 0; i < _obstacles.Count; i++)
            {
                _obstacles[i].Deactivate();
            }
        }

        private void WarmUp()
        {
            if (obstaclePrefab == null)
            {
                return;
            }

            for (int i = _obstacles.Count; i < initialSize; i++)
            {
                CreateObstacle();
            }
        }

        private Obstacle CreateObstacle()
        {
            Transform parent = poolRoot == null ? transform : poolRoot;
            Obstacle obstacle = Instantiate(obstaclePrefab, parent);
            obstacle.name = "Obstacle";
            obstacle.Deactivate();
            _obstacles.Add(obstacle);
            return obstacle;
        }
    }
}
