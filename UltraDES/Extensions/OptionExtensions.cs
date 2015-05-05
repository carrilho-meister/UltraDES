using System.Collections.Generic;
using System.Linq;

namespace UltraDES
{
    public static class OptionExtensions
    {
        public static IEnumerable<T> OptionToValue<T>(this IEnumerable<Option<T>> list)
        {
            return list.OfType<Some<T>>().Select(op => op.Value);
        }

        public static IEnumerable<Some<T>> OnlySome<T>(this IEnumerable<Option<T>> list)
        {
            return list.OfType<Some<T>>();
        }
    }
}