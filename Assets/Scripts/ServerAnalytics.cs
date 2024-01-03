using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Photon.Pun;

public class ServerAnalytics : MonoBehaviour
{
    TMP_Text datatext;
    int lastframe = 0;
    int lastupdate = 60;
    float[] framearray = new float[60];

    private void Awake()
    {
        datatext = GameObject.Find("Server Data").GetComponent<TMP_Text>();
    }

    void Update()
    {
        datatext.text =
            $"Frames/Second: {CalculateFrames()}";

        if (PhotonNetwork.InRoom)
        {
            datatext.text +=
                $"\nServer Ping: {PhotonNetwork.GetPing()}" +
                $"\nRoom: {PhotonNetwork.CurrentRoom.Name}" +
                $"\nHost: {PhotonNetwork.MasterClient.NickName}";
        }

    }

    int CalculateFrames()
    {
        framearray[lastframe] = Time.deltaTime;
        lastframe++;
        if (lastframe == 60)
        {
            lastframe = 0;
            float total = 0;
            for (int i = 0; i < framearray.Length; i++)
                total += framearray[i];
            lastupdate = (int)(framearray.Length / total);
            return lastupdate;
        }
        return (lastupdate > 60) ? 60 : lastupdate;
    }
}
