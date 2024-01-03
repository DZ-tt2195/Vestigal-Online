using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using UnityEngine.UI;
using MyBox;

public class Player : MonoBehaviourPunCallbacks
{
    [ReadOnly] public PhotonView pv;
    [ReadOnly] public int playerposition;
    [ReadOnly] public Photon.Realtime.Player photonplayer;

    Button resign;
    Transform storePlayers;

    [ReadOnly] public bool waiting;
    [ReadOnly] public bool turnon;
    [ReadOnly] public TileData chosenTile;

    private void Awake()
    {
        pv = GetComponent<PhotonView>();
        resign = GameObject.Find("Resign Button").GetComponent<Button>();
        storePlayers = GameObject.Find("Store Players").transform;
    }

    private void Start()
    {
        if (PhotonNetwork.IsConnected)
            this.name = pv.Owner.NickName;
        this.transform.SetParent(storePlayers);
        if (!PhotonNetwork.IsConnected && this.pv.AmController)
            resign.onClick.AddListener(ResignTime);
    }

    void ResignTime()
    {
        if (PhotonNetwork.IsConnected)
        {
            Manager.instance.Finished($"{this.name} has resigned.");
        }
        else
        {
            Manager.instance.Finished($"{Manager.instance.currentPlayer.name} has resigned.");
        }
    }

    public IEnumerator TakeTurnRPC(string color)
    {
        if (PhotonNetwork.IsConnected) pv.RPC("TurnStart", RpcTarget.All); else TurnStart();
        if (PhotonNetwork.IsConnected) pv.RPC("TakeTurn", pv.Controller, color); else yield return TakeTurn(color);

        while (turnon)
            yield return null;
    }

    [PunRPC]
    void TurnStart()
    {
        turnon = true;
        Manager.instance.currentPlayer = this;
    }

    [PunRPC]
    void TurnOver()
    {
        turnon = false;
    }

    [PunRPC]
    IEnumerator WaitForPlayer(string playername)
    {
        waiting = true;
        Manager.instance.instructions.text = $"Waiting for {playername}...";
        while (waiting)
            yield return null;
    }

    [PunRPC]
    IEnumerator TakeTurn(string color)
    {
        if (PhotonNetwork.IsConnected) pv.RPC("WaitForPlayer", RpcTarget.Others, this.name);
        yield return MovePawn(color);
        if (PhotonNetwork.IsConnected) photonView.RPC("TurnOver", RpcTarget.All); else TurnOver();
    }

    IEnumerator MovePawn(string color)
    {
        Manager.instance.instructions.text = $"{this.name}: Choose a {color} pawn to move.";

        List<Pawn> availablePawns = new List<Pawn>();
        switch (color)
        {
            case "White":
                availablePawns = (Manager.instance.whitePawns);
                break;
            case "Blue":
                availablePawns = (Manager.instance.bluePawns);
                break;
            case "Black":
                availablePawns = (Manager.instance.blackPawns);
                break;
            case "Red":
                availablePawns = (Manager.instance.redPawns);
                break;
        }

        if (availablePawns.Count > 0)
        {
            for (int i = 0; i < availablePawns.Count; i++)
                availablePawns[i].currenttile.EnableButton(this);

            chosenTile = null;
            while (chosenTile == null)
                yield return null;

            for (int i = 0; i < availablePawns.Count; i++)
                availablePawns[i].currenttile.DisableButton();

            Pawn chosenPawn = chosenTile.pawnHere;
            yield return chosenPawn.Move(this);

            if (chosenTile == chosenPawn.currenttile)
            {
                yield return MovePawn(color);
            }
            else
            {
                chosenPawn.NewPositionRPC(chosenTile.position);
            }
        }
    }
}
