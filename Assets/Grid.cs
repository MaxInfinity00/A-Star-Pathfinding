using System;
using System.Collections.Generic;
using Newtonsoft.Json.Serialization;
using Unity.Plastic.Newtonsoft.Json.Linq;
using UnityEngine;

namespace AsPathfinding {
    public class Grid : MonoBehaviour {

        public bool displayGridGizmos;
        public LayerMask unwalkableMask;
        public Vector2 gridWorldSize;
        public float nodeRadius;
        public Node[,] grid;
        
        [HideInInspector]public float nodeDiameter;
        private int gridSizeX;
        private int gridSizeY;

        public int MaxSize => gridSizeX * gridSizeY;

        private void Awake() {
            nodeDiameter = nodeRadius * 2;
            gridSizeX = Mathf.RoundToInt(gridWorldSize.x / nodeDiameter);
            gridSizeY = Mathf.RoundToInt(gridWorldSize.y / nodeDiameter);
            CreateGrid();
        }

        private void CreateGrid() {
            grid = new Node[gridSizeX, gridSizeY];
            Vector3 WorldBottomLeft =
                transform.position - Vector3.right * gridWorldSize.x / 2 - Vector3.forward * gridWorldSize.y / 2;
            
            for (int x = 0; x < gridSizeX; x++) {
                for (int y = 0; y < gridSizeY; y++) {
                    Vector3 worldPoint = WorldBottomLeft + Vector3.right * (x * nodeDiameter + nodeRadius) +
                                         Vector3.forward * (y * nodeDiameter + nodeRadius);
                    bool walkable = !(Physics.CheckSphere(worldPoint,nodeRadius,unwalkableMask));
                    grid[x, y] = new Node(walkable, worldPoint,x,y);
                }
            }
        }

        public Node NodeFromWorldPoint(Vector3 worldPosition) {
            int x = Mathf.RoundToInt(Mathf.Clamp01((worldPosition.x + gridWorldSize.x / 2f) / gridWorldSize.x) * (gridSizeX - 1));
            int y = Mathf.RoundToInt(Mathf.Clamp01((worldPosition.z + gridWorldSize.y / 2f) / gridWorldSize.y) * (gridSizeY - 1));
            // Debug.Log(x + " " + y);
            return grid[x,y];
        }

        public List<Node> GetNeighboringNodes(Node node) {
            List<Node> neighbors = new List<Node>();
            for (int x = -1; x <= 1; x++) {
                for (int y = -1; y <= 1; y++) {
                    if (x == 0 && y == 0)
                        continue;
                    
                    int checkX = node.gridX+x;
                    int checkY = node.gridY+y;

                    if (checkX >= 0 && checkX < gridSizeX && checkY >= 0 && checkY < gridSizeY) {
                        neighbors.Add(grid[checkX, checkY]);
                    }
                }
            }

            return neighbors;
        }


        private void OnDrawGizmos() {
            Gizmos.DrawWireCube(transform.position, new Vector3(gridWorldSize.x,1,gridWorldSize.y));
            
           
        }
    }
}