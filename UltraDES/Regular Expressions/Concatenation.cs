using System;

namespace UltraDES
{
    [Serializable]
    public class Concatenation : RegularExpression
    {
        private readonly RegularExpression _a;
        private readonly RegularExpression _b;

        public Concatenation(RegularExpression a, RegularExpression b)
        {
            _a = a;
            _b = b;
        }

        public override RegularExpression StepSimplify
        {
            get
            {
                if (_a is Concatenation)
                {
                    var c = (Concatenation) _a;
                    return new Concatenation(c._a, new Concatenation(c._b, _b)).StepSimplify;
                }
                if (_a == Symbol.Epsilon) return _b.StepSimplify;
                if (_b == Symbol.Epsilon) return _a.StepSimplify;
                if (_a == Symbol.Empty) return Symbol.Empty;
                if (_b == Symbol.Empty) return Symbol.Empty;
                return new Concatenation(_a.StepSimplify, _b.StepSimplify);
            }
        }

        public override int GetHashCode()
        {
            return _a.GetHashCode() ^ _b.GetHashCode();
        }

        public override string ToString()
        {
            return String.Format("{0}.{1}", _a, _b);
        }

        public override bool Equals(object obj)
        {
            if (!(obj is Concatenation)) return false;
            var concat1 = this;
            var concat2 = (Concatenation) obj;

            return (concat1._a == concat2._a && concat1._b == concat2._b);
        }
    }
}