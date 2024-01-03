using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using MyBox;
using Photon.Pun;

public class TileData : MonoBehaviour
{
    [ReadOnly] public Pawn pawnHere;
    [ReadOnly] public Flag flagHere;

    [ReadOnly] public Button button;
    [ReadOnly] Image border;
    bool enableBorder = false;
    [ReadOnly] public Player choosingplayer;

    [ReadOnly] public int row;
    [ReadOnly] public int column;
    [ReadOnly] public int position;

    [ReadOnly] public TileData up;
    [ReadOnly] public TileData upLeft;
    [ReadOnly] public TileData upRight;
    [ReadOnly] public TileData left;
    [ReadOnly] public TileData down;
    [ReadOnly] public TileData downLeft;
    [ReadOnly] public TileData downRight;
    [ReadOnly] public TileData right;

    private void Awake()
    {
        button = GetComponent<Button>();
        button.onClick.AddListener(SendName);

        border = this.transform.GetChild(0).GetComponent<Image>();
        border.gameObject.SetActive(false);
    }

    private void FixedUpdate()
    {
        if (enableBorder)
        {
            border.SetAlpha( Manager.instance.opacity);
        }
        else
        {
            border.SetAlpha(0);
        }
    }

    public void DestroyPawnOnThis()
    {
        if (pawnHere != null)
        {
            pawnHere.DeathRPC();
        }
    }

    public void EnableButton(Player player)
    {
        choosingplayer = player;
        enableBorder = true;
        border.gameObject.SetActive(true);
        button.interactable = true;
    }

    public void DisableButton()
    {
        button.interactable = false;
        enableBorder = false;
        border.gameObject.SetActive(false);
    }

    public void SendName()
    {
        choosingplayer.chosenTile = this;
    }
}
