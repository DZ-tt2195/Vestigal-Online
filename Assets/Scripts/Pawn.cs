using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using MyBox;

public class Pawn : MonoBehaviour
{
    public enum PawnColor { White, Blue, Black, Red };
    public PawnColor myColor;

    [ReadOnly] PhotonView pv;
    [ReadOnly] public TileData currenttile;
    [ReadOnly] public Flag carryingFlag;

    private void Awake()
    {
        pv = GetComponent<PhotonView>();
    }

    public void NewPositionRPC(int position)
    {
        if (PhotonNetwork.IsConnected)
            this.pv.RPC("NewPosition", RpcTarget.All, position);
        else
            NewPosition(position);
    }

    [PunRPC]
    void NewPosition(int position)
    {
        if (currenttile != null)
            currenttile.pawnHere = null;

        TileData newTile = Manager.instance.listOfTiles[position];
        newTile.DestroyPawnOnThis();
        newTile.pawnHere = this;

        this.transform.SetParent(newTile.transform);
        this.transform.SetSiblingIndex(1);
        currenttile = newTile;
        this.transform.localPosition = new Vector3(0, 0, 0);

        if (newTile.flagHere != null)
        {
            Flag.FlagColor compare = newTile.flagHere.myColor;

            if (myColor == PawnColor.White && (compare == Flag.FlagColor.White || compare == Flag.FlagColor.Black))
                carryingFlag = newTile.flagHere;
            else if (myColor == PawnColor.Black && (compare == Flag.FlagColor.White || compare == Flag.FlagColor.Black))
                carryingFlag = newTile.flagHere;
            else if (myColor == PawnColor.Blue && (compare == Flag.FlagColor.Blue || compare == Flag.FlagColor.Red))
                carryingFlag = newTile.flagHere;
            else if (myColor == PawnColor.Red && (compare == Flag.FlagColor.Blue || compare == Flag.FlagColor.Red))
                carryingFlag = newTile.flagHere;
        }

        if (this.carryingFlag)
            carryingFlag.NewPositionRPC(newTile.position);

        if ((myColor == PawnColor.White || myColor == PawnColor.Black) && carryingFlag != null && currenttile.row == 15)
            Manager.instance.Finished($"{Manager.instance.playerOrderGame[0].name} has won!");
        else if ((myColor == PawnColor.Blue || myColor == PawnColor.Red) && carryingFlag != null && currenttile.row == 0)
            Manager.instance.Finished($"{Manager.instance.playerOrderGame[1].name} has won!");
    }

    TileData Adjacent(TileData tile)
    {
        if (tile == null)
            return null;

        if (tile.pawnHere != null)
        {
            PawnColor compare = tile.pawnHere.myColor;

            if (myColor == PawnColor.White && (compare == PawnColor.White || compare == PawnColor.Black))
                return null;
            else if (myColor == PawnColor.Black && (compare == PawnColor.White || compare == PawnColor.Black))
                return null;
            else if (myColor == PawnColor.Blue && (compare == PawnColor.Blue || compare == PawnColor.Red))
                return null;
            else if (myColor == PawnColor.Red && (compare == PawnColor.Blue || compare == PawnColor.Red))
                return null;
        }
        return tile;
    }

    TileData ScanTiles(int row, int col)
    {
        int distance = 0;
        TileData nextTile = Manager.instance.GetPosition(currenttile.row + row, currenttile.column + col);

        while (true)
        {
            distance++;
            if (nextTile == null)
                return null;
            else if (nextTile.pawnHere == null)
                nextTile = Manager.instance.GetPosition(nextTile.row + row, nextTile.column + col);
            else if (nextTile.pawnHere.myColor == this.myColor)
                return null;
            else
                break;
        }

        do
        {
            nextTile = Manager.instance.GetPosition(nextTile.row + row, nextTile.column + col);
            if (nextTile == null)
                return null;
            distance--;
            if (distance != 0 && nextTile.pawnHere != null)
                return null;
        }
        while (distance > 0);

        if (nextTile.pawnHere == null)
        {
            return nextTile;
        }
        else
        {
            PawnColor compare = nextTile.pawnHere.myColor;

            if (myColor == PawnColor.White && (compare == PawnColor.White || compare == PawnColor.Black))
                return null;
            else if (myColor == PawnColor.Black && (compare == PawnColor.White || compare == PawnColor.Black))
                return null;
            else if (myColor == PawnColor.Blue && (compare == PawnColor.Blue || compare == PawnColor.Red))
                return null;
            else if (myColor == PawnColor.Red && (compare == PawnColor.Blue || compare == PawnColor.Red))
                return null;

            return nextTile;
        }
    }

    public IEnumerator Move(Player currPlayer)
    {
        Manager.instance.instructions.text = "Move the pawn. (To undo, click the pawn itself.)";
        List<TileData> possibleTiles = new()
        {
            currenttile,

            Adjacent(currenttile.up),
            Adjacent(currenttile.down),
            Adjacent(currenttile.left),
            Adjacent(currenttile.right),
            Adjacent(currenttile.upLeft),
            Adjacent(currenttile.upRight),
            Adjacent(currenttile.downLeft),
            Adjacent(currenttile.downRight),

            ScanTiles(0, -1),
            ScanTiles(0, 1),
            ScanTiles(-1, 0),
            ScanTiles(1, 0),
            ScanTiles(-1, -1),
            ScanTiles(-1, 1),
            ScanTiles(1, -1),
            ScanTiles(1, 1)
        };

        for (int i = 0; i < possibleTiles.Count; i++)
            if (possibleTiles[i] != null)
                possibleTiles[i].EnableButton(currPlayer);

        currPlayer.chosenTile = null;
        while (currPlayer.chosenTile == null)
            yield return null;

        for (int i = 0; i < possibleTiles.Count; i++)
            if (possibleTiles[i] != null)
                possibleTiles[i].DisableButton();
    }

    public void DeathRPC()
    {
        if (PhotonNetwork.IsConnected)
            this.pv.RPC("Death", RpcTarget.All);
        else
            Death();
    }

    [PunRPC]
    void Death()
    {
        if (carryingFlag != null)
            carryingFlag.Death();

        switch (myColor)
        {
            case PawnColor.White:
                Manager.instance.whitePawns.Remove(this);
                break;
            case PawnColor.Blue:
                Manager.instance.bluePawns.Remove(this);
                break;
            case PawnColor.Black:
                Manager.instance.blackPawns.Remove(this);
                break;
            case PawnColor.Red:
                Manager.instance.redPawns.Remove(this);
                break;
        }
        Destroy(this.gameObject);
    }
}
