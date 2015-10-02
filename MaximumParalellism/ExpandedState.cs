using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UltraDES;

namespace MaximumParalellism
{
    [Serializable]
    class ExpandedState : State
    {
        public uint Tasks { get; private set; }

        public ExpandedState(string alias, uint tasks, Marking marking = Marking.Unmarked)
            : base(alias, marking)
        {
            Tasks = tasks;
        }

        public override AbstractState ToMarked
        {
            get
            {
                return IsMarked ? this : new ExpandedState(Alias, Tasks, Marking.Marked);
            }
        }

        public override AbstractState ToUnmarked
        {
            get
            {
                return !IsMarked ? this : new ExpandedState(Alias, Tasks, Marking.Unmarked);
            }
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(this, obj)) return true;

            // If parameter cannot be cast to Point return false.
            var p = obj as State;
            if ((Object)p == null) return false;

            // Return true if the fields match:
            return Alias == p.Alias && Marking == p.Marking;
        }

        public override AbstractCompoundState MergeWith(AbstractState s2, int count, bool allMarked)
        {
            return ((IsMarked || s2.IsMarked) && !allMarked)
                ? (AbstractCompoundState)new CompoundExpandedState(this, s2, count).ToMarked
                : new CompoundExpandedState(this, s2, count);
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
