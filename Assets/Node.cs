using UnityEngine;
using System;

namespace AsPathfinding {
    public class Node : IHeapItem<Node> {
        public bool walkable;
        public Vector3 worldPosition;
        public int gridX;
        public int gridY;

        public int gCost;
        public int hCost;
        public Node parent;
        private int heapIndex;

        public int fCost => gCost + hCost;

        public int HeapIndex {
            get => heapIndex;
            set => heapIndex = value;
        }


        public Node(bool walkable, Vector3 worldPosition, int gridX, int gridY) {
            this.walkable = walkable;
            this.worldPosition = worldPosition;
            this.gridX = gridX;
            this.gridY = gridY;
        }

        public int CompareTo(Node nodeToCompare) {
            int compare = fCost.CompareTo(nodeToCompare.fCost);
            if (compare == 0) {
                compare = hCost.CompareTo(nodeToCompare.hCost);
            }

            return -compare;
        }
    }
}