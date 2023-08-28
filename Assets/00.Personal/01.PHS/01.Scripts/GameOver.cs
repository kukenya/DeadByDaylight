using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameOver : MonoBehaviour
{
    public Image fadeImage;

    private void OnTriggerEnter(Collider other)
    {
        if(other.CompareTag("Survivor"))
        {
            fadeImage.DOFade(0, 2f).OnComplete(() => { Time.timeScale = 0; });
        }
    }
}
