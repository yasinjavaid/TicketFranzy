using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Cinemachine;
using Photon.Pun;
using ExitGames.Client.Photon;
using TMPro;



public class SquiggleSecondPlayer : MonoBehaviour
{
    [SerializeField] protected Transform virtualCamerasParent;
    [Header("Balls")]
    [SerializeField] private List<Ball> RedBalls;
    public Ball yellowBall;
    private bool ballActive;

    [Header("Score")]
    [SerializeField] private TextMeshPro scoreText;

    private CinemachineVirtualCamera[] virtualCameras;
    private BallsPos ballsPos = new BallsPos();
    public static Action<int> OnGameEndScoreUpdate;

    private void OnEnable()
    {
        PhotonNetwork.NetworkingClient.EventReceived += NetworkingClientOnEventReceived;
        virtualCameras = virtualCamerasParent.GetComponentsInChildren<CinemachineVirtualCamera>();
        //OnGameEndScoreUpdate += GameEndScoreUpdate;
    }

    private void OnDisable()
    {
        PhotonNetwork.NetworkingClient.EventReceived -= NetworkingClientOnEventReceived;
    }


    private void Start()
    {
        scoreText.text = "";

    }

    #region event methods
    private void NetworkingClientOnEventReceived(EventData obj)
    {
        byte eventCode = obj.Code;
        object[] data = (object[])obj.CustomData;

        switch (eventCode)
        {
            case NetworkManager.BallRespawnEventCode:
                if ((int)data[0] != NetworkManager.Instance.LocalPlayer.ActorNumber)
                {
                    bool setActive = (bool)data[1];
                    bool redBall = (bool)data[2];
                    ballActive = setActive;

                    if (redBall)
                    {
                        setRedBallVisibility(setActive);
                        if (!setActive)
                        {
                            scoreText.text = "";
                        }
                    }
                    else
                    {
                        yellowBall.gameObject.SetActive(setActive);
                    }
                }
                break;
            case NetworkManager.BallPositionEventCode:
                if ((int)data[0] != NetworkManager.Instance.LocalPlayer.ActorNumber)
                {
                    yellowBall.transform.localPosition = (Vector3)data[19];
                    for(int i=0; i < RedBalls.Count; i++)
                    {
                        RedBalls[i].transform.localPosition = (Vector3)data[i + 1];
                    }
                }
                break;
            case NetworkManager.BallScoredEventCode:
                if ((int)data[0] != NetworkManager.Instance.LocalPlayer.ActorNumber)
                {
                    int tickets = (int)data[1];
                    scoreText.text = tickets.ToString();
                    OnGameEndScoreUpdate?.Invoke(tickets);
                }
                break;
            case NetworkManager.CameraPanningEventCode:
                if ((int) data[0] != NetworkManager.Instance.LocalPlayer.ActorNumber)
                {
                    CameraPanEvent(data);
                }
                break;
        }
    }
    #endregion

    #region private methods

    private void setRedBallVisibility(bool setActive)
    {
        foreach (Ball ball in RedBalls)
        {
            ball.gameObject.SetActive(setActive);
        }
    }

    private void setRedBallPositions(List<Vector3> balls)
    {
        for(int i=0; i < RedBalls.Count;i++)
        {
            RedBalls[i].transform.localPosition = balls[i];
        }
    }

    private void CameraPanEvent(object[] data)
    {
        var str = data[1].ToString();
        SetCamera(str);
    }
    private void SetCamera(string cameraName)
    {
        foreach (var vcam in virtualCameras)
            vcam.gameObject.SetActive(vcam.name.StartsWith(cameraName));
    }


    #endregion
}
