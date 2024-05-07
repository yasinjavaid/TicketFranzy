using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.InputSystem;
using DebugConsole;
using UnityEngine.Events;
using ExitGames.Client.Photon;
using Hashtable = ExitGames.Client.Photon.Hashtable;
using Photon.Realtime;

public class FullTiltNetwork : ArcadeGame
{
    private Mouse mouse => Mouse.current;
    private Camera cam => Camera.main;


    [Header("Ball")]
    public Ball ball;
    public float ballMaxSpeed = 0.8f;
    private bool isReleased;

    private Vector3 previousBallPosition;
    public float ballPositionChangeThreshold = 0.01f;

    [Header("Gear")]
    public float ToothCount;
    public TextMeshPro topScoreDisplay;
    [SerializeField] public Gear inputGear;
    [SerializeField] public Gear outputGear1;
    [SerializeField] public Gear outputGear2;

    [Header("InputSystem")]
    public float maxAngleForPlatform = 45;
    public float minAngleForPlatform = -45;
    public GameObject WheelCol;
    public Transform Wheel;

    private float WheelAngle = 0f;
    private float LastWheelAngle = 0f;
    private Vector2 center;



    [Header("InputSystem Mouse")]
    private bool canDragMouse = false;

    [SerializeField] float frictionForWheel = 1f;
    public float turnSpeed = 1f;

    float rotation;
    [SerializeField] float testSpeed = 0;

    [Header("Input Keyboard")]
    private float keyPressValue = 0;
    public float turnSpeedKey = 10;



    [Header("Points")]
    public GameObject bottomDisplayBar;
    private int Points;
    private int ShowSlotId;

    public UnityEvent OnResetGame;


    protected override void Awake()
    {
        base.Awake();
        DebugCommand IncreaseMaxSpeed = new DebugCommand("max_ball_speed", "increase maximum speed of ball", "max_ball_speed",
           () => { ballMaxSpeed = 2f; }
       );
        DebugCommand ResetPosition = new DebugCommand("reset_position", "reset ball", "reset_position",
         () => { spawnBall(); }
        );
        DebugCommand StopBall = new DebugCommand("stop_ball", "Set ballspeed at it's position", "stop_ball",
         () => { removeMotion(); isReleased = false; }
        );
        DebugCommand StartBall = new DebugCommand("start_ball", "Set ball to motion again", "start_ball",
         () => { ReleaseBall(); }
        );
        DebugCommand<int> SetMouseTurnSpeed = new DebugCommand<int>("set_mouse_turn_speed", "Set wheel turn speed by mouse drag (default = 5)", "set_mouse_turn_speed<speed>",
         (spd) => { turnSpeed = spd; }
        );
        DebugCommand<int> SetTurnSpeed = new DebugCommand<int>("set_turn_speed", "Set wheel turn speed by keyboard (default = 100)", "set_turn_speed<speed>",
          (spd) => { turnSpeedKey = spd; }
         );
        DebugCommand<float> SetBallBounciness = new DebugCommand<float>("set_ball_bounce", "Set ball bounciness between 0 and 1 (default = 0.1)", "set_ball_bounce<bounce>",
          (bounce) => { ball.GetCollider.material.bounciness = bounce; }
         );


        DebugController.commandList.Add(IncreaseMaxSpeed);
        DebugController.commandList.Add(ResetPosition);
        DebugController.commandList.Add(StopBall);
        DebugController.commandList.Add(StartBall);
        DebugController.commandList.Add(SetMouseTurnSpeed);
        DebugController.commandList.Add(SetTurnSpeed);
        DebugController.commandList.Add(SetBallBounciness);
    }

    protected override void OnEnable()
    {
        GameInput.Register("Press", GameInput.ReferencePriorities.Character, TouchPress);
        GameInput.Register("KeyboardWheel", GameInput.ReferencePriorities.Character, KeyPress);
        GameInput.Register("ReleaseBallKey", GameInput.ReferencePriorities.Character, KeyRelease);
        FullTiltSecondPlayer.OnGameEndScoreUpdate += OnGameEndScoreUpdate;

        NetworkManager.Instance.onCustomPropertiesChange += ONCustomPropertiesChange;
    }

    protected override void OnDisable()
    {
        GameInput.Deregister("Press", GameInput.ReferencePriorities.Character, TouchPress);
        GameInput.Deregister("KeyboardWheel", GameInput.ReferencePriorities.Character, KeyPress);
        GameInput.Deregister("ReleaseBallKey", GameInput.ReferencePriorities.Character, KeyRelease);
        QuickDropSecondPlayer.OnGameEndScoreUpdate -= OnGameEndScoreUpdate;

        NetworkManager.Instance.onCustomPropertiesChange -= ONCustomPropertiesChange;
    }

    private void OnGameEndScoreUpdate(int obj)
    {
        ticketsReceivedUI.ShowTicketsForPlayerB(obj);
    }


    public override void StartGame()
    {
        SetPlayerScoreProperty(false);
        spawnBall();
        ShowDisplayBar();
        Points = -1;
        base.StartGame();
    }

    protected override void Update()
    {
        if (canDragMouse)
        {
            CheckClickCollision();
        }

        if (keyPressValue != 0)
        {
            rotateWheelKey();
        }

        Vector3 temp = ball.transform.localPosition - previousBallPosition;
        if(Mathf.Abs(temp.magnitude) > ballPositionChangeThreshold)
        {
            previousBallPosition = ball.transform.localPosition;
            SendBallPositionEvent(ball.transform.localPosition);
            Debug.Log("ball position event called");
        }
    }

    private void FixedUpdate()
    {
        if (isReleased)
        {
            if (ball.GetRigidbody.IsSleeping())
            {
                ball.GetRigidbody.AddForce(new Vector3(Random.Range(0.00001f, -0.00001f), Random.Range(0.00001f, -0.00001f)));
            }

            if (ball.Velocity.magnitude > ballMaxSpeed)
            {
                ball.Velocity = ball.Velocity.normalized * ballMaxSpeed;
            }
        }
    }


    #region ball
    public void spawnBall()
    {
        //ResetMachineEvent();
        ball.transform.localPosition = Vector3.zero;
        previousBallPosition = ball.transform.localPosition;
        SendBallPositionEvent(ball.transform.localPosition);
        removeMotion();
        isReleased = false;
    }

    public void removeMotion()
    {
        ball.Velocity = Vector3.zero;
        ball.GetRigidbody.useGravity = false;
    }

    public void ReleaseBall()
    {
        //SendReleaseBallEvent();
        if (!isReleased)
        {

        }

        isReleased = true;
        ball.GetRigidbody.useGravity = true;
    }




    #endregion

    #region MouseInput
    private bool TouchPress(InputAction.CallbackContext ctx)
    {
        if (ctx.started)
        {
            CastRayOnScreenPoint();
        }
        else if (ctx.performed)
        {
        }
        else if (ctx.canceled)
        {
            canDragMouse = false;
        }

        return true;
    }

    private void CastRayOnScreenPoint()
    {
        Vector3 coor = mouse.position.ReadValue();
        RaycastHit hit;
        if (Physics.Raycast(cam.ScreenPointToRay(coor), out hit))
        {
            if (hit.collider.gameObject.name == WheelCol.name)
            {
                canDragMouse = true;
                onPress();
            }
            else
            {

            }
        }

    }

    public void onPress()
    {
        center = RectTransformUtility.WorldToScreenPoint(cam, Wheel.transform.position);
        LastWheelAngle = Vector2.Angle(Vector2.up, Mouse.current.position.ReadValue() - center);
        if (Gear.InspectorAngles(Wheel.localRotation.eulerAngles.z) <= 5 && Gear.InspectorAngles(Wheel.localRotation.eulerAngles.z) >= -5)
        {
            ReleaseBall();
        }

    }

    private void CheckClickCollision()
    {
        Vector3 coor = mouse.position.ReadValue();
        RaycastHit hit;
        Physics.Raycast(cam.ScreenPointToRay(coor), out hit);

        if (Physics.Raycast(cam.ScreenPointToRay(coor), out hit))
        {
            if (hit.collider.gameObject.name == WheelCol.gameObject.name)
            {
                onDragNew();
            }

        }
    }

    public void onDragNew()
    {
        float NewAngle = Vector2.Angle(Vector2.up, Mouse.current.position.ReadValue() - center);
        if ((Mouse.current.position.ReadValue() - center).sqrMagnitude >= 200)
        {
            if (Mouse.current.position.ReadValue().x > center.x)
            {
                WheelAngle += (NewAngle - LastWheelAngle) / frictionForWheel;
            }
            else
            {
                WheelAngle -= (NewAngle - LastWheelAngle) / frictionForWheel;
            }

        }
        WheelAngle = Mathf.Clamp(WheelAngle, minAngleForPlatform, maxAngleForPlatform);
        Wheel.rotation = Quaternion.Euler(0, 0, -WheelAngle);
        inputGear.Rotate(WheelAngle, ToothCount);
        LastWheelAngle = NewAngle;

        //LastWheelAngle = NewAngle;
        //Vector2 delta = Mouse.current.position.ReadValue() - lastMousePosition;
        //float angles = Gear.InspectorAngles(Wheel.localRotation.eulerAngles.z);

        //float distance = delta.magnitude;
        //testSpeed = distance;
        //testSpeed = Mathf.Clamp(testSpeed, -50, 50);

        //isLeftBool = isLeft(center, lastMousePosition, Mouse.current.position.ReadValue());
        //lastMousePosition = Mouse.current.position.ReadValue();

        //if (clamped && Gear.InspectorAngles(Wheel.localRotation.eulerAngles.z) >= maxAngleForPlatform && !isLeftBool)
        //{
        //    UnClampMouse();
        //}
        //if (clamped && Gear.InspectorAngles(Wheel.localRotation.eulerAngles.z) <= minAngleForPlatform && isLeftBool)
        //{
        //    UnClampMouse();
        //}
        //else
        //{
        //    rotateCodeMouse();
        //}



    }



    #endregion

    #region keyboard
    private bool KeyPress(InputAction.CallbackContext ctx)
    {
        if (ctx.started)
        {
            keyPressValue = ctx.ReadValue<float>();
        }
        else if (ctx.canceled)
        {
            keyPressValue = ctx.ReadValue<float>();
        }

        return true;
    }

    private void rotateWheelKey()
    {

        if (WheelAngle >= 45 && keyPressValue > 0)
        {
            WheelAngle = 45;
        }
        else if (WheelAngle <= -45 && keyPressValue < 0)
        {
            WheelAngle = -45;
        }
        else
        {
            rotation = turnSpeedKey * -keyPressValue * Time.deltaTime;
            Wheel.Rotate(rotation * transform.forward);
            SendWheelRotateEvent(Wheel.transform.rotation, rotation);
            WheelAngle = -Gear.InspectorAngles(Wheel.localRotation.eulerAngles.z);
            inputGear.Rotate(WheelAngle, ToothCount);
            //Code to rotate wheel
        }
        LastWheelAngle = WheelAngle;

    }

    private bool KeyRelease(InputAction.CallbackContext ctx)
    {
        if (ctx.started)
        {
            if (Gear.InspectorAngles(Wheel.localRotation.eulerAngles.z) <= 5 && Gear.InspectorAngles(Wheel.localRotation.eulerAngles.z) >= -5)
            {
                ReleaseBall();
            }

        }

        return true;
    }


    #endregion

    #region Points

    public void setPoints(int points)
    {
        Points = points;
        Debug.Log("Points: " + Points);
    }

    public int getPoints(int points)
    {
        return Points;
    }

    public override int Tickets => Score;

    public void DisplayScore()
    {
        topScoreDisplay.text = Points.ToString();
        Score = Points;
        SendScoreEvent(ShowSlotId, Points);
        StopGame();
        //        OnGameEnd();
        SetPlayerScoreProperty(true);
    }

    public void HideDisplayBar(int id)
    {
        ShowSlotId = id;
        int curr = 1;
        foreach (Transform child in bottomDisplayBar.GetChildren())
        {
            if (id != curr)
                child.SetActive(false);
            ++curr;
        }
    }

    public void ShowDisplayBar()
    {
        SendScoreEvent(-1, 0);
        foreach (Transform child in bottomDisplayBar.GetChildren())
        {
            child.SetActive(true);
        }
        topScoreDisplay.text = "";

    }

    #endregion

    #region GameStartAndEnd



    public void CalculateScore(int val)
    {
        setPoints(val);
        DisplayScore();
    }

    public override void Reset()
    {
        base.Reset();
        ticketsReceivedUI.onTicketActioninvokedFlag = false;
        ticketsReceivedUI.gameObject.SetActive(false);
        OnResetGame?.Invoke();
    }

    #endregion

    #region network
    public void IsFromGame()
    {
        PlayerPrefs.SetInt(NetworkManager.ISPLAYERFROMARCADEGAME, 1);
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

    private void ONCustomPropertiesChange(int playerActorNumber, Hashtable hashtable)
    {
        if (OngoingGame) return;
        if (CheckIfBothPlayerReadyForScore())
        {
            SetPlayerScoreProperty(false);
            OnGameEnd();
        }
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


    #endregion

    #region events

    //private void SendReleaseBallEvent()
    //{
    //    object[] dataToSend = new object[]
    //    {
    //        NetworkManager.Instance.LocalPlayer.ActorNumber,
    //    };
    //    NetworkManager.Instance.RaisePhotonEvent(dataToSend, 1, NetworkManager.BallScoredEventCode);
    //}

    private void SendWheelRotateEvent(Quaternion rotation, float rotationValue)
    {
        object[] dataToSend = new object[]
        {
            NetworkManager.Instance.LocalPlayer.ActorNumber,
            rotation,
            inputGear.transform.rotation,
            outputGear1.transform.rotation,
            outputGear2.transform.rotation
        };

        NetworkManager.Instance.RaisePhotonEvent(dataToSend, 1, NetworkManager.WheelRotateEventCode);
    }

    private void SendScoreEvent(int slotNo, int points)
    {
        object[] dataToSend = new object[]
        {
            NetworkManager.Instance.LocalPlayer.ActorNumber,
            slotNo,
            points
        };
        NetworkManager.Instance.RaisePhotonEvent(dataToSend, 1, NetworkManager.ScoreDisplayUpdateEventCode);
    }

    private void SendBallPositionEvent(Vector3 position)
    {
        object[] dataToSend = new object[]
        {
            NetworkManager.Instance.LocalPlayer.ActorNumber,
            position
        };
        NetworkManager.Instance.RaisePhotonEvent(dataToSend, 1, NetworkManager.BallPositionEventCode);
    }


#endregion



}