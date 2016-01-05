using System;

namespace UltraDES
{
    [Serializable]
    public class Empty : AbstractEvent
    {
        private Empty()
        {
            Controllability = Controllability.Controllable;
        }

        public static Empty EmptyEvent { get; } = new Empty();

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(this, obj)) return true;

            // If parameter cannot be cast to Point return false.
            var p = obj as Empty;
            if ((object) p == null) return false;

            // Return true if the fields match:
            return true;
        }

        public override int GetHashCode()
        {
            return "empty".GetHashCode();
        }

        public override string ToString()
        {
            return "\u2205";
        }
    }
}