﻿using OTTProject;
using System;
using System.Collections.Generic;

namespace OTTProject
{
    public class ConcurrentPriorityQueue<TKey, TValue>
       where TKey : IComparable

    {
        private readonly MinBinaryHeap<TKey, TValue> _minHeap = new MinBinaryHeap<TKey, TValue>();
        private readonly object _lock = new object();


        /// <summary>
        /// Add the queue wrapper.
        /// </summary>
        /// <param name="priority"></param>
        /// <param name="tvalue"></param>
        public void Enqueue(TKey priority, TValue tvalue)
        {

            Enqueue(new KeyValuePair<TKey, TValue>(priority, tvalue));
        }

        public void Enqueue(KeyValuePair<TKey, TValue> item)
        {
            Logger.Log("added item to queue:" + item.Value.ToString() + "with priority: " + item.Key);
            lock (_lock) _minHeap.Insert(item);
        }

        /// <summary>
        /// Try to get the first item in queue with removing it.
        /// 
        /// </summary>
        /// <param name="result"></param>
        /// <returns>false if heap is empty.</returns>
        public bool TryPeek(out KeyValuePair<TKey, TValue> result)
        {
            result = default(KeyValuePair<TKey, TValue>);
            try
            {
                result = _minHeap.Peek();
            }
            catch (Exception _)
            {
                return false;
            }
            return true;
        }

        /// <summary>
        /// Try to Dequeue, false if queue(heap) is empty.
        /// Since there's a lock we can be sure the heap will not be touched by other threads.
        /// </summary>
        /// <param name="result"></param>
        /// <returns></returns>
        public bool TryDequeue(out KeyValuePair<TKey, TValue> result)
        {
            result = default(KeyValuePair<TKey, TValue>);
            lock (_lock)
            {
                if (_minHeap.Count == 0) return false;
                result = _minHeap.Remove();
                return true;
            }
        }


    }


}