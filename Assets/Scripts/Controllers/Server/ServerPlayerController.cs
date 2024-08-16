using UnityEngine;

public class ServerPlayerController {
    private PlayerDataNet playerDataNet;

    public ServerPlayerController(PlayerDataNet playerDataNet) {
        this.playerDataNet = playerDataNet;
    }
    
    public void TakeDamage(int weaponId, int beAttackId, int attackPlayerID) {
        PlayerDataNet playerDataNet = DataCentreController.GetInstance().GetPlayerDataNet(beAttackId);
        if (playerDataNet == null) {
            Debug.LogError(" ServerPlayerController.instance.TakeDamege 中 playerData == null ");
            return;
        }
        
        if (playerDataNet.GetIsPlayerDead()) {
            Debug.LogError("玩家血量为 0 ，已死亡");
            return;
        }

        RifleWeapon rifleWeapon = DataCentreController.GetInstance().GetRifleWeapon(weaponId);
        playerDataNet.ChangeHealth(-rifleWeapon.Damage); 
        if (playerDataNet.GetIsPlayerDead()) {
            playerDataNet.KillListAdd(attackPlayerID);
            playerDataNet.RpcPlayerDead(true);
            PlayerDataNet attackPlayerDataNet = DataCentreController.GetInstance().GetPlayerDataNet(attackPlayerID);
            attackPlayerDataNet.KillListAdd(beAttackId);
            if (attackPlayerDataNet.KillList.Count >= 3) {
                attackPlayerDataNet.RpcGameOver(attackPlayerID);
            }
        }
    }
    
}
