using Photon.Pun;
using Photon.Realtime;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviourPunCallbacks, IPunObservable
{
    private float time = 300;
    public Image greenHp;
    public Image redHp;
    public Text timeText;
    private Wagon wagon;
    private PhotonView PV;

    void Awake()
    {
        Application.targetFrameRate = 144;
        wagon = GameObject.Find("Wagon").GetComponent<Wagon>();
        PV = GetComponent<PhotonView>();
    }

    void Start()
    {
        Generate();
    }

    void Update()
    {
        if (PV.IsMine)
        {
            PV.RPC("limitTime", RpcTarget.All);
        }
        Hp();
    }

    void Generate()
    {
        GameObject player = PhotonNetwork.Instantiate("Player", Vector3.zero, Quaternion.identity);
        player.name = PhotonNetwork.LocalPlayer.NickName;
    }

    void Hp()
    {
        greenHp.fillAmount = wagon.greenHp * 0.01f;
        redHp.fillAmount = wagon.redHp * 0.01f;
    }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            stream.SendNext(time);
        }
        else
        {
            time = (float)stream.ReceiveNext();
        }
    }

    [PunRPC]
    void limitTime()
    {
        if (time > 0)
        {
            time -= Time.deltaTime;
            timeText.text = TimeSpan.FromSeconds(time).ToString(@"m\:ss");
        }

        if(time <= 0)
        {
            SceneManager.LoadScene("Lobby");
        }
    }
}
