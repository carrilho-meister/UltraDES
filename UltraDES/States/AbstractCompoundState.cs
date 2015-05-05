namespace UltraDES
{
    public abstract class AbstractCompoundState:AbstractState
    {
        public abstract AbstractState S1 { get; protected set; }
        public abstract AbstractState S2 { get; protected set; }
    }
}
