using System;

namespace UltraDES
{
    [Serializable]
    public abstract class Symbol : RegularExpression
    {
        private static readonly Symbol epsilon = UltraDES.Epsilon.EpsilonEvent;
        private static readonly Symbol empty = UltraDES.Empty.EmptyEvent;

        public override RegularExpression StepSimplify
        {
            get { return this; }
        }

        public static Symbol Epsilon
        {
            get { return epsilon; }
        }

        public static Symbol Empty
        {
            get { return empty; }
        }

        public abstract override string ToString();
        public abstract override int GetHashCode();
        public abstract override bool Equals(object obj);
    }
}