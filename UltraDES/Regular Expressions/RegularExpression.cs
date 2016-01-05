using System;

namespace UltraDES
{
    [Serializable]
    public abstract class RegularExpression
    {
        public abstract RegularExpression StepSimplify { get; }

        public RegularExpression Simplify
        {
            get
            {
                var exp = this;
                var sim = exp.StepSimplify;

                while (sim != exp)
                {
                    exp = sim;
                    sim = exp.StepSimplify;
                }

                return sim;
            }
        }

        public abstract override int GetHashCode();
        public abstract override string ToString();
        public abstract override bool Equals(object obj);

        public static bool operator ==(RegularExpression a, RegularExpression b)
        {
            if (ReferenceEquals(a, b)) return true;
            return !ReferenceEquals(a, null) && a.Equals(b);
        }

        public static bool operator !=(RegularExpression a, RegularExpression b)
        {
            return !(a == b);
        }

        public static RegularExpression operator +(RegularExpression a, RegularExpression b)
        {
            return new Union(a, b);
        }

        public static RegularExpression operator *(RegularExpression a, RegularExpression b)
        {
            return new Concatenation(a, b);
        }
    }
}