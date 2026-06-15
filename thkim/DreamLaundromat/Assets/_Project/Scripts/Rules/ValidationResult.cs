using System.Collections.Generic;

namespace Thkim.DreamLaundromat.Rules
{
    public sealed class ValidationResult
    {
        private readonly List<string> _errors = new List<string>();
        private readonly List<string> _warnings = new List<string>();

        public IReadOnlyList<string> Errors => _errors;
        public IReadOnlyList<string> Warnings => _warnings;
        public bool IsValid => _errors.Count == 0;

        public void AddError(string message)
        {
            _errors.Add(message);
        }

        public void AddWarning(string message)
        {
            _warnings.Add(message);
        }
    }
}
