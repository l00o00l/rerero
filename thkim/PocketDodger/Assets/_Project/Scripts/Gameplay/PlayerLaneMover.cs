using UnityEngine;

namespace Thkim.PocketDodger.Gameplay
{
    public sealed class PlayerLaneMover : MonoBehaviour
    {
        [SerializeField] private LaneLayout laneLayout;
        [SerializeField] private LaneIndex startLane = LaneIndex.Center;
        [SerializeField] private float moveDuration = 0.16f;
        [SerializeField] private Transform visualRoot;
        [SerializeField] private float visualFeedbackDuration = 0.18f;
        [SerializeField] private float visualLeanDegrees = 12.0f;
        [SerializeField] private float visualStretch = 0.08f;

        private LaneIndex _currentLane;
        private Vector3 _moveStart;
        private Vector3 _moveTarget;
        private float _moveTimer;
        private Vector3 _visualBaseScale = Vector3.one;
        private float _visualFeedbackTimer;
        private int _visualDirection;
        private bool _hasVisualBaseScale;

        public LaneIndex CurrentLane => _currentLane;

        private void Awake()
        {
            CacheVisualBaseScale();
            ResetToStartLane();
        }

        private void Update()
        {
            UpdateMovement();
            UpdateVisualFeedback();
        }

        public void Configure(LaneLayout layout, LaneIndex initialLane, float duration)
        {
            laneLayout = layout;
            startLane = initialLane;
            moveDuration = duration;
        }

        public void ConfigureVisual(Transform root)
        {
            visualRoot = root;
            _hasVisualBaseScale = false;
            CacheVisualBaseScale();
        }

        public void ResetToStartLane()
        {
            MoveTo(startLane, true);
            ResetVisualFeedback();
        }

        public bool MoveLeft()
        {
            return MoveTo(_currentLane.Move(-1), false);
        }

        public bool MoveRight()
        {
            return MoveTo(_currentLane.Move(1), false);
        }

        private void UpdateMovement()
        {
            if (_moveTimer >= moveDuration)
            {
                return;
            }

            _moveTimer += Time.deltaTime;
            float t = moveDuration <= 0.0f ? 1.0f : Mathf.Clamp01(_moveTimer / moveDuration);
            transform.position = Vector3.LerpUnclamped(_moveStart, _moveTarget, EaseOutCubic(t));
        }

        private void UpdateVisualFeedback()
        {
            if (visualRoot == null || _visualFeedbackTimer >= visualFeedbackDuration)
            {
                return;
            }

            _visualFeedbackTimer += Time.deltaTime;
            float t = visualFeedbackDuration <= 0.0f ? 1.0f : Mathf.Clamp01(_visualFeedbackTimer / visualFeedbackDuration);
            float pulse = Mathf.Sin(t * Mathf.PI);
            float stretch = visualStretch * pulse;
            float lean = -_visualDirection * visualLeanDegrees * pulse;

            visualRoot.localScale = new Vector3(
                _visualBaseScale.x + stretch,
                _visualBaseScale.y - stretch * 0.45f,
                _visualBaseScale.z);
            visualRoot.localRotation = Quaternion.Euler(0.0f, 0.0f, lean);

            if (_visualFeedbackTimer >= visualFeedbackDuration)
            {
                ResetVisualFeedback();
            }
        }

        private bool MoveTo(LaneIndex lane, bool immediate)
        {
            LaneIndex previousLane = _currentLane;
            if (!immediate && lane == previousLane)
            {
                return false;
            }

            _currentLane = lane;

            if (laneLayout == null)
            {
                return false;
            }

            _moveStart = transform.position;
            _moveTarget = laneLayout.GetLanePosition(_currentLane, laneLayout.PlayerY);

            if (immediate || moveDuration <= 0.0f)
            {
                _moveTimer = moveDuration;
                transform.position = _moveTarget;
                return true;
            }

            _visualDirection = Mathf.Clamp((int)_currentLane - (int)previousLane, -1, 1);
            _visualFeedbackTimer = 0.0f;
            _moveTimer = 0.0f;
            return true;
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

        private void ResetVisualFeedback()
        {
            if (visualRoot == null)
            {
                return;
            }

            CacheVisualBaseScale();
            _visualFeedbackTimer = visualFeedbackDuration;
            _visualDirection = 0;
            visualRoot.localScale = _visualBaseScale;
            visualRoot.localRotation = Quaternion.identity;
        }

        private static float EaseOutCubic(float value)
        {
            float inverse = 1.0f - value;
            return 1.0f - inverse * inverse * inverse;
        }
    }
}
