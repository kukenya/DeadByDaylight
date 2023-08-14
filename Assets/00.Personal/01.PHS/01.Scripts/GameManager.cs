using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    public TextMeshProUGUI titleText;
    public TextMeshProUGUI subText;
    public Image lineImage;

    private void Awake()
    {
        if(Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(this.gameObject);
        }
        else
        {
            Destroy(this.gameObject);
        }
    }

    public float textFadeTime;
    public float textFadeOffset;
    // Start is called before the first frame update
    IEnumerator Start()
    {
        OffCursor();
        yield return new WaitForSeconds(textFadeOffset);
        titleText.DOFade(0, textFadeTime);
        subText.DOFade(0, textFadeTime);
        lineImage.DOFade(0, textFadeTime);
    }

    // Update is called once per frame
    void Update()
    {
        
    }


    void OnCursor()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    void OffCursor()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }
}
