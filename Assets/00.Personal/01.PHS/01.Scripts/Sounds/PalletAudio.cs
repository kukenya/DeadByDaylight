using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PalletAudio : MonoBehaviour
{
    public AudioClip palletDropDownSound;
    public AudioSource audioSource;

    public void PlayPalletDropDownSound()
    {
        audioSource.PlayOneShot(palletDropDownSound);
    }
}
