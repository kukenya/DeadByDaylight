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

    public GameObject ratio219;
    public Image fadeImage;

    public GameObject perks;
    public GameObject LeftDown;

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
            yield return null;
            Set0AllUI(LeftDown);
            Set0AllUI(perks);
            OffCursor();
            generatorText.text = maxGenerator.ToString();

            fadeImage.DOFade(0, 0.8f);

            yield return new WaitForSeconds(textFadeOffset);
            titleText.DOFade(0, textFadeTime);
            subText.DOFade(0, textFadeTime);
            lineImage.DOFade(0, textFadeTime);
            yield return new WaitForSeconds(2);
            ratio219.SetActive(false);

            for(int i = 0; i < perks.transform.childCount; i++)
            {
                perks.transform.GetChild(i).GetComponent<Image>().DOFade(1, 2);
            }

            FindAllUIInChild(LeftDown);
        }
    }

    void Set0AllUI(GameObject gameObject)
    {
        for (int i = 0; i < gameObject.transform.childCount; i++)
        {
            if (gameObject.transform.GetChild(i).childCount > 0)
            {
                Set0AllUI(gameObject.transform.GetChild(i).gameObject);
            }

            if (gameObject.transform.GetChild(i).GetComponent<Image>() != null)
            {
                gameObject.transform.GetChild(i).GetComponent<Image>().DOFade(0, 0);
            }

            if (gameObject.transform.GetChild(i).GetComponent<TextMeshProUGUI>() != null)
            {
                gameObject.transform.GetChild(i).GetComponent<TextMeshProUGUI>().DOFade(0, 0);
            }
        }
    }

    void FindAllUIInChild(GameObject gameObject)
    {
        for(int i = 0; i < gameObject.transform.childCount; i++)
        {
            if(gameObject.transform.GetChild(i).childCount > 0)
            {
                FindAllUIInChild(gameObject.transform.GetChild(i).gameObject);
            }

            if(gameObject.transform.GetChild(i).GetComponent<Image>() != null)
            {
                gameObject.transform.GetChild(i).GetComponent<Image>().DOFade(1, 1);
            }

            if (gameObject.transform.GetChild(i).GetComponent<TextMeshProUGUI>() != null)
            {
                gameObject.transform.GetChild(i).GetComponent<TextMeshProUGUI>().DOFade(1, 1);
            }
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
