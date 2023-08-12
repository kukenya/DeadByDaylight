using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundManager : MonoBehaviour
{
    public static SoundManager instance;
    private void Awake()
    {
        instance = this;
    }

    // 부수기
    public AudioSource destroySource;   // 오디오 소스
    public AudioClip[] destroyClips;    // 클립들

    public AudioClip[] smallAxeSounds;
    public AudioClip[] bigAxeSounds;
    public AudioClip[] hitSounds;

    public void PlaySmallAxeSounds(int index)
    {
        AudioSource.PlayClipAtPoint(smallAxeSounds[index], Camera.main.transform.position);
    }

    public void PlayBigAxeSounds(int index)
    {
        AudioSource.PlayClipAtPoint(bigAxeSounds[index], Camera.main.transform.position);
    }

    public void PlayHitSounds(int index)
    {
        AudioSource.PlayClipAtPoint(hitSounds[index], Camera.main.transform.position);
    }

    #region 판자

    // 부수는 소리 Play
    public void PlayDestroy(int index)
    {
        destroySource.clip = destroyClips[index];
        destroySource.Play();
    }

    // 부수는 소리 Stop
    public void StopDestroyPallets()
    {
        destroySource.Stop();
    }
    #endregion
}
