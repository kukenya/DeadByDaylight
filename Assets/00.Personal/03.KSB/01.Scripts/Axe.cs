using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Axe : MonoBehaviour
{
    Rigidbody smallAxeRigidbody;      // �Ѽյ��� Rigidbody ������Ʈ
    private void Start()
    {
        // ������ٵ��� chargingForce �� ������.
        Rigidbody smallAxeRigidbody = GetComponent<Rigidbody>();
        smallAxeRigidbody.AddForce(transform.forward * AnnaMove.instance.chargingForce, ForceMode.Impulse);
    }

    private void Update()
    {
        // ���ư� �� X������ ȸ���Ѵ�.
        transform.Rotate(new Vector3(600 * Time.deltaTime, 0, 0));
    }

    // �䱸�� ���� ������ �÷��̾�� ������ �÷��̾��� ü���� ���ҽ�Ų��.
    private void OnCollisionEnter(Collision collision)
    {
        Destroy(gameObject);
        if (collision.gameObject.name.Contains("Player"))
        {
            print("dlfkahjfksjdahflk");
        }
    }
}
