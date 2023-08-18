using Cinemachine;
using DG.Tweening;
using Photon.Pun;
using Photon.Pun.Demo.Cockpit;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class GameManager2 : MonoBehaviour
{
    public Transform spawnPos;

    void Start()
    {
        if (PhotonNetwork.IsMasterClient == true)
        {
            PhotonNetwork.Instantiate("AnnaAnimation", spawnPos.position, Quaternion.identity);
        }
        else
        {
            PhotonNetwork.Instantiate("AnnaPhoton", spawnPos.position, Quaternion.identity);
        }
    }

    // Update is called once per frame
    void Update()
    {

    }
}
