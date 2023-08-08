using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Util : MonoBehaviour
{
    public static Util instance;

    private void Awake()
    {
        instance = this;
    }


}
