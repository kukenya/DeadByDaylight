using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Axe : MonoBehaviour
{
    Rigidbody smallAxeRigidbody;        // �Ѽյ��� Rigidbody ������Ʈ
    GameObject survivor;         // ������
    bool canHit = false;                // ���� �� �ֳ� ����

    private void Start()
    {
        // ������ٵ��� chargingForce �� ������.
        
    }

    public void flying(float chargingForce)
    {
        Rigidbody smallAxeRigidbody = GetComponent<Rigidbody>();
        smallAxeRigidbody.AddForce(transform.forward * chargingForce, ForceMode.Impulse);

        // print(AnnaMove.instance.chargingForce);
        // StartCoroutine("FlyingSound");
    }

    private void Update()
    {
        //transform.position += transform.forward * AnnaMove.instance.chargingForce * Time.deltaTime;

        // ���ư� �� X������ ȸ���Ѵ�.
        transform.Rotate(new Vector3(600 * Time.deltaTime, 0, 0));
    }

    IEnumerator FlyingSound()
    {
        yield return null;
        SoundManager.instance.PlaySmallAxeSounds(3);
    }

    // �䱸�� ���� ������ �÷��̾�� ������ �÷��̾��� ü���� ���ҽ�Ų��.
    private void OnTriggerEnter(Collider other)
    {
        Destroy(gameObject);

        if (other.gameObject.name.Contains("Survivor") & canHit == false)
        {
            SoundManager.instance.PlayHitSounds(3);
            other.GetComponent<SurviverHealth>().NormalHit();
            canHit = true;
        }
        else if (other.gameObject.name.Contains("Pallet"))
        {
            SoundManager.instance.PlayHitSounds(0);
        }
    }
}
