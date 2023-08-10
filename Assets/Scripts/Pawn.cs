using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;

public class Pawn : MonoBehaviour
{
    public enum PawnColor { White, Blue, Black, Red };
    public PawnColor myColor;

    [HideInInspector] public PhotonView pv;
    [HideInInspector] public TileData currenttile;

    private void Awake()
    {
        pv = GetComponent<PhotonView>();
    }

    [PunRPC]
    public void NewPosition(int position)
    {
        if (currenttile != null)
            currenttile.pawnHere = null;

        TileData newTile = Manager.instance.listofTiles[position];
        newTile.DestroyPawnOnThis();
        newTile.pawnHere = this;

        this.transform.SetParent(newTile.transform);
        this.transform.SetSiblingIndex(1);
        currenttile = newTile;

        this.transform.localScale = new Vector2(0.3f, 0.3f);
        this.GetComponent<RectTransform>().localPosition = new Vector2(0, 0);
        this.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, 0);
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
            return nextTile;
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
        List<TileData> possibleTiles = new List<TileData>();
        possibleTiles.Add(currenttile);

        possibleTiles.Add(Adjacent(currenttile.up));
        possibleTiles.Add(Adjacent(currenttile.down));
        possibleTiles.Add(Adjacent(currenttile.left));
        possibleTiles.Add(Adjacent(currenttile.right));
        possibleTiles.Add(Adjacent(currenttile.upLeft));
        possibleTiles.Add(Adjacent(currenttile.upRight));
        possibleTiles.Add(Adjacent(currenttile.downLeft));
        possibleTiles.Add(Adjacent(currenttile.downRight));

        possibleTiles.Add(ScanTiles(0, -1));
        possibleTiles.Add(ScanTiles(0, 1));
        possibleTiles.Add(ScanTiles(-1, 0));
        possibleTiles.Add(ScanTiles(1, 0));
        possibleTiles.Add(ScanTiles(-1, -1));
        possibleTiles.Add(ScanTiles(-1, 1));
        possibleTiles.Add(ScanTiles(1, -1));
        possibleTiles.Add(ScanTiles(1, 1));

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

    [PunRPC]
    public void Death()
    {
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
