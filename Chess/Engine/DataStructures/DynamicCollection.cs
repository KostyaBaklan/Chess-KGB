using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace Engine.DataStructures
{
    public class DynamicCollection<T> : IEnumerable<T>
    {
        private Node<T> _root;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Add(T item)
        {
            _root = new Node<T>(item) { Next = _root };
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Clear()
        {
            var root = _root;
            _root = null;
            Task.Factory.StartNew(() =>
            {
                var next = root.Next;
                while (root != null)
                {
                    root.Next = null;
                    root = next;
                    next = next.Next;
                }
            });
        }

        #region Implementation of IEnumerable

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public IEnumerator<T> GetEnumerator()
        {
            var current = _root;
            while (current != null)
            {
                yield return current.Value;
                current = current.Next;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        #endregion
    }
}