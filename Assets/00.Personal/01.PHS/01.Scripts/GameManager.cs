using Cinemachine;
using DG.Tweening;
using Photon.Pun;
using Photon.Realtime;
using System;
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

    public TextMeshProUGUI generatorText;
    int maxGenerator = 4;
    public int Generator { get { return maxGenerator; } set { maxGenerator = value; generatorText.text = maxGenerator.ToString(); } }

    public CinemachineVirtualCamera survivorCamera1;

    public List<Transform> spawnPos;
    public List<Transform> generatorSpawnPos;
    public int playerListIdx = 0;

    public string survivorName;

    public SurvivorListManager listManager;

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

    public GameObject survivorCanvas;
    public GameObject mudererCanvas;

    // Start is called before the first frame update
    IEnumerator Start()
    {
        //RPC »£√‚ ∫Ûµµ
        PhotonNetwork.SendRate = 60;

        //OnPhotonSerializeView »£√‚ ∫Ûµµ
        PhotonNetwork.SerializationRate = 60;

        if (PhotonNetwork.IsMasterClient)
        {
            foreach (Transform tr in generatorSpawnPos)
            {
                PhotonNetwork.Instantiate("Generator", tr.position, tr.rotation);
            }
        }

        if(SelecterManager.Instance.IsSurvivor == false) 
        {
            PhotonNetwork.Instantiate("AnnaAnimation", spawnPos[0].position, Quaternion.identity);
            OffCursor();
            survivorCanvas.SetActive(false);
            mudererCanvas.SetActive(true); 
        }
        else
        {
            survivorCanvas.SetActive(true);
            mudererCanvas.SetActive(false);
            GameObject survivor = PhotonNetwork.Instantiate(survivorName, spawnPos[0].position, Quaternion.identity);
            survivorCamera1.Follow = survivor.transform.GetChild(0);
            listManager.SurvivorsAdd = survivor;
            photonView.RPC(nameof(UpdateSurvivorList), RpcTarget.All);

            foreach (Player player in PhotonNetwork.PlayerList)
            {
                print(PhotonNetwork.NickName);
                if (player.NickName.Equals(PhotonNetwork.NickName))
                {
                    break;
                }
                playerListIdx++;
            }
            yield return new WaitForSeconds(0.1f);

            OffCursor();
            generatorText.text = maxGenerator.ToString();
            yield return new WaitForSeconds(textFadeOffset);
            titleText.DOFade(0, textFadeTime);
            subText.DOFade(0, textFadeTime);
            lineImage.DOFade(0, textFadeTime);
            generatorText.DOFade(1, textFadeTime);
        }
    }

    [PunRPC]
    void UpdateSurvivorList()
    {
        GameObject[] go = GameObject.FindGameObjectsWithTag("Survivor");
        foreach( GameObject go2 in go)
        {
            if (listManager.Survivors.Contains(go2)) continue;
            listManager.SurvivorsAdd = go2;
            go2.GetComponent<SurviverHealth>().survivorRoomIdx = listManager.Survivors.IndexOf(go2);
        }
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
