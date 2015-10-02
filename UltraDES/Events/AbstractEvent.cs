using System;

namespace UltraDES
{
    [Serializable]
    public abstract class AbstractEvent : Symbol
    {

        public Controllability Controllability { get; protected set; }

        public bool IsControllable
        {
            get { return Controllability == Controllability.Controllable; }
        }

        public abstract override string ToString();
        public abstract override int GetHashCode();

        public abstract override bool Equals(object obj);

        public static bool operator ==(AbstractEvent a, AbstractEvent b)
        {
            if (ReferenceEquals(a, b)) return true;
            return !ReferenceEquals(a, null) && a.Equals(b);
        }

        public static bool operator !=(AbstractEvent a, AbstractEvent b)
        {
            return !(a == b);
        }
    }

    public enum Controllability : byte
    {
        Controllable = 1,
        Uncontrollable = 0
    }
}