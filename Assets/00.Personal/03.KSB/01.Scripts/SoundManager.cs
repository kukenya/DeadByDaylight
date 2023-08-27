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
    public AudioSource destroySource;   //  부수는소리 오디오 소스
    public AudioClip[] destroyClips;    // 클립들

    // 공격 사운드
    public AudioSource smallAxeSource;  // 도끼 던지기 오디오 소스
    public AudioClip[] smallAxeSounds;  // 클립들

    public AudioSource hitAudioSource;  // 때릴 때 오디오 소스
    public AudioClip[] hitSounds;


    // 던지는 도끼 맞았을 때
    public void PlaySmallAxeSounds(int index)
    {
        AudioSource.PlayClipAtPoint(smallAxeSounds[index], Camera.main.transform.position);
    }

    // 도끼 날아가는 소리
    public void PlaySmallAxeFlyingSounds(int index)
    {
        AudioSource.PlayClipAtPoint(smallAxeSounds[index], Camera.main.transform.position, 0.5f);
    }

    // 맞았을 때 소리
    public void PlayHitSounds(int index)
    {
        // hitAudioSource.clip = hitSounds[index];
        hitAudioSource.PlayOneShot(hitSounds[index], 0.5f);
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

    //public void WalkSound()
    //{
    //    AudioSource.PlayClipAtPoint(hitSounds[6], transform.position, 0.5f);
    //}
}
