using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SurviverSound : MonoBehaviour
{
    public AudioSource audioSource;

    public AudioClip[] injSound;
    public AudioClip[] downSound;

    public AudioSource windowSource;
    public AudioClip[] jumpWindowSounds;

    public AudioSource[] footStep;

    public void PlayFootStepLT()
    {
        footStep[0].Play();
    }

    public void PlayFootStepRT()
    {
        footStep[1].Play();
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

    public void PlayWindow()
    {
        windowSource.clip = jumpWindowSounds[Random.Range(0, jumpWindowSounds.Length)];
        windowSource.Play();
    }
}
