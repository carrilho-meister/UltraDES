using System;

namespace UltraDES
{
    [Serializable]
    public abstract class AbstractEvent : Symbol
    {
        //public delegate void EventTriggerHandler(object sender, EventTriggerArgs e);

        public Controllability Controllability { get; protected set; }

        public bool IsControllable
        {
            get { return Controllability == Controllability.Controllable; }
        }

        //public event EventTriggerHandler EventTrigger;
        public abstract override string ToString();
        public abstract override int GetHashCode();
        public abstract override bool Equals(object obj);

        public static bool operator ==(AbstractEvent a, AbstractEvent b)
        {
            if (ReferenceEquals(a, b)) return true;
            return !ReferenceEquals(a, null) && a.Equals(b);
        }

        public static bool operator !=(AbstractEvent a, AbstractEvent b)
        {
            return !(a == b);
        }

        //public virtual AbstractState OnEventTrigger(AbstractState s)
        //{
        //    var arg = new EventTriggerArgs(s);
        //    OnEventTrigger(arg);
        //    return arg.NextState;
        //}

        //protected virtual void OnEventTrigger(EventTriggerArgs e)
        //{
        //    var handler = EventTrigger;
        //    if (handler != null) handler(this, e);

        //    if (e.NextState == null) e.NextState = e.ActualState;
        //}
    }

    public class EventTriggerArgs : EventArgs
    {
        public EventTriggerArgs(AbstractState actualState)
        {
            ActualState = actualState;
        }

        public AbstractState ActualState { get; private set; }
        public AbstractState NextState { get; set; }
    }

    public enum Controllability : byte
    {
        Controllable = 1,
        Uncontrollable = 0
    }
}