using System;
using System.Collections;
using PlasticPipe.PlasticProtocol.Messages;
using UnityEngine;
using UnityEngine.InputSystem;

namespace AsPathfinding {
    public class Unit : MonoBehaviour, Controls.IPlayerActions {
        public float speed = 5f;
        private Vector3[] path;
        private int _targetIndex;
        private Vector3 _currentWaypoint;
        private bool _followingPath;
        [SerializeField] private LayerMask gridLayer;
        
        private void Awake() {
            Controls controls = new Controls();
            controls.Player.Enable();
            controls.Player.SetCallbacks(this);
        }

        public void OnClick(InputAction.CallbackContext context) {
            if (!context.performed) return;
            
            Ray ray = Camera.main.ScreenPointToRay(Mouse.current.position.ReadValue());
            if (Physics.Raycast(ray, out RaycastHit hit,Mathf.Infinity,gridLayer.value)) {
                PathRequestManager.RequestPath(transform.position,hit.point,OnPathFound);
            }
        }

        public void OnPathFound(Vector3[] newPath, bool pathSuccess) {
            if (pathSuccess) {
                // StopCoroutine(FollowPath());
                path = newPath;
                _currentWaypoint = path[0];
                _targetIndex = 0;
                _followingPath = true;
                // StartCoroutine(FollowPath());
            }
        }

        private void Update() {
            if (!_followingPath) return;
            if (transform.position == _currentWaypoint) {
                _targetIndex++;
                if (_targetIndex < path.Length) {
                    _currentWaypoint = path[_targetIndex];
                }
                else _followingPath = false;
            }

            transform.position = Vector3.MoveTowards(transform.position, _currentWaypoint, speed);
        }

        // IEnumerator FollowPath() {
        //
        //     while (true) {
        //         if (transform.position == _currentWaypoint) {
        //             _targetIndex++;
        //             if (_targetIndex < path.Length) {
        //                 _currentWaypoint = path[_targetIndex];
        //             }
        //             else yield break;
        //         }
        //
        //         transform.position = Vector3.MoveTowards(transform.position, _currentWaypoint, speed);
        //         yield return null;
        //     }
        // }

        private void OnDrawGizmos() {
            if (path != null) {
                for (int i = _targetIndex; i < path.Length; i++) {
                    Gizmos.color = Color.black;
                    Gizmos.DrawCube(path[i],Vector3.one * 0.2f);

                    if (i == _targetIndex) {
                        Gizmos.DrawLine(transform.position,path[i]);
                    }
                    else {
                        Gizmos.DrawLine(path[i-1],path[i]);
                    }
                }
            }
        }

    }
}