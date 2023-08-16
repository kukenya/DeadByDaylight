using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Survivor : MonoBehaviour
{
    SurviverHealth health;

    private void Start()
    {
        health = GetComponentInParent<SurviverHealth>();
    }
}
