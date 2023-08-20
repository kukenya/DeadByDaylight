using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ConnectManager : MonoBehaviourPunCallbacks
{
    // NickName InputField
    public InputField inputNickName;

    // Connect Button
    public Button btnConnect;

    // Start is called before the first frame update
    void Start()
    {
        // inputNickName�� ������ ����� �� ȣ��Ǵ� �Լ� ���
        inputNickName.onValueChanged.AddListener(OnValueChanged);

        // inputNickName���� ���� ���� �� ȣ��Ǵ� �Լ� ���
        inputNickName.onSubmit.AddListener(
            (string s) =>
            {
                // ��ư�� Ȱ��ȭ �Ǿ��ִٸ�
                if (btnConnect.interactable)
                {
                    // OnclickConnect ȣ��
                    OnClickConnect();
                }
            }
        );

        // ��ư ��Ȱ��ȭ
        btnConnect.interactable = false;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void OnValueChanged(string s)
    {
        // ���ӹ�ư Ȱ��ȭ ��Ȱ��ȭ
        btnConnect.interactable = s.Length > 0;
    }

    public void OnClickConnect()
    {
        // ���� ���� ��û
        PhotonNetwork.ConnectUsingSettings();
    }

    public override void OnConnectedToMaster()
    {
        base.OnConnectedToMaster();

        // �г��� ����
        PhotonNetwork.NickName = inputNickName.text;

        // �κ� ���� ��û
        PhotonNetwork.JoinLobby();
    }

    public override void OnJoinedLobby()
    {
        base.OnJoinedLobby();

        // �κ������ �̵�
        PhotonNetwork.LoadLevel("KJJ_Lobby");
    }
}
