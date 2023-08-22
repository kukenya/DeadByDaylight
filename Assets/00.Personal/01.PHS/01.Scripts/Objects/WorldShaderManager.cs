using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WorldShaderManager : MonoBehaviour
{
    public static WorldShaderManager Instance;

    private void Awake()
    {
        Instance = this;
    }

    public enum Survivor
    {
        None,
        OwnerDown,
        Hooked,
    }

    public Survivor survivorShader = Survivor.None;
    public Survivor SurvivorShader { get { return survivorShader; }  
        set 
        { 
            survivorShader = value;
            switch (survivorShader)
            {
                case Survivor.None:
                    foreach (GameObject go in SurvivorListManager.instance.Survivors)
                    {
                        if (go.GetPhotonView().IsMine == true) continue;
                        go.GetComponent<SurvivorShader>().YellowXray = false;
                    }
                    break;
                case Survivor.OwnerDown:
                    print("여기 진입함?");
                    foreach(GameObject go in SurvivorListManager.instance.Survivors)
                    {
                        print(go.transform.position);
                        if (go.GetPhotonView().IsMine == true) continue;
                        go.GetComponent<SurvivorShader>().YellowXray = true;
                    }
                    break;
                case Survivor.Hooked:
                    foreach (GameObject go in SurvivorListManager.instance.Survivors)
                    {
                        if (go.GetPhotonView().IsMine == true) continue;
                        go.GetComponent<SurvivorShader>().YellowXray = true;
                    }
                    break;
            }
        } 
    }
}
