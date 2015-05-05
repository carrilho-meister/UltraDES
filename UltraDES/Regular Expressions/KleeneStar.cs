using System;

namespace UltraDES
{
    [Serializable]
    public class KleeneStar : RegularExpression
    {
        private readonly RegularExpression _a;

        public KleeneStar(RegularExpression a)
        {
            _a = a;
        }

        public override RegularExpression StepSimplify
        {
            get
            {
                if (_a == Symbol.Empty) return Symbol.Epsilon;
                if (_a == Symbol.Epsilon) return Symbol.Epsilon;
                return new KleeneStar(_a.StepSimplify);
            }
        }

        public override int GetHashCode()
        {
            return _a.GetHashCode();
        }

        public override string ToString()
        {
            return String.Format("({0})*", _a);
        }

        public override bool Equals(object obj)
        {
            if (!(obj is KleeneStar)) return false;
            return _a == ((KleeneStar) obj)._a;
        }
    }
}