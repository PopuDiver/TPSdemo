using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BulletMove : MonoBehaviour {
    private Vector3 m_Velocity;
    public float gravity;
    public float lifeTime = 5f;
    private float timeAlive;
    private int weaponID;
    private int attackPlayerID;

    private void Start() {
        timeAlive = 0f;
    }

    public void SetBulletData(Vector3 vector3, int weaponId, int attackPlayerId) {
        RifleWeapon rifleWeapon = DataCentreController.Instance().GetRifleWeapon(weaponId);
        m_Velocity = vector3 * (float)rifleWeapon.BulletFlySpeed;
        gravity = (float)rifleWeapon.BulletGravity;
        weaponID = weaponId;
        attackPlayerID = attackPlayerId;
    }

    private void Update() {
        timeAlive += Time.deltaTime;
        if (timeAlive > lifeTime) {
            PlayerController_Client.Instance.ReturnBullet(this);
            return;
        }

        m_Velocity.y += gravity * Time.deltaTime;
        // 计算子弹的新位置
        Vector3 newPosition = transform.position + m_Velocity * Time.deltaTime;

        // 射线检测以检测碰撞
        Ray ray = new Ray(transform.position, m_Velocity.normalized);
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit, m_Velocity.magnitude * Time.deltaTime)) {
            if (attackPlayerID != -1)
                // 这里可以处理伤害等逻辑
                if (hit.collider.CompareTag("Player")) {
                    int playerID = PlayerController_Client.Instance.GameObjToGetPlayerID(hit.collider.gameObject);
                    Debug.LogError(" ***************************** playerID " + playerID);
                    if(PlayerController_Client.Instance.GetPlayerID() == attackPlayerID)
                        EventControl.Instance.Invoke(EventType.PlayerAttackHit);
                    PlayerController_Client.Instance.ClientSpawnEffect(PlayerController_Client.Instance.GetPlayerID(), hit.collider.gameObject.transform.position + new Vector3(0, 1, 0));
                    PlayerDataNet playerDataNet = DataCentreController.Instance().GetPlayerDataNet(attackPlayerID);
                    playerDataNet.TakeDamageClient(weaponID, playerID, attackPlayerID);
                }
            // 销毁子弹
            PlayerController_Client.Instance.ReturnBullet(this);
        } else {
            // 如果没有碰撞，则更新子弹的位置
            transform.position = newPosition;
        }
    }
}