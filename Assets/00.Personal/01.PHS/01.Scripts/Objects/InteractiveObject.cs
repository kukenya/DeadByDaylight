using System.Collections;
using System.Collections.Generic;
using UnityEditor.Rendering;
using UnityEngine;

public class InteractiveObject : MonoBehaviour
{
    public enum Type
    {
        Window,
        Pallet,
        Generator,
        Exit
    }

    public Type type = Type.Window;

    public Window window;
    public Pallet pallet;
    public Generator generator;
    public Exit exit;

    private void Start()
    {
        switch (type)
        {
            case Type.Window:
                window = GetComponent<Window>();
                break;
            case Type.Pallet:
                pallet = GetComponent<Pallet>();
                break;
            case Type.Generator:
                generator = GetComponent<Generator>();
                break;
            case Type.Exit:
                exit = GetComponent<Exit>();
                break;
        }
    }
}
