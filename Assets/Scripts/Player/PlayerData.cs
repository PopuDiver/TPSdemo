using System.Collections.Generic;
using UnityEngine;

namespace Player {
    public class PlayerData {
        private int playerID;
        public int currentHealth;
        public Vector3 localPosition;
        public Vector3 position;
        public Quaternion localRotation;
        public Quaternion rotation;
        public Vector3 localScale;
        public List<int> killList;
        public List<int> beKillList;
        public List<int> weaponList;
        public int currentWeaponID;

        public PlayerData() {
            
        }

        public PlayerData(int playerId, int currentHealth, Transform playerTransform, int currentWeaponID) {
            playerID = playerId;
            this.currentHealth = currentHealth;
            this.localPosition = playerTransform.localPosition;
            this.position = playerTransform.position;
            this.localRotation = playerTransform.localRotation;
            this.rotation = playerTransform.rotation;
            this.localScale = playerTransform.localScale;
            this.killList = new List<int>();
            this.beKillList = new List<int>();
            this.weaponList = new List<int>();
            this.currentWeaponID = currentWeaponID;
        }

        public void HealthChange(int newCurrentHealth) {
            currentHealth = newCurrentHealth;
        }
        
        public void TransformChange(Transform transform) {
            localPosition = transform.localPosition;
            position = transform.position;
            localRotation = transform.localRotation;
            rotation = transform.rotation;
            localScale = transform.localScale;
        }

        public void KillListChange(List<int> list)
        {
            killList.Clear();
            foreach (int i in list)
            {
                killList.Add(i);
            }
        }
        
        public void BeKillListChange(List<int> list)
        {
            beKillList.Clear();
            foreach (int i in list)
            {
                beKillList.Add(i);
            }
        }
        
        public void WeaponListChange(List<int> list)
        {
            weaponList.Clear();
            foreach (int i in list)
            {
                weaponList.Add(i);
            }
        }
        
        public void WeaponIdChange(int currentWeaponId)
        {
            currentWeaponID = currentWeaponId;
        }
    }
}