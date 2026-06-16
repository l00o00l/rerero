using System.Collections.Generic;

namespace Thkim.DreamLaundromat.DynamicLab
{
    internal struct SeededRandom
    {
        private uint _state;

        public SeededRandom(int seed)
        {
            _state = seed == 0 ? 0x6D2B79F5u : unchecked((uint)seed);
        }

        public int NextInt(int exclusiveMax)
        {
            _state = unchecked((_state * 1664525u) + 1013904223u);
            return (int)(_state % (uint)exclusiveMax);
        }
    }

    internal static class SeededShuffler
    {
        public static void Shuffle<T>(IList<T> items, int seed)
        {
            var random = new SeededRandom(seed);
            for (int i = items.Count - 1; i > 0; i--)
            {
                int swapIndex = random.NextInt(i + 1);
                T temp = items[i];
                items[i] = items[swapIndex];
                items[swapIndex] = temp;
            }
        }
    }
}
