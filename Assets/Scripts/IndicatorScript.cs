using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using TMPro;
using UnityEngine;
using Photon.Pun;

public class IndicatorScript : MonoBehaviour
{
    [HideInInspector] public PhotonView pv;
    Image toBlink;

    public static IndicatorScript instance;
    public List<Image> border = new List<Image>();
    public List<TMP_Text> playerText = new List<TMP_Text>();
    public List<TMP_Text> pawnText = new List<TMP_Text>();

    private void Awake()
    {
        pv = GetComponent<PhotonView>();
        instance = this;
    }

    private void FixedUpdate()
    {
        for (int i = 0; i<border.Count; i++)
        {
            if (border[i] == toBlink)
                border[i].color = new Color(0.7f, 0.7f, 0.7f, Manager.instance.opacity);
            else
                border[i].color = new Color(0.7f, 0.7f, 0.7f, 0);
        }

        if (toBlink != null)
        {
            toBlink.color = new Color(1, 1, 1, Manager.instance.opacity);
        }

        pawnText[0].text = Manager.instance.whitePawns.Count.ToString();
        pawnText[1].text = Manager.instance.bluePawns.Count.ToString();
        pawnText[2].text = Manager.instance.blackPawns.Count.ToString();
        pawnText[3].text = Manager.instance.redPawns.Count.ToString();
    }

    [PunRPC]
    public void ChangeIndicator(int n)
    {
        toBlink = border[n];
    }

    public void AssignPlayerName(int n, string name)
    {
        playerText[n].text = name;
        playerText[n+2].text = name;
    }
}
