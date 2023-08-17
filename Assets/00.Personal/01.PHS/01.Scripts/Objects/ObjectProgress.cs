using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ObjectProgress : MonoBehaviour
{
    public enum Type
    {
        Generator,
        ExitLever,
        Healing
    }
    public Type obj = Type.Generator;

    [Header("게이지 차는 시간")]
    public float generatorMaxTime = 80f;


    public Image progressBar; 

    float maxTime = 0;
    float progressTime = 0;

    bool increase = false;

    public bool Increase { get { return increase; } set {  increase = value; } }

    private void Start()
    {
        progressBar.fillAmount = 0;
        switch (obj)
        {
            case Type.Generator:
                maxTime = generatorMaxTime;
                break;
        }
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            Increase = true;
        }
        else if (Input.GetKeyUp(KeyCode.Alpha3))
        {
            Increase = false;
        }

        if(increase)
        {
            progressTime += Time.deltaTime / maxTime;
            UpdateProgressBar();
        }
    }

    void UpdateProgressBar()
    {
        progressBar.fillAmount = progressTime;
    }
}
