using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EndAnimation : MonoBehaviour
{
    GameObject anna;

    // Start is called before the first frame update
    void Start()
    {
        Invoke("FindAnna", 10);
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void FindAnna()
    {
        anna = GameObject.FindGameObjectWithTag("Killer");
    }

    public void OnEndAnimation()
    {
        anna.GetComponent<AnnaMove>().EndAnimation();
    }

}
