using System;

namespace UltraDES
{
    [Serializable]
    public abstract class Symbol : RegularExpression
    {
        public override RegularExpression StepSimplify
        {
            get { return this; }
        }

        public static Symbol Epsilon { get; } = UltraDES.Epsilon.EpsilonEvent;

        public static Symbol Empty { get; } = UltraDES.Empty.EmptyEvent;

        public abstract override string ToString();
        public abstract override int GetHashCode();
        public abstract override bool Equals(object obj);
    }
}