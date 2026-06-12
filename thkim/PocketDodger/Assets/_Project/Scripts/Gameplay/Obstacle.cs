using System;
using UnityEngine;

namespace Thkim.PocketDodger.Gameplay
{
    public sealed class Obstacle : MonoBehaviour
    {
        [SerializeField] private Transform visualRoot;
        [SerializeField] private float spinDegreesPerSecond = 110.0f;
        [SerializeField] private float pulseAmount = 0.035f;

        private float _speed;
        private float _despawnY;
        private float _spinDirection = 1.0f;
        private float _pulseTimer;
        private Vector3 _visualBaseScale = Vector3.one;
        private bool _isActive;
        private bool _hasVisualBaseScale;

        public Action<Obstacle> Despawned;
        public Action<Obstacle> PlayerHit;
        public LaneIndex Lane { get; private set; }

        private void Awake()
        {
            CacheVisualBaseScale();
        }

        private void Update()
        {
            if (!_isActive)
            {
                return;
            }

            transform.position += Vector3.down * (_speed * Time.deltaTime);
            UpdateVisual();

            if (transform.position.y <= _despawnY)
            {
                _isActive = false;
                Despawned?.Invoke(this);
            }
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (!_isActive || !other.CompareTag("Player"))
            {
                return;
            }

            _isActive = false;
            PlayerHit?.Invoke(this);
        }

        public void Activate(LaneIndex lane, Vector3 position, float speed, float despawnY)
        {
            Lane = lane;
            _speed = speed;
            _despawnY = despawnY;
            _isActive = true;
            transform.position = position;
            CacheVisualBaseScale();
            ResetVisualForActivation();
            gameObject.SetActive(true);
        }

        public void ConfigureVisual(Transform root)
        {
            visualRoot = root;
            _hasVisualBaseScale = false;
            CacheVisualBaseScale();
        }

        public void Deactivate()
        {
            _isActive = false;
            Despawned = null;
            PlayerHit = null;
            gameObject.SetActive(false);
        }

        private void UpdateVisual()
        {
            if (visualRoot == null)
            {
                return;
            }

            float deltaTime = Time.deltaTime;
            _pulseTimer += deltaTime * 6.0f;
            float pulse = 1.0f + Mathf.Sin(_pulseTimer) * pulseAmount;
            visualRoot.localScale = _visualBaseScale * pulse;
            visualRoot.Rotate(0.0f, 0.0f, _spinDirection * spinDegreesPerSecond * deltaTime, Space.Self);
        }

        private void ResetVisualForActivation()
        {
            if (visualRoot == null)
            {
                return;
            }

            _pulseTimer = UnityEngine.Random.Range(0.0f, Mathf.PI * 2.0f);
            _spinDirection = UnityEngine.Random.value < 0.5f ? -1.0f : 1.0f;
            visualRoot.localScale = _visualBaseScale;
            visualRoot.localRotation = Quaternion.Euler(0.0f, 0.0f, UnityEngine.Random.Range(-22.0f, 22.0f));
        }

        private void CacheVisualBaseScale()
        {
            if (visualRoot == null && transform.childCount > 0)
            {
                visualRoot = transform.GetChild(0);
            }

            if (visualRoot == null || _hasVisualBaseScale)
            {
                return;
            }

            _visualBaseScale = visualRoot.localScale;
            _hasVisualBaseScale = true;
        }
    }
}
