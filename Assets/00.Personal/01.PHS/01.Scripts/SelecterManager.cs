using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SelecterManager : MonoBehaviour
{
    public static SelecterManager Instance;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public bool IsSurvivor = true;
}
