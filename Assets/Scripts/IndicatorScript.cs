using System.Collections.Generic;
using UnityEngine.UI;
using TMPro;
using UnityEngine;
using Photon.Pun;
using MyBox;

[System.Serializable]
public class Indicator
{
    public Image border;
    public TMP_Text numPawns;
    public TMP_Text playerText;
}

public class IndicatorScript : MonoBehaviour
{
    [ReadOnly] PhotonView pv;
    Image toBlink;

    public static IndicatorScript instance;
    [SerializeField] List<Indicator> allIndicators = new List<Indicator>();

    private void Awake()
    {
        pv = GetComponent<PhotonView>();
        instance = this;
    }

    private void FixedUpdate()
    {
        foreach (Indicator indicators in allIndicators)
            indicators.border.SetAlpha(indicators.border == toBlink ? Manager.instance.opacity : 0);

        if (toBlink != null)
            toBlink.SetAlpha(Manager.instance.opacity);

        allIndicators[0].numPawns.text = Manager.instance.whitePawns.Count.ToString();
        allIndicators[1].numPawns.text = Manager.instance.bluePawns.Count.ToString();
        allIndicators[2].numPawns.text = Manager.instance.blackPawns.Count.ToString();
        allIndicators[3].numPawns.text = Manager.instance.redPawns.Count.ToString();
    }

    [PunRPC]
    void ChangeIndicator(int n)
    {
        toBlink = allIndicators[n].border;
    }

    public void ChangeIndicatorRPC(int n)
    {
        if (PhotonNetwork.IsConnected)
            this.pv.RPC("ChangeIndicator", RpcTarget.All, n);
        else
            ChangeIndicator(n);
    }

    public void AssignPlayerName(int n, string name)
    {
        allIndicators[n].playerText.text = name;
        allIndicators[n+2].playerText.text = name;
    }
}
