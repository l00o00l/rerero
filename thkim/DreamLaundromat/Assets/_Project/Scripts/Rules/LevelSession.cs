using System.Collections.Generic;
using Thkim.DreamLaundromat.Levels;

namespace Thkim.DreamLaundromat.Rules
{
    public sealed class LevelSession
    {
        private readonly LevelDefinition _definition;
        private readonly Stack<LevelState> _history = new Stack<LevelState>();

        public LevelSession(LevelDefinition definition)
        {
            _definition = definition;
            State = RulesEngine.CreateInitialState(definition);
        }

        public LevelState State { get; private set; }

        public void Restart()
        {
            _history.Clear();
            State = RulesEngine.CreateInitialState(_definition);
        }

        public ActionResult Apply(PlayerAction action)
        {
            LevelState snapshot = State.Clone();
            ActionResult result = RulesEngine.Apply(State, action);

            if (result.Success && result.ConsumedTurn)
            {
                _history.Push(snapshot);
            }

            return result;
        }

        public bool Undo()
        {
            if (_history.Count == 0)
            {
                return false;
            }

            State = _history.Pop();
            return true;
        }
    }
}
