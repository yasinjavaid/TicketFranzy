using System.Collections;
using System.Collections.Generic;
using AIs;
using UnityEngine;
using DG.Tweening;
using UnityEngine.InputSystem;
using DebugConsole;
using TMPro;



public class Squiggle : ArcadeGame, IAIActions
 {

     #region AI

     
     public AIModeRanges aiRanges;
     

     #endregion
     
    #region serialize fields
    [Header("Squiggle")][SerializeField] List<Ball> redBalls;
    [SerializeField] private TextMeshPro scoreText;
    [SerializeField] private GameObject button;
    [SerializeField] List<int> scores;
    [SerializeField] private Camera cam;
    #endregion

    #region public variables
    
    public BallQueue ballQueue;
    [Header("Main Ball")]
    public Ball yellowBall;
    public Transform ballSpawnPosition;

    #endregion
    
    #region private variables
    private Mouse mouse => Mouse.current;
  
    private Queue<Ball> countedBalls;
    
    public override int Tickets => points;
    
    
    private int queueSize;
    private int scoreBallPos;
    private int points;
    private bool isReset;
    #endregion


    #region monobehaviour callbacks

    protected override void OnEnable()
    {
        base.OnEnable();
        if (IsOtherPlayer) return;
        GameInput.Register("HoldBall", GameInput.ReferencePriorities.Character, OnInput_HoldBall);
        GameInput.Register("ActionButton", GameInput.ReferencePriorities.Character, ActionButtonPressed);
        RegisterCommands();
    }
    protected override void OnDisable()
    {
        base.OnDisable();
        if (IsOtherPlayer) return;
        GameInput.Deregister("HoldBall", GameInput.ReferencePriorities.Character, OnInput_HoldBall);
        GameInput.Deregister("ActionButton", GameInput.ReferencePriorities.Character, ActionButtonPressed);
    }
    public override void StartGame()
    {
        if(countedBalls.Count < 1)
        {
            base.StartGame();
            scoreText.text = "";
            HideBalls();
            isReset = false;
            queueSize = 0;
            scoreBallPos = -1;
            this.InvokeDelayed(2, ReleaseRedBall);
        }
    }
    #endregion
    
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

    #endregion
  

    #region raycast

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
   

    #region ball 

    private void ResetPositions()
    {
        ResetCenterBall();
        foreach (Ball ball in redBalls)
        {
            ball.gameObject.SetActive(false);
            ball.transform.localPosition = Vector3.zero;
            ball.GetRigidbody.isKinematic = true;
            ball.Velocity = Vector3.zero;
            ball.transform.rotation = Quaternion.identity;
            ball.GetRigidbody.angularVelocity = Vector3.zero;

        }
    }

    private void ResetCenterBall()
    {
        yellowBall.gameObject.SetActive(true);
        yellowBall.transform.localPosition = Vector3.zero;
        yellowBall.GetRigidbody.isKinematic = true;
        yellowBall.Velocity = Vector3.zero;
        yellowBall.transform.rotation = Quaternion.identity;
        yellowBall.GetRigidbody.angularVelocity = Vector3.zero;
    }
    private void ReleaseRedBall()
    {
        foreach (Ball ball in redBalls)
        {
            ball.gameObject.SetActive(true);
            ball.GetRigidbody.isKinematic = false;
            ball.GetRigidbody.collisionDetectionMode = CollisionDetectionMode.Continuous;
        }
    }

    private void ReleaseCenterBall()
    {
        yellowBall.gameObject.SetActive(true);
        yellowBall.GetRigidbody.isKinematic = false;
        yellowBall.GetRigidbody.collisionDetectionMode = CollisionDetectionMode.Continuous;
    }

    private void HideBalls()
    {
        foreach(Ball ball in redBalls)
        {
            ball.gameObject.SetActive(false);
        }
    }

    public void AddBallinQueue(Collider other)
    {
        Ball ball = other.GetComponent<Ball>();
        if (!countedBalls.Contains(ball))
        {
            countedBalls.Enqueue(ball);
            if (other.CompareTag("ScoreBall"))
            {
                scoreBallPos = queueSize;
                points = scores[scoreBallPos];
                if(points > 0)
                {
                    this.InvokeDelayed(3.0f, EndGame);
                }
                else
                {
                    this.InvokeDelayed(3.0f, ResetGame);
                }
            }
            ball.GetRigidbody.isKinematic = true;
            ball.transform.DOMove(ballQueue.BallPos[queueSize].position, 0.2f);
            ++queueSize;
//            ball.GetComponent<QuickDropBall>().isCounted = true;
        }
    }
    private void EmptyQueue()
    {
        if (countedBalls.Count > 0)
        {
            --queueSize;
            Ball ball = countedBalls.Dequeue();
            ball.transform.localPosition = Vector3.zero;
            ball.GetRigidbody.isKinematic = true;
            ball.gameObject.SetActive(false);
            for (int i = 1; i < countedBalls.Count; i++)
            {
                countedBalls.ToArray()[i].transform.DOMove(ballQueue.BallPos[i-1].position, 0.2f);
            }

            this.InvokeDelayed(0.2f, EmptyQueue);
        }
        else
        {
            countedBalls.Clear();
            ResetPositions();
            if (isReset)
            {
                StartGame();
            }
        }
    }
    
    #endregion

    #region private functions

    private void EndGame()
    {
        scoreText.text = points.ToString();
        EmptyQueue();
        OnGameEnd();
    }
    private void ResetGame()
    {
        StopGame();
        isReset = true;
        EmptyQueue();
    }
    private void ButtonPressAnimationPlay()
    {
        button.transform.DOLocalMoveZ(0.0073f, 0.3f).OnComplete(() =>
        {
            button.transform.DOLocalMoveZ(0.008f, 0.3f);
        });
    }

    #endregion

    #region commands

    private void RegisterCommands()
    {
        DebugCommand ReReleaseBall = new DebugCommand("RereleaseBall", "Respawn the ball from start position", "RereleaseBall",
            () => { ResetCenterBall(); ReleaseCenterBall(); }
        );
        countedBalls = new Queue<Ball>();
        DebugController.commandList.Add(ReReleaseBall);
    }


    #endregion

    public void DoInput()
    {
        var wait = 0.0f;
        switch (AI.Instance.AILevel)
        {
            case AI.AILevels.Simple:
                wait = Random.Range(aiRanges.easyRange[0], aiRanges.easyRange[1]);
                break;
            case AI.AILevels.Medium:
                wait = Random.Range(aiRanges.mediumRange[0], aiRanges.mediumRange[1]);
                break;
            case AI.AILevels.Hard:
                wait = Random.Range(aiRanges.hardRange[0], aiRanges.hardRange[1]);
                break;
        }   
         this.InvokeDelayed(4, ReleaseBall);
    }

    private void ReleaseBall()
    {
        ReleaseCenterBall();
        ButtonPressAnimationPlay();
    }
 }