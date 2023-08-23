using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Axe : MonoBehaviour
{
    Rigidbody smallAxeRigidbody;        // �Ѽյ��� Rigidbody ������Ʈ
    GameObject survivor;                // ������
    bool canHit = false;                // ���� �� �ֳ� ����
    bool isDestroy;
    private void Start()
    {
        
    }

    public void flying(float chargingForce)
    {
        Rigidbody smallAxeRigidbody = GetComponent<Rigidbody>();
        smallAxeRigidbody.AddForce(transform.forward * chargingForce, ForceMode.Impulse);

        StartCoroutine("FlyingSound");
    }

    private void Update()
    {
        // ���ư� �� X������ ȸ���Ѵ�.
        transform.Rotate(new Vector3(600 * Time.deltaTime, 0, 0));
    }

    IEnumerator FlyingSound()
    {
        while(isDestroy == false)
        {
            SoundManager.instance.PlaySmallAxeFlyingSounds(4);
            yield return new WaitForSeconds(0.27f);
            if (isDestroy == true)
            {
                break;
            }

            yield return null;
        }
       
        
    }

    // �䱸�� ���� ������ �÷��̾�� ������ �÷��̾��� ü���� ���ҽ�Ų��.
    private void OnTriggerEnter(Collider other)
    {
        Destroy(gameObject);

        isDestroy = true;

        if (other.gameObject.name.Contains("Survivor") & canHit == false)
        {
            SoundManager.instance.PlayHitSounds(3);
            other.GetComponent<SurviverHealth>().NormalHit();
            canHit = true;
        }
        else if(other.gameObject.name.Contains("����") & canHit == false)
        {
            SoundManager.instance.PlayHitSounds(1);
        }
        else
        {
            // �� �ϴ� �Ҹ�
        }
    }
}
