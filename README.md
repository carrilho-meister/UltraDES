# UltraDES
UltraDES is a library for modeling, analysis and control of Discrete Event Systems. It has been developed at LACSED | UFMG (http://www.lacsed.eng.ufmg.br).

## Creating States

```
State s1 = new State("s1", Marking.Marked);
State s2 = new State("s2", Marking.Unmarked);
```

## Creating Events

```
Event e1 = new Event("e1", Controllability.Controllable);
Event e2 = new Event("e2", Controllability.Uncontrollable);
```

## Creating Transitions

```
var t = new Transition(s1, e1, s2);
```

## Creating an Automaton

```
var G = new DeterministicFiniteAutomaton(new[]
  {
    new Transition(s1, e1, s2), 
    new Transition(s2, e2, s1)
  }, s1, "G");
```
