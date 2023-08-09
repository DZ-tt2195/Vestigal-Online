using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using TMPro;

public class SpawnPlayer : MonoBehaviour
{
    public GameObject playerprefab;
    PhotonView pv;
    GameObject x;

    // Start is called before the first frame update
    void Awake()
    {
        x = PhotonNetwork.Instantiate(playerprefab.name, new Vector2(0, 0), Quaternion.identity);

        pv = x.GetComponent<PhotonView>();
        pv.Owner.NickName = PlayerPrefs.GetString("Username");
    }
 
}
