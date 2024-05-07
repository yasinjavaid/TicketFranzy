using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using UnityEngine.InputSystem;
using UnityEngine.Events;
using Photon.Realtime;
using Hashtable = ExitGames.Client.Photon.Hashtable;
using DebugConsole;
using TMPro;



[Serializable]
public class BallsQueue
{
    public List<Ball> RedBalls;
}

public class BallsPos
{
    public List<Vector3> ballsPos = new List<Vector3>();
}

public class SquiggleNetwork : ArcadeGame
{
    private Mouse mouse => Mouse.current;



    [Header("Queue")]
    
    [SerializeField] private Camera cam;
    public BallQueue ballQueue;

    public BallsQueue balls;
    private BallsPos ballsPos;
    private Queue<Ball> countedBalls;
    [SerializeField] private Transform HidePosition;

    [Header("Button")]
    [SerializeField] public GameObject button;


    [Header("Main Ball")]
    public Ball yellowBall;
    public Transform ballSpawnPosition;

    [Header("Scores")]
    [SerializeField] List<int> Scores;
    [SerializeField] private TextMeshPro scoreText;





    private int QueueSize;
    private int ScoreBallPos;
    private int Points;
    private bool isReset;
    public override int Tickets => Points;

    private bool areBallReleased;
    private bool yellowBallReleased;

    private BallQueue sendQueue;

    public UnityEvent OnResetGame;




    protected override void OnEnable()
    {
        base.OnEnable();
        GameInput.Register("HoldBall", GameInput.ReferencePriorities.Character, OnInput_HoldBall);
        GameInput.Register("ActionButton", GameInput.ReferencePriorities.Character, ActionButtonPressed);
        countedBalls = new Queue<Ball>();

        SquiggleSecondPlayer.OnGameEndScoreUpdate += OnGameEndScoreUpdate;
        NetworkManager.Instance.onCustomPropertiesChange += ONCustomPropertiesChange;

        ballsPos = new BallsPos();
        DebugCommand ReReleaseBall = new DebugCommand("RereleaseBall", "Respawn the ball from start position", "RereleaseBall",
         () => { ResetCenterBall(); ReleaseCenterBall(); }
        );
        DebugController.commandList.Add(ReReleaseBall);
    }
    
    protected override void OnDisable()
    {
        base.OnDisable();
        GameInput.Deregister("HoldBall", GameInput.ReferencePriorities.Character, OnInput_HoldBall);
        GameInput.Deregister("ActionButton", GameInput.ReferencePriorities.Character, ActionButtonPressed);

        SquiggleSecondPlayer.OnGameEndScoreUpdate -= OnGameEndScoreUpdate;
        NetworkManager.Instance.onCustomPropertiesChange -= ONCustomPropertiesChange;

    }

    private void OnGameEndScoreUpdate(int obj)
    {
        ticketsReceivedUI.ShowTicketsForPlayerB(obj);
    }

    public override void StartGame()
    {
        if(countedBalls.Count < 1)
        {
            base.StartGame();
            SetPlayerScoreProperty(false);
            areBallReleased = false;
            yellowBallReleased = false;
            HideBalls();
            isReset = false;
            QueueSize = 0;
            ScoreBallPos = -1;
            SendCameraPanEvent("Start");
            scoreText.text = "";
            this.InvokeDelayed(2, ReleaseRedBall);
        }
    }


    private void FixedUpdate()
    {
        if (areBallReleased || yellowBallReleased)
        {
            SendBallPositionEvent(balls.RedBalls, yellowBall);
        }
    }

    #region Input

    protected virtual bool OnInput_HoldBall(InputAction.CallbackContext ctx)
    {

        if (ctx.started && OngoingGame && CheckButtonPress()) 
        {
            ReleaseCenterBall();
            ButtonPressAnimationPlay();
        }
        return true;
    }
    private bool ActionButtonPressed(InputAction.CallbackContext ctx)
    {
        if (ctx.started && OngoingGame)
        {
            ReleaseCenterBall();
            ButtonPressAnimationPlay();
        }
        return true;
    }

    private void ButtonPressAnimationPlay()
    {
        button.transform.DOLocalMoveZ(0.0073f, 0.3f).OnComplete(() =>
        {
            button.transform.DOLocalMoveZ(0.008f, 0.3f);
        });
    }

    private bool CheckButtonPress()
    {
        Vector3 coor = mouse.position.ReadValue();
        RaycastHit hit;
        if (Physics.Raycast(cam.ScreenPointToRay(coor), out hit))
        {
            if (hit.collider.gameObject.name == button.name)
            {
                return true;
            }
        }
        return false;

    }

    #endregion



    #region resetBallPositions

    public void ResetPositions()
    {
        areBallReleased = false;
        ResetCenterBall();
        SendBallVisibilityEvent(false, true);
        foreach (Ball ball in balls.RedBalls)
        {
            ball.gameObject.SetActive(false);
            ball.transform.localPosition = Vector3.zero;
            ball.GetRigidbody.isKinematic = true;
            ball.Velocity = Vector3.zero;
            ball.transform.rotation = Quaternion.identity;
            ball.GetRigidbody.angularVelocity = Vector3.zero;
        }
    }

    public void ResetCenterBall()
    {
        SendBallVisibilityEvent(true, false);
        yellowBallReleased = false;
//        SendBallVisibilityEvent(true, false);
        yellowBall.gameObject.SetActive(true);
        yellowBall.transform.localPosition = Vector3.zero;
        yellowBall.GetRigidbody.isKinematic = true;
        yellowBall.Velocity = Vector3.zero;
        yellowBall.transform.rotation = Quaternion.identity;
        yellowBall.GetRigidbody.angularVelocity = Vector3.zero;
    }
    public void ReleaseRedBall()
    {
        areBallReleased = true;
        SendBallVisibilityEvent(true, true);
        foreach (Ball ball in balls.RedBalls)
        {
            ball.gameObject.SetActive(true);
            ball.GetRigidbody.isKinematic = false;
            ball.GetRigidbody.collisionDetectionMode = CollisionDetectionMode.Continuous;
        }
    }

    public void ReleaseCenterBall()
    {
        SendBallVisibilityEvent(true, false);
        yellowBallReleased = true;
        yellowBall.gameObject.SetActive(true);
        yellowBall.GetRigidbody.isKinematic = false;
        yellowBall.GetRigidbody.collisionDetectionMode = CollisionDetectionMode.Continuous;
    }
    private void SendCameraPanEvent(string camName)
    {
        object[] dataToSend = new object[]
        {
            NetworkManager.Instance.LocalPlayer.ActorNumber,
            camName
        };
        NetworkManager.Instance.RaisePhotonEvent(dataToSend, 1, NetworkManager.CameraPanningEventCode);
    }

    private void HideBalls()
    {
        SendBallVisibilityEvent(false, true);
        foreach (Ball ball in balls.RedBalls)
        {
            ball.gameObject.SetActive(false);
        }
    }

    #endregion


    #region BallFillQueueFunction
    public void AddBallinQueue(Collider other)
    {
        Ball ball = other.GetComponent<Ball>();
        if (!countedBalls.Contains(ball))
        {
            countedBalls.Enqueue(ball);
            if (other.CompareTag("ScoreBall"))
            {
                ScoreBallPos = QueueSize;
                Points = Scores[ScoreBallPos];
                if(Points > 0)
                {
                    this.InvokeDelayed(3.0f, EndGame);
                }
                else
                {
                    this.InvokeDelayed(3.0f, ResetGame);
                }
            }
            ball.GetRigidbody.isKinematic = true;
            ball.transform.DOMove(ballQueue.BallPos[QueueSize].position, 0.2f);
            ++QueueSize;
//            ball.GetComponent<QuickDropBall>().isCounted = true;
        }
    }

    private void EmptyQueue()
    {
        if (countedBalls.Count > 0)
        {

            --QueueSize;
            Ball ball = countedBalls.Dequeue();
            ball.GetRigidbody.isKinematic = true;
            ball.Position = HidePosition.position;
            ball.gameObject.SetActive(false);
            if (ball.gameObject.CompareTag("ScoreBall"))
            {
                SendBallVisibilityEvent(false, false);
            }
            for (int i = 1; i < countedBalls.Count; i++)
            {
                countedBalls.ToArray()[i].transform.DOMove(ballQueue.BallPos[i-1].position, 0.2f);
            }

            Invoke("EmptyQueue", 0.2f);
        }
        else
        {
            //SendBallVisibilityEvent(false, true);
            countedBalls.Clear();
            ResetPositions();
            if (isReset)
            {
                StartGame();
            }
        }
    }

    #endregion


    public void EndGame()
    {
        scoreText.text = Points.ToString();
        EmptyQueue();
        StopGame();
        SetPlayerScoreProperty(true);
    }

    private void ResetGame()
    {
        StopGame();
        isReset = true;
        EmptyQueue();
    }

    public override void Reset()
    {
        base.Reset();
        ticketsReceivedUI.onTicketActioninvokedFlag = false;
        ticketsReceivedUI.gameObject.SetActive(false);
        OnResetGame?.Invoke();
    }


    #region events

    private void SendBallVisibilityEvent(bool visible, bool isRedBall)
    {
        object[] dataToSend = new object[]
        {
                NetworkManager.Instance.LocalPlayer.ActorNumber,
                visible,
                isRedBall
        };

        NetworkManager.Instance.RaisePhotonEvent(dataToSend, 1, NetworkManager.BallRespawnEventCode);
    }

    private void SendBallPositionEvent(List<Ball> redBalls, Ball yellowBall)
    {

       
        object[] dataToSend = new object[]
        {
            NetworkManager.Instance.LocalPlayer.ActorNumber,
            redBalls[0].transform.localPosition,
            redBalls[1].transform.localPosition,
            redBalls[2].transform.localPosition,
            redBalls[3].transform.localPosition,
            redBalls[4].transform.localPosition,
            redBalls[5].transform.localPosition,
            redBalls[6].transform.localPosition,
            redBalls[7].transform.localPosition,
            redBalls[8].transform.localPosition,
            redBalls[9].transform.localPosition,
            redBalls[10].transform.localPosition,
            redBalls[11].transform.localPosition,
            redBalls[12].transform.localPosition,
            redBalls[13].transform.localPosition,
            redBalls[14].transform.localPosition,
            redBalls[15].transform.localPosition,
            redBalls[16].transform.localPosition,
            redBalls[17].transform.localPosition,
            yellowBall.transform.localPosition
        };

        NetworkManager.Instance.RaisePhotonEvent(dataToSend, 1, NetworkManager.BallPositionEventCode);
    }

    private void SendScoreUpdate(int points)
    {
        object[] dataToSend = new object[]
        {
                    NetworkManager.Instance.LocalPlayer.ActorNumber,
                    points
        };
        NetworkManager.Instance.RaisePhotonEvent(dataToSend, 1, NetworkManager.BallScoredEventCode);


    }

    #endregion



    #region network

    private void ONCustomPropertiesChange(int playerActorNumber, Hashtable hashtable)
    {
        if (OngoingGame) return;
        if (CheckIfBothPlayerReadyForScore())
        {
            SetPlayerScoreProperty(false);
            OnGameEnd();
        }
    }

    public void IsFromGame()
    {
        PlayerPrefs.SetInt(NetworkManager.ISPLAYERFROMARCADEGAME, 1);
    }


    private bool CheckIfBothPlayerReadyForScore()
    {
        foreach (Player p in NetworkManager.Instance.AllNetworkPlayers)
        {
            object readyForScore;
            if (p.CustomProperties.TryGetValue(NetworkManager.PLAYERREADYFORSCORE, out readyForScore))
            {
                if (!(bool)readyForScore)
                {
                    return false;
                }
            }
            else
            {
                return false;
            }
        }

        //all players ready for Play
        return true;
    }

    public void SetPlayerScoreProperty(bool value)
    {
        if (NetworkManager.Instance.LocalPlayer.IsLocal)
        {
            Hashtable props = new Hashtable()
            {
                {NetworkManager.PLAYERREADYFORSCORE, value}
            };
            NetworkManager.Instance.LocalPlayer.SetCustomProperties(props);
        }
    }

    #endregion


}