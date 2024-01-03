using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using UnityEngine.SceneManagement;

public class ConnectToLobby : MonoBehaviourPunCallbacks
{
    [SerializeField] Button localPlay;

    public void Start()
    {
        localPlay.onClick.AddListener(LocalPlay);
        Application.targetFrameRate = 60;
    }

    void LocalPlay()
    {
        SceneManager.LoadScene("3. Local");
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
