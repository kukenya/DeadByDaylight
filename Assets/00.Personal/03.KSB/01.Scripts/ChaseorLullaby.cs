using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChaseorLullaby : MonoBehaviour
{
    // 사운드
    public AudioSource lullaby;             // 자장가
    public AudioSource chase;               // 추적

    public bool isChasing;                  // 추적 중인가?
    public bool isLullaby;

    private void Start()
    {
        lullaby.PlayDelayed(4);
        //lullaby.volume = 0.1f;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.transform.gameObject.layer == 6 && isChasing == false)
        {
            PlayChaseBG();
            isChasing = true;
            isLullaby = false;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.transform.gameObject.layer == 6 && isLullaby == false)
        {
            PlayLullaby();
            isLullaby = true;
            isChasing = false;
        }
    }

    void PlayChaseBG()
    {
        lullaby.Stop();
        chase.Play();
        //chase.volume = 0.1f;
    }

    void PlayLullaby()
    {
        lullaby.Play();
        //lullaby.volume = 0.1f;
        chase.Stop();
    }
}
