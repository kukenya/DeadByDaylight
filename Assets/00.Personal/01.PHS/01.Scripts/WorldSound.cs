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

    public AudioSource genClear;
    public AudioClip genClearSound;

    public void PlayGeneratorClear()
    {
        genClear.Play();
    }
}
