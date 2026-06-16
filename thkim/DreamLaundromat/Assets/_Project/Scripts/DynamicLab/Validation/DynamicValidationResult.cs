using System.Collections.Generic;

namespace Thkim.DreamLaundromat.DynamicLab
{
    public sealed class DynamicValidationResult
    {
        public readonly List<string> Errors = new List<string>();
        public readonly List<string> Warnings = new List<string>();

        public bool IsValid => Errors.Count == 0;

        public void AddError(string message)
        {
            Errors.Add(message);
        }

        public void AddWarning(string message)
        {
            Warnings.Add(message);
        }
    }
}
