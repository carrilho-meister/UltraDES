using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace UltraDES
{
    [Serializable]
    public class AdjacencyMatrix
    {
        private readonly SortedList<int, int>[] _internal;

        public AdjacencyMatrix(int states)
        {
            _internal = new SortedList<int, int>[states];
        }

        public int this[int s, int e]
        {
            get
            {
                if (s >= _internal.Length || s < 0 || _internal[s] == null) return -1;
                return _internal[s].ContainsKey(e) ? _internal[s][e] : -1;
            }
        }

        public SortedList<int, int> this[int s]
        {
            get { return _internal[s] ?? (_internal[s] = new SortedList<int, int>()); }
        }

        public int Length
        {
            get { return _internal.Length; }
        }

        public void Add(int origin, Tuple<int, int>[] values)
        {
            _internal[origin] = new SortedList<int, int>(values.Length);
            foreach (var value in values)
                _internal[origin].Add(value.Item1, value.Item2);
        }

        public void TrimExcess()
        {
            Parallel.ForEach(_internal.Where(i => i != null), i => i.TrimExcess());
        }
    }
}