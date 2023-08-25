using DG.Tweening;
using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HeartSound : MonoBehaviour
{
    public Transform ownerSurvivor;
    public Transform slasher;
    AudioSource heartAudio;
    public AudioClip heartSound;

    public float heartBeatTiming = 0;
    public float dist = 0;

    public bool heartSoundPlaying = false;

    // Start is called before the first frame update
    IEnumerator Start()
    {
        slasher = transform;
        heartAudio = GetComponent<AudioSource>();
        minHeartBeatTiming = heartSound.length;
        yield return new WaitForSeconds(1);

        if(gameObject.transform.parent.name.Contains("Survivor") == false)
        {
            yield break;
        }

        foreach (GameObject go in SurvivorListManager.instance.Survivors)
        {
            if (go.GetPhotonView().IsMine)
            {
                ownerSurvivor = go.transform;
                break;
            }
        }
        float currentTime = 0;
        StartCoroutine(BackGroundSound());


        // ���� Ư�� ������Ʈ ���� ������ 0.1�ʸ��� �ݺ��ǰ� 30m ���������� 1�� �ݺ��ȴ�.
        // 0.1 + (dist / 25) * 0.9;
        while (true)
        {
            if (currentTime >= heartBeatTiming)
            {
                heartAudio.PlayOneShot(heartSound);
                currentTime = 0;
            }

            currentTime += Time.deltaTime;

            float dist = Vector3.Distance(ownerSurvivor.position, slasher.position);

            if (heartSoundPlaying == false) dist = 100f;

            print(dist);
            float a = Mathf.InverseLerp(heartStartPosition, heartEndPosition, dist);
            heartAudio.volume = a;
            a = 1 - a;
            if(a >= 1)
            {
                heartBeatTiming = float.MaxValue;
            }
            else if(a >= 0 && a <= 1) 
            {
                heartBeatTiming = minHeartBeatTiming + a * (maxHeartBeatTiming - minHeartBeatTiming);
            }
            else if(a <= 0)
            {
                heartBeatTiming = minHeartBeatTiming;
            }
            yield return null;
        }
    }

    // �Ÿ��� ���� �Ҹ��� �ݺ� ����Ǵ� �����ð��� �ְ� �ʹ�.
    [Header("Heart Sound")]
    public float heartStartPosition = 30;
    public float heartEndPosition = 5;
    public float minHeartBeatTiming;
    public float maxHeartBeatTiming = 1.0f;

    [Header("�߰� ����")]
    public float chase2SoundStartDist = 10;
    public float chase2SoundEndDelay = 2f;
    public float chase1SoundStartDist = 30;
    public float chase1SoundEndDelay = 1f;

    public bool chase2SoundPlaying = false;
    public bool chase1SoundPlaying = false;

    public AudioSource chaseAudioSource;
    public AudioClip[] chaseAudioClips;

    public enum ChaseSoundState
    {
        Chase2,
        Chase1,
        None,
    }

    public ChaseSoundState soundState = ChaseSoundState.None;

    public float a = 1;

    IEnumerator BackGroundSound()
    {
        while (true)
        {
            
            float dist = Vector3.Distance(ownerSurvivor.position, slasher.position);
            if (heartSoundPlaying == false) dist = 100f;
            print(dist);
            if(dist < chase2SoundStartDist)
            {
                if (soundState != ChaseSoundState.Chase1)
                {
                    soundState = ChaseSoundState.Chase1;
                    chaseAudioSource.clip = chaseAudioClips[0];
                    chaseAudioSource.Play();
                    chaseAudioSource.DOFade(1, 1);
                }
            }
            else
            {
                if(soundState != ChaseSoundState.None)
                {
                    soundState = ChaseSoundState.None;
                    chaseAudioSource.DOFade(0, 1).onComplete += () => { chaseAudioSource.Stop(); };
                }
            }
            yield return null;
        }
    }

    private void Update()
    {
        
    }
}
