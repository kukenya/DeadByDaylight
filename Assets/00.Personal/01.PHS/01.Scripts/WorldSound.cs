using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WorldSound : MonoBehaviour
{
    public static WorldSound Instacne;

    private void Awake()
    {
        if (Instacne == null)
        {
            Instacne = this;
            print("WorldSound Awake!!!");
        }
        else
        {
            Destroy(gameObject);
        }
    }

    AudioSource worldSound;
    public AudioClip[] worldAudios;

    private void Start()
    {
        worldSound = GetComponent<AudioSource>();
    }
    bool bCheck;
    private void Update()
    {
        if (false == bCheck && worldAudios.Length == 0)
        {
            bCheck = true;
            print("worldAudios.Length is 0");
        }   
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
