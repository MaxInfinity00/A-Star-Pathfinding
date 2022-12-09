using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.PlayerLoop;

namespace AsPathfinding{
    public class Heap<T> where T : IHeapItem<T> {
        [SerializeField]private T[] items;
        private int currentCount;

        public Heap(int maxHeapSize) {
            items = new T[maxHeapSize];
        }

        public void Add(T item) {
            item.HeapIndex = currentCount;
            
            items[currentCount] = item;
            SortUp(item);
            currentCount++;
        }

        public T RemoveFirstItem() {
            currentCount--;
            T returnable = items[0];
            returnable.HeapIndex = default;
            items[currentCount].HeapIndex = 0;
            items[0] = items[currentCount];
            items[currentCount] = default(T);
            if(currentCount > 0) SortDown(items[0]);

            return returnable;
        }

        public void UpdateItem(T item) {
            SortUp(item);
        }
        
        public int Count => currentCount;

        public bool Contains(T item) {
            return item.HeapIndex < currentCount && Equals(items[item.HeapIndex],item);
        }
        
        
        public void SortDown(T item) {

            while (true) {
                int childIndex1 = item.HeapIndex*2 + 1;
                int childIndex2 = childIndex1 + 1;
                if (childIndex1 < currentCount) {
                    int swapIndex = childIndex1;
                    if (childIndex2 < currentCount && items[childIndex1].CompareTo(items[childIndex2]) < 0){
                        swapIndex = childIndex2;
                    }

                    T childItem = items[swapIndex];
                    if (item.CompareTo(childItem) < 0 ) {
                        Swap(item,childItem);
                    }
                    else {
                        return;
                    }
                }
                else {
                    return;
                }
            }
        }
        public void SortUp(T item) {

            while (true) {
                int parentIndex = (item.HeapIndex - 1) / 2;
                T parentItem = items[parentIndex];
                if (item.CompareTo(parentItem) > 0) {
                    Swap(item,parentItem);
                }
                else {
                    return;
                }
            }
        }

        public void Swap(T itemA, T itemB) {
            items[itemA.HeapIndex] = itemB;
            items[itemB.HeapIndex] = itemA;
            (itemA.HeapIndex, itemB.HeapIndex) = (itemB.HeapIndex, itemA.HeapIndex);
        }
    }

    public interface IHeapItem<T> : IComparable<T> {
        int HeapIndex {
            get;
            set;
        }
    }
}