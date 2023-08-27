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

    // �μ���
    public AudioSource destroySource;   //  �μ��¼Ҹ� ����� �ҽ�
    public AudioClip[] destroyClips;    // Ŭ����

    // ���� ����
    public AudioSource smallAxeSource;  // ���� ������ ����� �ҽ�
    public AudioClip[] smallAxeSounds;  // Ŭ����

    public AudioSource hitAudioSource;  // ���� �� ����� �ҽ�
    public AudioClip[] hitSounds;


    // ������ ���� �¾��� ��
    public void PlaySmallAxeSounds(int index)
    {
        AudioSource.PlayClipAtPoint(smallAxeSounds[index], Camera.main.transform.position);
    }

    // ���� ���ư��� �Ҹ�
    public void PlaySmallAxeFlyingSounds(int index)
    {
        AudioSource.PlayClipAtPoint(smallAxeSounds[index], Camera.main.transform.position, 0.5f);
    }

    // �¾��� �� �Ҹ�
    public void PlayHitSounds(int index)
    {
        // hitAudioSource.clip = hitSounds[index];
        hitAudioSource.PlayOneShot(hitSounds[index], 0.5f);
    }

    #region ����

    // �μ��� �Ҹ� Play
    public void PlayDestroy(int index)
    {
        destroySource.clip = destroyClips[index];
        destroySource.Play();
    }

    // �μ��� �Ҹ� Stop
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
