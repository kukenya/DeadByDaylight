using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnnaCameraController : MonoBehaviour
{
    public Camera cam;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        cam.transform.position = this.transform.position;
        cam.transform.rotation = this.transform.rotation;
    }
}
