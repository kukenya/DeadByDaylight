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
    public AudioSource destroySource;   // ����� �ҽ�
    public AudioClip[] destroyClips;    // Ŭ����

    // ���� ����
    public AudioSource smallAxeSource;  // ����� �ҽ�
    public AudioClip[] smallAxeSounds;  // Ŭ����

    public AudioSource hitAudioSource;
    public AudioClip[] hitSounds;

    public void PlaySmallAxeSounds(int index)
    {
        AudioSource.PlayClipAtPoint(smallAxeSounds[index], Camera.main.transform.position);
    }

    public void PlayHitSounds(int index)
    {
        // hitAudioSource.clip = hitSounds[index];
        hitAudioSource.PlayOneShot(hitSounds[index]);
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
}
