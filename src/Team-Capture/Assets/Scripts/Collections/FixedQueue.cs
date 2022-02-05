// Team-Capture
// Copyright (c) 2019-2022 Voltstro-Studios
// 
// This project is governed by the AGPLv3 License.
// For more details see the LICENSE file.

using System;
using UnityEngine.Scripting;

namespace Team_Capture.Collections
{
    //From:
    //https://github.com/Unity-Technologies/multiplayer-community-contributions/blob/da6250d3a96c0344c03e5a490897dcc298e77e2e/com.community.netcode.extensions/Runtime/LagCompensation/FixedQueue.cs
    
    /// <summary>
    ///     Queue with a fixed size
    /// </summary>
    /// <typeparam name="T">The type of the queue</typeparam>
    [Preserve]
    public sealed class FixedQueue<T>
    {
        private readonly T[] queue;
        private int queueStart;

        /// <summary>
        ///     Creates a new FixedQueue with a given size
        /// </summary>
        /// <param name="maxSize">The size of the queue</param>
        public FixedQueue(int maxSize)
        {
            queue = new T[maxSize];
            queueStart = 0;
        }

        /// <summary>
        ///     The amount of enqueued objects
        /// </summary>
        public int Count { get; private set; }

        /// <summary>
        ///     Gets the element at a given virtual index
        /// </summary>
        /// <param name="index">The virtual index to get the item from</param>
        /// <returns>The element at the virtual index</returns>
        public T this[int index] => queue[(queueStart + index) % queue.Length];

        /// <summary>
        ///     Enqueues an object
        /// </summary>
        /// <param name="t"></param>
        /// <returns></returns>
        public bool Enqueue(T t)
        {
            queue[(queueStart + Count) % queue.Length] = t;
            if (++Count > queue.Length)
            {
                --Count;
                return true;
            }

            return false;
        }

        /// <summary>
        ///     Dequeues an object
        /// </summary>
        /// <returns></returns>
        public T Dequeue()
        {
            if (--Count == -1) throw new IndexOutOfRangeException("Cannot dequeue empty queue!");

            T res = queue[queueStart];
            queueStart = (queueStart + 1) % queue.Length;
            return res;
        }

        public T GetMostRecentElement()
        {
            return queue[Count % queue.Length];
        }

        /// <summary>
        ///     Gets the element at a given virtual index
        /// </summary>
        /// <param name="index">The virtual index to get the item from</param>
        /// <returns>The element at the virtual index</returns>
        public T ElementAt(int index)
        {
            return queue[(queueStart + index) % queue.Length];
        }
    }
}