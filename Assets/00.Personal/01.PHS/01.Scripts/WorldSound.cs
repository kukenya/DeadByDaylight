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

    private void Update()
    {
        if(Input.GetKeyDown(KeyCode.Alpha5))
        {
          //  WorldSound.Instacne.PlayWorldSound(0);
        }
    }

    public void PlayWorldSound(int idx)
    {
        if(worldAudios == null)
        {
            print("00000000000000000000");

        }
            
        if(worldAudios.Length <= 0)
        {
            print("!!!!!!!!!!!!!!!!!!!!!!!!");
        }
        if(worldSound == null)
        {
            print("2222222222222222222222");
        }
        if (worldAudios[idx] == null) return;
        worldSound.PlayOneShot(worldAudios[idx]);
    }
}
