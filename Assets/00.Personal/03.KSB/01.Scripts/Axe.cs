using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Axe : MonoBehaviourPun
{
    public AudioSource axeAudio;
    public PhotonView photonView2;
    Rigidbody smallAxeRigidbody;        // �Ѽյ��� Rigidbody ������Ʈ
    GameObject survivor;                // ������
    bool canHit = false;                // ���� �� �ֳ� ����
    bool isDestroy;
    public GameObject bloodEffectFactory;    // �� ����Ʈ ����

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
        //else if (collision.gameObject.name.Contains("����") & canHit == false)
        //{
        //    SoundManager.instance.PlayHitSounds(1);
        //}
        //else
        //{
        //    // �� �ϴ� �Ҹ�
        //}
    }
}
