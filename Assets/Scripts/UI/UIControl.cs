using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIControl : MonoBehaviour {
    private static UIControl instance;

    private Text textAmmoLeft;
    private Text textAmmoTotal;
    [SerializeField] private Slider healthSlider;
    [SerializeField] private Text ruleText;
    [SerializeField] public Image ruleImage;
    [SerializeField] private Text overText;
    
    private float typingSpeed = 0.05f;
    private string fullText;
    private string currentText = "";
    private float fadeDuration = 3.0f;
    private bool isStartFade;
    private Image overImage;

    private void Awake() {
        if (null == instance) {
            instance = this;
        }
        textAmmoLeft = transform.Find("PanelAmmo/TextAmmoLeft").GetComponent<Text>();
        textAmmoTotal = transform.Find("PanelAmmo/TextAmmoTotal").GetComponent<Text>();
        EventControl.Instance.Register<int, int>(EventType.PlayerAttack, UpdateAmmoDisplay);
        EventControl.Instance.Register<float>(EventType.PlayerHealthChange, HealthChange);
        EventControl.Instance.Register<string>(EventType.GameOverPlayerUI, ShowOverImage);
    }
    
    private void Start() {
        fullText = ruleText.text;
        ruleText.text = "";
        StartCoroutine(ShowRuleText());
    }

    private void Update() {
        if (isStartFade) {
            Color originalColor = ruleImage.color;

            if (fadeDuration > 0) {
                fadeDuration -= Time.deltaTime;
                float alpha = Mathf.Lerp(0f, 1f, fadeDuration);
                ruleImage.color = new Color(originalColor.r, originalColor.g, originalColor.b, alpha);
                ruleText.color = new Color(ruleText.color.r, ruleText.color.g, ruleText.color.b, alpha);
            } else {
                // 确保最终完全透明
                ruleImage.color = new Color(originalColor.r, originalColor.g, originalColor.b, 0f);
                ruleText.color = ruleImage.color;
                isStartFade = false;
            }
        }
    }

    public static UIControl GetInstance() {
        return instance;
    }

    private void HealthChange(float health) {
        healthSlider.value = health;
    }
    
    /// <summary>
    /// 更新弹药数量显示
    /// </summary>
    /// <param name="ammoleft"></param>
    /// <param name="ammoTotal"></param>
    public void UpdateAmmoDisplay(int ammoleft, int ammoTotal) {
        textAmmoLeft.text = ammoleft.ToString();
        textAmmoTotal.text = ammoTotal.ToString();
    }

    IEnumerator ShowRuleText() {
        for (int i = 0; i < fullText.Length; i++) {
            currentText += fullText[i];
            ruleText.text = currentText;
            yield return new WaitForSeconds(typingSpeed); // 控制每个字符出现的间隔时间
        }

        yield return new WaitForSeconds(1);
        isStartFade = true;
    }

    public void ShowOverImage(string str) {
        overText.text = str;
        overImage.gameObject.SetActive(true);
    }
}