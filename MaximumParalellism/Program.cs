using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UltraDES;

namespace MaximumParalellism
{
    class Program
    {
        static void Main(string[] args)
        {
            var G = IndustrialTransferLineSBAI();

            int numProducts = 1000;
            int productEvents = 12;

            var path = MaxParallelPath(G, numProducts*productEvents, G.InitialState);

            Console.WriteLine(path.Aggregate("", (a, b) => a + ";" + b));
            Console.ReadLine();
        }

        private static DeterministicFiniteAutomaton IndustrialTransferLineSBAI()
        {

            var s = Enumerable.Range(0, 4)
                .ToDictionary(i => i, i => new ExpandedState(i.ToString(), i == 0 ? 0u : 1u, i == 0 ? Marking.Marked : Marking.Unmarked));

            var e = (new[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12 }).ToDictionary(ev => ev,
                ev => new Event(ev.ToString(), ev % 2 == 0 ? Controllability.Uncontrollable : Controllability.Controllable));

            var m1 = new DeterministicFiniteAutomaton(
                new[]
                {
                    new Transition(s[0], e[1], s[1]),
                    new Transition(s[1], e[2], s[0])
                },
                s[0], "M1");

            var m2 = new DeterministicFiniteAutomaton(
                new[]
                {
                    new Transition(s[0], e[3], s[1]),
                    new Transition(s[1], e[4], s[0])
                },
                s[0], "M2");

            var m3 = new DeterministicFiniteAutomaton(
                new[]
                {
                    new Transition(s[0], e[5], s[1]),
                    new Transition(s[1], e[6], s[0])
                },
                s[0], "M3");

            var m4 = new DeterministicFiniteAutomaton(
                new[]
                {
                    new Transition(s[0], e[7], s[1]),
                    new Transition(s[1], e[8], s[0])
                },
                s[0], "M4");

            var m5 = new DeterministicFiniteAutomaton(
                new[]
                {
                    new Transition(s[0], e[9], s[1]),
                    new Transition(s[1], e[10], s[0])
                },
                s[0], "M5");

            var m6 = new DeterministicFiniteAutomaton(
                new[]
                {
                    new Transition(s[0], e[11], s[1]),
                    new Transition(s[1], e[12], s[0])
                },
                s[0], "M6");

            s = Enumerable.Range(0, 4)
                .ToDictionary(i => i,
                    i => new ExpandedState(i.ToString(), 0u, i == 0 ? Marking.Marked : Marking.Unmarked));

            var e1 = new DeterministicFiniteAutomaton(
                new[]
                {
                    new Transition(s[0], e[2], s[1]),
                    new Transition(s[1], e[3], s[0])
                },
                s[0], "E1");

            var e2 = new DeterministicFiniteAutomaton(
                new[]
                {
                    new Transition(s[0], e[6], s[1]),
                    new Transition(s[1], e[7], s[0])
                },
                s[0], "E2");

            var e3 = new DeterministicFiniteAutomaton(
                new[]
                {
                    new Transition(s[0], e[4], s[1]),
                    new Transition(s[1], e[8], s[2]),
                    new Transition(s[0], e[8], s[3]),
                    new Transition(s[3], e[4], s[2]),
                    new Transition(s[2], e[9], s[0])
                },
                s[0], "E3");

            var e4 = new DeterministicFiniteAutomaton(
                new[]
                {
                    new Transition(s[0], e[10], s[1]),
                    new Transition(s[1], e[11], s[0])
                },
                s[0], "E4");

            return DeterministicFiniteAutomaton.MonoliticSupervisor(new[] { m1, m2, m3, m4, m5, m6 },
                new[] { e1, e2, e3, e4 }, true);
            }

        private static IEnumerable<AbstractEvent> MaxParallelPath(DeterministicFiniteAutomaton g, int depth,
            AbstractState target)
        {
            var transitions = g.Transitions.AsParallel().GroupBy(t => t.Origin).ToDictionary(gr => gr.Key, gr => gr.ToArray());

            var distance = new ConcurrentDictionary<AbstractState, Tuple<uint, ImmutableList<AbstractEvent>>>();
            distance.TryAdd(g.InitialState, Tuple.Create(0u, ImmutableList<AbstractEvent>.Empty));

            for (var i = 0; i < depth; i++)
            {
                var nextDistance = new ConcurrentDictionary<AbstractState, Tuple<uint, ImmutableList<AbstractEvent>>>();

                if (distance.Count < 100)
                {
                    foreach (var kvp in distance)
                    {
                        var s1 = kvp.Key;
                        var d = kvp.Value;

                        foreach (var t in transitions[s1])
                        {
                            var s2 = t.Destination;
                            var e = t.Trigger;

                            var w = d.Item1;
                            if (s2 is ExpandedState) w += ((ExpandedState)s2).Tasks;
                            else if (s2 is CompoundExpandedState) w += ((CompoundExpandedState)s2).Tasks;

                            var lst = d.Item2.Add(e);

                            nextDistance.AddOrUpdate(s2, Tuple.Create(w, lst),
                                (key, old) => w > old.Item1 ? Tuple.Create(w, lst) : old);
                        }
                    }
                }
                else
                {

                    Parallel.ForEach(distance, kvp =>
                    {
                        var s1 = kvp.Key;
                        var d = kvp.Value;

                        //Parallel.ForEach(transitions[s1], t =>
                        foreach (var t in transitions[s1])
                        {
                            var s2 = t.Destination;
                            var e = t.Trigger;

                            var w = d.Item1;
                            if (s2 is ExpandedState) w += ((ExpandedState)s2).Tasks;
                            else if (s2 is CompoundExpandedState) w += ((CompoundExpandedState)s2).Tasks;

                            var lst = d.Item2.Add(e);

                            nextDistance.AddOrUpdate(s2, Tuple.Create(w, lst),
                                (key, old) => w > old.Item1 ? Tuple.Create(w, lst) : old);
                        } //);
                    });
                }

                distance = nextDistance;
            }

            return !distance.ContainsKey(target) ? ImmutableList<AbstractEvent>.Empty : distance[target].Item2;
        }
    }
}
