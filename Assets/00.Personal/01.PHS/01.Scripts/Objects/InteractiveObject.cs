using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InteractiveObject : MonoBehaviour
{
    public KeyCode keyCode = KeyCode.Space;
    public enum Type
    {
        Window,
        Pallet,
        ExitLever
    }

    public Type interaction;

    PalletChild palletChild;
    SurviverObstacles obstacles;

    public bool interactive = false;

    private void Start()
    {
        obstacles = SurviverObstacles.instance;
        switch (interaction)
        {
            case Type.Window:
                break;
            case Type.Pallet:
                palletChild = GetComponent<PalletChild>();
                break;
        }
    }

    public void Interact()
    {
        switch (interaction)
        {
            case Type.Window:
                obstacles.StartJumpWindow(transform);
                break;
            case Type.Pallet:
                palletChild.Interact();
                break;
        }
    }
}
