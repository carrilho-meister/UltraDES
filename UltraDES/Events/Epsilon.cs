using System;

namespace UltraDES
{
    [Serializable]
    public class Epsilon : AbstractEvent
    {
        private Epsilon()
        {
            Controllability = Controllability.Controllable;
        }

        public static Epsilon EpsilonEvent { get; } = new Epsilon();

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(this, obj)) return true;

            // If parameter cannot be cast to Point return false.
            var p = obj as Epsilon;
            if ((object) p == null) return false;

            // Return true if the fields match:
            return true;
        }

        public override int GetHashCode()
        {
            return "epsilon".GetHashCode();
        }

        public override string ToString()
        {
            return "\u03B5";
        }
    }
}