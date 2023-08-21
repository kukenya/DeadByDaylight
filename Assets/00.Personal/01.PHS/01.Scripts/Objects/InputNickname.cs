using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class InputNickname : MonoBehaviour
{
    public TMP_InputField text;
    

    public void ∞·¡§()
    {
        PhotonNetwork.NickName = text.text;
    }
}
