namespace Thkim.PocketDodger.Gameplay
{
    public sealed class ScoreCounter
    {
        private readonly DifficultySettings _difficultySettings;
        private float _elapsedSeconds;
        private int _bonusScore;

        public ScoreCounter(DifficultySettings difficultySettings)
        {
            _difficultySettings = difficultySettings;
        }

        public float ElapsedSeconds => _elapsedSeconds;
        public int Score => _difficultySettings.GetScore(_elapsedSeconds, _bonusScore);

        public void Reset()
        {
            _elapsedSeconds = 0.0f;
            _bonusScore = 0;
        }

        public void Tick(float deltaTime)
        {
            if (deltaTime > 0.0f)
            {
                _elapsedSeconds += deltaTime;
            }
        }

        public void AddBonus(int amount)
        {
            if (amount > 0)
            {
                _bonusScore += amount;
            }
        }
    }
}
