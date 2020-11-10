using System;
using System.Collections.Generic;
using System.Linq;

namespace SlideSharp
{
    public class FixedList<T>
    {
        private readonly T[] _data;
        private readonly bool[] _setValues;
        private readonly Stack<int> _availableSlots;
        public int Count { get; private set; }

        public bool IsReadOnly => true;

        public T this[int index]
        {
            get => _data[index];
            set => throw new NotSupportedException();
        }

        public int Capacity { get; private set; }

        public FixedList(int capacity)
        {
            Capacity = capacity;
            _data = new T[capacity];
            _setValues = new bool[capacity];
            _availableSlots = new Stack<int>(capacity);
            for (var i = 0; i < capacity - 1; i++) {
                _setValues[i] = false;
                _availableSlots.Push(i);
            }
        }

        /// <summary>
        /// Add a new item
        /// </summary>
        /// <param name="value">item to add</param>
        public void Add(T value)
        {
            var availableIndex = _availableSlots.Pop();
            _data[availableIndex] = value;
            _setValues[availableIndex] = true;
            Count++;
        }

        /// <summary>
        /// Removes an item at index
        /// </summary>
        /// <param name="index">index to look for</param>
        public void RemoveAt(int index)
        {
            if (_setValues[index]) {
                _data[index] = default;
                Count--;
                _availableSlots.Push(index);
                _setValues[index] = false;
            } else {
                throw new KeyNotFoundException(nameof(index));
            }
        }

        /// <summary>
        /// removes an item
        /// </summary>
        /// <param name="value">item to look for</param>
        /// <returns></returns>
        public bool Remove(T value)
        {
            for (var i = 0; i < Capacity; i++) {
                if (!_setValues[i] || !_data[i].Equals(value)) continue;
                this.RemoveAt(i);
                return true;
            }

            return false;
        }

        /// <summary>
        /// Remove all items matching predicate function
        /// </summary>
        /// <param name="predicate">function returning true if match is found</param>
        /// <returns></returns>
        public int RemoveAll(Func<T, bool> predicate)
        {
            var removed = 0;
            for (var i = 0; i < Capacity; i++) {
                if (!_setValues[i] || !predicate(_data[i])) continue;
                RemoveAt(i);
                removed++;
            }

            return removed;
        }

        /// <summary>
        /// returns the index of the matching item
        /// </summary>
        /// <param name="item">item to look for</param>
        /// <returns></returns>
        public int IndexOf(T item)
        {
            for (var i = 0; i < Capacity; i++) {
                if (_setValues[i] && _data[i].Equals(item)) return i;
            }

            return -1;
        }

        /// <summary>
        /// Clears the list
        /// </summary>
        public void Clear()
        {
            for (var i = 0; i < Capacity; i++) {
                if (!_setValues[i]) continue;
                RemoveAt(i);
            }
        }

        /// <summary>
        /// check if item exists
        /// </summary>
        /// <param name="item">item to look for</param>
        /// <returns></returns>
        public bool Contains(T item)
        {
            return _data.AsEnumerable<T>().Contains(item);
        }

        /// <summary>
        /// copy the non-null contents of the FixedList to a array<T>
        /// </summary>
        /// <param name="array">array to copy to</param>
        /// <param name="arrayIndex">starting index of array</param>
        public void CopyTo(ref T[] array, int arrayIndex)
        {
            for (var i = 0; i <= Capacity; i++) {
                if (!_setValues[i]) continue;
                array[arrayIndex] = _data[i];
                arrayIndex++;
            }
        }

        /// <summary>
        /// perform an action on all elements of the FixedList
        /// </summary>
        /// <param name="action">a action that gets passed the item</param>
        public void ForEach(Action<T> action)
        {
            for (var i = 0; i < Capacity; i++) {
                if (_setValues[i]) {
                    action(_data[i]);
                }
            }
        }

        /// <summary>
        /// perform an action on all elements of the FixedList
        /// </summary>
        /// <param name="action">a action that gets passed the item and its index</param>
        public void ForEachAt(Action<T, int> action)
        {
            for (var i = 0; i < Capacity; i++) {
                if (_setValues[i]) {
                    action(_data[i], i);
                }
            }
        }

        /// <summary>
        /// Find an item matching the predicate function
        /// </summary>
        /// <param name="predicate">function returning true if match is found</param>
        /// <returns></returns>
        public T Find(Func<T, bool> predicate)
        {
            for (var i = 0; i < Capacity; i++) {
                if (_setValues[i] && predicate(_data[i])) {
                    return _data[i];
                }
            }

            return default;
        }

        /// <summary>
        /// Create a FixedList of a new capacity keeping as many items as can fit in the new collection, this will also shift all items to the beginning of the new FixedList.
        /// </summary>
        /// <param name="newCapacity">the new capacity, use the FixedLists old capacity to just shift contents left in the new FixedList</param>
        /// <returns></returns>
        public FixedList<T> Resize(int newCapacity)
        {
            var newFixedList = new FixedList<T>(newCapacity);

            for (var i = 0; i < Capacity; i++) {
                if (_setValues[i]) {
                    newFixedList.Add(_data[i]);
                }
            }

            return newFixedList;
        }
    }
}
