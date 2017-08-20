
using System;
using System.Collections;
using System.Collections.Generic;

namespace UNEB.Utility
{
    /// <summary>
    /// A simple stack with a limited capacity.
    /// In order to make more room, the first element (not the top) in the stack is removed.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class FiniteStack<T> : IEnumerable<T>
    {
        private LinkedList<T> _container;
        private int _capacity;

        /// <summary>
        /// Called when the stack runs out of space and removes
        /// the first item (bottom of stack) to make room.
        /// </summary>
        public event Action<T> OnRemoveBottomItem;

        public FiniteStack(int capacity)
        {
            _container = new LinkedList<T>();
            _capacity = capacity;
        }

        public void Push(T value)
        {
            _container.AddLast(value);

            // Out of room, remove the first element in the stack.
            if (_container.Count == _capacity) {

                T first = _container.First.Value;
                _container.RemoveFirst();

                if (OnRemoveBottomItem != null)
                    OnRemoveBottomItem(first);
            }
        }

        public T Peek()
        {
            return _container.Last.Value;
        }

        public T Pop()
        {
            var lastVal = _container.Last.Value;
            _container.RemoveLast();

            return lastVal;
        }

        public void Clear()
        {
            _container.Clear();
        }

        public int Count
        {
            get { return _container.Count; }
        }

        public IEnumerator<T> GetEnumerator()
        {
            return _container.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return _container.GetEnumerator();
        }
    }
}