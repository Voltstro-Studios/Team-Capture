using System;
using System.Collections;
using System.Collections.Generic;

//Taken from https://github.com/lordofduct/spacepuppy-unity-framework/blob/master/SpacepuppyBase/Collections/SamplingStack.cs
//Copyright (c) 2015, Dylan Engelman, Jupiter Lighthouse Studio

namespace Team_Capture.Collections
{
    /// <summary>
    ///     Represents a stack of static size. If you push a value onto the stack when it's full,
    ///     the value at the bottom (oldest) of the stack is removed.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class SamplingStack<T> : ICollection<T>
    {
        private T[] values;
        private int head;
        private int version;
    
        public SamplingStack(int size)
        {
            if (size < 0) 
                throw new ArgumentException("Size must be non-negative.", nameof(size));
        
            values = new T[size];
            Count = 0;
            head = -1;
        }

        public int Size => values.Length;

        public int Count { get; private set; }

        public void Push(T value)
        {
            if (values.Length == 0) 
                return;

            head = (head + 1) % values.Length;
            values[head] = value;
            if(Count < values.Length)
                Count++;

            version++;
        }

        public T Pop()
        {
            if (Count == 0) 
                throw new InvalidOperationException("SamplingStack is empty.");

            T result = values[head];
            values[head] = default(T);
            head = (head > 0) ? head - 1 : values.Length - 1;
            Count--;
            version++;
            return result;
        }

        public bool TryPop(out T result)
        {
            if(Count == 0)
            {
                result = default;
                return false;
            }

            result = values[head];
            values[head] = default;
            head = (head > 0) ? head - 1 : values.Length - 1;
            Count--;
            version++;
            return true;
        }

        public T Shift()
        {
            if (Count == 0) 
                throw new InvalidOperationException("SamplingStack is empty.");

            int index = (head - Count + 1);
            if (index < 0) index += values.Length;
            T result = values[index];
            values[index] = default;
            Count--;
            version++;
            return result;
        }

        public bool TryShift(out T result)
        {
            if (Count == 0)
            {
                result = default(T);
                return false;
            }

            int index = (head - Count + 1);
            if (index < 0) index += values.Length;
            result = values[index];
            values[index] = default;
            Count--;
            version++;
            return true;
        }

        public T Peek()
        {
            if (Count == 0) 
                throw new InvalidOperationException("SamplingStack is empty.");

            return values[head];
        }

        public IEnumerable<T> Peek(int cnt)
        {
            int index = head;
            if (cnt > Count) cnt = Count;
            for(int i = 0; i < cnt; i++)
            {
                yield return values[index];
                head = (head > 0) ? head - 1 : values.Length - 1;
            }
        }

        public T PeekShift()
        {
            if (Count == 0) 
                throw new InvalidOperationException("SamplingStack is empty.");

            int index = (head - Count + 1);
            if (index < 0) index += values.Length;
            version++;
            return values[index];
        }
    
        public void Resize(int size)
        {
            if (size < 0)
                throw new ArgumentException("Size must be non-negative.", nameof(size));

            if(head >= Count - 1)
            {
                Array.Resize(ref values, size);
                version++;
            }
            else
            {
                T[] arr = new T[size];
                int index = head;
                Count = Math.Min(size, Count);
                for (int i = 0; i < Count; i++)
                {
                    arr[i] = values[index];
                    index = (index > 0) ? index - 1 : values.Length - 1;
                }
            
                values = arr;
                head = Count - 1;
                version++;
            }
        }

        public void Clear()
        {
            for (int i = 0; i < values.Length; i++)
                values[i] = default;

            Count = 0;
            head = -1;
            version++;
        }

        public Enumerator GetEnumerator() => new Enumerator(this);

        IEnumerator<T> IEnumerable<T>.GetEnumerator() => GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        void ICollection<T>.Add(T item) => Push(item);

        public bool Contains(T item) => Array.IndexOf(values, item) >= 0;

        void ICollection<T>.CopyTo(T[] array, int arrayIndex)
        {
            int index = head;
            for (int i = 0; i < Count; i++)
            {
                array[arrayIndex + 1] = values[index];
                index = (index > 0) ? index - 1 : values.Length - 1;
            }
        }
    
        bool ICollection<T>.IsReadOnly => false;
    
        bool ICollection<T>.Remove(T item) => throw new NotSupportedException();

        public struct Enumerator : IEnumerator<T>
        {
            private SamplingStack<T> stack;
            private int index;
            private int ver;

            internal Enumerator(SamplingStack<T> stack)
            {
                this.stack = stack;
                index = -1;
                Current = default;
                ver = this.stack.version;
            }
        
            public T Current { get; private set; }

            object IEnumerator.Current => Current;

            public bool MoveNext()
            {
                if (stack == null) 
                    return false;
                if (ver != stack.version) 
                    throw new System.InvalidOperationException("SamplingStack was modified while enumerating.");
                if (index >= stack.Count - 1) 
                    return false;

                index++;
                int i = stack.head - index;
                if (i < 0) 
                    i += stack.values.Length;
            
                Current = stack.values[i];
                return true;
            }

            public void Dispose()
            {
                stack = null;
                index = -1;
                Current = default;
                ver = 0;
            }

            public void Reset() => throw new NotImplementedException();
        }
    }
}