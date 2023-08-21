using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;

public class BigAxe : MonoBehaviourPun
{
    bool hit = false;               // hit 했나?

    float currentTime;              // 현재시간

    GameObject goBloodImage;        // 핏자국 이미지
    Image bloodImage;
    Color color;                    // 컬러 <- 알파값

    // public GameObject bloodEffectFactory;


    private void Start()
    {
        if(photonView.IsMine== true)
        {
            goBloodImage = GameObject.Find("Blood");
            bloodImage = goBloodImage.GetComponent<Image>();
            color = bloodImage.GetComponent<Image>().color;
            color.a = 0;
            bloodImage.GetComponent<Image>().color = color;
        }

    }

    private void Update()
    {
        if (hit == true)
        {
            currentTime += Time.deltaTime;

            if (currentTime >= 1f)
            {
                StartCoroutine("FadeOut");
                currentTime = 0;
            }
        }
    }


    private void OnTriggerEnter(Collider other)
    {
        print(other.gameObject.name);
        if (other.gameObject.name.Contains("Survivor"))
        {
            hit = true;                                                 // 1초 후에 코루틴 함수를 호출한다.

            other.GetComponent<SurviverHealth>().NormalHit();           // 생존자의 NormalHit 함수를 호출한다.

            GetComponent<AnnaMove>().OffAxe();                          // 도끼 콜라이더를 끈다.
              
            color.a = 1;                                                // 화면에 피 튀기는 UI 알파값을 1로 만든다.       
            bloodImage.GetComponent<Image>().color = color;         

            SoundManager.instance.PlayHitSounds(4);                     // 도끼에 맞는 소리를 재생한다.

            //GameObject bloodEffect = Instantiate(bloodEffectFactory); // 피 이펙트 공장에서 피 이펙트를 만든다.
            //bloodEffect.transform.position = this.transform.position; // 내 위치에 생성하고 플레이한다.
            //bloodEffect.transform.position.Normalize();               // 방향은 노말벡터
        }
    }

    IEnumerator FadeOut()
    {
        while (color.a > 0)
        {
            color.a -= 0.005f;
            bloodImage.GetComponent<Image>().color = color;

            if (color.a <= 0)
            {
                hit = false;
                break;
            }
            yield return null;
        }

    }
}
