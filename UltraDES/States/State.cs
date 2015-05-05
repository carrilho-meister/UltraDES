using System;

namespace UltraDES
{
    [Serializable]
    public class State : AbstractState
    {
        public State(string alias, Marking marking = Marking.Unmarked)
        {
            Alias = alias;
            Marking = marking;
        }

        public string Alias { get; private set; }

        public override AbstractState ToMarked
        {
            get
            {
                return IsMarked ? this : new State(Alias, Marking.Marked);
            }
        }

        public override AbstractState ToUnmarked
        {
            get
            {
                return !IsMarked ? this : new State(Alias, Marking.Unmarked);
            }
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(this, obj)) return true;

            // If parameter cannot be cast to Point return false.
            var p = obj as State;
            if ((Object) p == null) return false;

            // Return true if the fields match:
            return Alias == p.Alias && Marking == p.Marking;
        }

        public override AbstractCompoundState MergeWith(AbstractState s2, int count, bool allMarked)
        {
            return ((IsMarked || s2.IsMarked) && !allMarked)
                ? (AbstractCompoundState) new CompoundState(this, s2, count).ToMarked
                : new CompoundState(this, s2, count);
        }

        public override int GetHashCode()
        {
            return Alias.GetHashCode();
        }

        public override string ToString()
        {
            return Alias;
        }
    }
}