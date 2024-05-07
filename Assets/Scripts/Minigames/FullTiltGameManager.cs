using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.InputSystem;
using DebugConsole;
using DG.Tweening;

public class FullTiltGameManager : ArcadeGame
{

    enum SouthButtonActions { None, Release, Reset };
    SouthButtonActions SouthButton;

    private Mouse mouse => Mouse.current;
    private Camera cam => Camera.main;

    private Gamepad Gamepad => Gamepad.current;

    [Header("Ball")]
    public Ball ball;
    public float ballMaxSpeed = 0.8f;
    private bool isReleased;

    [Header("Gear")]
    public float ToothCount;
    public TextMeshPro topScoreDisplay;
    [SerializeField] public Gear inputGear;

    [Header("InputSystem")]
    public float maxAngleForPlatform = 45;
    public float minAngleForPlatform = -45;
    public GameObject WheelCol;
    public Transform Wheel;
    private Quaternion startRotationAngle;
    private float startAngle;

    [SerializeField] private float WheelAngle = 0f;
    private float LastWheelAngle = 0f;
    private Vector2 center;
    private bool allowJoystickInput;



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
        GameInput.Register("JoystickWheel", GameInput.ReferencePriorities.Character, JoystickPress);
        GameInput.Register("JoystickKeyTrigger", GameInput.ReferencePriorities.Character, JoystickKeyPress);
    }

    protected override void OnDisable()
    {
        GameInput.Deregister("Press", GameInput.ReferencePriorities.Character, TouchPress);
        GameInput.Deregister("KeyboardWheel", GameInput.ReferencePriorities.Character, KeyPress);
        GameInput.Deregister("ReleaseBallKey", GameInput.ReferencePriorities.Character, KeyRelease);
        GameInput.Deregister("JoystickWheel", GameInput.ReferencePriorities.Character, JoystickPress);
        GameInput.Deregister("JoystickKeyTrigger", GameInput.ReferencePriorities.Character, JoystickKeyPress);

    }

    protected override void Start()
    {
        base.Start();
        startRotationAngle = Wheel.rotation;
        startAngle = Gear.InspectorAngles(Wheel.rotation.eulerAngles.z);
        Points = -1;
        allowJoystickInput = false;
        SouthButton = SouthButtonActions.Release;
    }



    public override void StartGame()
    {
        spawnBall();
        base.StartGame();
//        this.InvokeDelayed(0.1f, () => releaseBall = true);
    }

    protected override void Update()
    {
        if (!OngoingGame)
        {
            WheelAngle = -Gear.InspectorAngles(Wheel.localRotation.eulerAngles.z);
            inputGear.Rotate(WheelAngle, ToothCount);
            return;
        }


        if (canDragMouse)
        {
            CheckClickCollision();
        }

        if(keyPressValue != 0)
        {
            rotateWheelKey();
        }

        if (allowJoystickInput)
        {
            RotateWheelJoystick();
        }

        
    }

    private void FixedUpdate()
    {
        if (!OngoingGame)
        {
            return;
        }
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
        ball.transform.localPosition = Vector3.zero;
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
        if (!OngoingGame)
            return;
        isReleased = true;
        ball.GetRigidbody.useGravity = true;
    }




    #endregion

    #region MouseInput
    private bool TouchPress(InputAction.CallbackContext ctx)
    {
        if (!OngoingGame)
        {
            return false;
        }
        if (ctx.performed)
        {
            CastRayOnScreenPoint();
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
        if (Gear.InspectorAngles(Wheel.localRotation.eulerAngles.z) <= 5 && Gear.InspectorAngles(Wheel.localRotation.eulerAngles.z) >= -5 && OngoingGame)
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
        if((Mouse.current.position.ReadValue()- center).sqrMagnitude>= 200)
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
        Wheel.rotation = Quaternion.Euler(0, 0, -(WheelAngle-startAngle));
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
        if (!OngoingGame)
        {
            return false;
        }


        if (ctx.performed)
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
            WheelAngle = -Gear.InspectorAngles(Wheel.localRotation.eulerAngles.z);
            inputGear.Rotate(WheelAngle, ToothCount);
            //Code to rotate wheel
        }
        LastWheelAngle = WheelAngle;

    }

    private bool KeyRelease(InputAction.CallbackContext ctx)
    {

        if (ctx.performed)
        {
            if (Gear.InspectorAngles(Wheel.localRotation.eulerAngles.z) <= 5 && Gear.InspectorAngles(Wheel.localRotation.eulerAngles.z) >= -5 && OngoingGame)
            {
                    ReleaseBall();
            }

        }

        //if (ctx.canceled && OngoingGame && !allowRelease)
        //{
        //    allowRelease = true;
        //}

        return true;
    }


    #endregion

    #region joysticks
    private bool JoystickPress(InputAction.CallbackContext ctx)
    {
        if (!OngoingGame)
            return false;


        if (ctx.performed)
        {
            allowJoystickInput = true;
        }
        else if (ctx.canceled)
        {
            allowJoystickInput = false;
        }

        return true;
    }

    private void RotateWheelJoystick()
    {
        float angle = Mathf.Atan2(Gamepad.leftStick.y.ReadValue(), Gamepad.leftStick.x.ReadValue()) * Mathf.Rad2Deg;

        if(Gamepad.leftStick.y.ReadValue() < 0)
        {
            if(Gamepad.leftStick.x.ReadValue() < 0)
            {
                WheelAngle = minAngleForPlatform;
            }
            else if (Gamepad.leftStick.x.ReadValue() > 0)
            {
                WheelAngle = maxAngleForPlatform;
            }
        }
        else
        {
            angle = angle - 90;
            WheelAngle = -Mathf.Clamp(angle, minAngleForPlatform, maxAngleForPlatform);
        }

        Wheel.rotation = Quaternion.Euler(0, 0, -(WheelAngle-startAngle));
        inputGear.Rotate(WheelAngle, ToothCount);
    }

    private bool JoystickKeyPress(InputAction.CallbackContext ctx)
    {
        if (ctx.performed)
        {
            if (SouthButton.Equals(SouthButtonActions.None) || SouthButton.Equals(SouthButtonActions.Reset))
            {
                SouthButton = SouthButtonActions.Release;
            }
            else if (SouthButton.Equals(SouthButtonActions.Release))
            {
                KeyRelease(ctx);
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
     
        OnGameEnd();
    }

    public void HideDisplayBar(int id)
    {
        int curr = 1;
        foreach(Transform child in bottomDisplayBar.GetChildren())
        {
            if(id != curr)
                child.SetActive(false);
            ++curr;
        }
    }

    public void ShowDisplayBar()
    {
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
        ShowDisplayBar();
        ticketsReceivedUI.gameObject.SetActive(false);
        WheelAngle = -Gear.InspectorAngles(Wheel.localRotation.eulerAngles.z);
        inputGear.Rotate(WheelAngle, ToothCount);
    }

    public override void OnGameEnd()
    {
        base.OnGameEnd();
        SouthButton = SouthButtonActions.Reset;
        canDragMouse = false;
        keyPressValue = 0;
        allowJoystickInput = false;
        Wheel.DORotate(startRotationAngle.eulerAngles, 2.0f);
    }


    #endregion
}
