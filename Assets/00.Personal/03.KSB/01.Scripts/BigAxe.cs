using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;

public class BigAxe : MonoBehaviourPun
{
    public GameObject anna;                     // Anna
    public BoxCollider box;                     // 도끼 콜라이더
    public GameObject bloodEffectFactory;       // 피 이펙트 공장
    public Transform rayStart;

    float currentTime;                          // 현재시간

    bool hit = false;                           // hit 했나?

    GameObject goBloodImage;                    // 핏자국 겜옵젝
    Image bloodImage;                           // 핏자국 이미지
    Color color;                                // 컬러 <- 알파값



    private void Start()
    {
        if (photonView.IsMine == true)
        {
            box = anna.GetComponent<AnnaMove>().bigAxeCollider; // 도끼 콜라이더

            goBloodImage = GameObject.Find("Blood");            // 피 이미지
            bloodImage = goBloodImage.GetComponent<Image>();
            color = bloodImage.GetComponent<Image>().color;
            color.a = 0;
            bloodImage.GetComponent<Image>().color = color;
        }
    }

    private void Update()
    {
        //Ray ray = new Ray(rayStart.position, rayStart.forward);

        //RaycastHit hitinfo;

        //if (Physics.Raycast(ray, out hitinfo, 1))
        //{
        //    if (hitinfo.collider.gameObject.name.Contains("Hook"))
        //    {
        //        //GameObject bloodEffect = Instantiate(bloodEffectFactory);
        //        //bloodEffect.transform.position = hitinfo.point;
        //        //bloodEffect.transform.forward = hitinfo.normal;
        //    }
        //}

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

        if (photonView.IsMine && other.gameObject.name.Contains("Survivor"))
        {
            hit = true;                                                     // 1초 후에 코루틴 함수를 호출한다.

            other.GetComponent<SurviverHealth>().NormalHit();               // 생존자의 NormalHit 함수를 호출한다.

            box.enabled = false;                                            // 도끼 콜라이더를 끈다. 

            color.a = 1;                                                    // 화면에 피 튀는 UI 알파값을 1로 만든다.       
            bloodImage.GetComponent<Image>().color = color;

            SoundManager.instance.PlayHitSounds(4);                         // 도끼에 맞는 소리를 재생한다.

           // photonView.RPC(nameof(MakeEffect), RpcTarget.All);              // 피 튀는 이펙트를 보낸다.
        }
    }

    [PunRPC]
    public void MakeEffect()
    {
        GameObject bloodEffect = Instantiate(bloodEffectFactory);           // 도끼 생성
        bloodEffect.transform.position = rayStart.position;                 // 도끼 던지는 위치
        bloodEffect.transform.forward = rayStart.forward;                   // 도끼 앞 방향


        //GameObject bloodEffect = Instantiate(bloodEffectFactory);     // 피 이펙트 공장에서 피 이펙트를 만든다.
        //bloodEffect.transform.position = this.transform.position;     // 내 위치에 생성하고 플레이한다.
        //bloodEffect.transform.position.Normalize();                   // 방향은 노말벡터
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
