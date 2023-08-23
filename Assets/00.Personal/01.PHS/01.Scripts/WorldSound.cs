using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WorldSound : MonoBehaviour
{
    public static WorldSound Instacne;

    private void Awake()
    {
        Instacne = this;
    }

    AudioSource worldSound;
    public AudioClip[] worldAudios;

    private void Start()
    {
        worldSound = GetComponent<AudioSource>();
    }

    public void PlayWorldSound(int idx)
    {
        worldSound.PlayOneShot(worldAudios[idx]);
    }
}
