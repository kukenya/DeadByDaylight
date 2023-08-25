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
            // 나다.
            instance = this;
            DontDestroyOnLoad(gameObject);

            // UI 비활성화
            murdererUI.enabled = false;

            // 단독 UI 비활성화
            throwUI.SetActive(false);
            soloSpace.SetActive(false);
            soloText.enabled = false;

            // 같이 UI 비활성화
            toGetherThrowUI.SetActive(false);
            togetherSpace.SetActive(false);
            togetherText.enabled = false;

            // 게이지 UI 비활성화
            gageSlider.enabled = false;
            sliderBG.enabled = false;
            gageText.enabled = false;
            gageImage.enabled = false;
        }
    }

    #region UI 변수
    // 기본 UI
    public Canvas murdererUI;               // UI        
    // 카운트 UI
    public TextMeshProUGUI axeCount;        // 도끼 갯수 UI
    public TextMeshProUGUI genCount;        // 발전기 갯수 UI

    // 단독 상호작용 UI
    public GameObject throwUI;              // 도끼 던지기 UI
    public GameObject soloSpace;            // 스페이스바 이미지
    public TextMeshProUGUI soloText;        // 상호작용 내용

    // 같이 상호작용 UI
    public GameObject toGetherThrowUI;      // 도끼 던지기 UI
    public GameObject togetherSpace;        // 스페이스바 이미지
    public TextMeshProUGUI togetherText;    // 상호작용 내용

    // 게이지 UI
    public Slider gageSlider;               // 슬라이더
    public Image sliderBG;                  // 게이지 슬라이더 배경
    public TextMeshProUGUI gageText;        // 게이지 텍스트
    public Image gageImage;                 // 게이지 이미지
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
