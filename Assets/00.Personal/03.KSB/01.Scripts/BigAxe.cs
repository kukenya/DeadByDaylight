using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BigAxe : MonoBehaviour
{
    public GameObject survivor;

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.name == "Surviver")
        {
            print(other.gameObject.name);

            survivor.GetComponent<SurviverHealth>().NormalHit();

            //// �ǰ��� �����ڸ� ���� ��
            //if(survivor.GetComponent<SurviverHealth>().state == SurviverHealth.HealthState.Healthy)
            //{
            //    // ������ �λ� ����
            //    survivor.GetComponent<SurviverHealth>().NormalHit();
            //    // ������ UI �ٲٱ�
            //}
            //// �λ� ���� �����ڸ� ���� ��
            //else if(survivor.GetComponent<SurviverHealth>().state == SurviverHealth.HealthState.Injured)
            //{
            //    // ������ ���� ����
            //    survivor.GetComponent<SurviverHealth>().NormalHit();
            //    // ������ UI �ٲٱ�
            //}




            //����
            // ȭ�鿡 �� Ƣ���

            // ������ �´� �Ҹ� ����
            SoundManager.instance.PlayHitSounds(4);

            AnnaMove.instance.OffAxe();
        }



        
        // ��ġ�� �̽� �ִϸ��̼� �����ؾ� �Ǵµ� �𸣰ڴ�...
    }
}
