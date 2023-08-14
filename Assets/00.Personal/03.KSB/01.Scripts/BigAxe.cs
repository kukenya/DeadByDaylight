using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BigAxe : MonoBehaviour
{
    public GameObject survivor;
    public Image blood;

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.name == "Surviver")
        {
            print(other.gameObject.name);

            survivor.GetComponent<SurviverHealth>().NormalHit();

            //����
            // ȭ�鿡 �� Ƣ���
            // blood.alp
            // ������ �´� �Ҹ� ����
            SoundManager.instance.PlayHitSounds(4);

            AnnaMove.instance.OffAxe();
        }



        
        // ��ġ�� �̽� �ִϸ��̼� �����ؾ� �Ǵµ� �𸣰ڴ�...
    }
}
