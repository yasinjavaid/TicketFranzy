using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using Cinemachine;
using ExitGames.Client.Photon;
using TMPro;
using Photon.Pun;
using UnityEngine;

public class GhostFollow : MonoBehaviour
{
    
    #region serializeFields
    [SerializeField] private Ball[] ball;
    [SerializeField] private float ballTranslateSpeed = 10;
    [SerializeField] private Transform ballSpwanPoint;
    [SerializeField] private TextMeshPro scoreText;
    [SerializeField] protected Transform virtualCamerasParent;
    #endregion

    #region public fields

    public static Action<int> OnGameEndScoreUpdate;
   

    #endregion
    #region private fields
    private CinemachineVirtualCamera[] virtualCameras;
    private Vector3[] playerPos = new []{Vector3.zero, Vector3.zero, Vector3.zero, Vector3.zero, Vector3.zero};
    private Vector3 ballStartOffset;
    private bool isClientOwner = true;
    private  Vector3 updatedPos = Vector3.zero;
    private bool isBalShot = false;
    private Quaternion playerRotation;
    private float mAngle;
    private float mDistance;
    private int currentBall { get; set; }
    
    #endregion

    #region monobehaviour callbacks

    private void OnEnable()
    {
        PhotonNetwork.NetworkingClient.EventReceived += NetworkingClientOnEventReceived;
        //OnGameEndScoreUpdate += GameEndScoreUpdate;
        currentBall = -1;
        virtualCameras = virtualCamerasParent.GetComponentsInChildren<CinemachineVirtualCamera>();
    }
    public void Start()
    {
        isClientOwner = true;
    }

    private void Update()
    {
        if (isBalShot) return;
        if (isClientOwner)
        {
            
        }
        else
        {
            if (currentBall < ball.Length && currentBall > -1)
            {
                ball[currentBall].transform.position = Vector3.MoveTowards(ball[currentBall].transform.position, playerPos[currentBall], this.mDistance * (1.0f / PhotonNetwork.SerializationRate)); 
                ball[currentBall].transform.rotation = Quaternion.RotateTowards(ball[currentBall].transform.rotation,playerRotation, this.mAngle * (1.0f / PhotonNetwork.SerializationRate));
            }
        }
    }
    
    private void OnDisable()
    {
        PhotonNetwork.NetworkingClient.EventReceived -= NetworkingClientOnEventReceived;
    }

    #endregion

    #region public methods

    #endregion

    #region private methods

    private void SetCamera(string cameraName)
    {
        foreach (var vcam in virtualCameras)
            vcam.gameObject.SetActive(vcam.name.StartsWith(cameraName));
    }


    private void NetworkingClientOnEventReceived(EventData obj)
    {
        byte eventCode = obj.Code;
        if (eventCode == NetworkManager.BallPositionEventCode)
        {
            object[] data = (object[]) obj.CustomData;
            isClientOwner = (int) data[0] == NetworkManager.Instance.LocalPlayer.ActorNumber;
            if (!isClientOwner)
            {
                PlayerPositionUpdate(data);
            }
        }

        if (eventCode == NetworkManager.BallShotEventCode)
        {
            object[] data = (object[]) obj.CustomData;
            if ((int) data[0] != NetworkManager.Instance.LocalPlayer.ActorNumber)
            {
                ShotBall(data);
            }
        }
        else if (eventCode == NetworkManager.BallRespawnEventCode)
        {
            object[] data = (object[]) obj.CustomData;
            if ((int) data[0] != NetworkManager.Instance.LocalPlayer.ActorNumber)
            {
           //     RespawnBall();
            }
        }
        else if (eventCode == NetworkManager.BallScoredEventCode)
        {
            object[] data = (object[]) obj.CustomData;
            if ((int) data[0] != NetworkManager.Instance.LocalPlayer.ActorNumber)
            {
                ScoredUpdate((int) data[1]);
            }
        }
        else if (eventCode == NetworkManager.GameEndScoreUpdateEventCode)
        {
            object[] data = (object[]) obj.CustomData;
            if ((int) data[0] != NetworkManager.Instance.LocalPlayer.ActorNumber)
            {
                OnGameEndScoreUpdate?.Invoke((int) data[1]);
            }
        }
        else if (eventCode == NetworkManager.CameraPanningEventCode)
        {
            object[] data = (object[]) obj.CustomData;
            if ((int) data[0] != NetworkManager.Instance.LocalPlayer.ActorNumber)
            {
                CameraPanEvent(data);
            }
        }
    }
    private void ScoredUpdate(int score)
    {
        scoreText.text = score.ToString();
    }

    private void PlayerPositionUpdate(object[] data)
    {
        currentBall = (int)data[1];
        playerPos[currentBall].x = (float) data[2];
        playerPos[currentBall].y = (float) data[3];
        playerPos[currentBall].z = (float) data[4];
        ball[currentBall].GetRigidbody.isKinematic = true;
        mDistance = Vector3.Distance(ball[currentBall].transform.position,  playerPos[currentBall]);
        isClientOwner = (int) data[0] == NetworkManager.Instance.LocalPlayer.ActorNumber;
    }


    private void ShotBall(object[] data)
    {
        var shotForce = (float) data[2];
        var ballNo = (int) data[3];
        playerPos[currentBall] = (Vector3) data[1];
        playerRotation = (Quaternion) data[4];
        mDistance = Vector3.Distance(ball[ballNo].transform.position,  playerPos[ballNo]);
        mAngle = Quaternion.Angle(ball[ballNo].transform.rotation, playerRotation);
        
        
        ball[ballNo].GetTrailRenderer.enabled = true;
        currentBall = -1;
        ball[ballNo].GetRigidbody.isKinematic = false;
        ball[ballNo].GetRigidbody.AddRelativeForce (ball[ballNo].transform.forward * shotForce, ForceMode.Impulse);
    }
    
    public void RespawnBall()
    {
    
    }

    private void CameraPanEvent(object[] data)
    {
        var str = data[1].ToString();
        SetCamera(str);
    }
    #endregion
}
