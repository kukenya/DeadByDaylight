using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BlackHoleEffect : MonoBehaviour
{
    Camera mainCamera;
    float currentTime = 0f;
    public float maxTime = 5f;

    public Action action;
    // Start is called before the first frame update
    void Start()
    {
        mainCamera = Camera.main;
        IndicatorsManager.instance.AddTargetIndicator(this.gameObject);
    }

    // Update is called once per frame
    void Update()
    {
        if (currentTime > maxTime) Destroy(this.gameObject);

        float dist = Vector3.Distance(mainCamera.transform.position, transform.position);
        transform.localScale = new Vector3(dist, dist, dist);
        currentTime += Time.deltaTime;
    }

    public void OnDestroy()
    {
        action?.Invoke();
        IndicatorsManager.instance.RemoveTargetIndicator(this.gameObject);
    }
}
