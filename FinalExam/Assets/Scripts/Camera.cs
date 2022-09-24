using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class Camera : MonoBehaviour
{
    public GameObject cameraPlayer; //Player스크립트에서 player게임오브젝트를 받아온다.
    private Transform playerTr;
    private float distance = 10;
    private float height = 3;
    private GameObject playerObj;
    private GameManager gameManager;
    private GameObject[] players;
    Vector3 cameraPos;

    void Start()
    {
        gameManager = GameObject.Find("GameManager").GetComponent<GameManager>();
        players = gameManager.players;
    }
    void Update()
    {
        CameraPos();
    }

    void CameraPos()
    {
        players = gameManager.players;
        playerTr = cameraPlayer.transform;
        if (cameraPlayer.active == true)
        {
            cameraPos = playerTr.position - (Vector3.forward * distance) + (Vector3.up * height);
        }
        else
        {
            foreach (var player in players)
            {
                if (player.GetComponent<Player>().isDead != true)
                    cameraPos = player.transform.position - (Vector3.forward * distance) + (Vector3.up * height);
            }
        }
        gameObject.transform.position = cameraPos;
    }
}
