using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Axe : MonoBehaviourPun
{
    public AudioSource axeAudio;
    public PhotonView photonView2;
    Rigidbody smallAxeRigidbody;        // 한손도끼 Rigidbody 컴포넌트
    GameObject survivor;                // 생존자
    bool canHit = false;                // 때릴 수 있나 여부
    bool isDestroy;
    public GameObject bloodEffectFactory;    // 피 이펙트 공장

    private void Start()
    {
        
    }

    public void flying(float chargingForce)
    {
        Rigidbody smallAxeRigidbody = GetComponent<Rigidbody>();
        smallAxeRigidbody.AddForce(transform.forward * chargingForce, ForceMode.Impulse);
        // StartCoroutine("FlyingSound");
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

    private void OnCollisionEnter(Collision collision)
    {
        Vector3 point = collision.contacts[0].point;
        Vector3 normal = collision.contacts[0].normal;

        Destroy(gameObject);

        isDestroy = true;

        if (collision.gameObject.name.Contains("Survivor") & canHit == false)
        {
            photonView2.RPC("SmallAxeBlood", RpcTarget.All, point, normal);

            SoundManager.instance.PlayHitSounds(3);
            
            if (photonView2.IsMine)
            {
                
                collision.gameObject.GetComponent<SurviverHealth>().NormalHit();
            }

            canHit = true;
        }
        //else if (collision.gameObject.name.Contains("나무") & canHit == false)
        //{
        //    SoundManager.instance.PlayHitSounds(1);
        //}
        //else
        //{
        //    // 깡 하는 소리
        //}
    }
}
