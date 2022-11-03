using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

public class RunningGame : MonoBehaviourPunCallbacks
{
    GameManager gameManager;
    GameObject maps;
    GameObject frontLineBoard;
    Vector3 preBoardPos;
    List<GameObject> TrackList;
    public bool isFirstTrackCreated = false;
    public bool isChariotSpawnerOn = false;

    public GameObject chariot;
    private GameObject chariotObj;
    float chariotSpeed = 1f;
    const int chariotGenTime = 3;

    const int trackNum = 10;
    const int trackLength = 16;
    const int rTrackPatternCount = 6;
    private int[] randNumArray;
    public GameObject firstTrack;
    public GameObject endTrack;
    public GameObject runningTrack;

    PhotonView PV;


    private void Awake()
    {
        TrackList = new List<GameObject>();
        gameManager = GameObject.Find("GameManager").GetComponent<GameManager>();
        maps = transform.Find("Maps").gameObject;
        PV = GetComponent<PhotonView>();
    }
    void Update()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            if (gameManager.isStart == true)
            {
                //ChariotSpawner();
                ChariotAcceleration();
                RemovePastTrack();
            }
            FirstTrackSet();
            TrackGenerator();
        }        
    }

    private void ChariotSpawner()
    {
        if(isChariotSpawnerOn == false)
        {
            isChariotSpawnerOn = true;
            StartCoroutine(ChariotSpawn());
        }
    }

    void TrackGenerator()
    {
        foreach (var player in gameManager.players)
        {
            if (TrackList[TrackList.Count - 1].transform.position.z - player.transform.position.z < 60f)
            {
                int randPatternNum = Random.Range(0, rTrackPatternCount);
                preBoardPos += new Vector3(0f, 0f, 20f);
                PV.RPC(nameof(RunningTrackCreateSync), RpcTarget.All, preBoardPos, randPatternNum);
            }
        }
    }

    void FirstTrackSet()
    {
        if(isFirstTrackCreated == false)
        {
            isFirstTrackCreated = true;
            PV.RPC(nameof(FirstTrackCreateSync), RpcTarget.All);
        }
    }

    [PunRPC]
    void FirstTrackCreateSync()
    {
        GameObject fT = Instantiate(firstTrack);
        TrackList.Add(fT);
        fT.transform.position = Vector3.zero;
        fT.transform.parent = maps.transform;
        preBoardPos = Vector3.zero;
    }

    [PunRPC]
    void RunningTrackCreateSync(Vector3 initPos, int randNum)
    {
        GameObject obj = Instantiate(runningTrack, maps.transform);
        TrackList.Add(obj);
        obj.transform.position = initPos;
        obj.transform.Find("ObstaclePattern").GetChild(randNum).gameObject.SetActive(true);
    }
    IEnumerator ChariotSpawn()
    {
        float timer = 0;
        while(timer < chariotGenTime)
        {
            yield return new WaitForEndOfFrame();
            timer += Time.deltaTime;
        }
        PV.RPC(nameof(ChariotCreateSync), RpcTarget.All);
        yield return null;
    }
    [PunRPC]
    void ChariotCreateSync()
    {
        GameObject obj = Instantiate(chariot, maps.transform);
        obj.transform.position = new Vector3(-4f, 0f, 0f);
        obj.transform.localScale = new Vector3(4f, 2.5f, 2.5f);
        chariotObj = obj;
    }
    
    void ChariotAcceleration()
    {
        if(chariotObj != null)
        {
            chariotObj.transform.Translate(Vector3.forward * chariotSpeed * Time.deltaTime);
            chariotSpeed *= 1.001f;
        }
    }

    void RemovePastTrack()
    {
        if (chariotObj != null)
        {
            foreach (var track in TrackList)
            {
                if (chariotObj.transform.position.z > track.transform.position.z + 20)
                {
                    TrackList.Remove(track);
                    Destroy(track.gameObject);
                }
            }
        }
    }
}