using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EndingLine : MonoBehaviour
{
    public Image upperPrograssImage;
    public Image lowerPrograssImage;

    public Image prograssPoint;


    float currentTime = 0;
    public float maxTime = 2;
    public float decreaseTime;
    public Ease deceaseEase;
    public Ease upperDecreaseEase = Ease.Linear;

    private void Awake()
    {
        foreach(Transform child in transform)
        {
            child.gameObject.SetActive(false);
        }
        this.enabled = false;
    }

    private void OnEnable()
    {
        foreach (Transform child in transform)
        {
            child.gameObject.SetActive(true);
        }
        lowerPrograssImage.fillAmount = 1;
        upperPrograssImage.fillAmount = 1;
        upperPrograssImage.DOFillAmount(0, maxTime).SetEase(upperDecreaseEase);
    }

    private void OnDisable()
    {
        foreach (Transform child in transform)
        {
            child.gameObject.SetActive(false);
        }
    }

    void Update()
    {
        if(currentTime >= maxTime / 4)
        {
            currentTime = 0;
            float targetAmount = lowerPrograssImage.fillAmount -= 0.25f;
            if (targetAmount == 0) lowerPrograssImage.DOFillAmount(targetAmount, decreaseTime).SetEase(deceaseEase).OnComplete(OffEndingLine);
            else lowerPrograssImage.DOFillAmount(targetAmount, decreaseTime).SetEase(deceaseEase);
        }
        currentTime += Time.deltaTime;
    }

    void OffEndingLine()
    {
        print("³¡");
        this.enabled = false;
    }
}
