using System;

namespace UltraDES
{
    public interface Option<T>
    {
        bool IsSome { get; }
        bool IsNone { get; }
        T Value { get; }
    }

    [Serializable]
    public class Some<T> : Option<T>
    {

        private Some(T value)
        {
            Value = value;
        }

        public bool IsNone
        {
            get { return false; }
        }

        public bool IsSome
        {
            get { return true; }
        }

        public T Value { get; private set; }

        public static Option<T> Create(T value)
        {
            return new Some<T>(value);
        }
    }

    [Serializable]
    public class None<T> : Option<T>
    {
        private static readonly None<T> Singleton = new None<T>();

        private None()
        {
        }

        public bool IsNone
        {
            get { return true; }
        }

        public bool IsSome
        {
            get { return false; }
        }

        public T Value
        {
            get { return default(T); }
        }

        public static Option<T> Create()
        {
            return Singleton;
        }
    }
}