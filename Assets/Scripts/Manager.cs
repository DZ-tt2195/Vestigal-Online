using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using TMPro;
using Photon.Realtime;
using ExitGames.Client.Photon;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using MyBox;

public class Manager : MonoBehaviour, IOnEventCallback
{

#region Variables

    [ReadOnly] public const byte AddNextPlayerEvent = 1;
    [ReadOnly] public const byte GameOverEvent = 4;
    [ReadOnly] public Player currentPlayer;
    public static Manager instance;

    [Foldout("Prefabs", true)]
        [SerializeField] TileData tileClone;
        [SerializeField] Player playerClone;

    [Foldout("UI", true)]
        Transform gameboard;
        [ReadOnly] public Canvas canvas;
        [ReadOnly] public TMP_Text instructions;
        [SerializeField] Button leaveRoom;


    [Foldout("Animations", true)]
        [ReadOnly] public float opacity = 1;
        [ReadOnly] public bool decrease = true;
        [ReadOnly] public bool gameon = false;

    [Foldout("Lists", true)]
        [ReadOnly] public List<TileData> listOfTiles = new List<TileData>();
        public List<Flag> listOfFlags = new List<Flag>();
        public List<Pawn> whitePawns = new List<Pawn>();
        public List<Pawn> blackPawns = new List<Pawn>();
        public List<Pawn> bluePawns = new List<Pawn>();
        public List<Pawn> redPawns = new List<Pawn>();
        [ReadOnly] public List<Player> playerOrderGame = new List<Player>();
        [ReadOnly] public List<Photon.Realtime.Player> playerOrderPhoton = new List<Photon.Realtime.Player>();

    #endregion

#region Setup

    void Awake()
    {
        instance = this;
        canvas = GameObject.Find("Canvas").GetComponent<Canvas>();
        instructions = GameObject.Find("Instructions").GetComponent<TMP_Text>();
        gameboard = GameObject.Find("Battlefield").transform;
    }

    private void OnEnable()
    {
        PhotonNetwork.NetworkingClient.EventReceived += OnEvent;
    }

    private void OnDisable()
    {
        PhotonNetwork.NetworkingClient.EventReceived -= OnEvent;
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
                listOfTiles.Add(nextTile);
                nextTile.row = i;
                nextTile.column = j;
                nextTile.name = $"{i}:{j}";
                nextTile.position = (i * 16) + j;
            }
        }

        for (int i = 0; i < listOfTiles.Count; i++)
        {
            TileData x = listOfTiles[i];
            x.up = GetPosition(x.row - 1, x.column);
            x.left = GetPosition(x.row, x.column - 1);
            x.down = GetPosition(x.row + 1, x.column);
            x.right = GetPosition(x.row, x.column + 1);

            x.upLeft = GetPosition(x.row - 1, x.column - 1);
            x.upRight = GetPosition(x.row - 1, x.column + 1);
            x.downLeft = GetPosition(x.row + 1, x.column - 1);
            x.downRight = GetPosition(x.row + 1, x.column + 1);
        }

        whitePawns[0].NewPositionRPC(CalculatePosition(0, 0));
        whitePawns[1].NewPositionRPC(CalculatePosition(2, 2));
        whitePawns[2].NewPositionRPC(CalculatePosition(3, 12));
        whitePawns[3].NewPositionRPC(CalculatePosition(0, 13));
        whitePawns[4].NewPositionRPC(CalculatePosition(2, 15));

        blackPawns[0].NewPositionRPC(CalculatePosition(0, 2));
        blackPawns[1].NewPositionRPC(CalculatePosition(2, 0));
        blackPawns[2].NewPositionRPC(CalculatePosition(3, 3));
        blackPawns[3].NewPositionRPC(CalculatePosition(2, 13));
        blackPawns[4].NewPositionRPC(CalculatePosition(0, 15));

        bluePawns[0].NewPositionRPC(CalculatePosition(15, 0));
        bluePawns[1].NewPositionRPC(CalculatePosition(13, 2));
        bluePawns[2].NewPositionRPC(CalculatePosition(12, 12));
        bluePawns[3].NewPositionRPC(CalculatePosition(13, 15));
        bluePawns[4].NewPositionRPC(CalculatePosition(15, 13));

        redPawns[0].NewPositionRPC(CalculatePosition(13, 0));
        redPawns[1].NewPositionRPC(CalculatePosition(15, 2));
        redPawns[2].NewPositionRPC(CalculatePosition(12, 3));
        redPawns[3].NewPositionRPC(CalculatePosition(13, 13));
        redPawns[4].NewPositionRPC(CalculatePosition(15, 15));

        listOfFlags[0].NewPositionRPC(CalculatePosition(8, 4));
        listOfFlags[1].NewPositionRPC(CalculatePosition(7, 4));
        listOfFlags[2].NewPositionRPC(CalculatePosition(8, 11));
        listOfFlags[3].NewPositionRPC(CalculatePosition(7, 11));

        if (PhotonNetwork.IsConnected)
        {
            StartCoroutine(WaitForPlayer());
        }
        else
        {
            for (int i = 0; i < 2; i++)
            {
                Player nextPlayer = Instantiate(playerClone);
                nextPlayer.name = PlayerPrefs.GetString($"P{i + 1}");
            }
            StartCoroutine(PlayGame());
        }
    }

    public int CalculatePosition(int r, int c)
    {
        return (r * 16) + c;
    }

    public TileData GetPosition(int r, int c)
    {
        if (r < 0 || r > 15 || c < 0 || c > 15)
            return null;
        else
            return listOfTiles[r * 16 + (c)];
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
        List<string> playerAssignment = new List<string>();
        List<Photon.Realtime.Player> playerRealtimes = new List<Photon.Realtime.Player>();

        if (PhotonNetwork.IsConnected)
        {
            for (int i = 0; i < 2; i++)
            {
                playerAssignment.Add(PhotonNetwork.PlayerList[i].NickName);
                playerRealtimes.Add(PhotonNetwork.PlayerList[i]);
            }
        }
        else
        {
            playerAssignment.Add(PlayerPrefs.GetString("P1"));
            playerAssignment.Add(PlayerPrefs.GetString("P2"));
        }

        for (int i = 0; i < 2; i++)
        {
            int randomremove = UnityEngine.Random.Range(0, playerAssignment.Count);

            if (PhotonNetwork.IsConnected)
            {
                object[] sendingdata = new object[3];
                sendingdata[0] = i;
                sendingdata[1] = playerAssignment[randomremove];
                sendingdata[2] = playerRealtimes[randomremove];

                RaiseEventOptions raiseEventOptions = new RaiseEventOptions
                { Receivers = ReceiverGroup.All };
                PhotonNetwork.RaiseEvent(AddNextPlayerEvent, sendingdata, raiseEventOptions, SendOptions.SendReliable);
            }
            else
            {
                AddPlayer(i, playerAssignment[randomremove]);
            }

            playerAssignment.RemoveAt(randomremove);
        }

        yield return new WaitForSeconds(0.5f);
    }

    void AddPlayer(int position, string name)
    {
        playerOrderGame.Add(GameObject.Find(name).GetComponent<Player>());
        IndicatorScript.instance.AssignPlayerName(position, name);
    }

    #endregion

#region Gameplay

    private void FixedUpdate()
    {
        if (decrease)
            opacity -= 0.05f;
        else
            opacity += 0.05f;
        if (opacity < 0 || opacity > 1)
            decrease = !decrease;
    }

    IEnumerator PlayGame()
    {
        yield return GetPlayers();
        yield return new WaitForSeconds(0.3f);
        gameon = true;

        while (gameon)
        {
            for (int i = 0; i < playerOrderGame.Count; i++)
            {
                IndicatorScript.instance.ChangeIndicatorRPC(0);
                yield return playerOrderGame[0].TakeTurnRPC("White");
                yield return new WaitForSeconds(0.3f);

                IndicatorScript.instance.ChangeIndicatorRPC(1);
                yield return playerOrderGame[1].TakeTurnRPC("Blue");
                yield return new WaitForSeconds(0.3f);

                IndicatorScript.instance.ChangeIndicatorRPC(2);
                yield return playerOrderGame[0].TakeTurnRPC("Black");
                yield return new WaitForSeconds(0.3f);

                IndicatorScript.instance.ChangeIndicatorRPC(3);
                yield return playerOrderGame[1].TakeTurnRPC("Red");
                yield return new WaitForSeconds(0.3f);
            }
        }
    }

    #endregion

#region End

    void GameOver(string text)
    {
        leaveRoom.gameObject.SetActive(true);
        instructions.text = text;

        StopAllCoroutines();
        for (int i = 0; i < playerOrderGame.Count; i++)
            playerOrderGame[i].StopAllCoroutines();
    }

    public void OnEvent(EventData photonEvent)
    {
        if (photonEvent.Code == AddNextPlayerEvent)
        {
            object[] data = (object[])photonEvent.CustomData;
            int playerposition = (int)data[0];
            string playername = (string)data[1];
            Photon.Realtime.Player playerRealtime = (Photon.Realtime.Player)data[2];

            if (PhotonNetwork.IsConnected)
                playerOrderPhoton.Add(playerRealtime);

            AddPlayer(playerposition, playername);
        }

        else if (photonEvent.Code == GameOverEvent)
        {
            object[] data = (object[])photonEvent.CustomData;
            string endgame = (string)data[0];
            GameOver(endgame);
        }
    }

    public void Finished(string text)
    {
        if (PhotonNetwork.IsConnected)
        {
            object[] lol = new object[1];
            lol[0] = text;
            RaiseEventOptions raiseEventOptions = new RaiseEventOptions
            { Receivers = ReceiverGroup.All };
            PhotonNetwork.RaiseEvent(GameOverEvent, lol, raiseEventOptions, SendOptions.SendReliable);
        }
        else
            GameOver(text);
    }

    public void Quit()
    {
        if (PhotonNetwork.IsConnected)
        {
            PhotonNetwork.LeaveRoom();
            SceneManager.LoadScene("1. Lobby");
        }
        else
        {
            SceneManager.LoadScene("3. Local");
        }
    }

#endregion

}
