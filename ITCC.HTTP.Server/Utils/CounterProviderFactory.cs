using System;
using ITCC.HTTP.Server.Enums;
using ITCC.HTTP.Server.Interfaces;

namespace ITCC.HTTP.Server.Utils
{
    internal static class CounterProviderFactory
    {
        public static IIntervalCounterProvider BuildCounterProvider(MemoryAlarmStrategy alarmStrategy, long startValue)
        {
            IIntervalCounterProvider counterProvider;
            switch (alarmStrategy)
            {
                case MemoryAlarmStrategy.Constant:
                    counterProvider = new ConstantCounterProvider();
                    break;
                case MemoryAlarmStrategy.Linear:
                    counterProvider = new LinearCounterProvider();
                    break;
                case MemoryAlarmStrategy.Geometric:
                    counterProvider = new GeometricCounterProvider();
                    break;
                case MemoryAlarmStrategy.Fibonacci:
                    counterProvider = new FibonacciCounterProvider();
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(alarmStrategy), alarmStrategy, null);
            }

            counterProvider.StartValue = startValue;
            return counterProvider;
        }
    }
}
