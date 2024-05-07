 //using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using TMPro;
using DebugConsole;
using DG.Tweening;


public class QuickDropManager : ArcadeGame
{
    private Mouse mouse => Mouse.current;
    private Camera cam => Camera.main;


    [Header("Balls")]
    private int RemainingBalls;
    public int MaxBalls = 50;
    private int BallsUsed;
    private int BallsScored;
    public GameObject SpawnedBalls;
    public BallQueue ballQueue;
    public int JackpotScore;
    public float ballDropTime = 0.2f;
    public Transform ballDropPosition;

    private int ballsToSpawn;


    [Header("Time")]
    public float maxTimeInSec = 30;
    private float RemainingTime;

    [Header("Wheel")]
    public GameObject RotatingDisc;
    public float DiscRotationSpeed=0;

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

    public GameObject ButtonCol;

    //public BallSpawner ballPool;

    public override void StartGame()
    {
        maxQueue = ballQueue.BallPos.Count;
        readyToLaunchBall = new Queue<GameObject>();
        allowStart = true;
        JackPotScoreText.text = JackpotScore.ToString();
        base.StartGame();
    }


    public override void Reset()
    {
        base.Reset();
        RemainingBalls = MaxBalls;
//        ballsToSpawn = MaxBalls;
        BallsUsed = 0;
        BallsScored = 0;
        RemainingTime = maxTimeInSec;
        allowStart = true;
    }



    protected override void Awake()
    {
        base.Awake();
        DebugCommand<float> SetWheelTurnSpeed = new DebugCommand<float>("set_wheel_turn_speed", "Set wheel turn speed", "set_wheel_turn_speed<speed>",
        (spd) => { DiscRotationSpeed = spd; });

        DebugCommand<int> SetMaxBalls = new DebugCommand<int>("set_max_balls", "Set max balls", "set_max_balls<balls>",
        (ball) => { MaxBalls = ball; });

        DebugCommand<int> SetMaxTime = new DebugCommand<int>("set_max_time", "Set max time", "set_max_time<time>",
        (time) => { maxTimeInSec = time; });

        DebugController.commandList.Add(SetWheelTurnSpeed);
        DebugController.commandList.Add(SetMaxBalls);
        DebugController.commandList.Add(SetMaxTime);


    }



    protected override void OnEnable()
    {
        base.OnEnable();
        GameInput.Register("BallDrop", GameInput.ReferencePriorities.Character, DropBall);
        GameInput.Register("Press", GameInput.ReferencePriorities.Character, TouchPress);

    }

    protected override void OnDisable()
    {
        base.OnDisable();
        GameInput.Deregister("BallDrop", GameInput.ReferencePriorities.Character, DropBall);
        GameInput.Deregister("Press", GameInput.ReferencePriorities.Character, TouchPress);


    }

    protected override void Update()
    {
        BallsRemainingText.text = RemainingBalls.ToString();
        BallsUsedText.text = BallsUsed.ToString();
        BallsScoredText.text = BallsScored.ToString();
        int tempTime = (int)RemainingTime;
        TimeText.text = tempTime.ToString();
        if (isGameStarted)
        {
            if (RemainingTime > 0)
            {
                RemainingTime = RemainingTime - Time.deltaTime;
            }
            else if (RemainingTime < 0)
            {
                RemainingTime = 0;
                EndGame();
            }

            if (RemainingBalls <= 0)
            {
                Invoke("EndGame",1);
                allowStart = false;
                isGameStarted = false;
                allowStart = false;
            }


        }


    }

    private void FixedUpdate()
    {
        if (isGameStarted)
        {
            rotateWheel();
        }
    }

    #region StartandEndRegion

    public void StartPlaying()
    {
        isGameStarted = true;
        RemainingBalls = MaxBalls;
        ballsToSpawn = MaxBalls;
        BallsUsed = 0;
        BallsScored = 0;
        RemainingTime = maxTimeInSec;
        fillLaunchQueue();

    }

    public void EndGame()
    {
        isGameStarted = false;
        allowStart = false;
        //resetDiscRotation();
        Points = CalculateTickets(BallsScored);
        PutBallsBackInQueue();
        readyToLaunchBall.Clear();
//        fillLaunchQueue();
        OnGameEnd();
    }


    public void fillLaunchQueue()
    {
        while (readyToLaunchBall.Count < maxQueue)
        {
            GameObject ball = spawnBall();
            if(ball != null)
            {
                readyToLaunchBall.Enqueue(ball);
            }
            else
            {
                break;
            }
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
    }


    #endregion

    #region Input
    private bool DropBall(InputAction.CallbackContext ctx)
    {
        if (allowStart)
        {
            if (ctx.started)
            {
                if (!isGameStarted)
                {
                    StartPlaying();
                }
                else
                {
                    if (RemainingBalls > 0 && RemainingTime > 0)
                    {
                        releaseCurrentBall();
                        --RemainingBalls;
                        ++BallsUsed;
                        if(ballsToSpawn > 0)
                        {
                             readyToLaunchBall.Enqueue(spawnBall());
                        }
                    }
                    else
                    {
                        EndGame();
                    }
                }
            }

        }
        return true;
    }

    #endregion

    #region Ball
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
                ballsToSpawn--;
                return ball;
                //                        ball.GetComponent<Rigidbody>().AddTorque(GetRandomVector(-1000, 1000));
            }
        }

        return null;

    }

    public void RestoreBall(Collider other)
    {
            other.gameObject.GetComponent<Rigidbody>().velocity = Vector3.zero;
            other.gameObject.transform.position = Vector3.zero;
            other.gameObject.SetActive(false);
    }

    public void releaseCurrentBall()
    {
        GameObject ball = readyToLaunchBall.Dequeue();
        ball.GetComponent<Rigidbody>().isKinematic = false;
        //        ball.transform.DOMove(ballDropPosition.position, 0.3f);
        ball.GetComponent<Rigidbody>().AddForce(new Vector3(0, -1, 0),ForceMode.Impulse);

        int curCount = 0;

        foreach (GameObject ballObj in readyToLaunchBall)
        {
            ballObj.transform.DOMove(ballQueue.BallPos[curCount].position, ballDropTime);
            curCount++;
        }

    }



    #endregion

    #region score
    public void AddScore(Collider other)
    {
        if (other.CompareTag("ball"))
        {
            if (!other.gameObject.GetComponent<QuickDropBall>().isCounted)
            {
                ++BallsScored;
                other.gameObject.GetComponent<QuickDropBall>().isCounted = true;
            }
            //CaughtBalls.Add(other.gameObject);
        }
    }


    public void BouncedOut(Collider other)
    {
        if (other.CompareTag("ball"))
        {
            if (other.gameObject.GetComponent<QuickDropBall>().isCounted)
            {
                --BallsScored;
                other.gameObject.GetComponent<QuickDropBall>().isCounted = false;
            }
            //            CaughtBalls.Remove(other.gameObject);
        }
    }

    #endregion

    #region rotateWheel
    public void rotateWheel()
    {
        RotatingDisc.transform.Rotate(Vector3.back * DiscRotationSpeed);
    }
    #endregion


    #region calculateTickets

    public override int Tickets => Points;
    public int CalculateTickets(int score)
    {
        if(score >= 2 && score <= 19)
        {
            return 25;
        }
        else if(score >=20 && score <= 39)
        {
            return 50;
        }
        else if(score >= 40 && score <= 49)
        {
            return 100;
        }
        else if(score == 50)
        {
            return 500;
        }
        else
        {
            return 0;
        }

    }

    #endregion

    #region MouseInput
    private void CastRayOnScreenPoint(InputAction.CallbackContext ctx)
    {
        Vector3 coor = mouse.position.ReadValue();
        RaycastHit hit;
        if (Physics.Raycast(cam.ScreenPointToRay(coor), out hit))
        {
            if (hit.collider.gameObject.name == ButtonCol.name)
            {
                DropBall(ctx);
            }
            else
            {

            }
        }

    }

    private bool TouchPress(InputAction.CallbackContext ctx)
    {
        if (ctx.started)
        {
            CastRayOnScreenPoint(ctx);
        }
        return true;
    }

        #endregion


    }
