using System;

namespace UltraDES
{
    [Serializable]
    public  class CompoundState : AbstractCompoundState
    {
        private readonly int _hashcode;

        public CompoundState(AbstractState s1, AbstractState s2, int count)
        {
            S1 = s1;
            S2 = s2;
            Marking = (s1.Marking == s2.Marking) ? s1.Marking : Marking.Unmarked;
            _hashcode = s1.GetHashCode()*count + s2.GetHashCode();
        }

        public CompoundState(AbstractState s1, AbstractState s2, Marking marking)
        {
            S1 = s1;
            S2 = s2;
            Marking = marking;
            _hashcode = s1.GetHashCode() ^ s2.GetHashCode();
        }

        public override AbstractState S1 { get; protected set; }
        public override AbstractState S2 { get; protected set; }

        public override AbstractState ToMarked
        {
            get
            {
                return IsMarked ? this : new CompoundState(S1, S2, Marking.Marked);
            }
        }

        public override AbstractState ToUnmarked
        {
            get
            {
                return !IsMarked ? this : new CompoundState(S1, S2, Marking.Unmarked);
            }
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(this, obj)) return true;

            // If parameter cannot be cast to Point return false.
            var p = obj as CompoundState;
            if ((Object) p == null) return false;

            if (_hashcode != p._hashcode) return false;

            // Return true if the fields match:
            return S1 == p.S1 && S2 == p.S2 && Marking == p.Marking;
        }

        public override AbstractCompoundState MergeWith(AbstractState s2, int count = 0, bool allMarked = true)
        {
            return ((IsMarked || s2.IsMarked) && !allMarked)
                ? (AbstractCompoundState)(new CompoundState(this, s2, count)).ToMarked
                : (new CompoundState(this, s2, count));
        }

        public override int GetHashCode()
        {
            return _hashcode;
        }

        public override string ToString()
        {
            return string.Format("{0}|{1}", S1, S2);
        }
    }
}