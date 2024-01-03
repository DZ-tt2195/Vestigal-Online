using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using TMPro;

public class SpawnPlayer : MonoBehaviour
{
    [SerializeField] GameObject playerprefab;
    PhotonView pv;

    void Awake()
    {
        if (PhotonNetwork.IsConnected)
        {
            GameObject x = PhotonNetwork.Instantiate(playerprefab.name, new Vector2(0, 0), Quaternion.identity);
            pv = x.GetComponent<PhotonView>();
            pv.Owner.NickName = PlayerPrefs.GetString("Username");
            x.name = pv.Owner.NickName;
        }
    }
}
