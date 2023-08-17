using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SurvivorShader : MonoBehaviour
{
    public GameObject go;

    public void OnRedXray()
    {
        go.layer = 9;
    }

    public void OffShader()
    {
        go.layer = 6;
    }
}
