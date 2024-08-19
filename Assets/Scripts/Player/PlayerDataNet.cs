using Mirror;
using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Org.BouncyCastle.Crmf;
using Player;

public class PlayerDataNet : NetworkBehaviour {
    [SyncVar]
    private int playerID;

    [SyncVar(hook = nameof(OnHealthChanged))]
    [SerializeField] private int currentHealth;
    
    private int maxHealth = 100;
    private int minHealth = 0;
    private Transform playerTransform;
    private bool isInit;
    private List<int> killList;
    private List<int> beKillList;
    private List<int> weaponList;
    private int currentWeaponID;
    private ServerPlayerController serverPlayerController;


    public PlayerDataNet() {
    }

    private void Awake() {
        if (killList == null && beKillList == null) {
            killList = new List<int>();
            beKillList = new List<int>();
            weaponList = new List<int>();
            weaponList.Add(0);
            currentWeaponID = weaponList[0];
        }
    }

    private void Start() {
        
        if (isServer) {
            serverPlayerController = new ServerPlayerController(this);
        }
        
        if (isLocalPlayer) {
            PlayerController_Client.Instance.InitPlayerDataNet(GetPlayerDataObj());
            isInit = true;
            PlayerController_Client.Instance.InitTransform();
        }
        
        if (isClient) {
            DataCentreController.Instance().PlayerAddDataClient(GetPlayerData());
            PlayerController_Client.Instance.ClientAddPlayerModel();
        } else {
            Debug.Log(" 在 PlayerData.Awake 中 PlayerController_Client.instance == null || isClient == false ");
        }
    }

    public int PlayerID {
        get { return playerID; }
        set { playerID = value; }
    }

    public int CurrentHealth {
        get { return currentHealth; }
    }

    public int CurrentWeaponID {
        get { return currentWeaponID; }
        set { currentWeaponID = value; }
    }

    public List<int> KillList {
        get { return killList; }
    }

    public void KillListAdd(int playerId) {
        killList.Add(playerId);
    }

    private PlayerDataNet GetPlayerData() {
        return this;
    }

    public bool GetIsInit() {
        return isInit;
    }

    public GameObject GetPlayerDataObj() {
        return gameObject;
    }

    /// <summary>
    /// 改变玩家血量，增加减少同在
    /// </summary>
    /// <param name="health"></param>
    public void ChangeHealth(int damage) {
        int newHealth = currentHealth + damage;
        if (newHealth > minHealth && newHealth < maxHealth) {
            currentHealth = newHealth;
        } else if (newHealth >= maxHealth) {
            currentHealth = maxHealth;
        } else if (newHealth <= minHealth) {
            currentHealth = minHealth;
            StartCoroutine(PlayerRevive());
        }
    }

    IEnumerator PlayerRevive() {
        yield return new WaitForSeconds(5.0f);
        ChangeHealth(maxHealth);
        RpcPlayerDead(false);
    }

    public void PlayerTransformChange(int playerId, Transform transform1, float horizontal, float vertical) {
        Debug.Log(" PlayerTransformChange ");
        gameObject.transform.position = transform1.position;
        gameObject.transform.rotation = transform1.rotation;
        gameObject.transform.localScale = transform1.localScale;
        CmdPlayerMoveSync(playerId, horizontal, vertical);
    }

    public void PlayerRotChange(Quaternion rotation) {
        Debug.Log(" PlayerRotChange ");
        gameObject.transform.rotation = rotation;
        if (isLocalPlayer)
            CmdPlayerRotaSync(playerID);
    }

    public void PlayerAnimSetFloat(float x, float y) {
        CmdPlayerAnimSetFloatSync(playerID, x, y);
    }

    public void PlayerAnimSetBool(string name, bool flag) {
        CmdPlayerAnimSetBoolSync(playerID, name, flag);
    }

    public void PlayerAnimSetTrigger(string name) {
        if(isLocalPlayer)
            CmdPlayerAnimSetTriggerlSync(playerID, name);
    }

    /// <summary>
    /// 玩家开火
    /// </summary>
    public void PlayerAttack(Vector3 spawnPos, Quaternion spawnRot, Vector3 dir, int weaponId) {
        CmdPlayerAttack(spawnPos, spawnRot, dir, weaponId);
    }

    public void SpawnEffect(Vector3 startPos) {
        CmdSpawnEffect(startPos);
    }

    public void TakeDamageClient(int weaponId, int beAttackId, int attackPlayerID) {
        Debug.LogError("  ------------------------ TakeDamageClient ");
        CmdTakeDamage(weaponId, beAttackId, attackPlayerID);
    }

    public void OnHealthChanged(int oldHealth, int newHealth) {
        if (isLocalPlayer) {
            Debug.Log(" 血量改变回调进行 ");
            PlayerController_Client.Instance.OnHealthChanged(newHealth, maxHealth);
        }
    }

    public bool GetIsPlayerDead() {
        return currentHealth <= minHealth;
    }

    [Command]
    public void CmdPlayerMoveSync(int playerId, float horizontal, float vertical) {
        Debug.Log(" CmdPlayerMoveSync + playerId + playerID " + playerId + playerID);
        RpcPlayerMoveSync(playerId, horizontal, vertical);
    }

    [Command]
    public void CmdPlayerRotaSync(int playerId) {
        Debug.Log(" CmdPlayerRotaSync  ");
        RpcPlayerRotSync(playerId);
    }

    [Command]
    public void CmdPlayerAnimSetFloatSync(int playerId, float x, float y) {
        Debug.Log(" CmdPlayerAnimSetFloatSync  ");
        RpcPlayerAnimSetFloatSync(playerId, x, y);
    }

    [Command]
    public void CmdPlayerAnimSetBoolSync(int playerId, string name, bool flag) {
        Debug.Log(" CmdPlayerAnimSetBoolSync  ");
        RpcPlayerAnimSetBoolSync(playerId, name, flag);
    }

    [Command]
    public void CmdPlayerAnimSetTriggerlSync(int playerId, string name) {
        Debug.Log(" CmdPlayerAnimSetTriggerlSync  ");
        RpcPlayerAnimSetTriggerSync(playerId, name);
    }

    [Command]
    public void CmdPlayerAttack(Vector3 spawnPos, Quaternion spawnRot, Vector3 dir, int weaponId) {
        RpcPlayerSpawnBullet(spawnPos, spawnRot, dir, weaponId);
    }

    [Command]
    public void CmdSpawnEffect(Vector3 startPos) {
        RpcSpawnEffect(playerID, startPos);
    }

    [Command]
    public void CmdTakeDamage(int weaponId, int beAttackId, int attackPlayerID) {
        serverPlayerController.TakeDamage(weaponId, beAttackId, attackPlayerID);
    }

    // ------------------------------------------------- 客户端 Command 方法 && 服务端 ClientRpc 方法分界线 ------------------------------------------------------

    [ClientRpc]
    public void RpcPlayerMoveSync(int playerId, float horizontal, float vertical) {
        Debug.Log(" RpcPlayerMoveSync ");
        if (!isLocalPlayer) {
            Debug.Log(" RpcPlayerMoveSync 1111 ");
            PlayerController_Client.Instance.ClientOtherPlayerMove(playerId, horizontal, vertical);
        }
    }

    [ClientRpc]
    public void RpcPlayerRotSync(int playerId) {
        Debug.Log(" RpcPlayerRotSync ");
        if (PlayerController_Client.Instance != null && !isLocalPlayer)
            PlayerController_Client.Instance.ClientOtherPlayerRot(playerId);
    }

    [ClientRpc]
    public void RpcPlayerAnimSetFloatSync(int playerId, float x, float y) {
        Debug.Log(" RpcPlayerAnimSetFloatSync ");
        if (!isLocalPlayer) 
            PlayerController_Client.Instance.ClientOtherPlayerAnimSetFloat(playerId, x, y);
    }

    [ClientRpc]
    public void RpcPlayerAnimSetBoolSync(int playerId, string name, bool flag) {
        Debug.Log(" RpcPlayerAnimSetBoolSync ");
        if (!isLocalPlayer) 
            PlayerController_Client.Instance.ClientOtherPlayerAnimSetBool(playerId, name, flag);
    }

    [ClientRpc]
    public void RpcPlayerSpawnBullet(Vector3 startPoint, Quaternion startRot, Vector3 dir, int weaponId) {
        Debug.Log(" RpcPlayerSpawnBullet ");
        if (!isLocalPlayer) 
            PlayerController_Client.Instance.ClientSpawnBullet(playerID, startPoint, startRot, dir, weaponId);
    }

    [ClientRpc]
    public void RpcGameOver(int playerId) {
        Debug.LogError(" 有玩家获得胜利，playerId =  " + playerId);
        PlayerController_Client.Instance.ShowOverImage(playerId);
        Time.timeScale = 0;
    }

    [ClientRpc]
    public void RpcPlayerAnimSetTriggerSync(int playerId, string name) {
        Debug.Log(" RpcPlayerAnimSetTriggerSync ");
        if (!isLocalPlayer)  {
            PlayerController_Client.Instance.ClientOtherPlayerAnimSetTrigger(playerId, name);
        }
    }

    [ClientRpc]
    public void RpcSpawnEffect(int playerId, Vector3 startPos) {
        Debug.Log(" RpcSpawnEffect ");
        if (!isLocalPlayer) 
            PlayerController_Client.Instance.ClientSpawnEffect(playerId, startPos);
    }

    [TargetRpc]
    public void RpcPlayerDead(bool isPlayerDead) {
        Debug.Log(" RpcPlayerDead ");
        if (!isLocalPlayer) 
            PlayerController_Client.Instance.PlayerDead(playerID, "isPlayerDead", isPlayerDead);
    }
}