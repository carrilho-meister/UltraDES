using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UltraDES;

namespace MaximumParalellism
{
    [Serializable]
    class CompoundExpandedState : CompoundState
    {
        public uint Tasks { get; private set; }

        public CompoundExpandedState(AbstractState s1, AbstractState s2, int count) : base(s1, s2, count)
        {
            var i = s1 is ExpandedState
                ? ((ExpandedState)s1).Tasks
                : s1 is CompoundExpandedState ? ((CompoundExpandedState)s1).Tasks : 0;
            var j = s2 is ExpandedState
                ? ((ExpandedState)s2).Tasks
                : s2 is CompoundExpandedState ? ((CompoundExpandedState)s2).Tasks : 0;

            Tasks = i + j;
        }

        public CompoundExpandedState(AbstractState s1, AbstractState s2, Marking marking) : base(s1, s2, marking)
        {
            var i = s1 is ExpandedState
                ? ((ExpandedState)s1).Tasks
                : s1 is CompoundExpandedState ? ((CompoundExpandedState)s1).Tasks : 0;
            var j = s2 is ExpandedState
                ? ((ExpandedState)s2).Tasks
                : s2 is CompoundExpandedState ? ((CompoundExpandedState)s2).Tasks : 0;

            Tasks = i + j;
        }

        public override AbstractState ToMarked
        {
            get
            {
                return IsMarked ? this : new CompoundExpandedState(S1, S2, Marking.Marked);
            }
        }

        public override AbstractState ToUnmarked
        {
            get
            {
                return !IsMarked ? this : new CompoundExpandedState(S1, S2, Marking.Unmarked);
            }
        }

        public override AbstractCompoundState MergeWith(AbstractState s2, int count = 0, bool allMarked = true)
        {
            return ((IsMarked || s2.IsMarked) && !allMarked)
                ? (AbstractCompoundState)(new CompoundExpandedState(this, s2, count)).ToMarked
                : (new CompoundExpandedState(this, s2, count));
        }
    }
}
