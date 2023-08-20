using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IndicatorsManager : MonoBehaviour
{
    public static IndicatorsManager instance;

    private void Awake()
    {
        instance = this; 
    }

    Canvas canvas;

    public List<TargetIndicator> targetIndicators = new List<TargetIndicator>();

    public GameObject TargetIndicatorPrefab;

    public GameObject Enemy;

    void Start()
    {
        canvas = GetComponent<Canvas>();
    }

    private void Update()
    {
        if (targetIndicators.Count > 0)
        {
            for (int i = 0; i < targetIndicators.Count; i++)
            {
                targetIndicators[i].UpdateTargetIndicator();
            }
        }
    }

    public void AddTargetIndicator(GameObject target)
    {
        TargetIndicator indicator = GameObject.Instantiate(TargetIndicatorPrefab, canvas.transform).GetComponent<TargetIndicator>();
        indicator.InitialiseTargetIndicator(target, Camera.main, canvas);
        targetIndicators.Add(indicator);
    }

    public void RemoveTargetIndicator(GameObject target)
    {
        int removeIndex = -1;
        for (int i = 0; i < targetIndicators.Count; i++)
        {
            if (targetIndicators[i].target == target)
            {
                removeIndex = i;
            }
        }

        if(removeIndex > -1)
        {
            Destroy(targetIndicators[removeIndex].gameObject);
            targetIndicators.RemoveAt(removeIndex);
        }
    }
}
