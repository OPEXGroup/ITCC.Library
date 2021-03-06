﻿// This is an open source non-commercial project. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++ and C#: http://www.viva64.com
using System.Threading;
using ITCC.HTTP.Server.Interfaces;

namespace ITCC.HTTP.Server.Utils
{
    internal class ConstantCounterProvider : IIntervalCounterProvider
    {
        #region IIntervalCounterProvider

        public long StartValue
        {
            get
            {
                lock (Lock)
                {
                    return InnerStartValue;
                }
            }
            set
            {
                lock (Lock)
                {
                    InnerStartValue = value;
                }
                Thread.MemoryBarrier();
                Reset();
            }
        }

        public long GetNextCount()
        {
            lock (Lock)
            {
                Iteration++;
                return InnerGetNextValue();
            }
        }

        public virtual void Reset()
        {
            lock (Lock)
            {
                Iteration = 0;
            }
        }
        #endregion

        #region protected

        protected virtual long InnerGetNextValue() => InnerStartValue;

        protected int Iteration;
        protected long InnerStartValue;
        protected readonly object Lock = new object();

        #endregion
    }
}
