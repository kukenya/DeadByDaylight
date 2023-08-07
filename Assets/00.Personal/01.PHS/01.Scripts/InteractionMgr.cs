using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InteractionMgr : MonoBehaviour
{
    public enum Interaction
    {
        None,
        Obstacles,
    }

    public Interaction interaction;

    ObstaclesMgr obstacle;

    private void Start()
    {
        switch (interaction)
        {
            case Interaction.Obstacles:
                obstacle = GetComponent<ObstaclesMgr>();
                break;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            other.GetComponent<SurviverInteractionUI>();
            switch (interaction)
            {
                case Interaction.None:
                    break;
                case Interaction.Obstacles:
                    other.GetComponent<SurviverObstacles>().Obstacle = obstacle.jumpPos;
                    break;
            }
        }
    }

    private void OnTriggerStay(Collider other)
    {
        if (other.CompareTag("Player"))
        {

        }
    }



    private void OnTriggerExit(Collider other)
    {
        
    }
}
