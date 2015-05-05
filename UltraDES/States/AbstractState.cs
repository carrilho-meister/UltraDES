using System;

namespace UltraDES
{
    [Serializable]
    public abstract class AbstractState
    {

        public Marking Marking { get; protected set; }

        public bool IsMarked
        {
            get { return Marking == Marking.Marked; }
        }

        public abstract override string ToString();
        public abstract override int GetHashCode();
        public abstract override bool Equals(object obj);

        public abstract AbstractState ToMarked { get; }

        public abstract AbstractState ToUnmarked { get; }

        public abstract AbstractCompoundState MergeWith(AbstractState s2, int count = 0, bool allMarked = true);

        public static bool operator ==(AbstractState a, AbstractState b)
        {
            if (ReferenceEquals(a, b)) return true;
            return !ReferenceEquals(a, null) && a.Equals(b);
        }

        public static bool operator !=(AbstractState a, AbstractState b)
        {
            return !(a == b);
        }
    }

    public enum Marking : byte
    {
        Marked = 1,
        Unmarked = 0
    }
}