using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SurviverSound : MonoBehaviour
{
    public AudioSource audioSource;

    public AudioClip[] injSound;
    public AudioClip[] downSound;
    public AudioClip screamingSound;
    public AudioClip hookSound;

    public AudioSource windowSource;
    public AudioClip[] jumpWindowSounds;

    public AudioSource[] footStep;
    public AudioClip[] sprintSound;

    SurviverController controller;

    public void PlayHookSound()
    {
        audioSource.PlayOneShot(hookSound);
    }

    public void PlayFootStepLT()
    {
        footStep[0].Play();
    }

    public void PlayFootStepRT()
    {
        footStep[1].Play();
    }

    public void PlayScreamingSound()
    {
        audioSource.PlayOneShot(screamingSound);
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

    private void Start()
    {
        controller = GetComponent<SurviverController>();
    }

    private void Update()
    {
        SprintSound();
    }

    void SprintSound()
    {
        if (controller.enabled == false) return;

        if (controller.BanMove) return;

        if (controller.sprintTime >= controller.maxSprintTime)
        {
            if (audioSource.isPlaying) return;
            audioSource.clip = sprintSound[Random.Range(0, sprintSound.Length)];
            audioSource.Play();
        }
    }

    void PlayPalletJumpSound()
    {

    }
}
