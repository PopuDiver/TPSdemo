using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using LitJson;
using Mirror;
using Player;

public enum WeaponType {
    Gun,
    Thrown
}

public class DataCentreController : NetworkBehaviour {
    private static DataCentreController instance;
    private Dictionary<int, PlayerDataNet> playerNetDictionary;
    private Dictionary<int, RifleWeapon> rifleWeaponDataDictionary;
    private Dictionary<int, PlayerData> playerDictionaryServer;

    private void Awake() {
        if (instance == null) {
            instance = this;
        }

        if (instance != null) {
            playerNetDictionary = new Dictionary<int, PlayerDataNet>();
            rifleWeaponDataDictionary = new Dictionary<int, RifleWeapon>();
            ReadJsonData();
        }

        if (isServerOnly) {
            if (playerDictionaryServer == null) {
                playerDictionaryServer = new Dictionary<int, PlayerData>();
            }
        }
    }

    public static DataCentreController GetInstance() {
        return instance;
    }
    
    private void Start() {
        playerNetDictionary = new Dictionary<int, PlayerDataNet>();
        rifleWeaponDataDictionary = new Dictionary<int, RifleWeapon>();
        ReadJsonData();
    }

    private void ReadJsonData() {
        TextAsset rifleWeaponFile = Resources.Load<TextAsset>("JSON/RifleWeaponData");
        string rifleWeaponJsonData = rifleWeaponFile.text; //物品信息的Json格式
        
        if (rifleWeaponDataDictionary.Count == 0) {
            rifleWeaponDataDictionary = new Dictionary<int, RifleWeapon>();
        }
        
        JsonData data = JsonMapper.ToObject(rifleWeaponJsonData);
        foreach (JsonData temp in data) {
            int id = (int)temp["ID"];
            double shootCd = (double)temp["ShootCD"];
            int damage = (int)temp["Damage"];
            int maxBulletCount = (int)temp["MaxBulletCount"];
            int reloadCd = (int)temp["ReloadCD"];
            int weaponType = (int)temp["WeaponType"];
            double flySpeed = (double)temp["BulletFlySpeed"];
            double gravity = (double)temp["BulletGravity"];
            RifleWeapon rifleWeapon = new RifleWeapon(id, shootCd, maxBulletCount, reloadCd, (WeaponType)weaponType, flySpeed, gravity, damage);
            rifleWeaponDataDictionary.Add(rifleWeapon.ID, rifleWeapon);
        }
    }

    public void PlayerAddData(PlayerDataNet playerDataNet) {
        int playerID = playerNetDictionary.Count;
        playerDataNet.PlayerID = playerID;
        playerDataNet.ChangeHealth(100);
    }

    public void PlayerAddDataClient(PlayerDataNet playerDataNet) {
        if (playerNetDictionary.ContainsKey(playerDataNet.PlayerID)) {
            return;
        }
        playerNetDictionary.Add(playerDataNet.PlayerID, playerDataNet);
    }
    
    public PlayerDataNet GetPlayerDataNet(int playerID) {
        if (!playerNetDictionary.ContainsKey(playerID)) {
            Debug.LogError(" 此 playerID 没有对应的 Player 脚本！");
            return null;
        }
        return playerNetDictionary[playerID];
    }

    public List<PlayerDataNet> GetPlayerDataNetList() {
        List<PlayerDataNet> playerDatas = new List<PlayerDataNet>();
        foreach (var playerDataDiction in playerNetDictionary) {
            playerDatas.Add(playerDataDiction.Value);
        }
        return playerDatas;
    }

    public List<PlayerDataNet> GetPlayerDataList() {
        List<PlayerDataNet> playerDatas = new List<PlayerDataNet>();
        foreach (var playerDataDiction in playerNetDictionary) {
            playerDatas.Add(playerDataDiction.Value);
        }
        return playerDatas;
    }

    public RifleWeapon GetRifleWeapon(int id) {
        if (!rifleWeaponDataDictionary.ContainsKey(id)) {
            Debug.LogError(" 此 id 没有对应的 rifleWEAPON 脚本！");
            return null;
        } else {
            return rifleWeaponDataDictionary[id];
        }
    }
}