using Thkim.PocketDodger.Input;
using Thkim.PocketDodger.Infrastructure;
using Thkim.PocketDodger.UI;
using UnityEngine;

namespace Thkim.PocketDodger.Gameplay
{
    public sealed class GameController : MonoBehaviour
    {
        [SerializeField] private PlayerLaneMover player;
        [SerializeField] private ObstaclePool obstaclePool;
        [SerializeField] private ObstacleSpawner obstacleSpawner;
        [SerializeField] private PlayerInputRouter inputRouter;
        [SerializeField] private DifficultySettings difficultySettings;
        [SerializeField] private GameHudPresenter hudPresenter;
        [SerializeField] private StartPanelPresenter startPanelPresenter;
        [SerializeField] private GameOverPresenter gameOverPresenter;
        [SerializeField] private SimpleSfxPlayer sfxPlayer;
        [SerializeField] private Transform cameraTransform;
        [SerializeField] private float hitShakeDuration = 0.18f;
        [SerializeField] private float hitShakeMagnitude = 0.10f;

        private ScoreCounter _scoreCounter;
        private Vector3 _cameraBasePosition;
        private float _hitShakeTimer;

        public GameState State { get; private set; } = GameState.Ready;
        public int CurrentScore => _scoreCounter == null ? 0 : _scoreCounter.Score;

        private void Awake()
        {
            if (cameraTransform != null)
            {
                _cameraBasePosition = cameraTransform.localPosition;
            }

            _scoreCounter = new ScoreCounter(difficultySettings);
            inputRouter.Initialize(HandleMoveCommand);
            obstacleSpawner.PlayerHit = EndGame;
            obstacleSpawner.ObstacleDodged = HandleObstacleDodged;
            startPanelPresenter.Initialize(StartGame);
            gameOverPresenter.Initialize(StartGame);
            EnterReady();
        }

        private void Update()
        {
            UpdateCameraShake();

            if (State != GameState.Playing)
            {
                return;
            }

            _scoreCounter.Tick(Time.deltaTime);
            hudPresenter.SetScore(_scoreCounter.Score);
        }

        public void Configure(
            PlayerLaneMover playerMover,
            ObstaclePool pool,
            ObstacleSpawner spawner,
            PlayerInputRouter router,
            DifficultySettings settings,
            GameHudPresenter hud,
            StartPanelPresenter startPanel,
            GameOverPresenter gameOverPanel,
            SimpleSfxPlayer sfx,
            Transform cameraRoot)
        {
            player = playerMover;
            obstaclePool = pool;
            obstacleSpawner = spawner;
            inputRouter = router;
            difficultySettings = settings;
            hudPresenter = hud;
            startPanelPresenter = startPanel;
            gameOverPresenter = gameOverPanel;
            sfxPlayer = sfx;
            cameraTransform = cameraRoot;
        }

        private void EnterReady()
        {
            State = GameState.Ready;
            obstacleSpawner.StopSpawning();
            obstaclePool.ReturnAll();
            player.ResetToStartLane();
            _scoreCounter.Reset();
            hudPresenter.SetScore(0);
            hudPresenter.SetHighScore(HighScoreStore.Load());
            startPanelPresenter.SetVisible(true);
            gameOverPresenter.SetVisible(false);
        }

        private void StartGame()
        {
            State = GameState.Playing;
            obstaclePool.ReturnAll();
            player.ResetToStartLane();
            _scoreCounter.Reset();
            hudPresenter.SetScore(0);
            hudPresenter.SetHighScore(HighScoreStore.Load());
            startPanelPresenter.SetVisible(false);
            gameOverPresenter.SetVisible(false);
            obstacleSpawner.StartSpawning();
        }

        private void EndGame()
        {
            if (State != GameState.Playing)
            {
                return;
            }

            State = GameState.GameOver;
            obstacleSpawner.StopSpawning();
            obstaclePool.ReturnAll();
            StartHitShake();
            if (sfxPlayer != null)
            {
                sfxPlayer.PlayHit();
            }
            int highScore = HighScoreStore.SaveIfHigher(_scoreCounter.Score);
            hudPresenter.SetHighScore(highScore);
            gameOverPresenter.Show(_scoreCounter.Score, highScore);
        }

        private void HandleMoveCommand(MoveCommand command)
        {
            if (State != GameState.Playing)
            {
                return;
            }

            if (command == MoveCommand.Left)
            {
                bool moved = player.MoveLeft();
                if (moved && sfxPlayer != null)
                {
                    sfxPlayer.PlayMove();
                }
            }
            else if (command == MoveCommand.Right)
            {
                bool moved = player.MoveRight();
                if (moved && sfxPlayer != null)
                {
                    sfxPlayer.PlayMove();
                }
            }
        }

        private void HandleObstacleDodged()
        {
            if (State == GameState.Playing)
            {
                _scoreCounter.AddBonus(difficultySettings.ObstacleDodgeBonus);
            }
        }

        private void StartHitShake()
        {
            if (cameraTransform == null)
            {
                return;
            }

            _hitShakeTimer = hitShakeDuration;
        }

        private void UpdateCameraShake()
        {
            if (cameraTransform == null || _hitShakeTimer <= 0.0f)
            {
                return;
            }

            _hitShakeTimer -= Time.unscaledDeltaTime;
            float normalized = hitShakeDuration <= 0.0f ? 0.0f : Mathf.Clamp01(_hitShakeTimer / hitShakeDuration);

            if (normalized <= 0.0f)
            {
                cameraTransform.localPosition = _cameraBasePosition;
                return;
            }

            float time = Time.unscaledTime;
            float x = Mathf.Sin(time * 91.7f) * hitShakeMagnitude * normalized;
            float y = Mathf.Cos(time * 117.3f) * hitShakeMagnitude * normalized;
            cameraTransform.localPosition = _cameraBasePosition + new Vector3(x, y, 0.0f);
        }
    }
}
