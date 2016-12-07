using System;
using System.Collections.Generic;

namespace OTTProject
{
    public sealed class MinBinaryHeap<TKey, TValue> where TKey : IComparable
    {

        private readonly IList<KeyValuePair<TKey, TValue>> _items = new List<KeyValuePair<TKey, TValue>>();

        /*
        // support a way of providing key and value pair.
        */
        public void Insert(TKey tkey, TValue tvalue)
        {
            Insert(new KeyValuePair<TKey, TValue>(tkey, tvalue));
        }

        public void Insert(KeyValuePair<TKey, TValue> item)
        {
            _items.Add(item);

            // Current position in the array.
            int position = _items.Count - 1;

            // if it is the first array, the heap is cool
            if (position == 0) { return; }
            while (position > 0)
            {
                // check parent and swap if needed (below apply if init the array with 0)
                // parent is  position / 2
                // children is position * 2 and position * 2 + 1
                int nextPosition = (position - 1) / 2;


                var itemToCheck = _items[nextPosition];

                // if the key of the item we are trying to add is
                // smaller than parent, swap them nicely.
                if (item.Key.CompareTo(itemToCheck.Key) >= 0) { break; }
                _items[position] = itemToCheck;
                position = nextPosition;
            }

            _items[position] = item;
        }

        public KeyValuePair<TKey, TValue>Remove()
        {
            if (_items.Count == 0) throw new Exception("the heap is empty!");
            KeyValuePair<TKey, TValue> result = _items[0];
            if (_items.Count <= 2)
            {
                _items.RemoveAt(0);
                return result;
            }
            // move last item to first position.
            _items[0] = _items[_items.Count - 1];
            // remove it.
            _items.RemoveAt(_items.Count - 1);
            int currentPosition = 0, swapPosition = 0;
            while (true)
            {
                int leftChildPosition = currentPosition * 2 + 1;
                int rightChildPosition = leftChildPosition + 1;
                KeyValuePair<TKey, TValue> parent = _items[currentPosition];
                // no left child, that's enough.
                if (leftChildPosition >= _items.Count) { break; }

                // check if its okay to swap with left child.
                if (leftChildPosition < _items.Count)
                {
                    // swap with left, why not, it might change later.
                    KeyValuePair<TKey, TValue> leftChild = _items[leftChildPosition];
                    if (leftChild.Key.CompareTo(parent.Key) > 0)
                    {
                        swapPosition = leftChildPosition;
                    }
                }
                // check if its okay to swap with right
                if (rightChildPosition < _items.Count)
                {
                    // grab the minimum child to swap with.
                    KeyValuePair<TKey, TValue> leftChild = _items[leftChildPosition];
                    KeyValuePair<TKey, TValue> rightChild = _items[rightChildPosition];
                    swapPosition = leftChildPosition;
                    if (leftChild.Key.CompareTo(rightChild.Key) >= 0)
                    {
                        swapPosition = rightChildPosition;
                    }
                }
                // if swap changed and parent actually bigger, switch up
                // otherwise break.
                if (swapPosition != currentPosition
                    && (parent.Key.CompareTo(_items[swapPosition].Key) > 0)
                    )
                {
                    var temp = _items[currentPosition];
                    _items[currentPosition] = _items[swapPosition];
                    _items[swapPosition] = temp;
                    currentPosition = swapPosition;
                    continue;
                }
                break;
            }
            return result;
        }

        public KeyValuePair<TKey, TValue>Peek()
        {
            if (_items.Count == 0) throw new Exception("the heap is empty!");
            return _items[0];
        }

        public int Count
        {
            get { return _items.Count; }
        }

    }

}