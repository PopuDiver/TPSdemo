using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class DrawCrossHair : MonoBehaviour {
    public static DrawCrossHair instance;

    [Header("最大距离")]
    public float maxDistance;

    private Vector2 maxSize;

    [Header("最小距离")]
    public float minDistance;

    private Vector2 minSize;

    [Header("缩小的速度")]
    public float narrowSpeed;

    private RectTransform rectTrans;
    private bool isNarrowRot = true;

    private void Awake() {
        if (instance == null)
            instance = this;
        rectTrans = GetComponent<RectTransform>();
        minSize = new Vector2(minDistance, minDistance);
        maxSize = new Vector2(maxDistance, maxDistance);
        StartCoroutine(Narrow());
        
    }

    public void Expand(float distancePlus) {
        if (minDistance + distancePlus < maxDistance)
            rectTrans.sizeDelta += new Vector2(distancePlus, distancePlus);
        else
            rectTrans.sizeDelta = new Vector2(maxDistance, maxDistance);
    }

    public void Rot() {
        rectTrans.GetChild(0).GetComponent<Image>().color = Color.red;
        rectTrans.GetChild(1).GetComponent<Image>().color = Color.red;
        rectTrans.GetChild(2).GetComponent<Image>().color = Color.red;
        rectTrans.GetChild(3).GetComponent<Image>().color = Color.red;
        if (isNarrowRot) {
            rectTrans.rotation = Quaternion.Euler(0, 0, 45);
            isNarrowRot = false;
            StopCoroutine(NarrowRot());
            StartCoroutine(NarrowRot());
        }
    }

    IEnumerator NarrowRot() {
        yield return new WaitForSeconds(0.5f);
        rectTrans.GetChild(0).GetComponent<Image>().color = Color.white;
        rectTrans.GetChild(1).GetComponent<Image>().color = Color.white;
        rectTrans.GetChild(2).GetComponent<Image>().color = Color.white;
        rectTrans.GetChild(3).GetComponent<Image>().color = Color.white;
        rectTrans.rotation = Quaternion.Euler(0, 0, 0);
        isNarrowRot = true;
    }

    IEnumerator Narrow() {
        while (true) {
            rectTrans.sizeDelta = Vector2.Lerp(rectTrans.sizeDelta, minSize, narrowSpeed * Time.deltaTime);
            yield return null;
        }
    }

    public void Hide() {
        rectTrans.anchoredPosition = new Vector2(1000, 0);
    }

    public void Show() {
        rectTrans.anchoredPosition = Vector2.zero;
    }
}