using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Axe : MonoBehaviour
{
    Rigidbody smallAxeRigidbody;        // 한손도끼 Rigidbody 컴포넌트
    GameObject survivor;                // 생존자
    bool canHit = false;                // 때릴 수 있나 여부
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
        // 날아갈 때 X축으로 회전한다.
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

    // 토구가 던진 도끼가 플레이어에게 닿으면 플레이어의 체력을 감소시킨다.
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
        else if(other.gameObject.name.Contains("나무") & canHit == false)
        {
            SoundManager.instance.PlayHitSounds(1);
        }
        else
        {
            // 깡 하는 소리
        }
    }
}
