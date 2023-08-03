using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Axe : MonoBehaviour
{
    Rigidbody axeRigidbody;
    public float power = 5;
    // 태어날 때 Rigidbody 에 힘을 받아 앞방향으로 날아가고 싶다.
    private void Start()
    {
        axeRigidbody = GetComponent<Rigidbody>();
        axeRigidbody.AddForce(Vector3.forward * power, ForceMode.Impulse);
    }


    // 토구가 던진 도끼가 플레이어에게 닿으면 플레이어의 체력을 감소시킨다.
    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.name.Contains("Player"))
        {
            // HP--;
        }
    }
}
