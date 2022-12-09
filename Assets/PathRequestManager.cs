using UnityEngine;
using System;
using System.Collections.Generic;

namespace AsPathfinding {
    [RequireComponent(typeof(PathFinder))]
    public class PathRequestManager : MonoBehaviour {
        private Queue<PathRequest> pathRequestQueue = new();
        private PathRequest currentPathRequest;

        public PathFinder pathFinder;

        private bool isProcessingPath;

        private static PathRequestManager instance;

        private void Awake() {
            instance = this;
            if (pathFinder == null) pathFinder = GetComponent<PathFinder>();
        }

        public static void RequestPath(Vector3 pathStart, Vector3 pathEnd, Action<Vector3[], bool> callback) {
            PathRequest newRequest = new PathRequest(pathStart, pathEnd, callback);
            instance.pathRequestQueue.Enqueue(newRequest);
            instance.TryProcessNext();
        }

        private void TryProcessNext() {
            if (!isProcessingPath && pathRequestQueue.Count > 0) {
                currentPathRequest = pathRequestQueue.Dequeue();
                pathFinder.StartFindPath(currentPathRequest.pathStart,currentPathRequest.pathEnd);
                isProcessingPath = true;
            }
        }

        public void FinishedProcessingPath(Vector3[] path, bool pathFound) {
            currentPathRequest.callback(path,pathFound);
            isProcessingPath = false;
            TryProcessNext();
        }

        struct PathRequest {
            public Vector3 pathStart;
            public Vector3 pathEnd;
            public Action<Vector3[], bool> callback;

            public PathRequest(Vector3 pathStart, Vector3 pathEnd, Action<Vector3[], bool> callback) {
                this.pathStart = pathStart;
                this.pathEnd = pathEnd;
                this.callback = callback;
            }
        }
    }
}