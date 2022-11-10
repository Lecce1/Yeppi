using Photon.Pun;
using Photon.Realtime;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Audio;
using UnityEngine.SceneManagement;
using Random = UnityEngine.Random;
using TMPro;

public class GameManager : MonoBehaviourPunCallbacks
{
    public Material[] Skyboxes;
    public GameObject cameraObject;
    public GameObject runningGame;
    public GameObject cannonGame;
    public GameObject speedGame;
    public GameObject ui;
    public GameObject fade;
    public GameObject[] players;
    public GameObject joystick;
    public GameObject pause;
    public GameObject set;
    public AudioMixer audioMixer;
    public Slider set_Fx;
    public Slider set_Music;
    public GameObject set_Vibration_Yes;
    public GameObject set_Vibration_No;
    public TMP_Text timeText;
    public TMP_Text myScoreText;
    public TMP_Text otherScoreText;
    private float limitTime;
    public bool isRandom = false;
    public bool isStart = false;
    public bool isResult = false;
    private bool isGiveUp = false;
    public bool redReady = false;
    public bool blueReady = false;
    private string lastCanvas;
    public int blueScore = 0;
    public int redScore = 0;
    public int isWin = 0;
    public int random = 0;
    PhotonView PV;
    LobbyManager lobbyManager;

    private string team = null;
    public int blueRound = 0;
    public int redRound = 0;

    void Awake()
    {
        PV = GetComponent<PhotonView>();
        lobbyManager = GameObject.Find("LobbyManager").GetComponent<LobbyManager>();
    }

    void Update()
    {
        if (PhotonNetwork.IsMasterClient && isRandom == false)
        {
            random = Random.Range(1,4);
            PV.RPC("RandomMap", RpcTarget.All, random);
        }

        players = GameObject.FindGameObjectsWithTag("Player");

        foreach (GameObject player in players)
        {
            player.transform.GetComponent<PlayerController>().enabled = true;
            player.transform.parent = GameObject.Find("InGame").transform;
            if (player.name == lobbyManager.nickName)
            {
                team = player.transform.GetComponent<PlayerController>().team;
                cameraObject.GetComponent<CameraController>().player = player;
            }
        }

        if (isStart == true)
        {
            Score();
            PV.RPC("Statue", RpcTarget.All);
            if (PhotonNetwork.IsMasterClient)
            {
                limitTime -= Time.deltaTime;
                PV.RPC("LimitTime", RpcTarget.All, limitTime);
            }
        }

        RenderSettings.skybox.SetFloat("_Rotation", Time.time * 2f);
        LastCanvas();
    }

    [PunRPC]
    void RandomMap(int rand)
    {
        random = rand;

        if (random == 1)
        {
            runningGame.SetActive(true);
            RenderSettings.skybox = Skyboxes[0];
            RenderSettings.skybox.SetFloat("_Rotation", 0);
        }
        else if (random == 2)
        {
            cannonGame.SetActive(true);
            cannonGame.transform.GetComponent<CannonGame>().randGenTrigger = true;
            cannonGame.transform.GetComponent<CannonGame>().lineGenTrigger = true;
            RenderSettings.skybox = Skyboxes[1];
            RenderSettings.skybox.SetFloat("_Rotation", 0);
            limitTime = 60;
        }
        else if (random == 3)
        {
            speedGame.SetActive(true);
            RenderSettings.skybox = Skyboxes[2];
            RenderSettings.skybox.SetFloat("_Rotation", 0);
            limitTime = 30;
        }
        isRandom = true;
        Generate();
    }

    void Generate()
    {
        Vector3 position = Vector3.zero;
        string team = null;

        if (random == 1)
        {
            if (PhotonNetwork.IsMasterClient)
            {
                position = new Vector3(-8, 0, 10);
                team = "Blue";
            }
            else
            {
                position = new Vector3(0, 0, 10);
                team = "Red";
            }
        }
        else if (random == 2)
        {
            if (PhotonNetwork.IsMasterClient)
            {
                position = new Vector3(4, 0, 0);
                team = "Blue";
            }
            else
            {
                position = new Vector3(-4, 0, 0);
                team = "Red";
            }
        }
        else if (random == 3)
        {
            if (PhotonNetwork.IsMasterClient)
            {
                position = new Vector3(4, 0, -485);
                team = "Blue";
            }
            else
            {
                position = new Vector3(-4, 0, -485);
                team = "Red";
            }
        }

        GameObject player = PhotonNetwork.Instantiate("Player", position, Quaternion.identity);
        player.name = lobbyManager.nickName;
        player.transform.GetComponent<PlayerController>().team = team;
        player.transform.parent = GameObject.Find("InGame").transform;
        cameraObject.transform.GetComponent<CameraController>().enabled = true;

        StartCoroutine(Fade());
    }

    IEnumerator Fade()
    {
        ui.SetActive(false);
        fade.SetActive(true);

        GameObject player = GameObject.Find(lobbyManager.nickName);
        player.transform.GetComponent<PlayerController>().enabled = true;
        cameraObject.GetComponent<CameraController>().player = player;

        cameraObject.transform.position = cameraObject.GetComponent<CameraController>().player.transform.position - (Vector3.forward * 10) + (Vector3.up * 17);
        float count = 1;

        while (count > 0)
        {
            count -= 0.3f * Time.deltaTime;
            yield return new WaitForSeconds(0.01f);
            //fade.transform.GetComponent<Image>().color = new Color(0, 0, 0, count);
            if (cameraObject.transform.position.y > 7.3f)
            {
                cameraObject.transform.position = new Vector3(cameraObject.transform.position.x, cameraObject.transform.position.y - (Time.deltaTime * 3.1f), cameraObject.transform.position.z);
            }
            else if (cameraObject.transform.position.y < 7.3f)
            {
                cameraObject.transform.position = new Vector3(cameraObject.transform.position.x, 7.3f, cameraObject.transform.position.z);
            }
        }

        if (random == 1)
        {
            ui.transform.Find("JoyStick").gameObject.SetActive(true);
            ui.transform.Find("Button_Jump").gameObject.SetActive(true);
            ui.transform.Find("Button_Slide").gameObject.SetActive(true);
            ui.transform.Find("Button_Attack").gameObject.SetActive(false);
            ui.transform.Find("Button_Run").gameObject.SetActive(false);
        }
        else if (random == 2)
        {
            ui.transform.Find("JoyStick").gameObject.SetActive(true);
            ui.transform.Find("Button_Jump").gameObject.SetActive(true);
            ui.transform.Find("Button_Slide").gameObject.SetActive(true);
            ui.transform.Find("Button_Attack").gameObject.SetActive(true);
            ui.transform.Find("Button_Run").gameObject.SetActive(false);
        }
        else if (random == 3)
        {
            ui.transform.Find("JoyStick").gameObject.SetActive(false);
            ui.transform.Find("Button_Jump").gameObject.SetActive(false);
            ui.transform.Find("Button_Slide").gameObject.SetActive(false);
            ui.transform.Find("Button_Attack").gameObject.SetActive(false);
            ui.transform.Find("Button_Run").gameObject.SetActive(true);
        }

        fade.SetActive(false);

        if (player.transform.GetComponent<PlayerController>().team == "Blue")
        {
            PV.RPC("BlueReady", RpcTarget.All);
        }
        else if (player.transform.GetComponent<PlayerController>().team == "Red")
        {
            PV.RPC("RedReady", RpcTarget.All);
        }

        bool check = false;

        if (lobbyManager.isVibration == 1)
        {
            check = true;
        }

        while (check == false)
        {
            yield return new WaitForSeconds(0.1f);
            if (blueReady == true && redReady == true)
            {
                check = true;
            }
        }

        isStart = true;
        blueReady = false;
        redReady = false;
        ui.SetActive(true);

        yield return null;
    }

    [PunRPC]
    void BlueReady()
    {
        blueReady = true;
    }

    [PunRPC]
    void RedReady()
    {
        redReady = true;
    }

    [PunRPC]
    void LimitTime(float limit)
    {
        limitTime = limit;
        if(random == 1)
        {
            timeText.text = "∞";
            timeText.fontSize = 100;
        }
        else
        {
            if(limitTime > 0)
            {
                timeText.text = TimeSpan.FromSeconds(limitTime).ToString(@"m\:ss");
                timeText.fontSize = 50;
            }
            else if(limitTime <= 0)
            {
                timeText.text = TimeSpan.FromSeconds(0).ToString(@"m\:ss");
                timeText.fontSize = 50;
            }
        }
    }

    void Score()
    {
        foreach (GameObject player in players)
        {
            if (player.name == lobbyManager.nickName)
            {
                if (player.GetComponent<PlayerController>().team == "Blue")
                {
                    myScoreText.text = blueScore.ToString();
                    otherScoreText.text = redScore.ToString();
                }
                else if (player.GetComponent<PlayerController>().team == "Red")
                {
                    myScoreText.text = redScore.ToString();
                    otherScoreText.text = blueScore.ToString();
                }
            }
        }
    }

    [PunRPC]
    void Statue()
    {
        if(isResult == false)
        {
            foreach (GameObject player in players)
            {
                if (player.transform.position.y < -10)
                {
                    if (player.GetComponent<PlayerController>().team == "Blue")
                    {
                        PV.RPC("RedRound", RpcTarget.All);
                    }
                    else if (player.GetComponent<PlayerController>().team == "Red")
                    {
                        PV.RPC("BlueRound", RpcTarget.All);
                    }                    
                    
                    RoundCheck();
                }

                if (cannonGame.activeSelf == true)
                {
                    if (limitTime <= 0)
                    {
                        if (player.name == lobbyManager.nickName)
                        {
                            if (player.GetComponent<PlayerController>().team == "Blue")
                            {
                                if (blueScore > redScore)
                                {
                                    PV.RPC("BlueRound", RpcTarget.All);
                                }
                                else if (blueScore < redScore)
                                {
                                    PV.RPC("RedRound", RpcTarget.All);
                                }
                                else if (blueScore == redScore)
                                {
                                    PV.RPC("BlueRound", RpcTarget.All);
                                    PV.RPC("RedRound", RpcTarget.All);
                                }
                            }
                            else if (player.GetComponent<PlayerController>().team == "Red")
                            {
                                if (redScore > blueScore)
                                {
                                    PV.RPC("RedRound", RpcTarget.All);
                                }
                                else if (redScore < blueScore)
                                {
                                    PV.RPC("BlueRound", RpcTarget.All);
                                }
                                else if (redScore == blueScore)
                                {
                                    PV.RPC("BlueRound", RpcTarget.All);
                                    PV.RPC("RedRound", RpcTarget.All);
                                }
                            }

                            RoundCheck();
                        }
                    }
                }
                else if (speedGame.activeSelf == true)
                {
                    if (player.transform.position.z >= 450)
                    {
                        if (player.GetComponent<PlayerController>().team == "Blue")
                        {
                            PV.RPC("BlueRound", RpcTarget.All);
                        }
                        else if (player.GetComponent<PlayerController>().team == "Red")
                        {
                            PV.RPC("RedRound", RpcTarget.All);
                        }

                        RoundCheck();
                    }

                    if (limitTime <= 0)
                    {
                        if(team == "Blue")
                        {
                            PV.RPC("BlueRound", RpcTarget.All);
                        }
                        else if(team == "Red")
                        {
                            PV.RPC("RedRound", RpcTarget.All);
                        }

                        RoundCheck();
                    }
                }
            }
        }
    }

    [PunRPC]
    void BlueRound()
    {
        blueRound++;
    }

    [PunRPC]
    void RedRound()
    {
        redRound++;
    }

    void RoundCheck()
    {
        isResult = true;

        if(team == "Blue")
        {
            PV.RPC("BlueReady", RpcTarget.All);
        }
        else if(team == "Red")
        {
            PV.RPC("RedReady", RpcTarget.All);
        }

        if(blueRound < 3 && redRound < 3)
        {
            if(PhotonNetwork.IsMasterClient)
            {
                StartCoroutine(RoundReady());
            }
        }
        else
        {
            StartCoroutine(RoundFinish());
        }
    }

    IEnumerator RoundReady()
    {
        bool check = false;

        if (lobbyManager.isVibration == 1)
        {
            check = true;
        }

        while (check == false)
        {
            yield return new WaitForSeconds(0.1f);
            if (blueReady == true && redReady == true)
            {
                check = true;
            }
        }

        PV.RPC("RoundReadyRpc", RpcTarget.All);
    }

    [PunRPC]
    void RoundReadyRpc()
    {
        isRandom = false;
        isResult = false;
        Reset();
    }

    IEnumerator RoundFinish()
    {
        bool check = false;

        if (lobbyManager.isVibration == 1)
        {
            check = true;
        }

        while (check == false)
        {
            yield return new WaitForSeconds(0.1f);
            if (blueReady == true && redReady == true)
            {
                check = true;
            }
        }

        if(blueRound == 3)
        {
            if(team == "Blue")
            {
                isWin = 1;
            }
            else if(team == "Red")
            {
                isWin = 0;
            }
        }
        else if(redRound == 3)
        {                
            if(team == "Blue")
            {
                isWin = 0;
            }
            else if(team == "Red")                
            {
               isWin = 1;
            }
        }

        lobbyManager.LobbyResult();
        Reset();
        gameObject.SetActive(false);            
        isResult = false;
        isRandom = false;
        blueRound = 0;            
        redRound = 0;
    }

    public void Reset()
    {
        var child = transform.GetComponents<Transform>();

        if (random == 1)
        {
            child = runningGame.transform.GetChild(0).GetComponentsInChildren<Transform>();
            runningGame.SetActive(false);
            runningGame.GetComponent<RunningGame>().isChariotSpawnerOn = false;
            runningGame.GetComponent<RunningGame>().isFirstTrackCreated = false;
        }
        else if (random == 2)
        {
            child = cannonGame.transform.GetChild(1).GetComponentsInChildren<Transform>();
            cannonGame.SetActive(false);
            cannonGame.GetComponent<CannonGame>().isDiamond = false;
        }
        else if (random == 3)
        {
            speedGame.SetActive(false);
        }

        foreach (var item in child)
        {
            if (random == 1 && item.name != "Maps")
            {
                Destroy(item.gameObject);
            }
            else if (random == 2 && item.name != "Cannons")
            {
                Destroy(item.gameObject);
            }
        }

        foreach (GameObject player in players)
        {
            Destroy(player);
        }

        pause.SetActive(false);
        set.SetActive(false);
        cameraObject.transform.GetComponent<CameraController>().enabled = false;
        blueScore = 0;
        redScore = 0;
        isWin = 0;
        isStart = false;
        isGiveUp = false;
        redReady = false;
        blueReady = false;
        joystick.GetComponent<JoyStick>().Reset();
    }

    public void Pause()
    {
        pause.SetActive(true);
    }

    public void Set()
    {
        set_Fx.value = lobbyManager.fxValue;
        set_Music.value = lobbyManager.musicValue;
        if (lobbyManager.isVibration == 0)
        {
            set_Vibration_Yes.SetActive(false);
            set_Vibration_No.SetActive(true);
        }
        else if (lobbyManager.isVibration == 1)
        {
            set_Vibration_Yes.SetActive(true);
            set_Vibration_No.SetActive(false);
        }
        set.SetActive(true);
    }

    public void Set_Fx()
    {
        audioMixer.SetFloat("FX", Mathf.Log10(set_Fx.value) * 20);
        PlayerPrefs.SetFloat("fxValue", set_Fx.value);
        lobbyManager.fxValue = PlayerPrefs.GetFloat("fxValue");
    }

    public void Set_Music()
    {
        audioMixer.SetFloat("Music", Mathf.Log10(set_Music.value) * 20);
        PlayerPrefs.SetFloat("musicValue", set_Music.value);
        lobbyManager.musicValue = PlayerPrefs.GetFloat("musicValue");
    }

    public void Set_Vibration()
    {
        if (lobbyManager.isVibration == 0)
        {
            set_Vibration_Yes.SetActive(true);
            set_Vibration_No.SetActive(false);
            lobbyManager.isVibration = 1;
            PlayerPrefs.SetInt("isVibration", lobbyManager.isVibration);
            lobbyManager.isVibration = PlayerPrefs.GetInt("isVibration");
        }
        else if (lobbyManager.isVibration == 1)
        {
            set_Vibration_Yes.SetActive(false);
            set_Vibration_No.SetActive(true);
            lobbyManager.isVibration = 0;
            PlayerPrefs.SetInt("isVibration", lobbyManager.isVibration);
            lobbyManager.isVibration = PlayerPrefs.GetInt("isVibration");
        }
    }

    public void GiveUp()
    {
        isGiveUp = true;
        PV.RPC("GiveUp_Rpc", RpcTarget.All);
    }

    [PunRPC]
    void GiveUp_Rpc()
    {
        foreach (GameObject player in players)
        {
            if (isGiveUp == true)
            {
                isWin = 0;
            }
            else if (isGiveUp == false)
            {
                isWin = 1;
            }

            lobbyManager.LobbyResult();
            Reset();
            gameObject.SetActive(false);
            isResult = false;
            isRandom = false;
            blueRound = 0;
            redRound = 0;
        }
    }

    public void Back()
    {
        if (lastCanvas == "Pause")
        {
            pause.SetActive(false);
        }
        else if (lastCanvas == "Set")
        {
            set.SetActive(false);
        }
    }

    void LastCanvas()
    {
        if (pause.activeSelf == true)
        {
            if (set.activeSelf == true)
            {
                lastCanvas = "Set";
            }
            else
            {
                lastCanvas = "Pause";
            }
        }
    }
}
