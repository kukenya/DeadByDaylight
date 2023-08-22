using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// 1. ���� �Ŵ���
public class LobbySoundManager : MonoBehaviour
{
    // ���� ���� static����
    public static LobbySoundManager instance;

    // BGM ����
    public enum EBgm
    {
        BGM_LOBBY
    }

    // bgm audio clip ���� �� �ִ� �迭
    public AudioClip bgms;

    // 1-7. BGM Play �ϴ� AudioSource ���� ����
    public AudioSource audioBgm;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // 1-6. BGM Play �Լ� ����
    public void PlayBGM(EBgm bgmIdx)
    {
        // 1-9. play�� bgm ���� -> ConnectManager
        audioBgm.clip = bgms;
        audioBgm.Play();
    }

    // 1-12. bgm stop
    public void StopBGM()
    {
        audioBgm.Stop();
    }

    private void Start()
    {
        PlayBGM(EBgm.BGM_LOBBY);
    }
}
