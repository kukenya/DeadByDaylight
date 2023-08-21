using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// 1. 사운드 매니저
public class LobbySoundManager : MonoBehaviour
{
    // 나를 담을 static변수
    public static LobbySoundManager instance;

    // BGM 종류
    public enum EBgm
    {
        BGM_LOBBY
    }

    // bgm audio clip 담을 수 있는 배열
    public AudioClip bgms;

    // 1-7. BGM Play 하는 AudioSource 담을 변수
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

    // 1-6. BGM Play 함수 생성
    public void PlayBGM(EBgm bgmIdx)
    {
        // 1-9. play할 bgm 설정 -> ConnectManager
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
