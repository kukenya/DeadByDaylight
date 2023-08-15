using DG.Tweening;
using Photon.Pun;
using Photon.Pun.Demo.Cockpit;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    public TextMeshProUGUI titleText;
    public TextMeshProUGUI subText;
    public Image lineImage;

    public List<Transform> spawnPos;

    public string survivorName;

    private void Awake()
    {
        if(Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(this.gameObject);
        }
        else
        {
            Destroy(this.gameObject);
        }
    }

    public float textFadeTime;
    public float textFadeOffset;
    // Start is called before the first frame update
    IEnumerator Start()
    {
        for(int i  = 0; i < transform.childCount; i++) 
        {
            spawnPos.Add(transform.GetChild(i));
        }

        //RPC ȣ�� ��
        PhotonNetwork.SendRate = 30;

        //OnPhotonSerializeView ȣ�� ��
        PhotonNetwork.SerializationRate = 30;

        PhotonNetwork.Instantiate(survivorName, spawnPos[0].position, Quaternion.identity);

        OffCursor();
        yield return new WaitForSeconds(textFadeOffset);
        titleText.DOFade(0, textFadeTime);
        subText.DOFade(0, textFadeTime);
        lineImage.DOFade(0, textFadeTime);
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
