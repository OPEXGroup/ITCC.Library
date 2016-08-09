using System;
using System.Collections.Generic;

namespace ITCC.Logging.Utils
{
    /// <summary>
    ///     Simple custom concurrent queue implementation
    /// </summary>
    /// <typeparam name="T">Queue element type</typeparam>
    internal class ConcurrentBoundedQueue<T>
    {
        #region public
        public ConcurrentBoundedQueue(int capacity)
        {
            if (capacity < 0)
                throw new ArgumentOutOfRangeException(nameof(capacity), "Capacity cannot be negative");
            _innerList = new LinkedList<T>();
            _capacity = capacity;
        }

        public void Enqueue(T value)
        {
            lock (_listLock)
            {
                if (_innerList.Count == _capacity)
                    _innerList.RemoveLast();
                _innerList.AddFirst(value);
            }
        }

        public bool TryDequeue(out T value)
        {
            lock (_listLock)
            {
                if (_innerList.Count == 0)
                {
                    value = default(T);
                    return false;
                }
                value = _innerList.Last.Value;
                _innerList.RemoveLast();
                return true;
            }
        }

        public void Flush()
        {
            lock (_listLock)
            {
                while (_innerList.Count > 0)
                {
                    _innerList.RemoveLast();
                }
            }
        }

        /// <summary>
        ///     This will return queue in last-to-first order
        /// </summary>
        /// <returns></returns>
        public List<T> ToList()
        {
            lock (_listLock)
            {
                return new List<T>(_innerList);
            }
        } 

        public int Count => _innerList.Count;
        #endregion

        #region private

        private readonly LinkedList<T> _innerList;

        private readonly int _capacity;

        private readonly object _listLock = new object();

        #endregion
    }
}
