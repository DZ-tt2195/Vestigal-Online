using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine.UI;
using TMPro;

public class CreateAndJoinRooms : MonoBehaviourPunCallbacks
{
    public TMP_InputField create;
    public TMP_InputField join;
    public TMP_InputField username;
    public TMP_Text error;
    public Toggle fullScreen;

    private void Start()
    {
        error.gameObject.SetActive(false);
        if (PlayerPrefs.GetString("Username") != "")
        {
            username.text = PlayerPrefs.GetString("Username");
        }
        fullScreen.isOn = Screen.fullScreen;
        fullScreen.onValueChanged.AddListener(delegate { WindowMode(); });
    }

    public void WindowMode()
    {
        Screen.fullScreenMode = (fullScreen.isOn) ? FullScreenMode.ExclusiveFullScreen : FullScreenMode.Windowed;
    }

    IEnumerator ErrorMessage(string x)
    {
        error.gameObject.SetActive(true);
        error.text = x;
        yield return new WaitForSeconds(3f);
        error.gameObject.SetActive(false);
    }

    public void NewUsername(string input)
    {
        PlayerPrefs.SetString("Username", input);
    }

    public void CreateRoom()
    {
        if (create.text == "")
        {
            StartCoroutine(ErrorMessage("You forgot to type in a room code."));
        }
        else if (username.text == "")
        {
            StartCoroutine(ErrorMessage("You forgot to type in a username."));
        }
        else
        { 
            RoomOptions roomOptions = new RoomOptions();
            roomOptions.MaxPlayers = (byte)2;
            PhotonNetwork.CreateRoom(create.text.ToUpper(), roomOptions, null);
        }
    }

    public void JoinRoom()
    {
        if (join.text == "")
        {
            StartCoroutine(ErrorMessage("You forgot to type in a room code."));
        }
        else if (username.text == "")
        {
            StartCoroutine(ErrorMessage("You forgot to type in a username."));
        }
        else
        {
            PhotonNetwork.JoinRoom(join.text.ToUpper());
        }
    }

    public override void OnCreateRoomFailed(short returnCode, string message)
    {
        StartCoroutine(ErrorMessage("That room already exists."));
    }

    public override void OnJoinRoomFailed(short returnCode, string message)
    {
        StartCoroutine(ErrorMessage("That room is already full."));
    }

    public override void OnJoinedRoom()
    {
        PhotonNetwork.LoadLevel("2. Game");
    }
}
