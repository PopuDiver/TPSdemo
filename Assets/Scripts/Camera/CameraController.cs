using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;

public class CameraController : NetworkBehaviour {
    public GameObject cameraPrefab;
    public Camera playerCamera;
    public float rotationSpeed = 5.0f;
    private Vector3 offset = new Vector3(3, 6, -5f); // 偏移量用于保持摄像机距离玩家固定
    public Transform point;
    public float recoilAmount = 2f;          // 后坐力的幅度
    public float recoilSpeed = 5f;           // 恢复速度
    public float recoilRecoverySpeed = 10f;  // 恢复到初始位置的速度

    private Vector3 originalRotation;
    private Vector3 currentRecoil;
    private Vector3 recoilTarget;
    private const int fov = 60;
    public GameObject aimPosition;
    public Transform aimCameraLookPosition;
    public float aimFov = 40f;
    public float aimSpeed = 5f;
    public bool isAiming;
    public GameObject def;
    public LayerMask collisionMask; // 碰撞层
    public Transform initCameraPoint;
    
    void Start()
    {
        if (playerCamera != null) {
            originalRotation = playerCamera.transform.localEulerAngles;
            currentRecoil = Vector3.zero;
            recoilTarget = Vector3.zero;
        } else {
            // if (isLocalPlayer)
            // {
                // GameObject cameraObject = new GameObject("PlayerCamera");
                GameObject cameraObject = Instantiate(cameraPrefab, Vector3.zero, Quaternion.identity);
                playerCamera = cameraObject.GetComponent<Camera>();
                // playerCamera = cameraObject.AddComponent<Camera>();
                playerCamera.transform.position = transform.position + offset;
                initCameraPoint = playerCamera.transform;
                playerCamera.transform.LookAt(point);
                
            // }

            AAA();
        }
    }

    public void GetCamera(GameObject cameraObject) {
        playerCamera = cameraObject.AddComponent<Camera>();
        playerCamera.transform.position = transform.position + offset;
        playerCamera.transform.LookAt(point);
    }
    
    private void Awake()
    {
    }

    // 当本地玩家开始时调用
    public override void OnStartLocalPlayer()
    {
        base.OnStartLocalPlayer();
        Debug.LogError("00.0.0.");
        // // 仅为本地玩家创建摄像机
        // if (isLocalPlayer)
        // {
        //     GameObject cameraObject = new GameObject("PlayerCamera");
        //     playerCamera = cameraObject.AddComponent<Camera>();
        //     playerCamera.transform.position = transform.position + offset;
        //     playerCamera.transform.LookAt(point);
        // }
    }

    public void aim()
    {
        if (isAiming)
        {
            playerCamera.transform.parent = aimPosition.transform;
            playerCamera.transform.localPosition = new Vector3(0,0.38f,0);
            playerCamera.transform.localRotation = Quaternion.Euler(new Vector3(0,0,0));
        }
        else
        {
            playerCamera.transform.parent = null;
            Vector3 desiredPosition = transform.position + transform.TransformDirection(offset);
            playerCamera.transform.position = Vector3.Lerp(playerCamera.transform.position, desiredPosition, aimSpeed * Time.deltaTime);

            Quaternion desiredRotation = Quaternion.LookRotation(transform.forward);
            playerCamera.transform.rotation = Quaternion.Lerp(playerCamera.transform.rotation, desiredRotation, aimSpeed * Time.deltaTime);
            playerCamera.transform.LookAt(point);
        }
    }

    private void LateUpdate()
    {
        // if (isLocalPlayer && playerCamera != null)
        // {
            HandleCameraRotation();
            UpdateCameraPositionAndRotation();
        // }
    }

    // 更新摄像机的位置和朝向
    private void UpdateCameraPositionAndRotation()
    {
        if (isAiming)
        {
        }
        else {
            AAA();
        }
    }

    public void AAA() {
        Vector3 dir = transform.position + offset - point.transform.position;
        Ray ray = new Ray(point.transform.position, dir);
        // 发射射线，并获取所有的碰撞点
        RaycastHit[] hits = Physics.RaycastAll(ray, 10f);
        if (hits.Length == 0) {
            playerCamera.transform.position = transform.position + offset;
            playerCamera.transform.LookAt(point);
        } else {
            Debug.DrawLine(playerCamera.transform.position, hits[0].point, Color.red, 1f);
            playerCamera.transform.position = hits[0].point + playerCamera.transform.forward * 2.0f;
            playerCamera.transform.LookAt(PlayerController_Client.GetInstence().FireObj.transform.position);
        }
    }

    // 处理摄像机的旋转
    private void HandleCameraRotation()
    {
        
        float mouseX = Input.GetAxis("Mouse X") * rotationSpeed;
        float mouseY = -Input.GetAxis("Mouse Y") * rotationSpeed;

        // 使用 Quaternion 来更新摄像机的旋转
        Quaternion camTurnAngle = Quaternion.AngleAxis(mouseX, Vector3.up);

        offset = camTurnAngle * offset;

        // 防止摄像机上下翻转
        offset = Quaternion.AngleAxis(mouseY, transform.right) * offset;
        float desiredAngleX = playerCamera.transform.eulerAngles.x + mouseY;
        if (desiredAngleX > 70 && desiredAngleX < 290)
        {
            offset = Quaternion.AngleAxis(-mouseY, transform.right) * offset;
        }
        
    }
    /// <summary>
    /// 模拟后坐力，限制摄像机的水平旋转
    /// </summary>
    public void BackForce()
    { 
        // 每次射击时应用后坐力
        recoilTarget += new Vector3(-recoilAmount, Random.Range(-recoilAmount, recoilAmount), 0f);
    }

    void ApplyRecoilEffect()
    {
        // 平滑插值实现后坐力的恢复
        currentRecoil = Vector3.Lerp(currentRecoil, recoilTarget, recoilRecoverySpeed * Time.deltaTime);
        recoilTarget = Vector3.Lerp(recoilTarget, Vector3.zero, recoilRecoverySpeed * Time.deltaTime);
    }
    
    IEnumerator recoverFOV()
    {
        yield return null;
        playerCamera.fieldOfView = fov;
        StopCoroutine(recoverFOV());
    }
    
    // 当玩家对象被销毁时销毁摄像机
    private void OnDestroy()
    {
        if (playerCamera != null)
        {
            Destroy(playerCamera.gameObject);
        }
    }
}
