using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;

public class TileData : MonoBehaviour
{
    [HideInInspector] public Pawn pawnHere;
    [HideInInspector] public Button button;
    [HideInInspector] Image border;
    bool enableBorder = false;
    [HideInInspector] public Player choosingplayer;

    [HideInInspector] public int row;
    [HideInInspector] public int column;
    [HideInInspector] public int position;

    [HideInInspector] public TileData up;
    [HideInInspector] public TileData upLeft;
    [HideInInspector] public TileData upRight;
    [HideInInspector] public TileData left;
    [HideInInspector] public TileData down;
    [HideInInspector] public TileData downLeft;
    [HideInInspector] public TileData downRight;
    [HideInInspector] public TileData right;

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
            border.color = new Color(1, 1, 1, Manager.instance.opacity);
        }
        else
        {
            border.color = new Color(1, 1, 1, 0);
        }
    }

    public void DestroyPawnOnThis()
    {
        if (pawnHere != null)
        {
            PhotonNetwork.Destroy(pawnHere.pv);
            Debug.Log($"{pawnHere.name} is dead");
        }
    }

    public void EnableButton(Player player)
    {
        choosingplayer = player;
        enableBorder = true;
        button.interactable = true;
    }

    public void DisableButton()
    {
        button.interactable = false;
        enableBorder = false;
    }

    public void SendName()
    {
        choosingplayer.chosenTile = this;
    }
}
