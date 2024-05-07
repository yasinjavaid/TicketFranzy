using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using ExitGames.Client.Photon;
using TMPro;
using Photon.Pun;
using UnityEngine;
using DG.Tweening;


public class QuickDropSecondPlayer : MonoBehaviour
{
    [Header("Ball")]
    private int RemainingBalls = 50;
    public int MaxBalls = 50;
    private int BallsUsed= 0;
    private int BallsScored=0;
    public GameObject SpawnedBalls;
    public BallQueue ballQueue;
    public float ballDropTime = 0.2f;

    private int ballsToSpawn;

    [Header("Time")]
    private float RemainingTime = 22;

    [Header("Wheel")]
    public GameObject RotatingDisc;
    public float DiscRotationSpeed = 0;

    [Header("GUI")]
    public TextMeshPro BallsRemainingText;
    public TextMeshPro BallsUsedText;
    public TextMeshPro BallsScoredText;
    public TextMeshPro TimeText;
    public TextMeshPro JackPotScoreText;

    private bool isGameStarted;
    private bool allowStart;
    private Queue<GameObject> readyToLaunchBall;
    private int maxQueue;
    private int Points;



    [SerializeField]
    private QuickDropNetwork Player1Machine;

    //Actions
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

    // Start is called before the first frame update
    void Start()
    {
        maxQueue = ballQueue.BallPos.Count;
        readyToLaunchBall = new Queue<GameObject>();
//        fillLaunchQueue();
        allowStart = true;
        BallsRemainingText.text = RemainingBalls.ToString();
        BallsUsedText.text = BallsUsed.ToString();
        BallsScoredText.text = BallsScored.ToString();
        TimeText.text = RemainingTime.ToString();


    }

    // Update is called once per frame
    void Update()
    {
        if (isGameStarted)
        {
            int TempTime = (int) Player1Machine.RemainingTime;
            TimeText.text = TempTime.ToString();
        }
    }

    private void FixedUpdate()
    {
        //if (allowRotation)
        //{
        //    rotateWheel();
        //}
    }

    private void NetworkingClientOnEventReceived(EventData obj)
    {
        byte eventCode = obj.Code;
        object[] data = (object[])obj.CustomData;

        switch (eventCode)
        {
            case NetworkManager.ScoreDisplayUpdateEventCode:
                if ((int)data[0] != NetworkManager.Instance.LocalPlayer.ActorNumber)
                {
                    RemainingBalls = (int)data[1];
                    BallsUsed = (int)data[2];
                    BallsScored = (int)data[3];
                    RemainingTime = (float)data[4];
                    setUIDisplay();
                }
                break;
            case NetworkManager.WheelRotateEventCode:
                if ((int)data[0] != NetworkManager.Instance.LocalPlayer.ActorNumber)
                {
                    RotatingDisc.transform.rotation = (Quaternion)data[1];
                    //recievedQueue = (Queue<Transform>)data[2];
                    //int index = 0;

                    //foreach(Transform pos in spawnedBallsQueue)
                    //{
                    //    pos.position = recievedQueue.Dequeue().position;
                    //}
                }
                break;
            case NetworkManager.BallCountChangedEvent:
                if ((int)data[0] != NetworkManager.Instance.LocalPlayer.ActorNumber)
                {
                    RemainingBalls = (int)data[1];
                    BallsUsed = (int)data[2];
                    setUIDisplay();
                }
                break;
            case NetworkManager.BallScoredEventCode:
                if ((int)data[0] != NetworkManager.Instance.LocalPlayer.ActorNumber)
                {
                    BallsScored = (int)data[1];
                    setUIDisplay();
                }
                break;
            case NetworkManager.BallShotEventCode:
                if ((int)data[0] != NetworkManager.Instance.LocalPlayer.ActorNumber)
                {
                    RemainingBalls = (int)data[1];
                    BallsUsed = (int)data[2];
                    releaseCurrentBall();
                    RotatingDisc.transform.rotation = (Quaternion)data[3];
//                    spawnedBallsQueue.Enqueue((Transform)data[3]);
                    setUIDisplay();
                }
                break;
            case NetworkManager.BallRespawnEventCode:
                if ((int)data[0] != NetworkManager.Instance.LocalPlayer.ActorNumber)
                {
                    PutBallsBackInQueue();
                }
                break;
            case NetworkManager.GameEndScoreUpdateEventCode:
                if ((int)data[0] != NetworkManager.Instance.LocalPlayer.ActorNumber)
                {
                    isGameStarted = false;
                    int recievedTickets = (int)data[1];
                    if (recievedTickets == 500 && Player1Machine.Tickets == 500)
                    {
                        recievedTickets = 1000;
                        Player1Machine.Points = 1000;
                    }

                    OnGameEndScoreUpdate?.Invoke(recievedTickets);
                }
                break;
            case NetworkManager.StartGameEvent:
                if ((int)data[0] != NetworkManager.Instance.LocalPlayer.ActorNumber)
                {
                    Player1Machine.StartPlaying();
                    ballsToSpawn = MaxBalls;
                    fillLaunchQueue();
                    isGameStarted = true;
                }
                break;

        }

    }

    public void fillLaunchQueue()
    {
        while (readyToLaunchBall.Count < maxQueue)
        {
            GameObject ball = spawnBall();
            if (ball != null)
            {
                readyToLaunchBall.Enqueue(ball);
            }
            else
            {
                break;
            }
        }
    }


    public GameObject spawnBall()
    {
        if (ballsToSpawn > 0 && BallSpawner.sharedInstance.isPoolContain("ball"))
        {
            GameObject ball = BallSpawner.sharedInstance.SpawnFromPool("ball", ballQueue.BallPos[ballQueue.BallPos.Count - 1].position, ballQueue.BallPos[readyToLaunchBall.Count].rotation);
            ball.transform.DOMove(ballQueue.BallPos[readyToLaunchBall.Count].position, ballDropTime);

            if (ball != null)
            {
                ball.transform.parent = SpawnedBalls.transform;
                ball.gameObject.GetComponent<Rigidbody>().isKinematic = true;
                --ballsToSpawn;
                return ball;
                //                        ball.GetComponent<Rigidbody>().AddTorque(GetRandomVector(-1000, 1000));
            }
        }

        return null;

    }

    public void setUIDisplay()
    {
        BallsRemainingText.text = RemainingBalls.ToString();
        BallsUsedText.text = BallsUsed.ToString();
        BallsScoredText.text = BallsScored.ToString();
        TimeText.text = RemainingTime.ToString();


    }


    public void releaseCurrentBall()
    {
        GameObject ball = readyToLaunchBall.Dequeue();
//        spawnedBallsQueue.Enqueue(ball.transform);
        //if (spawnedBallsQueue.Count > Player1Machine.maxReleaseQueueSize)
        //{
        //    spawnedBallsQueue.Dequeue();
        //}

        ball.GetComponent<Rigidbody>().isKinematic = false;
        //        ball.transform.DOMove(ballDropPosition.position, 0.3f);
        
        int curCount = 0;

        foreach (GameObject ballObj in readyToLaunchBall)
        {
            ballObj.transform.DOMove(ballQueue.BallPos[curCount].position, ballDropTime);
            curCount++;
        }
        if (ballsToSpawn > 0)
        {
            readyToLaunchBall.Enqueue(spawnBall());
        }
    }

    public void PutBallsBackInQueue()
    {
        int childCount = SpawnedBalls.transform.childCount;
        for (int i = childCount - 1; i >= 0; i--)
        {
            GameObject ball = SpawnedBalls.GetChild(i);
            ball.SetActive(true);
            ball.GetComponent<Rigidbody>().velocity = Vector3.zero;
            ball.GetComponent<QuickDropBall>().isCounted = false;
            ball.transform.position = Vector3.zero;
            BallSpawner.sharedInstance.ReturnToPool("ball", ball.gameObject);
        }
        readyToLaunchBall.Clear();
//        fillLaunchQueue();
    }

}