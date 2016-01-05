using System;

namespace UltraDES
{
    [Serializable]
    public class Union : RegularExpression
    {
        private readonly RegularExpression _a;
        private readonly RegularExpression _b;

        public Union(RegularExpression a, RegularExpression b)
        {
            _a = a;
            _b = b;
        }

        public override RegularExpression StepSimplify
        {
            get
            {
                if (_a == _b) return _a;
                if (_a is Union)
                {
                    var u = (Union) _a;
                    return new Union(u._a, new Union(u._b, _b)).StepSimplify;
                }
                if (_a == Symbol.Empty) return _b;
                if (_b == Symbol.Empty) return _a;
                return new Union(_a.StepSimplify, _b.StepSimplify);
            }
        }

        public override int GetHashCode()
        {
            return _a.GetHashCode() ^ _b.GetHashCode();
        }

        public override string ToString()
        {
            return string.Format("({0} + {1})", _a, _b);
        }

        public override bool Equals(object obj)
        {
            if (!(obj is Union)) return false;
            var union1 = this;
            var union2 = (Union) obj;

            return (union1._a == union2._a && union1._b == union2._b) ||
                   (union1._a == union2._b && union1._b == union2._a);
        }
    }
}