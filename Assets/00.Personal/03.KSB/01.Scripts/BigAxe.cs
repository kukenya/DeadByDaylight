using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BigAxe : MonoBehaviour
{

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.name == "Surviver")
        {
            print(other.gameObject.name);

            // �ǰ��� �����ڸ� ���� ��
            // ������ �λ� ����
            // ������ UI �ٲٱ�

            // �λ� ���� �����ڸ� ���� ��
            // ������ ���� ����
            // ������ UI �ٲٱ�


            //����
            // ȭ�鿡 �� Ƣ���
            // ������ �´� �Ҹ� ����
            SoundManager.instance.PlayHitSounds(4);

            AnnaMove.instance.OffAxe();
        }



        
        // ��ġ�� �̽� �ִϸ��̼� �����ؾ� �Ǵµ� �𸣰ڴ�...
    }
}
