using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class SkillCheck : MonoBehaviour
{
    public static SkillCheck Instance;

    private void Awake()
    {
        Instance = this;
        skillCheck.SetActive(false);
        this.enabled = false;
    }

    public GameObject skillCheck;
    public GameObject[] skillCheckRange;
    public GameObject pointer;

    public float pointerRotationTime = 1f;
    System.Action<int> action;

    [Header("¼Ò¸®")]
    public AudioSource skillCheckSound;
    public AudioClip startSound;
    public AudioClip normalCheckSound;
    public AudioClip hardCheckSound;

    private void OnEnable()
    {
        
    }

    private void OnDisable()
    {
        Check();
    }

    public void InputAction(System.Action<int> action)
    {
        this.action = action;
    }

    float currentTime;
    float checkTime = 1;
    void Update()
    {
        currentTime += Time.deltaTime;
        if(currentTime >= checkTime)
        {
            currentTime = 0;
            if (Random.Range(0, 5) == 1)
            {
                currentTime -= 3;
                StartCoroutine(SkillCheckCor());
            }
        }


        if (Input.GetKeyDown(KeyCode.Space))
        {
            Check();
        }
    }

    IEnumerator SkillCheckCor()
    {
        AudioPlay(startSound);
        yield return new WaitForSeconds(0.2f);
        StartSKillCheck();
    }
    
    public void AudioPlay(AudioClip audioClip)
    {
        skillCheckSound.clip = audioClip;
        skillCheckSound.Play();
    }

    public void StartSKillCheck()
    {
        for(int i = 0; i < skillCheckRange.Length; i++)
        {
            skillCheckRange[i].SetActive(false);
        }

        GameObject obj = skillCheckRange[Random.Range(0, skillCheckRange.Length)];

        float offsetAngle = Random.Range(200, 360);
        print(offsetAngle);
        obj.transform.parent.rotation = Quaternion.Euler(0, 0, -offsetAngle);

        obj.SetActive(true);
        SkillCheckRange range = obj.GetComponent<SkillCheckRange>();

        minCheckAngle = ClampAngle(range.normalCheckStartPos + offsetAngle);
        maxCheckAngle = ClampAngle(range.normalCheckEndPos + offsetAngle);
        minHardCheckAngle = ClampAngle(range.hardCheckStartPos + offsetAngle);
        maxHardCheckAngle = ClampAngle(range.hardCheckEndPos + offsetAngle);

        skillCheck.SetActive(true);
        skillCheckCor = StartCoroutine(PointerRotation());
    }

    float ClampAngle(float angle)
    {
        if(angle > 360)
        {
            angle -= 360;
        }
        else if(angle < 0)
        {
            angle += 360;
        }

        return angle;
    }

    // 23
    public void Check()
    {
        if (skillCheckCor == null) return;

        skillCheck.SetActive(false);
        StopCoroutine(skillCheckCor);
        skillCheckCor = null;
        if (checkAngle < maxCheckAngle && checkAngle > minCheckAngle)
        {
            if (checkAngle < maxHardCheckAngle && checkAngle > minHardCheckAngle)
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
    private Coroutine skillCheckCor;

    IEnumerator PointerRotation()
    {
        float currentTime = 0;
        while (true)
        {
            currentTime += Time.deltaTime;
            checkAngle = Mathf.Lerp(0, 360, currentTime / pointerRotationTime);
            pointer.transform.eulerAngles = new Vector3 (0, 0, -checkAngle);

            if(currentTime >= pointerRotationTime) break;
            yield return null;
        }
        Check();
    }
}
