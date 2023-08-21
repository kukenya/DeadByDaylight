using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;

public class BigAxe : MonoBehaviourPun
{
    bool hit = false;               // hit �߳�?

    float currentTime;              // ����ð�

    GameObject goBloodImage;        // ���ڱ� �̹���
    Image bloodImage;
    Color color;                    // �÷� <- ���İ�

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
            hit = true;                                                 // 1�� �Ŀ� �ڷ�ƾ �Լ��� ȣ���Ѵ�.

            other.GetComponent<SurviverHealth>().NormalHit();           // �������� NormalHit �Լ��� ȣ���Ѵ�.

            GetComponent<AnnaMove>().OffAxe();                          // ���� �ݶ��̴��� ����.
              
            color.a = 1;                                                // ȭ�鿡 �� Ƣ��� UI ���İ��� 1�� �����.       
            bloodImage.GetComponent<Image>().color = color;         

            SoundManager.instance.PlayHitSounds(4);                     // ������ �´� �Ҹ��� ����Ѵ�.

            //GameObject bloodEffect = Instantiate(bloodEffectFactory); // �� ����Ʈ ���忡�� �� ����Ʈ�� �����.
            //bloodEffect.transform.position = this.transform.position; // �� ��ġ�� �����ϰ� �÷����Ѵ�.
            //bloodEffect.transform.position.Normalize();               // ������ �븻����
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
