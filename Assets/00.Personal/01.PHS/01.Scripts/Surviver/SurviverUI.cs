using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class SurviverUI : MonoBehaviour
{
    public static SurviverUI instance;

    private void Awake()
    {
        instance = this;
    }

    public enum PrograssUI
    {
        None,
        Focus,
        On
    }

    public PrograssUI prograssUI = PrograssUI.None;

    public float uiAlpha = 0.3f;

    [Header("진행바")]
    public Image prograssBG;
    public Image prograssBar;
    public TextMeshProUGUI prograssText;
    public Image handImage;
    public Sprite[] bars;

    [Header("진행바 아래 튜토리얼 UI")]
    public Image leftMouseClick;
    public TextMeshProUGUI mouseClickText;

    [Header("스페이스바 튜토리얼 UI")]
    public Image spaceBar;
    public TextMeshProUGUI spaceText;


    public void ChangePrograssUI(PrograssUI prograssUI, string text = null)
    {
        this.prograssUI = prograssUI;
        OffFocusProgressUI();
        switch (this.prograssUI)
        {
            case PrograssUI.None:
                break;
            case PrograssUI.Focus:
                FocusProgressUI(text);
                break;
            case PrograssUI.On:
                OnProgressUI(text);
                break;
        }
    }

    public void ChangePrograssBarSprite(int idx)
    {
        idx--;
        idx = Mathf.Clamp(idx, 0, 2);
        prograssBar.sprite = bars[idx];
    }

    public void FocusProgressUI(string text)
    {
        prograssText.text = text;
        mouseClickText.text = text;

        prograssBG.gameObject.SetActive(true);
        leftMouseClick.gameObject.SetActive(true);

        Color[] colors = new Color[3];

        colors[0] = prograssBar.color;
        colors[1] = prograssText.color;
        colors[2] = prograssBG.color;


        for(int i = 0; i < colors.Length; i++)
        {
            colors[i].a = uiAlpha;
        }

        prograssBar.color = colors[0];
        prograssText.color = colors[1];
        prograssBG.color = colors[2];
    }

    public void OnProgressUI(string text)
    {
        handImage.gameObject.SetActive(true);
        prograssBG.gameObject.SetActive(true);
        prograssText.text = text;

        Color[] colors = new Color[3];

        colors[0] = prograssBar.color;
        colors[1] = prograssText.color;
        colors[2] = prograssBG.color;


        for (int i = 0; i < colors.Length; i++)
        {
            colors[i].a = 1;
        }

        prograssBar.color = colors[0];
        prograssText.color = colors[1];
        prograssBG.color = colors[2];
    }

    public void OffFocusProgressUI()
    {
        handImage.gameObject.SetActive(false);
        prograssBG.gameObject.SetActive(false);
        leftMouseClick.gameObject.SetActive(false);
    }

    public void FocusSpaceBarUI()
    {
        spaceBar.gameObject.SetActive(true);
        spaceText.gameObject.SetActive(true);
    }

    public void UnFocusSpaceBarUI()
    {
        spaceBar.gameObject.SetActive(false);
        spaceText.gameObject.SetActive(false);
    }
}
