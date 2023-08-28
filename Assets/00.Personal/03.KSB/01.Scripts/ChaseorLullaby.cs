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

    public LayerMask layerMask;

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

    private void Update()
    {

    }


    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.name.Contains("Survivor") && isChasing == false && photonView.IsMine)
        {

            lullaby.Stop();
            chase.PlayDelayed(0.5f);
            chase.volume = 0.8f;

            isChasing = true;
            isLullaby = false;

        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.name.Contains("Survivor") && isLullaby == false && photonView.IsMine)
        {
            chase.Stop();
            lullaby.PlayDelayed(0.5f);
            isLullaby = true;
            isChasing = false;
        }
    }
}

