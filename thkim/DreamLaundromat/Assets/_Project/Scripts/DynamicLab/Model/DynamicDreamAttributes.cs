using System;

namespace Thkim.DreamLaundromat.DynamicLab
{
    public readonly struct DynamicDreamAttributes : IEquatable<DynamicDreamAttributes>
    {
        public DynamicDreamAttributes(
            DreamTaint taint,
            DreamMood mood,
            DreamClarity clarity,
            DreamStability stability)
        {
            Taint = taint;
            Mood = mood;
            Clarity = clarity;
            Stability = stability;
        }

        public DreamTaint Taint { get; }
        public DreamMood Mood { get; }
        public DreamClarity Clarity { get; }
        public DreamStability Stability { get; }

        public DynamicDreamAttributes WithTaint(DreamTaint taint)
        {
            return new DynamicDreamAttributes(taint, Mood, Clarity, Stability);
        }

        public DynamicDreamAttributes WithMood(DreamMood mood)
        {
            return new DynamicDreamAttributes(Taint, mood, Clarity, Stability);
        }

        public DynamicDreamAttributes WithClarity(DreamClarity clarity)
        {
            return new DynamicDreamAttributes(Taint, Mood, clarity, Stability);
        }

        public DynamicDreamAttributes WithStability(DreamStability stability)
        {
            return new DynamicDreamAttributes(Taint, Mood, Clarity, stability);
        }

        public bool Equals(DynamicDreamAttributes other)
        {
            return Taint == other.Taint
                && Mood == other.Mood
                && Clarity == other.Clarity
                && Stability == other.Stability;
        }

        public override bool Equals(object obj)
        {
            return obj is DynamicDreamAttributes other && Equals(other);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int hash = 17;
                hash = (hash * 31) + (int)Taint;
                hash = (hash * 31) + (int)Mood;
                hash = (hash * 31) + (int)Clarity;
                hash = (hash * 31) + (int)Stability;
                return hash;
            }
        }

        public static bool operator ==(DynamicDreamAttributes left, DynamicDreamAttributes right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(DynamicDreamAttributes left, DynamicDreamAttributes right)
        {
            return !left.Equals(right);
        }
    }
}
