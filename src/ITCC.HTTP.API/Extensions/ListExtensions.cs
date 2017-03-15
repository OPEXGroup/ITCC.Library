// This is an open source non-commercial project. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++ and C#: http://www.viva64.com
using System;
using System.Collections;
using System.Collections.Generic;

namespace ITCC.HTTP.API.Extensions
{
    public static class ListExtensions
    {
        /// <summary>
        ///     Use this ONLY for small collections. Returns all list subsets in lazy manner
        /// </summary>
        /// <typeparam name="T">Collection element type</typeparam>
        /// <param name="list">Collection</param>
        /// <param name="includeEmpty">If false, enumerable will start with 1-element subsets</param>
        /// <returns>Lazy subset enumerable</returns>
        public static IEnumerable<List<T>> GetSubsets<T>(this IList<T> list, bool includeEmpty = true)
        {
            var count = list.Count;
            var subsetCount = Convert.ToInt32(Math.Pow(2, count));
            var startIndex = includeEmpty ? 0 : 1;
            for (var i = startIndex; i < subsetCount; ++i)
            {
                var currentResult = new List<T>();
                for (var j = 0; j < count; ++j)
                {
                    if (((1 << j) & i) != 0)
                        currentResult.Add(list[j]);
                }
                yield return currentResult;
            }
        }

        /// <summary>
        ///     Use this ONLY for small collections. Returns all list subsets in lazy manner
        /// </summary>
        /// <param name="list">Collection</param>
        /// <param name="includeEmpty">If false, enumerable will start with 1-element subsets</param>
        /// <returns>Lazy subset enumerable</returns>
        public static IEnumerable<List<object>> GetSubsets(this IList list, bool includeEmpty = true)
        {
            var count = list.Count;
            var subsetCount = Convert.ToInt32(Math.Pow(2, count));
            var startIndex = includeEmpty ? 0 : 1;
            for (var i = startIndex; i < subsetCount; ++i)
            {
                var currentResult = new List<object>();
                for (var j = 0; j < count; ++j)
                {
                    if (((1 << j) & i) != 0)
                        currentResult.Add(list[j]);
                }
                yield return currentResult;
            }
        }
    }
}
