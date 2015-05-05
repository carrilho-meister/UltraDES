using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using UltraDES.Data_Structures;
using ExpandedState = System.Tuple<UltraDES.AbstractState,UltraDES.Context,uint>;

namespace UltraDES.Extensions
{
    public static class AutomatonExtensions
    {
        public static string ToXML(this DeterministicFiniteAutomaton G)
        {
            var eventSet = G.Events.ToArray();
            var stateSet = G.States.ToArray();


            var doc = new XmlDocument();
            var automaton = (XmlElement)doc.AppendChild(doc.CreateElement("Automaton"));
            automaton.SetAttribute("Name", G.Name);

            var states = (XmlElement)automaton.AppendChild(doc.CreateElement("States"));
            for (int i = 0; i < stateSet.Length; i++)
            {
                var state = stateSet[i];

                var s = ((XmlElement) states.AppendChild(doc.CreateElement("State")));
                s.SetAttribute("Name", state.ToString());
                s.SetAttribute("Marking", state.Marking.ToString());
                s.SetAttribute("Id", i.ToString());
            }

            var initial = (XmlElement)automaton.AppendChild(doc.CreateElement("InitialState"));
            initial.SetAttribute("Id", Array.IndexOf(stateSet, G.InitialState).ToString());

            var events = (XmlElement)automaton.AppendChild(doc.CreateElement("Events"));
            for (int i = 0; i < eventSet.Length; i++)
            {
                var @event = eventSet[i];

                var e = ((XmlElement)events.AppendChild(doc.CreateElement("Event")));
                e.SetAttribute("Name", @event.ToString());
                e.SetAttribute("Controllability", @event.Controllability.ToString());
                e.SetAttribute("Id", i.ToString());
            }

            var transitions = (XmlElement)automaton.AppendChild(doc.CreateElement("Transitions"));
            for (int i = 0; i < stateSet.Length; i++)
            {
                var origin = stateSet[i];
                for (int j = 0; j < eventSet.Length; j++)
                {
                    var trigger = eventSet[j];
                    var destination = G.TransitionFunction(origin, trigger);
                    if (destination.IsNone) continue;

                    var t = (XmlElement)transitions.AppendChild(doc.CreateElement("Transition"));

                    t.SetAttribute("Origin", i.ToString());
                    t.SetAttribute("Trigger", j.ToString());
                    t.SetAttribute("Destination", Array.IndexOf(stateSet, destination.Value).ToString());
                }
            }

            return doc.OuterXml;
        }

        public static void ToXML(this DeterministicFiniteAutomaton G, string filepath)
        {
            var eventSet = G.Events.ToArray();
            var stateSet = G.States.ToArray();

            XmlWriterSettings settings = new XmlWriterSettings
            {
                Indent = true,
                IndentChars = ("\t"),
                NewLineChars = "\r\n",
                Async = true
            };

            using (var writer = XmlWriter.Create(filepath, settings))
            {

                writer.WriteStartElement("Automaton");
                writer.WriteAttributeString("Name", G.Name);

                writer.WriteStartElement("States");
                for (int i = 0; i < stateSet.Length; i++)
                {
                    var state = stateSet[i];

                    writer.WriteStartElement("State");
                    writer.WriteAttributeString("Name", state.ToString());
                    writer.WriteAttributeString("Marking", state.Marking.ToString());
                    writer.WriteAttributeString("Id", i.ToString());

                    writer.WriteEndElement();
                }

                writer.WriteEndElement();

                writer.WriteStartElement("InitialState");
                writer.WriteAttributeString("Id", Array.IndexOf(stateSet, G.InitialState).ToString());

                writer.WriteEndElement();

                writer.WriteStartElement("Events");
                for (int i = 0; i < eventSet.Length; i++)
                {
                    var @event = eventSet[i];

                    writer.WriteStartElement("Event");
                    writer.WriteAttributeString("Name", @event.ToString());
                    writer.WriteAttributeString("Controllability", @event.Controllability.ToString());
                    writer.WriteAttributeString("Id", i.ToString());

                    writer.WriteEndElement();
                }

                writer.WriteEndElement();

                writer.WriteStartElement("Transitions");
                for (int i = 0; i < stateSet.Length; i++)
                {
                    var origin = stateSet[i];
                    for (int j = 0; j < eventSet.Length; j++)
                    {
                        var trigger = eventSet[j];
                        var destination = G.TransitionFunction(origin, trigger);
                        if (destination.IsNone) continue;

                        writer.WriteStartElement("Transition");

                        writer.WriteAttributeString("Origin", i.ToString());
                        writer.WriteAttributeString("Trigger", j.ToString());
                        writer.WriteAttributeString("Destination", Array.IndexOf(stateSet, destination.Value).ToString());

                        writer.WriteEndElement();
                    }
                }

                writer.WriteEndElement();
            }
        }

        public static Dictionary<Tuple<AbstractState, int>, IEnumerable<AbstractEvent>> MaxParallelPath(
            this DeterministicFiniteAutomaton G, uint steps)
        {
            var distance = new Dictionary<Tuple<AbstractState, int>, int>();
            var path = new Dictionary<Tuple<AbstractState, int>, IEnumerable<AbstractEvent>>();
            distance.Add(Tuple.Create(G.InitialState, 1), 0);
            path.Add(Tuple.Create(G.InitialState, 1), new List<AbstractEvent>());
            
            var stop = false;
            var min = 1;
            var iterations = 1;

            while (!stop)
            {
                stop = true;
                for (var j = min; j < steps + 1; j++)
                {
                    bool step = true;
                    var j1 = j;
                    var states =
                        G.States.AsParallel()
                            .Select(s1 => Tuple.Create(s1, j1))
                            .Where(u => distance.ContainsKey(u))
                            .ToList();

                    foreach (var u in states)
                    {
                        foreach (var e in G.Events)
                        {
                            var s2 = G.TransitionFunction(u.Item1, e);

                            if (s2.IsNone) continue;

                            var v = Tuple.Create(s2.Value, j + 1);
                            var w = (e.IsControllable ? 1 : -1)*j;


                            if (!distance.ContainsKey(v))
                            {
                                distance.Add(v, distance[u] + w);

                                path.Add(v, path[u].Concat(new[]{e}));

                                stop = false;
                                step = false;
                            }
                            else
                            {
                                if (distance[u] + w >= distance[v]) continue;

                                distance[v] = distance[u] + w;
                                path[v] = path[u].Concat(new[] { e });
                                stop = false;
                                step = false;
                            }
                        }
                    }

                    if (j == min && step) min++;

                    //Console.WriteLine("Iteration {0} | Visited States {1}", iterations++, distance.Count);
                }
            }
            return path;
        }

        public static Dictionary<Tuple<AbstractState, int>, IEnumerable<AbstractEvent>> MaxParallelPath2(
            this DeterministicFiniteAutomaton G, uint steps)
        {
            var distance = new Dictionary<Tuple<AbstractState, int>, int>();
            var path = new Dictionary<Tuple<AbstractState, int>, IEnumerable<AbstractEvent>>();
            distance.Add(Tuple.Create(G.InitialState, 1), 0);
            path.Add(Tuple.Create(G.InitialState, 1), new List<AbstractEvent>());

            var stop = false;
            var min = 1;
            var iterations = 1;

            while (!stop)
            {
                stop = true;
                for (var j = min; j < steps + 1; j++)
                {
                    bool step = true;
                    var j1 = j;
                    var states =
                        G.States.AsParallel()
                            .Select(s1 => Tuple.Create(s1, j1))
                            .Where(u => distance.ContainsKey(u))
                            .ToList();

                    foreach (var u in states)
                    {
                        foreach (var e in G.Events)
                        {
                            var s2 = G.TransitionFunction(u.Item1, e);

                            if (s2.IsNone) continue;

                            var v = Tuple.Create(s2.Value, j + 1);
                            var w = (e.IsControllable ? -1 : +1) + distance[u];


                            if (!distance.ContainsKey(v))
                            {
                                distance.Add(v, distance[u] + w);

                                path.Add(v, path[u].Concat(new[] { e }));

                                stop = false;
                                step = false;
                            }
                            else
                            {
                                if (distance[u] + w >= distance[v]) continue;

                                distance[v] = distance[u] + w;
                                path[v] = path[u].Concat(new[] { e });
                                stop = false;
                                step = false;
                            }
                        }
                    }

                    if (j == min && step) min++;

                    //Console.WriteLine("Iteration {0} | Visited States {1}", iterations++, distance.Count);
                }
            }
            return path;
        }

        public static Dictionary<Tuple<AbstractState, int>, IEnumerable<AbstractEvent>> MaxParallelPathTimed(
            this DeterministicFiniteAutomaton G, uint steps, Dictionary<AbstractEvent, float> scheduler,
            Func<Dictionary<AbstractEvent, float>, AbstractEvent, Dictionary<AbstractEvent, float>> f)
        {
            var distance = new Dictionary<Tuple<AbstractState, int>, int>();
            var schedulers = new Dictionary<Tuple<AbstractState, int>, Dictionary<AbstractEvent, float>>();
            var path = new Dictionary<Tuple<AbstractState, int>, IEnumerable<AbstractEvent>>();
            distance.Add(Tuple.Create(G.InitialState, 1), 0);
            schedulers.Add(Tuple.Create(G.InitialState, 1), scheduler);
            path.Add(Tuple.Create(G.InitialState, 1), new List<AbstractEvent>());

            var stop = false;
            var min = 1;
            var iterations = 1;
            while (!stop)
            {
                stop = true;
                for (var j = min; j < steps + 1; j++)
                {
                    bool step = true;
                    var j1 = j;
                    var states =
                        G.States.AsParallel()
                            .Select(s1 => Tuple.Create(s1, j1))
                            .Where(u => distance.ContainsKey(u))
                            .ToList();

                    foreach (var u in states)
                    {
                        var evs =
                            G.Events.Where(
                                ev =>
                                    ev.IsControllable ||
                                    (!float.IsInfinity(schedulers[u][ev]) &&
                                     !schedulers[u].Any(ee => !ee.Key.IsControllable && ee.Value < schedulers[u][ev])))
                                .ToList();
                        foreach (var e in evs)
                        {
                            var s2 = G.TransitionFunction(u.Item1, e);

                            if (s2.IsNone) continue;

                            var v = Tuple.Create(s2.Value, j + 1);
                            var w = (e.IsControllable ? -1 : +1) + distance[u];


                            if (!distance.ContainsKey(v))
                            {
                                distance.Add(v, distance[u] + w);
                                schedulers.Add(v, f(schedulers[u], e));
                                path.Add(v, path[u].Concat(new[] {e}));

                                stop = false;
                                step = false;
                            }
                            else
                            {
                                if (distance[u] + w >= distance[v]) continue;

                                distance[v] = distance[u] + w;
                                schedulers[v] = f(schedulers[u], e);
                                path[v] = path[u].Concat(new[] {e});
                                stop = false;
                                step = false;
                            }
                        }
                    }

                    if (j == min && step) min++;

                    if (iterations++%1000 != 0) continue;

                    Console.WriteLine("Iteration {0} | Visited States {1}", iterations++, distance.Count);

                    GC.Collect();
                    GC.Collect();
                    GC.WaitForPendingFinalizers();
                }
            }
            return path;
        }
        public static List<Tuple<AbstractState, Context>> DijkstraShortestPath(
            this DeterministicFiniteAutomaton G, uint steps, AbstractState destination, Dictionary<AbstractEvent, float> scheduler,
            Func<Dictionary<AbstractEvent, float>, AbstractEvent, Dictionary<AbstractEvent, float>> f, Dictionary<AbstractEvent, byte> restriction)
        {
            var initial = new ExpandedState(G.InitialState, new Context(scheduler, restriction), 1);
            var distance = new PriorityQueue<ExpandedState, Context>();

            distance.Enqueue(initial, new Context(scheduler, restriction));

            var it = 0;

            var timer = new System.Diagnostics.Stopwatch();
            timer.Start();
            List<Tuple<AbstractState, Context>> fim = null;

            Random rnd = new Random();

            while (distance.Count>0)
            {

                Tuple<ExpandedState, Context> ele = null;
                if (distance.Count > 0)
                    ele = distance.Dequeue();



                        var u = ele.Item1;
                        var uContext = ele.Item2;

                        if (u.Item3 == steps && u.Item1 == destination)
                        {
                            
                            fim = new List<Tuple<AbstractState, Context>> {Tuple.Create(u.Item1, uContext)};
                        }


                        foreach (var e in G.Events.Where(e => uContext.EventAllowed(e)))
                        {
                            var s2 = G.TransitionFunction(u.Item1, e);
                            if (s2.IsNone) continue;

                            var context = new Context(uContext, e, f);
                            var v = new ExpandedState(s2.Value, context, u.Item3 + 1);

                            if (rnd.NextDouble() < 0.01) continue;

                            lock (distance)
                            {
                                if (distance.ContainsValue(v))
                                {
                                    if (v.Item2.Value < distance[v].Value)
                                        distance[v] = context;
                                }
                                else
                                    distance.Enqueue(v, context);
                            }
                        }


                //Tuple<ExpandedState, Context> ele;
                //lock (distance)
                //{
                //    if (distance.Count > 0)
                //        ele = distance.Dequeue();
                //    else return null;
                //}
                

                //var u = ele.Item1;
                //var uContext = ele.Item2;

                //if (u.Item3 == steps && u.Item1 == destination)
                //    return new List<Tuple<AbstractState, Context>> { Tuple.Create(u.Item1, uContext) };
                        

                //var controllable = false;

                //foreach (var e in G.Events.Where(e => /*e.IsControllable &&*/ uContext.EventAllowed(e)))
                //{
                //    var s2 = G.TransitionFunction(u.Item1, e);
                //    if (s2.IsNone) continue;

                //    var context = new Context(uContext, e, f);
                //    var v = new ExpandedState(s2.Value, context, u.Item3 + 1);

                //    lock(distance)
                //    {
                //        if (distance.ContainsValue(v))
                //        {
                //            if (v.Item2.Value < distance[v].Value)
                //            {
                //                distance[v] = context;
                //                controllable = true;
                //            }
                //        }
                //        else
                //        {
                //            distance.Enqueue(v, context);
                //            controllable = true;
                //        }
                //    }
                //}

                //if (!controllable)
                //{
                //    foreach (var e in G.Events.Where(e => !e.IsControllable && uContext.EventAllowed(e)))
                //    {
                //        var s2 = G.TransitionFunction(u.Item1, e);
                //        if (s2.IsNone) continue;

                //        var context = new Context(u.Item2, e, f);
                //        var v = new ExpandedState(s2.Value, context, u.Item3 + 1);

                //        if (distance.ContainsValue(v))
                //        {
                //            if (v.Item2.Value < distance[v].Value)
                //                distance[v] = context;
                //        }
                //        else
                //        {
                //            distance.Enqueue(v, context);
                //            //toVisit.Add(v);
                //        }
                //    }
                //}

                if (it % 1000 == 0)
                {
                    Console.WriteLine("Iteration {0} | Visited States: {1}\n Cycle Time: {2} s\n", it, distance.Count, timer.ElapsedMilliseconds / 1000f);
                    timer.Restart();
                }

                if (fim != null) return fim;

                it++;
            }

            return
                distance.AsParallel().Where(kvp => kvp.Item1.Item3 > 1)
                    .Select(kvp => Tuple.Create(kvp.Item1.Item1, kvp.Item2))
                    .ToList();
        }

        public static List<Tuple<AbstractState, Context>> DijkstraShortestPathNonblocking(
            this DeterministicFiniteAutomaton G, uint steps, AbstractState destination, Dictionary<AbstractEvent, float> scheduler,
            Func<Dictionary<AbstractEvent, float>, AbstractEvent, Dictionary<AbstractEvent, float>> f, Dictionary<AbstractEvent, byte> restriction)
        {
            var initial = new ExpandedState(G.InitialState, new Context(scheduler, restriction), 1);
            var distance = new PriorityQueue<ExpandedState, Context>();

            distance.Enqueue(initial, new Context(scheduler, restriction));

            var it = 0;

            var timer = new System.Diagnostics.Stopwatch();
            timer.Start();
            List<Tuple<AbstractState, Context>> fim = null;

            Random rnd = new Random();

            while (distance.Count > 0)
            {

                Tuple<ExpandedState, Context> ele;
                lock (distance)
                {
                    if (distance.Count > 0)
                        ele = distance.Dequeue();
                    else return null;
                }


                var u = ele.Item1;
                var uContext = ele.Item2;

                if (u.Item3 == steps && u.Item1 == destination)
                    return new List<Tuple<AbstractState, Context>> { Tuple.Create(u.Item1, uContext) };


                var controllable = false;

                foreach (var e in G.Events.Where(e => e.IsControllable && uContext.EventAllowed(e)))
                {
                    var s2 = G.TransitionFunction(u.Item1, e);
                    if (s2.IsNone) continue;

                    var context = new Context(uContext, e, f);
                    var v = new ExpandedState(s2.Value, context, u.Item3 + 1);

                    lock (distance)
                    {
                        if (distance.ContainsValue(v))
                        {
                            if (v.Item2.Value < distance[v].Value)
                            {
                                distance[v] = context;
                                controllable = true;
                            }
                        }
                        else
                        {
                            distance.Enqueue(v, context);
                            controllable = true;
                        }
                    }
                }

                if (!controllable)
                {
                    foreach (var e in G.Events.Where(e => !e.IsControllable && uContext.EventAllowed(e)))
                    {
                        var s2 = G.TransitionFunction(u.Item1, e);
                        if (s2.IsNone) continue;

                        var context = new Context(u.Item2, e, f);
                        var v = new ExpandedState(s2.Value, context, u.Item3 + 1);

                        if (distance.ContainsValue(v))
                        {
                            if (v.Item2.Value < distance[v].Value)
                                distance[v] = context;
                        }
                        else
                        {
                            distance.Enqueue(v, context);
                        }
                    }
                }

                if (it % 1000 == 0)
                {
                    Console.WriteLine("Iteration {0} | Visited States: {1}\n Cycle Time: {2} s\n", it, distance.Count, timer.ElapsedMilliseconds / 1000f);
                    timer.Restart();
                }

                if (fim != null) return fim;

                it++;
            }

            return
                distance.AsParallel().Where(kvp => kvp.Item1.Item3 > 1)
                    .Select(kvp => Tuple.Create(kvp.Item1.Item1, kvp.Item2))
                    .ToList();
        }


        public static DeterministicFiniteAutomaton TimeConstrainedSupervisor(this DeterministicFiniteAutomaton G,
            Dictionary<AbstractEvent, float> scheduler,
            Func<Dictionary<AbstractEvent, float>, AbstractEvent, Dictionary<AbstractEvent, float>> f)
        {
            var initial = Tuple.Create(G.InitialState, scheduler);
            var frontier = new List<Tuple<AbstractState, Dictionary<AbstractEvent, float>>> {initial};
            var dic = new Dictionary<AbstractState, List<Dictionary<AbstractEvent, float>>>();
            
            var uncontrollable = G.Events.Where(ev => !ev.IsControllable).ToList();

            var visited = 0;

            do
            {
                visited = dic.Count;

                var newFrontier = new List<Tuple<AbstractState, Dictionary<AbstractEvent, float>>>();

                foreach (var s in frontier)
                {
                    if (dic.ContainsKey(s.Item1) &&
                        dic[s.Item1].Any(sch => !sch.Any(ele => s.Item2[ele.Key] != ele.Value))) continue;

                    if (!dic.ContainsKey(s.Item1))
                        dic.Add(s.Item1, new List<Dictionary<AbstractEvent, float>> {s.Item2});
                    else
                        dic[s.Item1].Add(s.Item2);

                    foreach (var ev in G.Events.Where(ev => !float.IsInfinity(s.Item2[ev])))
                    {
                        if (!ev.IsControllable &&
                            uncontrollable.Any(eun => s.Item2[eun] < s.Item2[ev] && s.Item2[eun] != 0)) continue;

                        var next = G.TransitionFunction(s.Item1, ev);

                        if (next.IsNone) continue;

                        var newScheduler = f(s.Item2, ev);

                        newFrontier.Add(Tuple.Create(next.Value, newScheduler));
                    }
                }

                frontier = newFrontier;
            } while (visited < dic.Count);


            var transitions = G.States.SelectMany(s1 => G.Events.SelectMany(e =>
            {
                var s2 = G.TransitionFunction(s1, e);
                return s2.IsSome ? new[] {new Transition(s1, e, s2.Value)} : new Transition[0];
            })).Where(t => dic.ContainsKey(t.Origin) && dic.ContainsKey(t.Destination));

            return new DeterministicFiniteAutomaton(transitions, G.InitialState, string.Format("TimeRes({0})", G));
        }

        public static DeterministicFiniteAutomaton TimeConstrainedSupervisor2(this DeterministicFiniteAutomaton G,
            Dictionary<AbstractEvent, float> scheduler,
            Func<Dictionary<AbstractEvent, float>, AbstractEvent, Dictionary<AbstractEvent, float>> f)
        {
            var initial = Tuple.Create(G.InitialState, scheduler);
            var frontier = new List<Tuple<AbstractState, Dictionary<AbstractEvent, float>>> { initial };
            var dic = new Dictionary<AbstractState, List<Dictionary<AbstractEvent, float>>>();
            var transF = new Dictionary<AbstractState, HashSet<AbstractEvent>>();
            var transA = new Dictionary<AbstractState, HashSet<AbstractEvent>>();
            var uncontrollable = G.Events.Where(ev => !ev.IsControllable).ToList();

            var visited = 0;

            do
            {
                visited = dic.Count;

                var newFrontier = new List<Tuple<AbstractState, Dictionary<AbstractEvent, float>>>();

                foreach (var s in frontier)
                {
                    if (!dic.ContainsKey(s.Item1))
                        dic.Add(s.Item1, new List<Dictionary<AbstractEvent, float>> { s.Item2 });
                    else
                        dic[s.Item1].Add(s.Item2);

                    foreach (var ev in G.Events.Where(ev => !float.IsInfinity(s.Item2[ev])))
                    {
                        if (!ev.IsControllable &&
                            uncontrollable.Any(eun => s.Item2[eun] < s.Item2[ev] && s.Item2[eun] != 0)) continue;

                        var next = G.TransitionFunction(s.Item1, ev);

                        if (next.IsNone) continue;

                        var newScheduler = f(s.Item2, ev);


                        var ss = Tuple.Create(next.Value, newScheduler);
                        if (!dic.ContainsKey(ss.Item1) ||
                            !dic[ss.Item1].Any(sch => !sch.Any(ele => ss.Item2[ele.Key] != ele.Value)))
                        {
                            newFrontier.Add(ss);
                            if (transA.ContainsKey(s.Item1))
                            {
                                transA[s.Item1].Add(ev);
                            }
                            else
                            {
                                transA.Add(s.Item1, new HashSet<AbstractEvent> { ev });
                            }
                        }
                        else
                        {
                            if (transF.ContainsKey(s.Item1))
                            {
                                transF[s.Item1].Add(ev);
                            }
                            else
                            {
                                transF.Add(s.Item1, new HashSet<AbstractEvent> {ev});
                            }
                        }
                    }
                }

                frontier = newFrontier;
            } while (visited < dic.Count);


            var transitions = G.States.SelectMany(s1 => G.Events.Where(e => !transF.ContainsKey(s1) || !(transF[s1].Except(transA[s1]).Contains(e))).SelectMany(e =>
            {
                
                var s2 = G.TransitionFunction(s1, e);
                return s2.IsSome ? new[] { new Transition(s1, e, s2.Value) } : new Transition[0];
            })).Where(t => dic.ContainsKey(t.Origin) && dic.ContainsKey(t.Destination));

            return new DeterministicFiniteAutomaton(transitions, G.InitialState, string.Format("TimeRes({0})", G)).Trim;
        }

        public static List<AbstractEvent> DepthFirstSearchShortestPath(this DeterministicFiniteAutomaton G, uint depth, AbstractState target, Dictionary<AbstractEvent, float> scheduler,
            Func<Dictionary<AbstractEvent, float>, AbstractEvent, Dictionary<AbstractEvent, float>> f)
        {
            var frontier =
                new ConcurrentBag<Tuple<AbstractState, Dictionary<AbstractEvent, float>, AbstractEvent[], float>>
                {
                    Tuple.Create(G.InitialState, scheduler, new AbstractEvent[0], 0.0f)
                };

            var paths = new ConcurrentBag<Tuple<List<AbstractEvent>, float>>();

            var events = G.Events.ToList();

            for (int i = 0; i < depth-1; i++)
            {
                var newfrontier =
                    new ConcurrentBag<Tuple<AbstractState, Dictionary<AbstractEvent, float>, AbstractEvent[], float>>();

                Parallel.ForEach(frontier, sx =>
                {
                    foreach (var e in events)
                    {
                        var s = sx.Item1;

                        var s2 = G.TransitionFunction(s, e);
                        if (s2.IsNone) return;

                        var sch = sx.Item2;
                        var seq = sx.Item3;
                        var time = sx.Item4;

                        newfrontier.Add(Tuple.Create(s2.Value, f(sch, e), seq.Concat(new[] {e}).ToArray(), time + sch[e]));
                    }
                });

                frontier = newfrontier;
            }

            Parallel.ForEach(frontier, sx =>
            {
                foreach (var e in events)
                {
                    var s = sx.Item1;

                    var s2 = G.TransitionFunction(s, e);
                    if (s2.IsNone || s2.Value != target) continue;

                    var sch = sx.Item2;
                    var seq = sx.Item3;
                    var time = sx.Item4;

                    paths.Add(Tuple.Create(seq.Concat(new[] {e}).ToList(), time + sch[e]));
                }
            });

            return paths.Aggregate((a, b) => a.Item2 < b.Item2 ? a : b).Item1;
        } 
    }
}