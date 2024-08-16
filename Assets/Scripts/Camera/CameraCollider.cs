using System;
using System.Collections;
using UnityEngine;

public class CameraCollider : MonoBehaviour {
    [Header("CameraMoveData")]
    private float clipMoveTime = 0.05f;
    private float returnTime = 0.4f;
    private float sphereCastRadius = 0.1f;
    private bool visualiseInEditor;
    private float closestDistance = 0.5f;
    private string dontClipTag = "Player";
    private float moveVelocity;
    private float originalDist;
    private bool protecting { get; set; }

    [Header("TransformData")]
    private Transform cameraTransform;
    private Transform pivotTransform;
    private float currentDist;
    
    [Header("Ray")]
    private Ray ray;
    private RaycastHit[] hits;
    private RayHitComparer rayHitComparer;

    private void Start() {
        cameraTransform = GetComponentInChildren<Camera>().transform;
        pivotTransform = cameraTransform.parent;
        originalDist = cameraTransform.localPosition.magnitude;
        currentDist = originalDist;
        rayHitComparer = new RayHitComparer();
    }
    
    private void LateUpdate() {
        float targetDist = originalDist;
        ray.origin = pivotTransform.position + pivotTransform.forward * sphereCastRadius;
        ray.direction = -pivotTransform.forward;
        Collider[] cols = Physics.OverlapSphere(ray.origin, sphereCastRadius);
        bool initialIntersect = false;
        bool hitSomething = false;
        for (int i = 0; i < cols.Length; i++) {
            if (!cols[i].isTrigger && !(cols[i].attachedRigidbody != null && cols[i].attachedRigidbody.CompareTag(dontClipTag))) {
                initialIntersect = true;
                break;
            }
        }

        if (initialIntersect) {
            ray.origin += pivotTransform.forward * sphereCastRadius;
            hits = Physics.RaycastAll(ray, originalDist - sphereCastRadius);
        } else {
            hits = Physics.SphereCastAll(ray, sphereCastRadius, originalDist + sphereCastRadius);
        }

        Array.Sort(hits, rayHitComparer);
        float nearest = Mathf.Infinity;
        for (var i = 0; i < hits.Length; i++)
            if (hits[i].distance < nearest && !hits[i].collider.isTrigger && !(hits[i].collider.attachedRigidbody != null && hits[i].collider.attachedRigidbody.CompareTag(dontClipTag))) {
                nearest = hits[i].distance;
                targetDist = -pivotTransform.InverseTransformPoint(hits[i].point).z;
                hitSomething = true;
            }

        if (hitSomething) {
            Debug.DrawRay(ray.origin, -pivotTransform.forward * (targetDist + sphereCastRadius), Color.red);
        }

        protecting = hitSomething;
        currentDist = Mathf.SmoothDamp(currentDist, targetDist, ref moveVelocity, currentDist > targetDist? clipMoveTime : returnTime);
        currentDist = Mathf.Clamp(currentDist, closestDistance, originalDist);
        cameraTransform.localPosition = -Vector3.forward * currentDist;
    }

    public class RayHitComparer : IComparer {
        public int Compare(object x, object y) {
            return ((RaycastHit)x).distance.CompareTo(((RaycastHit)y).distance);
        }
    }
}