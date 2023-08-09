using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using UnityEngine.SceneManagement;

public class ConnectToLobby : MonoBehaviourPunCallbacks
{
    public void Start()
    {
        Application.targetFrameRate = 60;
    }

    public void Join(string region)
    {
        PhotonNetwork.PhotonServerSettings.AppSettings.FixedRegion = region;
        PhotonNetwork.ConnectUsingSettings();
        PlayerPrefs.SetString("Username", "");
    }

    public override void OnConnectedToMaster()
    {
        PhotonNetwork.JoinLobby();
    }

    public override void OnJoinedLobby()
    {
        SceneManager.LoadScene("1. Lobby");
    }
}
