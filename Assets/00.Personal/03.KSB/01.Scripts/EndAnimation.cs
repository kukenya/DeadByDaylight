using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EndAnimation : MonoBehaviour
{
    GameObject annaMove;

    // Start is called before the first frame update
    void Start()
    {
        annaMove = GameObject.FindGameObjectWithTag("Killer");
    }

    // Update is called once per frame
    void Update()
    {
        
    }


    public void OnEndAnimation()
    {
        annaMove.GetComponent<AnnaMove>().EndAnimation();
    }

}
