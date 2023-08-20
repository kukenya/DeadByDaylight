using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Axe : MonoBehaviour
{
    Rigidbody smallAxeRigidbody;        // 한손도끼 Rigidbody 컴포넌트
    GameObject survivor;         // 생존자
    bool canHit = false;                // 때릴 수 있나 여부

    private void Start()
    {
        // 리지드바디의 chargingForce 로 던진다.
        
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

        // 날아갈 때 X축으로 회전한다.
        transform.Rotate(new Vector3(600 * Time.deltaTime, 0, 0));
    }

    IEnumerator FlyingSound()
    {
        yield return null;
        SoundManager.instance.PlaySmallAxeSounds(3);
    }

    // 토구가 던진 도끼가 플레이어에게 닿으면 플레이어의 체력을 감소시킨다.
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
