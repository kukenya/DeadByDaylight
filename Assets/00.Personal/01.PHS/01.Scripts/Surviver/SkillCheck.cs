using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SkillCheck : MonoBehaviour
{
    public static SkillCheck Instance;

    private void Awake()
    {
        Instance = this;
    }

    public GameObject skillCheck;
    public GameObject[] skillCheckRange;
    public GameObject pointer;

    public float pointerRotationTime = 1f;

    Coroutine skillCheckCor;
    Coroutine randomSkillCheckCor;

    System.Action<int> action;

    [Header("¼Ò¸®")]
    public AudioSource skillCheckSound;
    public AudioClip startSound;
    public AudioClip normalCheckSound;
    public AudioClip hardCheckSound;

    private void Start()
    {
        skillCheck.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            Check();
        }
    }

    public void StartRandomSkillCheck(System.Action<int> action = null, float checkTime = 1)
    {
        this.action = action;
        if (randomSkillCheckCor != null) return;
        randomSkillCheckCor = StartCoroutine(RandomSkillCheck(checkTime));
    }

    public void EndRandomSkillCheck()
    {
        if (randomSkillCheckCor == null) return;
        StopCoroutine(randomSkillCheckCor);
        randomSkillCheckCor = null;
    }
    
    public void AudioPlay(AudioClip audioClip)
    {
        skillCheckSound.clip = audioClip;
        skillCheckSound.Play();
    }

    public IEnumerator RandomSkillCheck(float checkTime)
    {
        while (true)
        {
            yield return new WaitForSeconds(checkTime);
            if(Random.Range(0, 5) == 1)
            {
                AudioPlay(startSound);
                yield return new WaitForSeconds(0.2f);
                StartSKillCheck();
            }
        }
    }

    public void StartSKillCheck()
    {
        if (skillCheckCor != null) return;
        for(int i = 0; i < skillCheckRange.Length; i++)
        {
            skillCheckRange[i].SetActive(false);
        }

        GameObject obj = skillCheckRange[Random.Range(0, skillCheckRange.Length)];
        obj.SetActive(true);
        SkillCheckRange range = obj.GetComponent<SkillCheckRange>();

        minCheckAngle = range.normalCheckStartPos;
        maxCheckAngle = range.normalCheckEndPos;
        minHardCheckAngle = range.hardCheckStartPos;
        maxHardCheckAngle = range.hardCheckEndPos;

        skillCheck.SetActive(true);
        skillCheckCor = StartCoroutine(PointerRotation());
    }
    // 23
    public void Check()
    {
        if(skillCheckCor == null) return;

        skillCheck.SetActive(false);
        StopCoroutine(skillCheckCor);
        skillCheckCor = null;
        if (checkAngle < maxCheckAngle && checkAngle > minCheckAngle)
        {
            if(checkAngle < maxHardCheckAngle &&  checkAngle > minHardCheckAngle) 
            {
                action?.Invoke(2);
                AudioPlay(hardCheckSound);
            }
            else
            {
                action?.Invoke(1);
                AudioPlay(normalCheckSound);
            }
        }
        else
        {
            action?.Invoke(0);
        }
    }

    public float offset = 60;
    public float angleOffset = 70;

    float checkAngle;

    public float minCheckAngle;
    public float maxCheckAngle;

    public float minHardCheckAngle;
    public float maxHardCheckAngle;

    IEnumerator PointerRotation()
    {
        float currentTime = 0;
        while (true)
        {
            currentTime += Time.deltaTime;
            checkAngle = Mathf.Lerp(0, 360, currentTime / pointerRotationTime);
            pointer.transform.eulerAngles = new Vector3 (0, 0, -checkAngle);

            yield return null;
            if(currentTime >= pointerRotationTime) break;
        }
        Check();
    }
}
