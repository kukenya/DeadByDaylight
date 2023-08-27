using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GeneratorAudio : MonoBehaviour
{
    public AudioSource[] rt;
    public AudioSource[] lt;

    public void RTSound(int value)
    {
        rt[value].Play();
    }

    public void LTSound(int value)
    {
       lt[value].Play();
    }
}
