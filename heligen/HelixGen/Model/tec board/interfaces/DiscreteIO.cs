using System;

namespace Accel
{
    public interface IDiscreteIn<T>
    {
        T Value();
    }
    public interface IDiscreteOut<T>
    {
        void Set(T value);
    }

    public interface IBinaryIn : IDiscreteIn<bool> { }
    public interface IBinaryOut : IDiscreteOut<bool> { }

    public interface IAnalogIn<T, U> : IDiscreteIn<T>
    {
        U Level();
    }
    public interface IAnalogOut<T, U> : IDiscreteOut<T>
    {
        void SetLevel(U level);
    }
}
