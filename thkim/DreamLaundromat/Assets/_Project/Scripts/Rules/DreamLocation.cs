using System;

namespace Thkim.DreamLaundromat.Rules
{
    [Serializable]
    public struct DreamLocation : IEquatable<DreamLocation>
    {
        public LocationKind Kind;
        public string Id;

        public DreamLocation(LocationKind kind, string id)
        {
            Kind = kind;
            Id = id ?? string.Empty;
        }

        public static DreamLocation Queue()
        {
            return new DreamLocation(LocationKind.Queue, string.Empty);
        }

        public static DreamLocation Basket(string id)
        {
            return new DreamLocation(LocationKind.Basket, id);
        }

        public static DreamLocation Machine(string id)
        {
            return new DreamLocation(LocationKind.Machine, id);
        }

        public static DreamLocation Submitted(string orderId)
        {
            return new DreamLocation(LocationKind.Submitted, orderId);
        }

        public bool Equals(DreamLocation other)
        {
            return Kind == other.Kind && string.Equals(Id, other.Id, StringComparison.Ordinal);
        }

        public override bool Equals(object obj)
        {
            return obj is DreamLocation other && Equals(other);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return ((int)Kind * 397) ^ (Id != null ? StringComparer.Ordinal.GetHashCode(Id) : 0);
            }
        }

        public override string ToString()
        {
            return string.IsNullOrEmpty(Id) ? Kind.ToString() : $"{Kind}:{Id}";
        }
    }
}
