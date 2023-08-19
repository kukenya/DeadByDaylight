using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class SurvivorListManager : MonoBehaviour
{
    public static SurvivorListManager instance;
    private void Awake()
    {
        instance = this;
    }

    public List<GameObject> survivors;
    public List<GameObject> Survivors { get { return survivors; } set { survivors = value; } }
    public GameObject SurvivorsAdd { private get { return survivors[0]; } set { survivors.Add(value); UpdateSurvivorListUI(); } }

    public GameObject[] portraits;

    public void UpdateSurvivorListUI()
    {
        for(int i = 0; i < Survivors.Count; i++)
        {
            portraits[i].SetActive(true);
            portraits[i].transform.GetChild(3).GetComponent<TextMeshProUGUI>().text = Survivors[i].GetPhotonView().Owner.NickName;
        }
    }
}
