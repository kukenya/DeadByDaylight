using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Axe : MonoBehaviour
{
    Rigidbody smallAxeRigidbody;        // �Ѽյ��� Rigidbody ������Ʈ
    public GameObject survivor;         // ������
    bool canHit = false;                // ���� �� �ֳ� ����

    private void Start()
    {

        survivor = GameObject.Find("Survivor");

        // ������ٵ��� chargingForce �� ������.
        Rigidbody smallAxeRigidbody = GetComponent<Rigidbody>();
        smallAxeRigidbody.AddForce(transform.forward * AnnaMove.instance.chargingForce, ForceMode.Impulse);
        // StartCoroutine("FlyingSound");
    }

    private void Update()
    {
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
            survivor.GetComponent<SurviverHealth>().NormalHit();
            canHit = true;
        }
        else if (other.gameObject.name.Contains("Pallet"))
        {
            SoundManager.instance.PlayHitSounds(0);
        }
    }
}
