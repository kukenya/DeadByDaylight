using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Axe : MonoBehaviour
{
    Rigidbody axeRigidbody;
    public float power = 5;
    // �¾ �� Rigidbody �� ���� �޾� �չ������� ���ư��� �ʹ�.
    private void Start()
    {
        axeRigidbody = GetComponent<Rigidbody>();
        axeRigidbody.AddForce(Vector3.forward * power, ForceMode.Impulse);
    }


    // �䱸�� ���� ������ �÷��̾�� ������ �÷��̾��� ü���� ���ҽ�Ų��.
    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.name.Contains("Player"))
        {
            // HP--;
        }
    }
}
