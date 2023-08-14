using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BigAxe : MonoBehaviour
{
    public GameObject survivor;
    public Image bloodImage;

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.name == "Surviver")
        {
            print(other.gameObject.name);

            survivor.GetComponent<SurviverHealth>().NormalHit();

            //����
            // ȭ�鿡 �� Ƣ���
            Color color = bloodImage.GetComponent<Image>().color;
            color.a = 1;
            // ������ �´� �Ҹ� ����
            SoundManager.instance.PlayHitSounds(4);

            AnnaMove.instance.OffAxe();
            // StartCoroutine("FadeOut");
        }

        // ��ġ�� �̽� �ִϸ��̼� �����ؾ� �Ǵµ� �𸣰ڴ�...
    }
}
