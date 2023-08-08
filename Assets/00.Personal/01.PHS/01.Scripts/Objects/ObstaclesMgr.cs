using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObstaclesMgr : MonoBehaviour
{
    public Transform jumpPos;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            other.GetComponent<SurviverObstacles>().Obstacle = this.transform;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        
    }
}
