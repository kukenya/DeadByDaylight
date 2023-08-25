using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UIManager : MonoBehaviour
{
    public static UIManager instance;
    private void Awake()
    {
        if(instance = null)
        {
            Destroy(gameObject);
        }
        else
        {
            // ����.
            instance = this;
            DontDestroyOnLoad(gameObject);

            // UI ��Ȱ��ȭ
            murdererUI.enabled = false;

            // �ܵ� UI ��Ȱ��ȭ
            throwUI.SetActive(false);
            soloSpace.SetActive(false);
            soloText.enabled = false;

            // ���� UI ��Ȱ��ȭ
            toGetherThrowUI.SetActive(false);
            togetherSpace.SetActive(false);
            togetherText.enabled = false;

            // ������ UI ��Ȱ��ȭ
            gageSlider.enabled = false;
            sliderBG.enabled = false;
            gageText.enabled = false;
            gageImage.enabled = false;
        }
    }

    #region UI ����
    // �⺻ UI
    public Canvas murdererUI;               // UI        
    // ī��Ʈ UI
    public TextMeshProUGUI axeCount;        // ���� ���� UI
    public TextMeshProUGUI genCount;        // ������ ���� UI

    // �ܵ� ��ȣ�ۿ� UI
    public GameObject throwUI;              // ���� ������ UI
    public GameObject soloSpace;            // �����̽��� �̹���
    public TextMeshProUGUI soloText;        // ��ȣ�ۿ� ����

    // ���� ��ȣ�ۿ� UI
    public GameObject toGetherThrowUI;      // ���� ������ UI
    public GameObject togetherSpace;        // �����̽��� �̹���
    public TextMeshProUGUI togetherText;    // ��ȣ�ۿ� ����

    // ������ UI
    public Slider gageSlider;               // �����̴�
    public Image sliderBG;                  // ������ �����̴� ���
    public TextMeshProUGUI gageText;        // ������ �ؽ�Ʈ
    public Image gageImage;                 // ������ �̹���
    #endregion

    public void ThrowUI(bool axe)
    {
        throwUI.SetActive(axe);
    }

    public void SoloUI(bool interaction, string str)
    {
        soloSpace.SetActive(interaction);
        soloText.enabled = interaction;
        soloText.text = str;
    }

    public void DuoUI(bool boolean, string str)
    {
        toGetherThrowUI.SetActive(boolean);

        togetherSpace.SetActive(boolean);
        togetherText.enabled = boolean;
        togetherText.text = str;
    }

    public void GageUI(bool uiBool, bool imageBool, string str)
    {
        gageSlider.enabled = uiBool;
        sliderBG.enabled = uiBool;
        gageText.enabled = uiBool;
        gageImage.enabled = imageBool;
        gageText.text = str;
    }

    public void FillGage(Time time)
    {
        // Mathf.Lerp();
    }

    public void EmptyGage()
    {
        gageSlider.value = 0;
    }

    public void OffAllUI()
    {
        throwUI.SetActive(true);
        soloSpace.SetActive(false);
        soloText.enabled = false;
        gageSlider.enabled = false;
        sliderBG.enabled = false;
        gageText.enabled = false;
        gageImage.enabled = false;
    }
}
