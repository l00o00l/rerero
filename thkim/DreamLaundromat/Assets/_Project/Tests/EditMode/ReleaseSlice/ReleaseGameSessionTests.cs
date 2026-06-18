using NUnit.Framework;
using Thkim.DreamLaundromat.DynamicLab;
using Thkim.DreamLaundromat.Gameplay.ReleaseSlice;

namespace Thkim.DreamLaundromat.Tests.EditMode.ReleaseSlice
{
    public sealed class ReleaseGameSessionTests
    {
        [Test]
        public void StartLevel_CreatesPlayingRoundState()
        {
            var session = new ReleaseGameSession(ReleaseLevelPack.CreateDefault());

            session.StartLevel(0);

            Assert.That(session.CurrentLevel.LevelId, Is.EqualTo("DL-RS-001"));
            Assert.That(session.CurrentState, Is.Not.Null);
            Assert.That(session.CurrentState.Status, Is.EqualTo(DynamicRoundStatus.Playing));
            Assert.That(session.LastMessage, Is.EqualTo(session.CurrentLevel.Guidance));
        }

        [Test]
        public void RestartLevel_RecreatesInitialMoveCount()
        {
            var session = new ReleaseGameSession(ReleaseLevelPack.CreateDefault());
            session.StartLevel(0);
            int initialMoves = session.CurrentState.RemainingMoves;
            DynamicSolveResult solve = DynamicRoundSolver.Solve(session.CurrentLevel.CreateRoundDefinition());

            session.Apply(solve.FirstSolutionActions[0]);
            Assert.That(session.CurrentState.RemainingMoves, Is.LessThan(initialMoves));

            session.RestartLevel();

            Assert.That(session.CurrentState.RemainingMoves, Is.EqualTo(initialMoves));
            Assert.That(session.CurrentState.Status, Is.EqualTo(DynamicRoundStatus.Playing));
        }

        [Test]
        public void SolverSolution_ClearsCurrentLevelThroughSession()
        {
            var session = new ReleaseGameSession(ReleaseLevelPack.CreateDefault());
            session.StartLevel(0);
            DynamicSolveResult solve = DynamicRoundSolver.Solve(session.CurrentLevel.CreateRoundDefinition());

            for (int i = 0; i < solve.FirstSolutionActions.Count; i++)
            {
                DynamicActionResult result = session.Apply(solve.FirstSolutionActions[i]);
                Assert.That(result.Success, Is.True, result.Message);
            }

            Assert.That(session.CurrentState.Status, Is.EqualTo(DynamicRoundStatus.Cleared));
        }

        [Test]
        public void TryStartNextLevel_AdvancesWhenAvailable()
        {
            var session = new ReleaseGameSession(ReleaseLevelPack.CreateDefault());
            session.StartLevel(0);
            DynamicSolveResult solve = DynamicRoundSolver.Solve(session.CurrentLevel.CreateRoundDefinition());
            for (int i = 0; i < solve.FirstSolutionActions.Count; i++)
            {
                session.Apply(solve.FirstSolutionActions[i]);
            }

            bool advanced = session.TryStartNextLevel();

            Assert.That(advanced, Is.True);
            Assert.That(session.CurrentLevelIndex, Is.EqualTo(1));
            Assert.That(session.CurrentLevel.LevelId, Is.EqualTo("DL-RS-002"));
        }

        [Test]
        public void TryStartNextLevel_BlocksUntilCurrentLevelIsCleared()
        {
            var session = new ReleaseGameSession(ReleaseLevelPack.CreateDefault());
            session.StartLevel(0);

            bool advanced = session.TryStartNextLevel();

            Assert.That(advanced, Is.False);
            Assert.That(session.CurrentLevelIndex, Is.EqualTo(0));
        }

        [Test]
        public void StartDefaultLevel_UsesSavedHighestUnlockedLevel()
        {
            var store = new ReleaseMemoryProgressStore();
            store.Save(new ReleaseProgressState { HighestUnlockedLevelIndex = 2 });
            var session = new ReleaseGameSession(ReleaseLevelPack.CreateDefault(), store);

            session.StartDefaultLevel();

            Assert.That(session.CurrentLevelIndex, Is.EqualTo(2));
            Assert.That(session.CurrentLevel.LevelId, Is.EqualTo("DL-RS-003"));
        }

        [Test]
        public void ClearingLevel_SavesCompletionAndUnlocksNextLevel()
        {
            var store = new ReleaseMemoryProgressStore();
            var recorder = new ReleaseFeedbackRecorder();
            var session = new ReleaseGameSession(ReleaseLevelPack.CreateDefault(), store, recorder);
            session.StartLevel(0);
            DynamicSolveResult solve = DynamicRoundSolver.Solve(session.CurrentLevel.CreateRoundDefinition());

            for (int i = 0; i < solve.FirstSolutionActions.Count; i++)
            {
                session.Apply(solve.FirstSolutionActions[i]);
            }

            ReleaseProgressState saved = store.Load();
            Assert.That(saved.IsLevelCompleted("DL-RS-001"), Is.True);
            Assert.That(saved.IsLevelUnlocked(1), Is.True);
            Assert.That(recorder.Events.Exists(feedback => feedback.Type == ReleaseFeedbackEventType.LevelCleared), Is.True);
        }

        [Test]
        public void GuidedTutorial_BlocksWrongFirstActionWithoutSpendingMove()
        {
            var session = new ReleaseGameSession(ReleaseLevelPack.CreateDefault());
            session.StartLevel(0);
            int moves = session.CurrentState.RemainingMoves;

            DynamicActionResult result = session.Apply(DynamicPlayerAction.ApplyOperation(0, DynamicOperation.Settle));

            Assert.That(result.Success, Is.False);
            Assert.That(session.CurrentState.RemainingMoves, Is.EqualTo(moves));
            Assert.That(session.LastMessage, Does.Contain("Tutorial"));
        }
    }
}
