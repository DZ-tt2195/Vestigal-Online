using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class Flag : MonoBehaviour
{
    public enum FlagColor { White, Blue, Black, Red };
    public FlagColor myColor;

    [HideInInspector] public PhotonView pv;
    [HideInInspector] public TileData currenttile;
    [HideInInspector] public TileData originalTile;

    private void Awake()
    {
        pv = GetComponent<PhotonView>();
    }

    [PunRPC]
    public void NewPosition(int position)
    {
        if (currenttile != null)
            currenttile.flagHere = null;

        TileData newTile = Manager.instance.listofTiles[position];
        if (originalTile == null)
            originalTile = newTile;
        newTile.flagHere = this;

        this.transform.SetParent(newTile.transform);
        this.transform.SetSiblingIndex(1);
        currenttile = newTile;

        this.transform.localScale = new Vector2(0.3f, 0.3f);
        this.GetComponent<RectTransform>().localPosition = new Vector2(20, 20);
        this.GetComponent<RectTransform>().anchoredPosition = new Vector2(20, 20);
    }

    public void Death()
    {
        NewPosition(originalTile.position);
    }
}
