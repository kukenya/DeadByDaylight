using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class ChaseorLullaby : MonoBehaviourPun
{
    public static ChaseorLullaby instance;
    private void Awake()
    {
        instance = this;
    }

    // ����
    public AudioSource lullaby;             // ���尡
    public AudioSource chase;               // ����

    public bool isChasing;                  // ���� ���ΰ�?
    public bool isLullaby;

    private void Start()
    {
        lullaby.PlayDelayed(3);
        // lullaby.volume = 0.1f;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.transform.gameObject.name.Contains("Survivor") && isChasing == false)
        {
            if (photonView.IsMine)
            {
                PlayChaseBG();
            }
            
            isChasing = true;
            isLullaby = false;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.transform.gameObject.name.Contains("Survivor") && isLullaby == false)
        {
            PlayLullaby();
            isLullaby = true;
            isChasing = false;
        }
    }

    public void PlayChaseBG()
    {
        //isChasing = true;
        //isLullaby = false;

        lullaby.Stop();
        chase.Play();
        //chase.volume = 0.1f;
    }

    public void PlayLullaby()
    {
        //isChasing = false;
        //isLullaby = true;

        lullaby.Play();
        //lullaby.volume = 0.1f;
        chase.Stop();
    }
}
