// This is an open source non-commercial project. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++ and C#: http://www.viva64.com

using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;

namespace ITCC.UI.Utils
{
    public class ObservableRingBuffer<T> : INotifyCollectionChanged, IEnumerable<T>
        where T: class
    {
        #region INotifyCollectionChanged
        public event NotifyCollectionChangedEventHandler CollectionChanged;

        public void OnCollectionChanged(NotifyCollectionChangedEventArgs args) => CollectionChanged?.Invoke(this, args);
        #endregion

        #region IEnumerable

        public IEnumerator<T> GetEnumerator() => _innerList.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        #endregion

        #region public
        public ObservableRingBuffer(int capacity)
        {
            if (capacity < 1)
                throw new ArgumentOutOfRangeException(nameof(capacity));

            Capacity = capacity;
        }

        public void AddLast(T item)
        {
            _innerList.AddLast(item);
            OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, item, _innerList.Count - 1));
            if (_innerList.Count == Capacity)
                TruncateStart();
        }

        public int Capacity { get; }

        #endregion

        #region private

        private void TruncateStart()
        {
            var first = _innerList.First.Value;
            OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, first, 0));
            _innerList.RemoveFirst();
        }

        private readonly LinkedList<T> _innerList = new LinkedList<T>();

        #endregion
    }
}
