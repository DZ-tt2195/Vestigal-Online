using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using MyBox;

public class Flag : MonoBehaviour
{
    public enum FlagColor { White, Blue, Black, Red };
    public FlagColor myColor;

    [ReadOnly] public PhotonView pv;
    [ReadOnly] public TileData currenttile;
    [ReadOnly] public TileData originalTile;

    private void Awake()
    {
        pv = GetComponent<PhotonView>();
    }

    public void NewPositionRPC(int position)
    {
        if (PhotonNetwork.IsConnected)
            this.pv.RPC("MoveFlag", RpcTarget.All, position);
        else
            MoveFlag(position);
    }

    [PunRPC]
    void MoveFlag(int position)
    {
        if (currenttile != null)
            currenttile.flagHere = null;

        TileData newTile = Manager.instance.listOfTiles[position];
        if (originalTile == null)
            originalTile = newTile;
        newTile.flagHere = this;

        this.transform.SetParent(newTile.transform);
        this.transform.SetSiblingIndex(1);
        currenttile = newTile;
        this.transform.localPosition = new Vector3(0, 0, 0);
    }

    public void Death()
    {
        MoveFlag(originalTile.position);
    }
}
