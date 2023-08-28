using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;

public class BigAxe : MonoBehaviourPun
{
    public GameObject anna;                     // Anna
    public BoxCollider box;                     // ���� �ݶ��̴�
    public GameObject bloodEffectFactory;       // �� ����Ʈ ����
    public Transform rayStart;

    float currentTime;                          // ����ð�

    bool hit = false;                           // hit �߳�?

    GameObject goBloodImage;                    // ���ڱ� �׿���
    Image bloodImage;                           // ���ڱ� �̹���
    Color color;                                // �÷� <- ���İ�



    private void Start()
    {
        if (photonView.IsMine == true)
        {
            box = anna.GetComponent<AnnaMove>().bigAxeCollider; // ���� �ݶ��̴�

            goBloodImage = GameObject.Find("Blood");            // �� �̹���
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
            hit = true;                                                     // 1�� �Ŀ� �ڷ�ƾ �Լ��� ȣ���Ѵ�.

            other.GetComponent<SurviverHealth>().NormalHit();               // �������� NormalHit �Լ��� ȣ���Ѵ�.

            box.enabled = false;                                            // ���� �ݶ��̴��� ����. 

            color.a = 1;                                                    // ȭ�鿡 �� Ƣ�� UI ���İ��� 1�� �����.       
            bloodImage.GetComponent<Image>().color = color;

            SoundManager.instance.PlayHitSounds(4);                         // ������ �´� �Ҹ��� ����Ѵ�.

           // photonView.RPC(nameof(MakeEffect), RpcTarget.All);              // �� Ƣ�� ����Ʈ�� ������.
        }
    }

    [PunRPC]
    public void MakeEffect()
    {
        GameObject bloodEffect = Instantiate(bloodEffectFactory);           // ���� ����
        bloodEffect.transform.position = rayStart.position;                 // ���� ������ ��ġ
        bloodEffect.transform.forward = rayStart.forward;                   // ���� �� ����


        //GameObject bloodEffect = Instantiate(bloodEffectFactory);     // �� ����Ʈ ���忡�� �� ����Ʈ�� �����.
        //bloodEffect.transform.position = this.transform.position;     // �� ��ġ�� �����ϰ� �÷����Ѵ�.
        //bloodEffect.transform.position.Normalize();                   // ������ �븻����
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
