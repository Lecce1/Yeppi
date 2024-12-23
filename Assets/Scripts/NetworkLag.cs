using UnityEngine;
using Photon.Pun;

public class NetworkLag : MonoBehaviourPun, IPunObservable
{
    //Values that will be synced over network
    Vector3 latestPos;
    Quaternion latestRot;
    //Lag compensation
    float currentTime = 0;
    float t = 0;
    double currentPacketTime = 0;
    double lastPacketTime = 0;
    Vector3 positionAtLastPacket = Vector3.zero;
    Quaternion rotationAtLastPacket = Quaternion.identity;
    Rigidbody rb;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            //We own this player: send the others our data
            stream.SendNext(transform.position);
            stream.SendNext(transform.rotation);
            stream.SendNext(rb.velocity);
        }   
        else if (stream.IsReading)
        {
            //Network player, receive data
            latestPos = (Vector3)stream.ReceiveNext();
            latestRot =                 (Quaternion)stream.ReceiveNext();
            rb.velocity = (Vector3)stream.ReceiveNext();

            //Lag compensation
            currentTime = 0.0f;
            lastPacketTime = currentPacketTime;
            currentPacketTime = info.SentServerTime;
            positionAtLastPacket = transform.position;
            rotationAtLastPacket = transform.rotation;
        }
    }
    // Update is called once per frame
    void Update()
    {
        if (!photonView.IsMine)
        {
            //Lag compensation
            double timeToReachGoal = currentPacketTime - lastPacketTime;
            currentTime += Time.deltaTime;
            t = Mathf.Clamp((float)(currentTime / timeToReachGoal), 0f, 0.999f);

            //Update remote player
            transform.position = Vector3.Lerp(positionAtLastPacket, latestPos, t);
            transform.rotation = Quaternion.Lerp(rotationAtLastPacket, latestRot, t);
        }
    }
}