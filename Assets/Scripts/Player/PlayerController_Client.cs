using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class PlayerController_Client : MonoBehaviour {
    private static PlayerController_Client instance;
    private int playerID;

    [Header("需要序列化的 GameObject ")]
    [SerializeField] private GameObject cameraPrefab;

    [SerializeField] private BulletMove bulletPrefab;
    [SerializeField] private GameObject uiPrefab;
    [SerializeField] private GameObject fireObj;
    [SerializeField] private GameObject playerPrefab;
    [SerializeField] private GameObject weaponGameObject;
    [SerializeField] private BeAttackEffect attackEffectPrefab;
    [SerializeField] private GameObject rightHandGameObject;

    [Header("网络相关物体")]
    private PlayerDataNet playerDataNet;

    private GameObject playerDataNetObj;

    [Header("玩家输入数据")]
    private bool isGrounded = true;
    private float moveSpeed = 2f;
    private float angleYY;
    private float rotateSpeed = 5f;
    private bool isAiming;
    private float fireTime = 0.0f;
    private bool isReload = false;

    [Header("玩家下蹲、开镜数据")]
    private readonly Vector3 COLLIDER_STAND_CENTER = new Vector3(0, 1.15f, 0);
    private readonly Vector3 COLLIDER_CROUCH_CENTER = new Vector3(0, 0.95f, 0);
    private readonly Vector3 WEAPON_GAMEOBJECT_INITPOS = new Vector3(-0.491f, -0.067f, 0.267f);
    private readonly Quaternion WEAPON_GAMEOBJECT_INITROT = Quaternion.Euler(78.397f, 112.492f, -243.231f);
    private readonly Vector3 WEAPON_GAMEOBJECT_AIMPOS = new Vector3(-0.003f, -0.086f, 0.136f);
    private readonly Quaternion WEAPON_GAMEOBJECT_AIMROT = Quaternion.Euler(179.031f, -89.98199f, -182.19f);
    private float colliderStandHeigth = 2.3f;
    private float colliderCrouchHeigth = 2f;

    [Header("玩家需要的物体")]
    private RifleWeapon weapon;

    private Rigidbody rig;
    private Animator anim;
    private GameObject playerUI;
    private GameObject playerCamera;

    [Header("玩家维护的列表")]
    private Dictionary<int, GameObject> playerDictionary;

    private List<GameObject> playerModelList;

    [Header("对象池")]
    private ObjectPool<BulletMove> bulletPool;
    private ObjectPool<BeAttackEffect> beAttackEffectPool;

    private void Awake() {
        if (instance == null) {
            instance = this;
        }
    }

    public static PlayerController_Client Instance {
        get { return instance; }
    }

    public GameObject FireObj {
        get { return fireObj; }
    }

    /// <summary>
    /// 初始化客户端在服务器上的数据体
    /// </summary>
    public void InitPlayerDataNet(GameObject playerDataNetObj) {
        if (playerDataNet != null) {
            if (playerDataNet.GetIsInit())
                return;
            this.playerDataNetObj = playerDataNetObj;
            playerDataNet = this.playerDataNetObj.GetComponent<PlayerDataNet>();
        } else {
            this.playerDataNetObj = playerDataNetObj;
            playerDataNet = this.playerDataNetObj.GetComponent<PlayerDataNet>();
        }

        if (weapon == null) {
            weapon = DataCentreController.Instance().GetRifleWeapon(playerDataNet.CurrentWeaponID);
        }

        if (playerUI == null) {
            playerUI = Instantiate(uiPrefab, Vector2.zero, Quaternion.identity);
            EventControl.Instance.Invoke(EventType.PlayerAttackBulletCountUIChange, weapon.BulletCount, weapon.MaxBulletCount);
        } else {
            EventControl.Instance.Invoke(EventType.PlayerAttackBulletCountUIChange, weapon.BulletCount, weapon.MaxBulletCount);
        }

        Animator animator = this.playerDataNetObj.GetComponent<Animator>();
        AnimController.Instance.SetPlayerAnimatorSpeed(animator, 2, "Stand_Reload", weapon.ReloadCD);
        AnimController.Instance.SetPlayerAnimatorSpeed(animator, 2, "Crouch_Reload", weapon.ReloadCD);
        AnimController.Instance.SetPlayerAnimatorSpeed(animator, 1, "Stand_Shoot", (float)weapon.ShootCD / 2);
        AnimController.Instance.SetPlayerAnimatorSpeed(animator, 1, "Crouch_Shoot", (float)weapon.ShootCD / 2);
    }

    public void InitTransform() {
        playerDataNet.PlayerTransformChange(playerDataNet.PlayerID, gameObject.transform, 0, 0);
    }

    private void Start() {
        rig = GetComponent<Rigidbody>();
        anim = GetComponent<Animator>();
        playerDictionary = new Dictionary<int, GameObject>();
        playerCamera = Instantiate(cameraPrefab);
        weaponGameObject.transform.localPosition = WEAPON_GAMEOBJECT_INITPOS;
        weaponGameObject.transform.localRotation = WEAPON_GAMEOBJECT_INITROT;
        bulletPool = new ObjectPool<BulletMove>(bulletPrefab, 10, transform);
        beAttackEffectPool = new ObjectPool<BeAttackEffect>(attackEffectPrefab, 10, transform);
    }

    public void ReturnBullet(BulletMove bullet) {
        bulletPool.ReturnObject(bullet);
    }

    public void ReturnBeAttackEffect(BeAttackEffect effect) {
        beAttackEffectPool.ReturnObject(effect);
    }

    public void CharacterReload(int playerId, string name, bool isReloadFormal) {
        if (weapon.BulletCount != weapon.MaxBulletCount && !playerDataNet.GetIsPlayerDead() && !isReload) {
            if (playerDataNet.PlayerID == playerId) {
                isReload = isReloadFormal;
                AnimController.Instance.PlayerAnimatorSetBool(anim, name, isReload);
                playerDataNet.PlayerAnimSetBool(name, isReloadFormal);
                if (isAiming) {
                    playerCamera.GetComponent<CameraFollow>().IsAiming = !isAiming;
                    playerCamera.GetComponent<CameraFollow>().Aim();
                    weaponGameObject.transform.parent = rightHandGameObject.transform;
                    weaponGameObject.transform.localPosition = WEAPON_GAMEOBJECT_INITPOS;
                    weaponGameObject.transform.localRotation = WEAPON_GAMEOBJECT_INITROT;
                    foreach (GameObject obj in playerModelList) {
                        obj.GetComponent<SkinnedMeshRenderer>().enabled = true;
                    }
                }
                StartCoroutine(Reload());
            } else if (playerDictionary.ContainsKey(playerId)) {
                Animator anim = playerDictionary[playerId].GetComponent<Animator>();
                AnimController.Instance.PlayerAnimatorSetBool(anim, name, isReloadFormal);
            }
        }
    }

    public int GameObjToGetPlayerID(GameObject obj) {
        if (playerDictionary.ContainsValue(obj)) {
            foreach (var dic in playerDictionary) {
                if (dic.Value == obj) {
                    return dic.Key;
                }
            }
        }

        return -1;
    }

    public int GetPlayerID() {
        return playerDataNet.PlayerID;
    }

    /// <summary>
    /// 当新加入玩家时，生成对应的模型
    /// </summary>
    public void ClientAddPlayerModel() {
        if (playerDataNet == null)
            return;
        List<PlayerDataNet> tempPlayerDatas = new List<PlayerDataNet>();
        tempPlayerDatas = DataCentreController.Instance().GetPlayerDataNetList();
        foreach (PlayerDataNet tempData in tempPlayerDatas) {
            if (tempData.PlayerID != playerDataNet.PlayerID) {
                if (playerDictionary.ContainsKey(tempData.PlayerID))
                    continue;
                GameObject obj = Instantiate(playerPrefab, tempData.GetPlayerDataObj().transform.position, tempData.GetPlayerDataObj().transform.rotation);
                if (obj == null) {
                    Debug.LogError(" obj == null ");
                }

                obj.transform.localScale = tempData.gameObject.transform.localScale;
                playerDictionary.Add(tempData.PlayerID, obj);
            }
        }
    }

    public void Move(float horizontal, float vertical) {
        if (!playerDataNet.GetIsPlayerDead()) {
            ClientOtherPlayerMove(playerDataNet.PlayerID, horizontal, vertical);
        }
    }

    public void AnimRot(float horizontal, float vertical) {
        if (!playerDataNet.GetIsPlayerDead()) {
            ClientOtherPlayerAnimSetFloat(playerDataNet.PlayerID, horizontal, vertical);
        }
    }

    public void ClientOtherPlayerMove(int playerId, float horizontal, float vertical) {
        if (playerDataNet.PlayerID == playerId) {
            Vector3 movementV = transform.forward * moveSpeed * vertical * Time.deltaTime;
            Vector3 movementH = transform.right * moveSpeed * horizontal * Time.deltaTime;
            transform.localPosition += movementH + movementV;
            playerDataNet.PlayerTransformChange(playerId, transform, horizontal, vertical);
        } else if (playerDictionary.ContainsKey(playerId)) {
            PlayerDataNet tempPlayerDataNet = DataCentreController.Instance().GetPlayerDataNet(playerId);
            playerDictionary[playerId].transform.position = tempPlayerDataNet.gameObject.transform.position;
            playerDictionary[playerId].transform.rotation = tempPlayerDataNet.gameObject.transform.rotation;
            playerDictionary[playerId].transform.localScale = tempPlayerDataNet.gameObject.transform.localScale;
        }
    }

    public void AnimSetBool(string name, bool flag) {
        ClientOtherPlayerAnimSetBool(playerDataNet.PlayerID, name, flag);
    }

    public void ClientOtherPlayerAnimSetBool(int playerId, string name, bool flag) {
        if (!playerDataNet.GetIsPlayerDead()) {
            switch (name) {
                case "isCrouch": {
                    CharacterCrouch(playerId, name, flag);
                    break;
                }
                case "isJump": {
                    CharacterJump(playerId, name, flag);
                    break;
                }
                case "isReload": {
                    CharacterReload(playerId, name, flag);
                    break;
                }
                case "isPlayerDead": {
                    PlayerDead(playerId, name, flag);
                    break;
                }
            }
        }
    }

    public void ClientOtherPlayerRot(int playerId, float mouseX = 0) {
        if (playerDataNet == null) {
            Debug.LogError(" PlayerController_Client ClientOtherPlayerRot playerData == null ");
            return;
        }
        if (playerDataNet.PlayerID == playerId) {
            // 左右转动视角
            float angleY = mouseX * rotateSpeed;
            angleYY = angleY + angleYY;
            if (mouseX != 0) {
                transform.eulerAngles = new Vector3(0, angleYY, transform.eulerAngles.z);
            }

            if (playerDataNet != null && mouseX != 0) {
                playerDataNet.PlayerRotChange(transform.rotation);
            }
        } else if (playerDictionary.ContainsKey(playerId)) {
            PlayerDataNet tempPlayerDataNet = DataCentreController.Instance().GetPlayerDataNet(playerId);
            playerDictionary[playerId].transform.rotation = tempPlayerDataNet.gameObject.transform.rotation;
        }
    }

    public void ClientOtherPlayerAnimSetFloat(int playerId, float x, float y) {
        if (playerDataNet.PlayerID == playerId) {
            AnimController.Instance.PlayerAnimatorSetFloat(anim, "x", x);
            AnimController.Instance.PlayerAnimatorSetFloat(anim, "y", y);
            playerDataNet.PlayerAnimSetFloat(x, y);
        } else if (playerDictionary.ContainsKey(playerId)) {
            Animator anim = playerDictionary[playerId].GetComponent<Animator>();
            AnimController.Instance.PlayerAnimatorSetFloat(anim, "x", x);
            AnimController.Instance.PlayerAnimatorSetFloat(anim, "y", y);
        }
    }


    public void CharacterCrouch(int playerId, string name, bool flag) {
        if (playerDataNet.PlayerID == playerId) {
            AnimController.Instance.PlayerAnimatorSetBool(anim, name, flag);
            if (flag) {
                GetComponent<CapsuleCollider>().center = COLLIDER_CROUCH_CENTER;
                GetComponent<CapsuleCollider>().height = colliderCrouchHeigth;
            } else {
                GetComponent<CapsuleCollider>().center = COLLIDER_STAND_CENTER;
                GetComponent<CapsuleCollider>().height = colliderStandHeigth;
            }

            playerDataNet.PlayerAnimSetBool(name, flag);
        } else if (playerDictionary.ContainsKey(playerId)) {
            if (flag) {
                playerDictionary[playerId].GetComponent<CapsuleCollider>().center = COLLIDER_CROUCH_CENTER;
                playerDictionary[playerId].GetComponent<CapsuleCollider>().height = colliderCrouchHeigth;
            } else {
                playerDictionary[playerId].GetComponent<CapsuleCollider>().center = COLLIDER_STAND_CENTER;
                playerDictionary[playerId].GetComponent<CapsuleCollider>().height = colliderStandHeigth;
            }

            Animator anim = playerDictionary[playerId].GetComponent<Animator>();
            AnimController.Instance.PlayerAnimatorSetBool(anim, name, flag);
        }
    }

    public void ClientOtherPlayerAnimSetTrigger(int playerId, string name) {
        if (playerDataNet.PlayerID == playerId) {
            Debug.LogError(" ClientOtherPlayerAnimSetTrigger  playerData.playerID == playerId ");
            GetComponent<AudioSource>().Play();
            AnimController.Instance.PlayerAnimatorSetTrigger(anim, name);
            playerDataNet.PlayerAnimSetTrigger(name);
        } else if (playerDictionary.ContainsKey(playerId)) {
            playerDictionary[playerId].GetComponent<AudioSource>().Play();
            Animator animator = playerDictionary[playerId].GetComponent<Animator>();
            AnimController.Instance.PlayerAnimatorSetTrigger(animator, name);
        }
    }

    public void ClientOtherPlayerLeave(int playerId) {
        if (playerDataNet == null)
            return;
        List<PlayerDataNet> tempPlayerDatas = new List<PlayerDataNet>();
        tempPlayerDatas = DataCentreController.Instance().GetPlayerDataList();
        foreach (PlayerDataNet tempData in tempPlayerDatas) {
            Debug.LogError(" *------------- tempData.playerID " + tempData.PlayerID);
            if (tempData.PlayerID == playerId) {
                Destroy(playerDictionary[playerId]);
                playerDictionary.Remove(playerId);
            }
        }
    }

    public void PlayerAttack() {
        if (!playerDataNet.GetIsPlayerDead() && weapon != null && Time.time - fireTime >= weapon.ShootCD && !isReload) {
            Vector3 startPos = fireObj.transform.position;
            Quaternion startRot = fireObj.transform.rotation;
            Vector3 direction = fireObj.transform.forward;

            if (!isAiming) {
                Vector3 screenCenter = new Vector3(Screen.width / 2, Screen.height / 2, 0);
                // 将屏幕坐标转换为世界坐标
                Ray ray = playerCamera.GetComponentInChildren<Camera>().ScreenPointToRay(screenCenter);
                RaycastHit hit;
                Vector3 targetPoint;
                if (Physics.Raycast(ray, out hit)) {
                    // 如果射线击中了物体，获取击中点作为目标点
                    targetPoint = hit.point;
                } else {
                    // 如果没有击中物体，假设准心指向无穷远处
                    targetPoint = ray.GetPoint(1000);
                }

                // 计算从枪口到目标点的方向向量
                direction = (targetPoint - fireObj.transform.position).normalized;
            }

            fireTime = Time.time;
            playerCamera.GetComponent<CameraFollow>().ShakeOffset();
            ClientOtherPlayerAnimSetTrigger(playerDataNet.PlayerID, "Shoot");
            weapon.BulletCount--;
            if (weapon.BulletCount <= 0) {
                CharacterReload(playerDataNet.PlayerID, "isReload", true);
            }

            EventControl.Instance.Invoke(EventType.PlayerAttackBulletCountUIChange, weapon.BulletCount, weapon.MaxBulletCount);
            EventControl.Instance.Invoke(EventType.PlayerAttackCrossUIChange, 30.0f);
            ClientSpawnBullet(playerDataNet.PlayerID, startPos, startRot, direction, weapon.ID);
        }
    }
    
    public void ClientSpawnEffect(int playerId, Vector3 startPos) {
        if (playerDataNet.PlayerID != playerId) {
            BeAttackEffect effect = beAttackEffectPool.GetObject();
            effect.transform.position = startPos;
            effect.transform.rotation = Quaternion.identity;
            effect.transform.parent = playerDictionary[playerId].transform;
        } else {
            BeAttackEffect effect = beAttackEffectPool.GetObject();
            effect.transform.position = startPos;
            effect.transform.rotation = Quaternion.identity;
            effect.transform.parent = gameObject.transform;
            playerDataNet.SpawnEffect(startPos);
        }
    }

    public void ClientSpawnBullet(int playerId, Vector3 startPoint, Quaternion startRot, Vector3 dir, int weaponId) {
        if (playerDataNet.PlayerID != playerId) {
            BulletMove bullet = bulletPool.GetObject();
            bullet.transform.parent = null;
            bullet.transform.position = startPoint;
            bullet.transform.rotation = startRot;
            bullet.SetBulletData(dir, weaponId, playerId);
        } else {
            BulletMove bullet = bulletPool.GetObject();
            bullet.transform.parent = null;
            bullet.transform.position = startPoint;
            bullet.transform.rotation = startRot;
            bullet.SetBulletData(dir, weapon.ID, playerDataNet.PlayerID);
            playerDataNet.PlayerAttack(startPoint, startRot, dir, weapon.ID);
        }
    }

    IEnumerator Reload() {
        yield return new WaitForSeconds(weapon.ReloadCD);
        weapon.BulletCount = weapon.MaxBulletCount;
        EventControl.Instance.Invoke(EventType.PlayerAttackBulletCountUIChange, weapon.BulletCount, weapon.MaxBulletCount);
        isReload = false;
        ClientOtherPlayerAnimSetBool(playerID, "isReload", isReload);
        if (isAiming) {
            playerCamera.GetComponent<CameraFollow>().IsAiming = isAiming;
            playerCamera.GetComponent<CameraFollow>().Aim();
            weaponGameObject.transform.parent = playerCamera.transform.GetChild(0).GetChild(0).transform;
            weaponGameObject.transform.localPosition = WEAPON_GAMEOBJECT_AIMPOS;
            weaponGameObject.transform.localRotation = WEAPON_GAMEOBJECT_AIMROT;
            foreach (GameObject obj in playerModelList) {
                obj.GetComponent<SkinnedMeshRenderer>().enabled = false;
            }
        }
    }

    public void CharacterRotate(float mouseX) {
        if (null != playerDataNet && !playerDataNet.GetIsPlayerDead()) {
            ClientOtherPlayerRot(playerDataNet.PlayerID, mouseX);
        }
    }

    public void CharacterJump(int playerId, string name, bool flag) {
        if (!playerDataNet.GetIsPlayerDead() && isGrounded) {
            if (playerId == playerDataNet.PlayerID) {
                isGrounded = false;
                AnimController.Instance.PlayerAnimatorSetBool(anim, name, flag);
                rig.AddForce(Vector3.up * 5, ForceMode.Impulse);
                playerDataNet.PlayerAnimSetBool(name, flag);
            } else if (playerDictionary.ContainsKey(playerId)) {
                Animator anim = playerDictionary[playerId].GetComponent<Animator>();
                AnimController.Instance.PlayerAnimatorSetBool(anim, name, flag);
            }
        }
    }

    public void CharacterAim() {
        if (!playerDataNet.GetIsPlayerDead() && !isReload) {
            isAiming = !isAiming;
            playerCamera.GetComponent<CameraFollow>().IsAiming = isAiming;
            playerCamera.GetComponent<CameraFollow>().Aim();
            EventControl.Instance.Invoke(EventType.PlayerIsAiming, isAiming);
            if (isAiming) {
                weaponGameObject.transform.parent = playerCamera.transform.GetChild(0).GetChild(0).transform;
                weaponGameObject.transform.localPosition = WEAPON_GAMEOBJECT_AIMPOS;
                weaponGameObject.transform.localRotation = WEAPON_GAMEOBJECT_AIMROT;
                foreach (GameObject obj in playerModelList) {
                    obj.GetComponent<SkinnedMeshRenderer>().enabled = false;
                }
            } else {
                weaponGameObject.transform.parent = rightHandGameObject.transform;
                weaponGameObject.transform.localPosition = WEAPON_GAMEOBJECT_INITPOS;
                weaponGameObject.transform.localRotation = WEAPON_GAMEOBJECT_INITROT;
                foreach (GameObject obj in playerModelList) {
                    obj.GetComponent<SkinnedMeshRenderer>().enabled = true;
                }
            }
        }
    }

    private void OnCollisionEnter(Collision collision) {
        if (collision.gameObject.CompareTag("Floor")) {
            isGrounded = true;
            if (playerDataNet != null) {
                AnimController.Instance.PlayerAnimatorSetBool(anim, "isJump", false);
                playerDataNet.PlayerAnimSetBool("isJump", false);
            }
        }
    }

    public void ShowOverImage(int playerId) {
        if (playerDataNet.PlayerID == playerId) {
            string s = "恭喜您获得了胜利，祝您武运昌隆！！";
            EventControl.Instance.Invoke(EventType.GameOverPlayerUI, s);
        } else {
            string s = string.Format("游戏结束，获胜者为：{0}号玩家\n请大家继续努力！", playerId);
            EventControl.Instance.Invoke(EventType.GameOverPlayerUI, s);
        }
    }

    public void PlayerDead(int playerId, string name, bool flag) {
        if (playerDataNet != null) {
            if (playerDataNet.PlayerID == playerId) {
                AnimController.Instance.PlayerAnimatorSetBool(anim, name, flag);
                playerDataNet.PlayerAnimSetBool(name, flag);

                if (!flag) {
                    int i = Random.Range(0, CustomNetworkManager.instance.playerStartPointList.Count);
                    transform.localPosition = CustomNetworkManager.instance.playerStartPointList[i].position;
                    playerDataNet.PlayerTransformChange(playerDataNet.PlayerID, transform, 0, 0);
                } else {
                    if (isAiming) {
                        anim.speed = 1f;
                    }
                }
            } else if (playerDictionary.ContainsKey(playerId)) {
                if (flag) {
                    playerDictionary[playerId].GetComponent<Rigidbody>().useGravity = false;
                    playerDictionary[playerId].GetComponent<Collider>().enabled = false;
                } else {
                    playerDictionary[playerId].GetComponent<Collider>().enabled = true;
                    playerDictionary[playerId].GetComponent<Rigidbody>().useGravity = true;
                }

                Animator anim = playerDictionary[playerId].GetComponent<Animator>();
                AnimController.Instance.PlayerAnimatorSetBool(anim, name, flag);
            }
        }
    }

    /// <summary>
    /// 玩家血量变化会调用controller中回调
    /// </summary>
    /// <param name="oldHealth"></param>
    /// <param name="newHealth"></param>
    /// <param name="maxHealth"></param>
    public void OnHealthChanged(int newHealth, int maxHealth) {
        if (playerUI == null) {
            playerUI = Instantiate(uiPrefab, Vector2.zero, Quaternion.identity);
            EventControl.Instance.Invoke(EventType.PlayerHealthChange, newHealth * 1.0f / maxHealth);
        } else {
            EventControl.Instance.Invoke(EventType.PlayerHealthChange, newHealth * 1.0f / maxHealth);
        }
    }

    private void OnDestroy() {
        if (playerUI != null) {
            Destroy(playerUI);
        }
    }
}