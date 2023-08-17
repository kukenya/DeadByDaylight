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

public class GameManager : MonoBehaviourPun
{
    public static GameManager Instance;

    public TextMeshProUGUI titleText;
    public TextMeshProUGUI subText;
    public Image lineImage;

    public CinemachineVirtualCamera survivorCamera1;

    public List<Transform> spawnPos;
    public List<Transform> generatorSpawnPos;

    public string survivorName;

    public bool multiplay = false;

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
        PhotonNetwork.SendRate = 60;

        //OnPhotonSerializeView ȣ�� ��
        PhotonNetwork.SerializationRate = 60;

        if (PhotonNetwork.IsMasterClient)
        {
            foreach (Transform tr in generatorSpawnPos)
            {
                PhotonNetwork.Instantiate("Generator", tr.position, tr.rotation);
            }
        }

        GameObject survivor = PhotonNetwork.Instantiate(survivorName, spawnPos[0].position, Quaternion.identity);
        survivorCamera1.Follow = survivor.transform.GetChild(0);

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
