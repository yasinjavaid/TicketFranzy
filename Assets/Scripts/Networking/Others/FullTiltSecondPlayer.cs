using UnityEngine;
using TMPro;
using System;
using Photon.Pun;
using ExitGames.Client.Photon;
using System.Collections.Generic;


public class FullTiltSecondPlayer : MonoBehaviour
{
    [Header("Ball")]
    public Ball ball;
    public float ballMaxSpeed = 0.8f;
    private bool isReleased;

    [Header("Points")]
    public GameObject bottomDisplayBar;

    [Header("Gear")]
    public float ToothCount;
    public TextMeshPro topScoreDisplay;
    [SerializeField] public Gear inputGear;
    public Transform Wheel;

    [SerializeField] private List<Gear> BigGear;
    [SerializeField] private List<Gear> SmallGear;
    [SerializeField] private List<Gear> SmallGear2;



    public static Action<int> OnGameEndScoreUpdate;

    private void OnEnable()
    {
        PhotonNetwork.NetworkingClient.EventReceived += NetworkingClientOnEventReceived;
        //OnGameEndScoreUpdate += GameEndScoreUpdate;
    }

    private void OnDisable()
    {
        PhotonNetwork.NetworkingClient.EventReceived -= NetworkingClientOnEventReceived;
    }

    private void FixedUpdate()
    {
    }

    #region event methods
    private void NetworkingClientOnEventReceived(EventData obj)
    {
        byte eventCode = obj.Code;
        object[] data = (object[])obj.CustomData;

        switch (eventCode)
        {
            //case NetworkManager.BallScoredEventCode:
            //    ReleaseBall();
            //    break;
            case NetworkManager.WheelRotateEventCode:
                if ((int)data[0] != NetworkManager.Instance.LocalPlayer.ActorNumber)
                {
                    Wheel.transform.rotation = (Quaternion)data[1];
                    RotateGears(BigGear, (Quaternion)data[2]);
                    RotateGears(SmallGear, (Quaternion)data[3]);
                    RotateGears(SmallGear2, (Quaternion)data[4]);
                }
                break;
            case NetworkManager.ScoreDisplayUpdateEventCode:
                if ((int)data[0] != NetworkManager.Instance.LocalPlayer.ActorNumber)
                {
                    if ((int)data[1] > 0)
                    {
                        int tickets = (int)data[2];
                        OnGameEndScoreUpdate?.Invoke(tickets);
                        topScoreDisplay.text = tickets.ToString();
                        HideDisplayBar((int)data[1]);
                    }
                    else
                    {
                        HideDisplayBar((int)data[1]);
                        topScoreDisplay.text = "";
                    }

                }
                break;
            case NetworkManager.BallPositionEventCode:
                if ((int)data[0] != NetworkManager.Instance.LocalPlayer.ActorNumber)
                {
                    ball.transform.localPosition = (Vector3)data[1];
                }
                break;

                //case NetworkManager.BallRespawnEventCode:
                //    spawnBall();
                //    break;
        }

    }

    #endregion

    #region private methods

    public void HideDisplayBar(int id)
    {
        int curr = 1;
        if(id >= 0)
        {
            foreach (Transform child in bottomDisplayBar.GetChildren())
            {
                if (id != curr)
                    child.SetActive(false);
                ++curr;
            }
        }
        else
        {
            foreach (Transform child in bottomDisplayBar.GetChildren())
            {
                child.SetActive(true);
            }
        }
    }

    private void RotateGears(List<Gear> Gears, Quaternion rotation)
    {
        foreach(Gear obj in Gears)
        {
            obj.transform.rotation = rotation;
        }
    }

    #endregion
}
