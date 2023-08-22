using System;
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
        Exit,
        Survivor,
        Hook,
        Closet
    }

    public Type type = Type.Window;
    public MonoBehaviour interactScript;

    #region Unity
    private void Start()
    {
        switch (type)
        {
            case Type.Window:
                interactScript = GetComponent<Window>();
                break;
            case Type.Pallet:
                interactScript = GetComponent<Pallet>();
                break;
            case Type.Generator:
                interactScript = GetComponent<Generator>();
                break;
            case Type.Exit:
                interactScript = GetComponent<Exit>();
                break;
            case Type.Survivor:
                interactScript = GetComponentInParent<SurviverHealing>();
                break;
            case Type.Hook:
                interactScript = GetComponent<Hook>();
                break;
            case Type.Closet:interactScript = GetComponent<Closet>();
                break;
        }
    }
    #endregion
}
