using System;
using System.Collections;
using UnityEngine;

public class BeAttackEffect : MonoBehaviour
{
    
    private void OnEnable() {
        StartCoroutine(ReturnEffect());
    }

    private void OnDisable() {
        StopCoroutine(ReturnEffect());
    }

    IEnumerator ReturnEffect() {
        yield return new WaitForSeconds(1f);
        PlayerController_Client.Instance.ReturnBeAttackEffect(this);
    }
}
