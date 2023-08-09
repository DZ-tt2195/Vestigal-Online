using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;

public class Pawn : MonoBehaviour
{
    public enum PawnColor { White, Blue, Black, Red};
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
        Manager.instance.listofTiles[position].DestroyPawnOnThis();
        currenttile = Manager.instance.listofTiles[position];
        currenttile.pawnHere = this;

        this.transform.SetParent(currenttile.transform);
        this.transform.SetSiblingIndex(1);
        this.transform.localScale = new Vector2(0.3f, 0.3f);
        this.GetComponent<RectTransform>().localPosition = new Vector2(0, 0);
        this.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, 0);
    }

    bool IsAlly(TileData tile)
    {
        switch (tile.pawnHere.myColor)
        {
            case PawnColor.White:
                return (this.myColor == PawnColor.White || this.myColor == PawnColor.Black);
            case PawnColor.Black:
                return (this.myColor == PawnColor.White || this.myColor == PawnColor.Black);
            case PawnColor.Blue:
                return (this.myColor == PawnColor.Blue || this.myColor == PawnColor.Red);
            case PawnColor.Red:
                return (this.myColor == PawnColor.Blue || this.myColor == PawnColor.Red);
            default:
                return false;
        }
    }

    public IEnumerator Move()
    {
        yield return null;
    }
}
