using System;

namespace UltraDES
{
    [Serializable]
    public class Event : AbstractEvent
    {
        private readonly int _hashcode;

        public Event(string alias, Controllability controllability)
        {
            Alias = alias;
            Controllability = controllability;
            _hashcode = Alias.GetHashCode();
        }

        public string Alias { get; }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(this, obj)) return true;

            // If parameter cannot be cast to Point return false.
            var p = obj as Event;
            if ((object) p == null) return false;

            // Return true if the fields match:
            return Alias == p.Alias && Controllability == p.Controllability;
        }

        public override int GetHashCode()
        {
            return _hashcode;
        }

        public override string ToString()
        {
            return Alias;
        }
    }
}