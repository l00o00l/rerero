using UnityEngine;

namespace Thkim.PocketDodger.Infrastructure
{
    public static class HighScoreStore
    {
        private const string Key = "PocketDodger.HighScore";

        public static int Load()
        {
            return PlayerPrefs.GetInt(Key, 0);
        }

        public static int SaveIfHigher(int score)
        {
            int currentHighScore = Load();

            if (score <= currentHighScore)
            {
                return currentHighScore;
            }

            PlayerPrefs.SetInt(Key, score);
            PlayerPrefs.Save();
            return score;
        }
    }
}
