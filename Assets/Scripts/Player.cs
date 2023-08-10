using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using TMPro;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using ExitGames.Client.Photon;
using Photon.Realtime;
using System.Linq;

public class Player : MonoBehaviourPunCallbacks
{
    [HideInInspector] public PhotonView pv;
    [HideInInspector] public int playerposition;
    [HideInInspector] public Photon.Realtime.Player photonplayer;

    Button resign;
    Transform storePlayers;

    public bool waiting;
    public bool turnon;
    [HideInInspector] public TileData chosenTile;

    private void Awake()
    {
        pv = GetComponent<PhotonView>();
        resign = GameObject.Find("Resign Button").GetComponent<Button>();
        storePlayers = GameObject.Find("Store Players").transform;
    }

    private void Start()
    {
        this.name = pv.Owner.NickName;
        this.transform.SetParent(storePlayers);
    }

    public IEnumerator TakeTurnRPC(string color)
    {
        pv.RPC("TakeTurn", pv.Controller, color);
        pv.RPC("TurnStart", RpcTarget.All);
        while (turnon)
            yield return null;
    }

    [PunRPC]
    public void TurnStart()
    {
        turnon = true;
        Manager.instance.currentPlayer = this;
    }

    [PunRPC]
    public void TurnOver()
    {
        turnon = false;
    }

    [PunRPC]
    public IEnumerator WaitForPlayer(string playername)
    {
        waiting = true;
        Manager.instance.instructions.text = $"Waiting for {playername}...";
        while (waiting)
            yield return null;
    }

    [PunRPC]
    public IEnumerator TakeTurn(string color)
    {
        yield return null;
        if (pv.IsMine)
        {
            Debug.Log($"{this.name} takes their turn");
            pv.RPC("TurnStart", RpcTarget.All);
            pv.RPC("WaitForPlayer", RpcTarget.Others, this.name);

            switch (color)
            {
                case "White":
                    yield return MovePawn(Manager.instance.whitePawns);
                    break;
                case "Blue":
                    yield return MovePawn(Manager.instance.bluePawns);
                    break;
                case "Black":
                    yield return MovePawn(Manager.instance.blackPawns);
                    break;
                case "Red":
                    yield return MovePawn(Manager.instance.redPawns);
                    break;
            }

            photonView.RPC("TurnOver", RpcTarget.All);
        }
    }

    IEnumerator MovePawn(List<Pawn> availablePawns)
    {
        if (availablePawns.Count > 0)
        {
            Manager.instance.instructions.text = "Choose a pawn to move.";
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
                yield return MovePawn(availablePawns);
            }
            else
            {
                chosenPawn.pv.RPC("NewPosition", RpcTarget.All, chosenTile.position);
            }
        }    
    }
}
