using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SurviverSound : MonoBehaviour
{
    SurviverAnimation surviverAnimation;

    public AudioSource audioSource;

    public AudioClip[] injSound;
    public AudioClip[] downSound;

    void Start()
    {
        surviverAnimation = GetComponent<SurviverAnimation>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void PlayInjSound()
    {
        audioSource.clip = injSound[Random.Range(0, injSound.Length)];
        audioSource.Play();
    }

    public void PlayDownSound()
    {
        audioSource.clip = downSound[Random.Range(0, downSound.Length)];
        audioSource.Play();
    }
}
