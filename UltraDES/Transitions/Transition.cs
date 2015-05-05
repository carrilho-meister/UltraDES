using System;

namespace UltraDES
{
    [Serializable]
    public class Transition
    {
        public Transition(AbstractState origin, AbstractEvent trigger, AbstractState destination)
        {
            Origin = origin;
            Destination = destination;
            Trigger = trigger;
        }

        public AbstractState Origin { get; protected set; }
        public AbstractState Destination { get; protected set; }
        public AbstractEvent Trigger { get; protected set; }
        public bool IsControllableTransition { get { return Trigger.IsControllable; } }

        public override bool Equals(object obj)
        {
            if (obj == null) return false;

            // If parameter cannot be cast to Point return false.
            var p = obj as Transition;
            if ((Object)p == null) return false;

            // Return true if the fields match:
            if (Trigger != p.Trigger) return false;

            return Origin == p.Origin && Destination == p.Destination;
        }

        public override int GetHashCode()
        {
            return Origin.GetHashCode()*2 + Destination.GetHashCode()*7 + Trigger.GetHashCode();
        }

        public override string ToString()
        {
            return string.Format("({0} --{1}-> {2})", Origin, Trigger, Destination);
        }
    }
}