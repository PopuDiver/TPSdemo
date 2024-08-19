using System;
using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;
using Quaternion = UnityEngine.Quaternion;
using Random = UnityEngine.Random;
using Vector3 = UnityEngine.Vector3;

public class CustomNetworkManager : NetworkManager {
    public static CustomNetworkManager instance;
    public GameObject playerDataNetPrefab;
    public GameObject controllerCenterPrefab;
    public List<Transform> playerStartPointList;
    public GameObject playerObj;

    private void Awake() {
        base.Awake();
        if (instance == null) {
            instance = this;
            DontDestroyOnLoad(instance);
        } else {
            Destroy(gameObject);
        }
    }

    public GameObject GetNetworkManagerPrefab(string name) {
        foreach (GameObject prefab in instance.spawnPrefabs) {
            if (prefab.name == name) {
                return prefab;
            }
        }
        return null;
    }

    public override void OnStartServer() {
        base.OnStartServer();

        controllerCenterPrefab = GetNetworkManagerPrefab("ControllerCenter");
        GameObject controllerCenter = Instantiate(controllerCenterPrefab, Vector3.zero, Quaternion.identity);
        controllerCenter.name = "ControllerCenter";
        
        // 临时代码，将这个数据中心对象作为网络对象，连接到服务端
        NetworkConnectionToClient conn = new NetworkConnectionToClient(100);

        NetworkServer.AddPlayerForConnection(conn, controllerCenter);
        NetworkServer.Spawn(controllerCenter);
    }

    /// <summary>
    /// 玩家加入服务器时调用，跑在服务器上
    /// </summary>
    /// <param name="conn"></param>
    public override void OnServerAddPlayer(NetworkConnectionToClient conn) {
        playerDataNetPrefab = GetNetworkManagerPrefab("PlayerDataNet");
        GameObject playerDataNetGameObject = Instantiate(playerDataNetPrefab, Vector3.zero, Quaternion.identity);
        playerDataNetGameObject.name = "PlayerDataNet1";
        NetworkServer.AddPlayerForConnection(conn, playerDataNetGameObject);
        PlayerDataNet playerDataNet = playerDataNetGameObject.GetComponent<PlayerDataNet>();
        NetworkServer.Spawn(playerDataNetGameObject);
        
        if (playerDataNet == null) {
            Debug.LogError(" 玩家的 PlayerData 类为空 ");
        } else {
            DataCentreController.Instance().PlayerAddData(playerDataNet);
        }
    }

    public override void OnClientConnect() {
        base.OnClientConnect();

        if (!clientLoadedScene) {
            if (!NetworkClient.ready) {
                NetworkClient.Ready();
            }

            if (autoCreatePlayer) {
                if (playerPrefab == null) {
                    Debug.LogError(" playerPrefab == null ");
                }

                if (playerObj != null) {
                    int random = Random.Range(0, playerStartPointList.Count);
                    playerObj.transform.position = playerStartPointList[random].position;
                } else {
                    int random = Random.Range(0, playerStartPointList.Count);
                    playerObj = Instantiate(playerPrefab, playerStartPointList[random].position, Quaternion.identity);
                }
            }
        }
    }
}