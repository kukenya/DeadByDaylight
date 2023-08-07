using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InteractionMgr : MonoBehaviour
{
    public enum Type
    {
        Window,
        Pallet,
        ExitLever,
    }

    public Type interaction;

    private void Start()
    {
        switch (interaction)
        {
            case Type.Window:
                obstacle = GetComponent<ObstaclesMgr>();
                break;
        }
    }
}
