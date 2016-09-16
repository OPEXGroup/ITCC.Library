using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;

namespace ITCC.UI.Utils
{
    public class ObservableRingBuffer<T> : INotifyCollectionChanged, IEnumerable<T>
        where T: class
    {
        public event NotifyCollectionChangedEventHandler CollectionChanged;

        public ObservableRingBuffer(int capacity)
        {
            if (capacity < 1)
                throw new ArgumentOutOfRangeException(nameof(capacity));

            Capacity = capacity;
        }

        public IEnumerator<T> GetEnumerator() => new BufferEnumerator(_innerList);

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        public void AddLast(T item)
        {
            _innerList.AddLast(item);
            OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, item));
            if (_innerList.Count == Capacity)
                TruncateStart();
        }

        public int Capacity { get; }

        public void OnCollectionChanged(NotifyCollectionChangedEventArgs args) => CollectionChanged?.Invoke(this, args);

        #region private

        private void TruncateStart()
        {
            var first = _innerList.First.Value;
            _innerList.RemoveFirst();
            OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, first));
        }

        private readonly LinkedList<T> _innerList = new LinkedList<T>();

        private class BufferEnumerator : IEnumerator<T>
        {
            public BufferEnumerator(LinkedList<T> list)
            {
                _list = list;
                _listNode = _list.First;
            }

            public void Dispose()
            {
                
            }

            public bool MoveNext()
            {
                if (_listNode == null)
                    return false;
                _listNode = _listNode.Next;
                return _listNode != null;
            }

            public void Reset()
            {
                _listNode = _list.First;
            }

            public T Current => _listNode.Value;

            object IEnumerator.Current => Current;

            private LinkedListNode<T> _listNode;
            private readonly LinkedList<T> _list;
        }

        #endregion
    }
}
