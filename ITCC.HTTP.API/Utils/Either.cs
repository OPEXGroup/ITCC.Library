﻿namespace ITCC.HTTP.API.Utils
{
    /// <summary>
    ///     Represents 'either' monad (one of two possible values).
    ///     Exactly one of First and Second is null. 
    /// </summary>
    /// <typeparam name="TFirst">First type</typeparam>
    /// <typeparam name="TSecond">Second type</typeparam>
    public class Either<TFirst, TSecond>
        where TFirst : class
        where TSecond : class
    {
        /// <summary>
        ///     Create instance with First != null
        /// </summary>
        /// <param name="first">First value</param>
        public Either(TFirst first)
        {
            First = first;
        }

        /// <summary>
        ///     Create instance with Second != null
        /// </summary>
        /// <param name="second">Second value</param>
        public Either(TSecond second)
        {
            Second = second;
        }

        private Either() { }

        /// <summary>
        ///     First possible value
        /// </summary>
        public TFirst First { get; }
        /// <summary>
        ///     Second possible value
        /// </summary>
        public TSecond Second { get; }

        /// <summary> True iff First != null </summary>
        public bool HasFirst => First != null;
        /// <summary> True iff Second != null </summary>
        public bool HasSecond => Second != null;
    }
}
