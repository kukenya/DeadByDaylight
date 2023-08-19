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
        // OnPhotonSerializationView »£√‚ ∫Ûµµ
        PhotonNetwork.SerializationRate = 60;


        PhotonNetwork.Instantiate("AnnaAnimation", spawnPos.position, Quaternion.identity);
        OffCursor();
    }

    // Update is called once per frame
    void Update()
    {

    }

    void OnCursor()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    void OffCursor()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }
}
