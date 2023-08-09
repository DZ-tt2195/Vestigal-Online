using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using TMPro;
using System.Linq;
using System;
using Photon.Realtime;
using ExitGames.Client.Photon;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class Manager : MonoBehaviour, IOnEventCallback
{
    public TileData tileClone;
    [HideInInspector] public Player currentPlayer;
    public static Manager instance;

    [HideInInspector] public Canvas canvas;
    [HideInInspector] public TMP_Text instructions;

    [HideInInspector] public List<Player> playerOrderGame = new List<Player>();
    [HideInInspector] public List<Photon.Realtime.Player> playerOrderPhoton = new List<Photon.Realtime.Player>();

    [HideInInspector] public float opacity = 1;
    [HideInInspector] public bool decrease = true;
    [HideInInspector] public bool gameon = false;

    [HideInInspector] public const byte AddNextPlayerEvent = 1;
    [HideInInspector] public const byte GameOverEvent = 4;

    public Button leaveRoom;

    readonly public List<TileData> listofTiles = new List<TileData>();
    public List<Pawn> whitePawns = new List<Pawn>();
    public List<Pawn> blackPawns = new List<Pawn>();
    public List<Pawn> bluePawns = new List<Pawn>();
    public List<Pawn> redPawns = new List<Pawn>();
    Transform gameboard;

    private void FixedUpdate()
    {
        if (decrease)
            opacity -= 0.05f;
        else
            opacity += 0.05f;
        if (opacity < 0 || opacity > 1)
            decrease = !decrease;
    }

    void Awake()
    {
        instance = this;
        canvas = GameObject.Find("Canvas").GetComponent<Canvas>();
        instructions = GameObject.Find("Instructions").GetComponent<TMP_Text>();
        gameboard = GameObject.Find("Battlefield").transform;
    }

    void Start()
    {
        leaveRoom.onClick.AddListener(Quit);
        leaveRoom.gameObject.SetActive(false);

        for (int i = 0; i < 16; i++)
        {
            for (int j = 0; j < 16; j++)
            {
                TileData nextTile = Instantiate(tileClone, gameboard);
                listofTiles.Add(nextTile);
                nextTile.row = i;
                nextTile.column = j;
                nextTile.name = $"{i}:{j}";
                nextTile.position = i+j;
            }
        }

        for (int i = 0; i < listofTiles.Count; i++)
        {
            TileData x = listofTiles[i];
            x.up = GetPosition(x.row - 1, x.column);
            x.left = GetPosition(x.row, x.column - 1);
            x.down = GetPosition(x.row + 1, x.column);
            x.right = GetPosition(x.row, x.column + 1);

            x.upLeft = GetPosition(x.row - 1, x.column - 1);
            x.upRight = GetPosition(x.row - 1, x.column + 1);
            x.downLeft = GetPosition(x.row + 1, x.column - 1);
            x.downRight = GetPosition(x.row + 1, x.column + 1);
        }

        whitePawns[0].NewPosition(CalculatePosition(0, 0));
        whitePawns[1].NewPosition(CalculatePosition(2, 2));
        whitePawns[2].NewPosition(CalculatePosition(3, 12));
        whitePawns[3].NewPosition(CalculatePosition(0, 13));
        whitePawns[4].NewPosition(CalculatePosition(2, 15));

        blackPawns[0].NewPosition(CalculatePosition(0, 2));
        blackPawns[1].NewPosition(CalculatePosition(2, 0));
        blackPawns[2].NewPosition(CalculatePosition(3, 3));
        blackPawns[3].NewPosition(CalculatePosition(2, 13));
        blackPawns[4].NewPosition(CalculatePosition(0, 15));

        bluePawns[0].NewPosition(CalculatePosition(15, 0));
        bluePawns[1].NewPosition(CalculatePosition(13, 2));
        bluePawns[2].NewPosition(CalculatePosition(12, 12));
        bluePawns[3].NewPosition(CalculatePosition(13, 15));
        bluePawns[4].NewPosition(CalculatePosition(15, 13));

        redPawns[0].NewPosition(CalculatePosition(13, 0));
        redPawns[1].NewPosition(CalculatePosition(15, 2));
        redPawns[2].NewPosition(CalculatePosition(12, 3));
        redPawns[3].NewPosition(CalculatePosition(13, 13));
        redPawns[4].NewPosition(CalculatePosition(15, 15));

        StartCoroutine(WaitForPlayer());
    }

    public int CalculatePosition (int r, int c)
    {
        return (r * 16) + c;
    }

    public TileData GetPosition(int r, int c)
    {
        if (r < 0 || r > 15 || c < 0 || c > 15)
            return null;
        else
            return listofTiles[r * 16 + (c)];
    }

    IEnumerator WaitForPlayer()
    {
        Transform x = GameObject.Find("Store Players").transform;
        while (x.childCount < PhotonNetwork.CurrentRoom.MaxPlayers)
        {
            instructions.text = $"Waiting for more players...({PhotonNetwork.PlayerList.Length}/{PhotonNetwork.CurrentRoom.MaxPlayers})";
            yield return null;
        }

        instructions.text = "Everyone's in! Setting up...";

        yield return new WaitForSeconds(0.5f);

        if (PhotonNetwork.IsMasterClient)
        {
            yield return PlayGame();
        }
    }

    IEnumerator GetPlayers()
    {
        List<Photon.Realtime.Player> playerAssignment = new List<Photon.Realtime.Player>();
        for (int i = 0; i < PhotonNetwork.PlayerList.Length; i++)
            playerAssignment.Add(PhotonNetwork.PlayerList[i]);

        for (int i = 0; i < PhotonNetwork.PlayerList.Length; i++)
        {
            object[] sendingdata = new object[2];
            sendingdata[0] = i;

            int randomremove = UnityEngine.Random.Range(0, playerAssignment.Count);
            sendingdata[1] = playerAssignment[randomremove];
            playerAssignment.RemoveAt(randomremove);

            RaiseEventOptions raiseEventOptions = new RaiseEventOptions
            { Receivers = ReceiverGroup.All };
            PhotonNetwork.RaiseEvent(AddNextPlayerEvent, sendingdata, raiseEventOptions, SendOptions.SendReliable);
        }

        yield return new WaitForSeconds(0.5f);
    }

    IEnumerator PlayGame()
    {
        yield return GetPlayers();

        yield return new WaitForSeconds(1f);
        gameon = true;

        while (gameon)
        {
            yield return new WaitForSeconds(0.5f);
            for (int i = 0; i < playerOrderGame.Count; i++)
            {
                yield return playerOrderGame[0].TakeTurnRPC("White");
                yield return new WaitForSeconds(0.5f);

                yield return playerOrderGame[1].TakeTurnRPC("Blue");
                yield return new WaitForSeconds(0.5f);

                yield return playerOrderGame[0].TakeTurnRPC("Black");
                yield return new WaitForSeconds(0.5f);

                yield return playerOrderGame[1].TakeTurnRPC("Red");
                yield return new WaitForSeconds(0.5f);
            }
        }

        GameOver("The game has ended.", -1);
    }

    public void GameOver(string endText, int resignPosition)
    {
        Debug.Log($"{endText}, {resignPosition}");
        object[] sendingdata = new object[2];
        sendingdata[0] = endText;
        sendingdata[1] = resignPosition;
        RaiseEventOptions raiseEventOptions = new RaiseEventOptions
        { Receivers = ReceiverGroup.All };
        PhotonNetwork.RaiseEvent(GameOverEvent, sendingdata, raiseEventOptions, SendOptions.SendReliable);
    }

    public void OnEvent(EventData photonEvent)
    {
        if (photonEvent.Code == AddNextPlayerEvent)
        {
            object[] data = (object[])photonEvent.CustomData;
            int playerposition = (int)data[0];
            Photon.Realtime.Player playername = (Photon.Realtime.Player)data[1];

            playerOrderGame.Add(GameObject.Find(playername.NickName).GetComponent<Player>());
            playerOrderPhoton.Add(playername);

        }
        else if (photonEvent.Code == GameOverEvent)
        {
            object[] data = (object[])photonEvent.CustomData;
            string endgame = (string)data[0];
            leaveRoom.gameObject.SetActive(true);
            instructions.text = endgame;
        }
    }

    private void OnEnable()
    {
        PhotonNetwork.NetworkingClient.EventReceived += OnEvent;
    }

    private void OnDisable()
    {
        PhotonNetwork.NetworkingClient.EventReceived -= OnEvent;
    }

    public void Quit()
    {
        PhotonNetwork.LeaveRoom();
        SceneManager.LoadScene("1. Lobby");
    }
}
