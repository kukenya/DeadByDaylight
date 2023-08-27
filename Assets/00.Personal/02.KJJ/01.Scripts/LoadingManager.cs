using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;



public class LoadingManager : MonoBehaviourPun
{
    public Image progressbar;

    int readyCount;
    bool isReady;
    AsyncOperation operation;


    // Start is called before the first frame update
    void Start()
    {
        PhotonNetwork.AutomaticallySyncScene = false;
        StartCoroutine(LoadScene());
    }

    // Update is called once per frame
    void Update()
    {
        

        if (readyCount == PhotonNetwork.CurrentRoom.PlayerCount)
        {
            print(readyCount + " / " + PhotonNetwork.CurrentRoom.PlayerCount);
            operation.allowSceneActivation = true;
        }
    }
    private void LateUpdate()
    {
        if (readyCount != PhotonNetwork.CurrentRoom.PlayerCount)
        {
            print(readyCount + " / " + PhotonNetwork.CurrentRoom.PlayerCount);
            if(operation != null) operation.allowSceneActivation = false;
        }
    }

    [PunRPC]
    void RpcSetReady()
    {
        readyCount++;
        print("readyCount : " + readyCount);
    }

    IEnumerator LoadScene()
    {
        yield return null;

        operation = SceneManager.LoadSceneAsync("PHS");
        operation.allowSceneActivation = false;

        while(!operation.isDone)
        {
            yield return null;
            if(progressbar.fillAmount < 0.9f)
            {
                progressbar.fillAmount = Mathf.MoveTowards(progressbar.fillAmount, 0.9f, Time.deltaTime);
            }

            else if(operation.progress >= 0.9f)
            {
                progressbar.fillAmount = Mathf.MoveTowards(progressbar.fillAmount, 1f, Time.deltaTime);

            }

            if (progressbar.fillAmount >= 1f)
            {
                if (isReady == false)
                {
                    isReady = true;
                    photonView.RPC(nameof(RpcSetReady), RpcTarget.AllBuffered);
                }

            }
        }
    }
}
