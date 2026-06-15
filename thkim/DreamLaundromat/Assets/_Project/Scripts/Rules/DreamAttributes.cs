using System;

namespace Thkim.DreamLaundromat.Rules
{
    [Serializable]
    public struct DreamAttributes : IEquatable<DreamAttributes>
    {
        public DreamStain Stain;
        public DreamMoisture Moisture;

        public DreamAttributes(DreamStain stain, DreamMoisture moisture)
        {
            Stain = stain;
            Moisture = moisture;
        }

        public bool Equals(DreamAttributes other)
        {
            return Stain == other.Stain && Moisture == other.Moisture;
        }

        public override bool Equals(object obj)
        {
            return obj is DreamAttributes other && Equals(other);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return ((int)Stain * 397) ^ (int)Moisture;
            }
        }

        public override string ToString()
        {
            return $"stain={Stain}, moisture={Moisture}";
        }
    }
}
