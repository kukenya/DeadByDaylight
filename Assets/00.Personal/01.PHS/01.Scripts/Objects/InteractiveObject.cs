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

    Window window;
    Pallet pallet;
    SurviverObstacles obstacles;

    public bool interactive = false;

    private void Start()
    {
        obstacles = SurviverObstacles.instance;
        switch (interaction)
        {
            case Type.Window:
                window = GetComponent<Window>();
                break;
            case Type.Pallet:
                pallet = GetComponent<Pallet>();
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
                obstacles.StartJumpPallet(transform);
                break;
        }
    }
}
