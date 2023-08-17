using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SurvivorShader : MonoBehaviour
{
    public void OnRedXray()
    {
        gameObject.layer = 9;
    }

    public void OffShader()
    {
        gameObject.layer = 6;
    }
}
