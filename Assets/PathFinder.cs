using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Diagnostics;
using System.Linq;
using UnityEditor;
using Debug = UnityEngine.Debug;

namespace AsPathfinding {
    [RequireComponent(typeof(Grid))]
    public class PathFinder : MonoBehaviour {
        
        public GizmoOptions gizmoOption;
        public bool slowPathFinding;

        private Node _sourceNode;
        private Node _targetNode;
        private Grid _grid;        

        private Heap<Node> _openNodes;
        private readonly HashSet<Node> _closedNodes = new();

        private PathRequestManager _requestManager;

        private List<Node> path;

        private int maxFCost;
        private int minFCost;
        
        private void Awake() {
            if(_grid == null) _grid = GetComponent<Grid>();
            if(_requestManager == null) _requestManager = GetComponent<PathRequestManager>();
        }

        public void StartFindPath(Vector3 startPosition, Vector3 targetPosition) {
            StartCoroutine(FindPath(startPosition, targetPosition));
        }

        public IEnumerator FindPath(Vector3 startPosition, Vector3 targetPosition) {
            // Stopwatch sw = new();
            // sw.Start();
            minFCost = Int32.MaxValue;
            maxFCost = Int32.MinValue;

            path = new();
            Vector3[] waypoints = new Vector3[0];
            bool pathSuccess = false;
            
            _sourceNode = _grid.NodeFromWorldPoint(startPosition);
            _targetNode = _grid.NodeFromWorldPoint(targetPosition);

            if (_sourceNode.walkable && _targetNode.walkable) {
                _openNodes = new Heap<Node>(_grid.MaxSize);
                _closedNodes.Clear();
                OpenNode(_sourceNode,null,0);
                while (_openNodes.Count > 0) {
                    Node currentNode = _openNodes.RemoveFirstItem();
                    _closedNodes.Add(currentNode);
                    
                    if (currentNode == _targetNode) {
                        // sw.Stop();
                        // Debug.Log("Path found"); // + sw.ElapsedMilliseconds+"ms");
                        pathSuccess = true;
                        break;
                    }
                    else {
                        List<Node> neighbors = _grid.GetNeighboringNodes(currentNode);
                        foreach (Node neighbor in neighbors) {
                            if (neighbor.walkable && !_closedNodes.Contains(neighbor)) {
                                int distanceToNeighbor = GetDistance(currentNode, neighbor);
                                OpenNode(neighbor,currentNode, distanceToNeighbor);
                            }

                            if (slowPathFinding) { 
                                yield return new WaitForEndOfFrame();
                            }
                        }
                    }
                }
            }
            
            yield return null;

            if (pathSuccess) {
                waypoints = RetracePath();
            }
            _requestManager.FinishedProcessingPath(waypoints,pathSuccess);
        }

        Vector3[] RetracePath() {
            Node currentNode = _targetNode;
            
            while (currentNode != _sourceNode) {
                path.Add(currentNode);
                currentNode = currentNode.parent;
            }

            Vector3[] waypoints = SimplifyPath(path);
            Array.Reverse(waypoints);

            return waypoints;
        }

        Vector3[] SimplifyPath(List<Node> path) {
            List<Vector3> waypoints = new List<Vector3>();
            Vector2 directionOld = Vector2.zero;
            // waypoints.Add(path[0].worldPosition);

            for (int i = 1; i < path.Count-1; i++) {
                Vector2 directionNew = new Vector2(path[i-1].gridX - path[i].gridX,path[i-1].gridY - path[i].gridY);
                if (directionNew != directionOld) {
                    waypoints.Add(path[i-1].worldPosition);
                    directionOld = directionNew;
                }
            }
            waypoints.Add(path[path.Count - 1].worldPosition);

            return waypoints.ToArray();
        }

        private void OpenNode(Node node,Node parent,int distance) {
            if (!_openNodes.Contains(node)) {
                node.gCost = distance + (parent?.gCost ?? 0);
                node.hCost = GetDistance(node,_targetNode);
                node.parent = parent;
                _openNodes.Add(node);
            }
            else {
                int newGCost = parent.gCost + distance;
                if (newGCost < node.gCost) {
                    node.gCost = newGCost;
                    node.parent = parent;
                    _openNodes.UpdateItem(node);
                }
            }
            
            if(node.fCost > maxFCost) maxFCost = node.fCost;
            if(node.fCost < minFCost) minFCost = node.fCost;
        }

        private int GetDistance(Node nodeA, Node nodeB) {
            int dstX = Mathf.Abs(nodeA.gridX - nodeB.gridX);
            int dstY = Mathf.Abs(nodeA.gridY - nodeB.gridY);
            return 14 * Math.Min(dstX, dstY) + 10 * Mathf.Abs(dstX - dstY);
        }

        private void OnDrawGizmos() {
            if (gizmoOption == GizmoOptions.None) return;
            
            if (gizmoOption == GizmoOptions.GridOnly && _grid != null){
                foreach (Node node in _grid.grid) {
                    Gizmos.color = node.walkable ? Color.white : Color.black;
                    Gizmos.DrawCube(node.worldPosition,Vector3.one * (_grid.nodeDiameter - 0.1f));
                }
            }
            else if (gizmoOption == GizmoOptions.PathOnly && path != null) {
                Gizmos.color = Color.cyan;
                foreach (Node node in _grid.grid) {
                    if (path.Contains(node)) {
                        Gizmos.DrawCube(node.worldPosition,Vector3.one * (_grid.nodeDiameter - 0.1f));
                    }
                }
            }
            else if (gizmoOption == GizmoOptions.GridAndPath && path != null) {
                foreach (Node node in _grid.grid) {
                    Gizmos.color = node.walkable ? Color.white : Color.black;
                    if(path.Contains(node)) Gizmos.color = Color.cyan;
                    Gizmos.DrawCube(node.worldPosition,Vector3.one * (_grid.nodeDiameter - 0.1f));
                }
            }
            else if(gizmoOption == GizmoOptions.FCostGradient && _grid != null){
                foreach (Node node in _grid.grid) {
                    if(!node.walkable) Gizmos.color = Color.black;
                    else if(path!= null && path.Contains(node)) Gizmos.color = Color.cyan;
                    else if (_openNodes != null && _closedNodes != null && (_openNodes.Contains(node) || _closedNodes.Contains(node))) {
                        float t = Mathf.InverseLerp(minFCost, maxFCost, node.fCost);
                        Gizmos.color = Color.Lerp(Color.green, Color.red, t);
                    }
                    else {
                        Gizmos.color = Color.white;
                    }
                    Gizmos.DrawCube(node.worldPosition,Vector3.one * (_grid.nodeDiameter - 0.1f));
                }
            }
        }
    }

    public enum GizmoOptions {
        None,
        GridOnly,
        PathOnly,
        GridAndPath,
        FCostGradient
    }
}