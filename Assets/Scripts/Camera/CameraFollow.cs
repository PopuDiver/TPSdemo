using UnityEngine;

public class CameraFollow : MonoBehaviour {
    [Header("CameraMoveData")]
    [SerializeField] private Transform targetTransform;
    [SerializeField] private bool autoTargetPlayer = true;
    [SerializeField] private float moveSpeed = 1f;
    [Range(0f, 10f)] [SerializeField] private float turnSpeed = 1f;
    [SerializeField] private float turnSmoothing;
    [SerializeField] private float tiltMax = 75f;
    [SerializeField] private float tiltMin = 45f;
    [SerializeField] private bool lockCursor;
    [SerializeField] private bool verticalAutoReturn;
    private bool isAiming;
    private float shakeAmount = 0.1f;
    private Transform camTransform;
    private float lookAngle;
    private Transform pivotTransform;
    private Vector3 pivotEulers;
    private Quaternion pivotTargetRot;
    private float tiltAngle;
    private Quaternion transformTargetRot;

    private void Awake() {
        camTransform = GetComponentInChildren<Camera>().transform;
        pivotTransform = camTransform.parent;
        Cursor.lockState = lockCursor? CursorLockMode.Locked : CursorLockMode.None;
        Cursor.visible = !lockCursor;
        pivotEulers = pivotTransform.rotation.eulerAngles;
        pivotTargetRot = pivotTransform.transform.localRotation;
        transformTargetRot = transform.localRotation;
    }
    
    private void Update() {
        HandleRotationMovement();
        if (verticalAutoReturn && Input.GetMouseButtonUp(0)) {
            Cursor.lockState = verticalAutoReturn? CursorLockMode.Locked : CursorLockMode.None;
            Cursor.visible = !verticalAutoReturn;
        }
    }

    private void LateUpdate() {
        if (autoTargetPlayer && (targetTransform == null || !targetTransform.gameObject.activeSelf)) {
            FindAndTargetPlayer();
        }
        FollowTarget(Time.deltaTime);
    }
    
    private void OnDisable() {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    public bool IsAiming {
        get { return isAiming; }
        set { isAiming = value; }
    }
    
    public void Aim() {
        if (isAiming) {
            GetComponentInChildren<Camera>().fieldOfView = 40;
            GetComponentInChildren<Camera>().nearClipPlane = 0.01f;
            tiltMax = 40;
            tiltMin = 20;
        } else {
            GetComponentInChildren<Camera>().fieldOfView = 60;
            GetComponentInChildren<Camera>().nearClipPlane = 0.3f;
            tiltMax = 75;
            tiltMin = 45;
        }
    }

    public void FollowTarget(float deltaTime) {
        if (targetTransform == null) {
            return;
        }

        if (isAiming) {
            transform.position = targetTransform.position + targetTransform.forward * 5f - targetTransform.transform.up;
        } else {
            transform.position = Vector3.Lerp(transform.position, targetTransform.position + targetTransform.transform.right * 2, deltaTime * moveSpeed);
        }
    }

    public void ShakeOffset() {
        // 生成随机的抖动偏移量
        Vector3 shakeOffset = new Vector3(Random.Range(-shakeAmount, shakeAmount), Random.Range(-shakeAmount, shakeAmount), Random.Range(-shakeAmount, shakeAmount));
        if (isAiming) {
            transform.position += shakeOffset;
        } else {
            transform.position += shakeOffset;
        }
    }

    public void FindAndTargetPlayer() {
        var targetObj = GameObject.FindGameObjectWithTag("Player");
        if (targetObj) {
            SetTarget(targetObj.transform);
        }
    }

    public virtual void SetTarget(Transform newTransform) {
        targetTransform = newTransform;
    }

    private void HandleRotationMovement() {
        if (Time.timeScale < float.Epsilon) {
            return;
        }

        float x = Input.GetAxis("Mouse X");
        float y = Input.GetAxis("Mouse Y");

        lookAngle += x * turnSpeed;
        transformTargetRot = Quaternion.Euler(0f, lookAngle, 0f);

        if (verticalAutoReturn) {
            tiltAngle = y > 0? Mathf.Lerp(0, -tiltMin, y) : Mathf.Lerp(0, tiltMin, -y);
        } else {
            tiltAngle -= y * turnSpeed;
            tiltAngle = Mathf.Clamp(tiltAngle, -tiltMin, tiltMin);
        }

        pivotTargetRot = Quaternion.Euler(tiltAngle, pivotEulers.y, pivotEulers.z);

        if (turnSmoothing > 0) {
            pivotTransform.localRotation = Quaternion.Slerp(pivotTransform.localRotation, pivotTargetRot, turnSmoothing * Time.deltaTime);
            transform.localRotation = Quaternion.Slerp(transform.localRotation, transformTargetRot, turnSmoothing * Time.deltaTime);
        } else {
            pivotTransform.localRotation = pivotTargetRot;
            transform.localRotation = transformTargetRot;
        }
    }
}