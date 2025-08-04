using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SkyJam
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using Unity.Collections;

    [System.Serializable]
    public class RSQueue<T> : IEnumerable<T>
    {
        private readonly List<T> _items;
        private int _head;
        private readonly object _syncRoot = new object();

        public RSQueue()
        {
            _items = new List<T>();
            _head = 0;
        }

        public RSQueue(IEnumerable<T> input)
        {
            _items = new List<T>();
            _head = 0;

            foreach (var it in input)
            {
                Enqueue(it);
            }
        }

        public void Enqueue(IEnumerable<T> items)
        {
            lock (_syncRoot)
            {
                _items.AddRange(items);
            }
        }
        public void Enqueue(T item)
        {
            lock (_syncRoot)
            {
                _items.Add(item);
            }
        }

        public T Dequeue()
        {
            lock (_syncRoot)
            {
                if (IsEmpty)
                    throw new InvalidOperationException("Queue is empty.");

                T item = _items[_head];
                _head++;

                if (_head > _items.Count / 2)
                    Trim();

                return item;
            }
        }

        public T Peek()
        {
            lock (_syncRoot)
            {
                if (IsEmpty)
                    throw new InvalidOperationException("Queue is empty.");

                return _items[_head];
            }
        }

        public int Count
        {
            get
            {
                lock (_syncRoot)
                {
                    return _items.Count - _head;
                }
            }
        }

        public bool IsEmpty
        {
            get
            {
                lock (_syncRoot)
                {
                    return Count == 0;
                }
            }
        }

        public void Clear()
        {
            lock (_syncRoot)
            {
                _items.Clear();
                _head = 0;
            }
        }

        private void Trim()
        {
            // Removes the unused front segment
            _items.RemoveRange(0, _head);
            _head = 0;
        }

        // Enumeration support
        public IEnumerator<T> GetEnumerator()
        {
            lock (_syncRoot)
            {
                for (int i = _head; i < _items.Count; i++)
                    yield return _items[i];
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public T this[int index]
        {
            get
            {
                lock (_syncRoot)
                {
                    if (index < 0 || index >= Count)
                        throw new ArgumentOutOfRangeException(nameof(index), "Index is out of range.");

                    return _items[_head + index];
                }
            }
        }

        public void RemoveAt(int index)
        {
            lock (_syncRoot)
            {
                if (index < 0 || index >= Count)
                    throw new ArgumentOutOfRangeException(nameof(index), "Index is out of range.");

                for (int  j = _head + index; j > _head; --j)
                {
                    _items[j] = _items[j - 1];
                }
                _head++;
                //_items.RemoveAt(_head + index);
            }
        }
    }

}
