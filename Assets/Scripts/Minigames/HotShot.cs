using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Globalization;
using System.Net;
using DebugConsole;
using Unity.Mathematics;
using UnityEngine;
using TMPro;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using DG.Tweening;
using Random = UnityEngine.Random;

public class HotShot : ArcadeGame
{
    [Header("HotShot")]
    [SerializeField] private Canvas canvas;
    [SerializeField] private Ball[] ball;
    [SerializeField] private Image chargeFill;
    [SerializeField] private Image ballInplaceOuter;
    [SerializeField] private float shootForce = 10;
    [SerializeField] private float xMin;
    [SerializeField] private float xMax;
    [SerializeField] private TextMeshPro scoreText;
    [SerializeField] private TextMeshPro timeCounterText;
    [SerializeField] private float counterTime; 
    [SerializeField] private Vector3 ballRayDirection;
    [SerializeField] private int xForceForShotBall = 25;
    [SerializeField] private DirectionalArrow directionalArrow;
    [SerializeField] private int ballPositionCheckTime = 2;
    [SerializeField] private Material lightMeshMaterial;


    private bool canDrag = false;
    private bool isBallPlaced = false;
    private bool isBallGrabbed = false;
    private bool isStartCheckingBallPos = false;
    private bool isBallRightPlace = false;
    private bool isBallReadyForShot = false;
    private bool isBallShot = false;
    private bool fixZaxis = false;
    private Vector3 offset;
    private float zCordinates;
    private float zForballThrow;
    private Vector3 ballPlacedPosition = Vector3.zero;

    private bool isStartCharging = false;
    private float startTime = 0;
    private bool IsCounterStarted { get; set; }
    private DateTime startedTime;
    private int currentBall = -1;
    private int oldScaledBall = 0;
    
    
    private Ball PlayingBall { get; set; }
    private Mouse mouse => Mouse.current;
    
    private Gamepad gamepad => Gamepad.current;
    private Camera cam => Camera.main;
    public override int Tickets => Score >= 100 ? 500 : Score * 3;

    #region inputs

    private bool InputsHotShot(InputAction.CallbackContext ctx)
    {
        if (!IsCounterStarted) return false;
        if (gamepad != null) return false;
      
        if (ctx.started && !isBallShot)
        {
            if (isBallPlaced)
            {
                ToBallPosition(chargeFill.transform);
                PlayingBall.Rotation = GetShootingAngle();
                isStartCharging = true;
            }
            else if(isBallGrabbed && isBallRightPlace)
            {
                PlaceBall();
            }
            else if(!isBallGrabbed)
            {
                isBallGrabbed = CastRayOnScreenPoint();
                if (isBallGrabbed) GrabbedBall();
            }
            return true;
        }
        else if(ctx.canceled)
        {
            if (isStartCharging)
            {
                isStartCharging = false;
                ShotBall();
            }
            return true;
        }
        return false;
    }

   //GamePadControls
    private bool RightButton(InputAction.CallbackContext ctx)
    {
        if (isBallGrabbed)
        {
            return false;
        }
        if (ctx.performed)
        {
            currentBall++;
            if (currentBall == ball.Length)
            {
                currentBall = 0;
                oldScaledBall = ball.Length-1;
            }
            ball[oldScaledBall].transform.localScale = Vector3.one;
            oldScaledBall = currentBall;
            ball[currentBall].transform.DOScale(new Vector3(1.1f,1.1f,1.1f), 0.1f);
        
            return true;
        }
     
        return false;
    }
    
    //GamePadControls
    private bool LeftButton(InputAction.CallbackContext ctx)
    {
        if (isBallGrabbed)
        {
            return false;
        }
        if (ctx.performed)
        {
            currentBall--;
            if (currentBall == -1)
            {
                currentBall = ball.Length-1;
                oldScaledBall = 0;
            }
            ball[oldScaledBall].transform.localScale = Vector3.one;
            oldScaledBall = currentBall;
            ball[currentBall].transform.DOScale(new Vector3(1.1f,1.1f,1.1f), 0.1f);
            return true;
        }
        return true;
    }
    
    //GamePadControls
    private bool SelectGamePad(InputAction.CallbackContext ctx)
    {
        if (!IsCounterStarted) return false;
       
        if (ctx.performed && !isBallShot)
        {
            if (isBallPlaced)
            {
                ToBallPosition(chargeFill.transform);
                PlayingBall.Rotation = GetShootingAngle();
                isStartCharging = true;
            }
            else if(isBallGrabbed && isBallRightPlace)
            {
                PlaceBall();
            }
            else if (!isBallGrabbed)
            {
                SelectBallGamepad();
            }
            return true;
        }
        else if(ctx.canceled)
        {
            if (isStartCharging)
            {
                isStartCharging = false;
                ShotBall();
            }
            return true;
        }
        return false;
    }

    private Vector3 GetMouseWorldPos()
    {
        Vector3 cameraPoint = mouse.position.ReadValue();
        cameraPoint.z = zCordinates;
        return cam.ScreenToWorldPoint(cameraPoint);
    }

    #endregion



    #region monobehaviour callbacks

    protected override void OnEnable()
    {
        GameInput.Register("Press",GameInput.ReferencePriorities.Character, InputsHotShot);
        GameInput.Register("Right",GameInput.ReferencePriorities.Character, RightButton);
        GameInput.Register("Left",GameInput.ReferencePriorities.Character, LeftButton);
        GameInput.Register("Select", GameInput.ReferencePriorities.Character, SelectGamePad);
    }

    


    protected override void OnDisable()
    {
        GameInput.Deregister("Press",GameInput.ReferencePriorities.Character, InputsHotShot);
        GameInput.Deregister("Left",GameInput.ReferencePriorities.Character, LeftButton);
        GameInput.Deregister("Right",GameInput.ReferencePriorities.Character, RightButton);
        GameInput.Deregister("Select", GameInput.ReferencePriorities.Character, SelectGamePad);

    }
    public override void StartGame()
    {
        base.StartGame();
      
        SetCamera("Grab");
        RegisterCommands();
        ballInplaceOuter.gameObject.SetActive(false);
        this.InvokeDelayed(2.0f, StartTimeCounter);
    }
    protected override void Update()
    {
        if (!IsCounterStarted) return;
        base.Update();

        if (canDrag) DragBall();

       
        if (isStartCharging)
        {
            startTime += Time.deltaTime;
            //img_chargeFill.GetComponent<CanvasGroup>().alpha = 1;
            chargeFill.fillAmount = startTime / fullChargeTime;
            if (startTime >= fullChargeTime)
            {
                isStartCharging = false;
                ShotBall();
            }
        }
        timeCounterText.text = CounterTime();
    }
    #endregion



    
    
    #region Ball

    private void GrabbedBall()
    {
        zCordinates =  cam.WorldToScreenPoint(PlayingBall.transform.position).z;
        SetCamera("Throw");
        canDrag = true;
        this.InvokeDelayed(1, () => PlayingBall.transform.DOScale(Vector3.one/2,0.3f).OnComplete(() =>
        {
            fixZaxis = true;
        }));
        this.InvokeDelayed(ballPositionCheckTime, () => isStartCheckingBallPos = true);
    }

    private void SelectBallGamepad()
    {
        PlayingBall = ball[currentBall];
        PlayingBall.GetRigidbody.isKinematic = true;
        PlayingBall.GetCollider.isTrigger = true;
        PlayingBall.transform.localScale = Vector3.one;
        isBallGrabbed = true;
        SetCamera("Throw");

        PlayingBall.transform.DOMove(new Vector3(0.0303f, 1.384f, -1.064f), 1.5f).OnComplete(() =>  canDrag = true);
        isStartCheckingBallPos = true;
    }

    private void DragBall()
    {
        if (PlayingBall)
        {
            var ppp = GetMouseWorldPos();
            if (fixZaxis)
            {
                ppp.z = -1.7368f;
            }
            if (gamepad != null)
            {
                PlayingBall.transform.position = new Vector3(
                    Mathf.Clamp((gamepad.leftStick.ReadValue().x * Time.deltaTime + PlayingBall.transform.position.x),-0.6f, 0.6f),
                     Mathf.Clamp((gamepad.leftStick.ReadValue().y * Time.deltaTime + PlayingBall.transform.position.y),1.2f,1.8f),
                    -1.064f);
            }
            else
            {
                PlayingBall.transform.position = ppp;
            }
            if(isStartCheckingBallPos)CastRayOnPoint();
        }
    }

    private void PlaceBall()
    {
        fixZaxis = false;
        PlayingBall.GetCollider.isTrigger = false;
        canDrag = false;
        isBallPlaced = true;
        ballInplaceOuter.gameObject.SetActive(false);
        ballPlacedPosition.z = -0.193f;
        PlayingBall.transform.DOMove(ballPlacedPosition, 0.2f);
        PlayingBall.transform.DOScale(Vector3.one, 0.2f)
            .OnComplete(() =>
            {
                directionalArrow.Position = PlayingBall.Position;
                if(!isStartCharging && !isBallShot)directionalArrow.StarRotation();
            }
        );
    }
    private Quaternion GetShootingAngle()
    {
        directionalArrow.StopRotation();
   
        var angle = directionalArrow.Rotation;
        return angle;
    }
    private void ShotBall()
    {
        isBallShot = true;
        directionalArrow.StopRotation();
        SetPhysicsMaterialActive();
        shootForce *= chargeFill.fillAmount;
        PlayingBall.GetTrailRenderer.enabled = true;
        PlayingBall.GetRigidbody.isKinematic = false;
    
        PlayingBall.GetRigidbody.AddRelativeForce (PlayingBall.transform.forward * shootForce, ForceMode.Impulse);
        chargeFill.fillAmount = 0;
    }

    public void RespawnBall()
    {
        if (!PlayingBall) return;
        if (PlayingBall.GetCollider.isTrigger) PlayingBall.GetCollider.isTrigger = false;
        SetCamera("Grab");
        startTime = 0;
        shootForce = 10;
        chargeFill.fillAmount = 0;
        isStartCheckingBallPos = false;
        PlayingBall.GetTrailRenderer.enabled = false;
        PlayingBall.GetRigidbody.velocity = PlayingBall.GetRigidbody.velocity * 0.5f;
        SetPhysicsMaterialInActive();
       // AddRandomTorque();
        PlayingBall = null;
        isBallShot = false;
        this.InvokeDelayed(1.1f, () =>
        {
            isBallGrabbed = false;
            canDrag = false;
            isBallPlaced = false;
            isBallRightPlace = false;
        });
    }

    public void ScoredGoal()
    {
        Score += 10;
        scoreText.text = Score.ToString();
        StartCoroutine(BlinkMaterial());
    }

    public IEnumerator BlinkMaterial()
    {
        lightMeshMaterial.color = Color.white;
        yield return new WaitForSeconds(0.2f);
        lightMeshMaterial.color = Color.red;
        yield return new WaitForSeconds(0.2f);
        lightMeshMaterial.color = Color.white;
        yield return new WaitForSeconds(0.2f);
        lightMeshMaterial.color = Color.red;
        yield return new WaitForSeconds(0.2f);
        lightMeshMaterial.color = Color.white;
        yield return new WaitForSeconds(0.2f);
        lightMeshMaterial.color = Color.red;
    }

    private void ToBallPosition(Transform goToBallPos)
    {
        Vector3 screenPos = cam.WorldToScreenPoint(PlayingBall.transform.position);
        //Convert the screenpoint to ui rectangle local point
        RectTransformUtility.ScreenPointToLocalPointInRectangle(canvas.transform as RectTransform,
            screenPos, 
            canvas.worldCamera, 
            out var movePos);
        //Convert the local point to world point
        goToBallPos.position =  canvas.transform.TransformPoint(movePos);
    }
    #endregion

    #region physics material

    private void SetPhysicsMaterialInActive()
    {
        PlayingBall.GetCollider.material.dynamicFriction = 0.1f;
        PlayingBall.GetCollider.material.staticFriction = 0.1f;
        PlayingBall.GetCollider.material.bounciness = 0.1f;
    }
    private void SetPhysicsMaterialActive()
    {
        PlayingBall.GetCollider.material.dynamicFriction = 0.6f;
        PlayingBall.GetCollider.material.staticFriction = 0.6f;
        PlayingBall.GetCollider.material.bounciness = 0.4f;
    }

    #endregion

    #region add torque

    public void AddRandomTorque()
    {
        PlayingBall.GetRigidbody.AddTorque(Utils.Vector3Extensions.GetRandomVector(1,3));
    }
    #endregion

    #region timers
    private string CounterTime() => IsCounterStarted ? 
        CheckIsTimerFinished(counterTime - StartedTime()).ToString("00")
        : "";

    private float StartedTime() => (float) (DateTime.Now - startedTime).TotalSeconds;

    private void StartTimeCounter()
    {
        IsCounterStarted = true;
        startedTime = DateTime.Now;
    }
    private float CheckIsTimerFinished(float runningTime)
    {
        if (runningTime <= 0.0f)
        {
            IsCounterStarted = false;
            //Gameover
            chargeFill.fillAmount = 0;
            directionalArrow.gameObject.SetActive(false);
            // ReSharper disable once Unity.NoNullPropagation
            if(PlayingBall != null)PlayingBall.gameObject.SetActive(false);
            OnGameEnd();
        }
        return runningTime;
    }
    //  private float CountCounterTime
    

    #endregion


    #region cast rays

    private bool CastRayOnScreenPoint()
    {
        Vector3 coor = mouse.position.ReadValue();
        RaycastHit hit; 
        if (Physics.Raycast(cam.ScreenPointToRay(coor), out hit) ) 
        {
            if (hit.collider.gameObject.TryGetComponent(out Ball playingBall))
            {
                PlayingBall = playingBall;
                playingBall.Velocity = Vector3.zero;
                playingBall.GetRigidbody.isKinematic = true;
                playingBall.GetCollider.isTrigger = true;
                return true;
            }
            else
            {
                canDrag = false;
                return false;
            }
        }
        return false;
    }

    private void CastRayOnPoint()
    {
        if (Physics.Raycast(PlayingBall.Position, ballRayDirection, out var hit))
        {
            Debug.DrawLine(PlayingBall.Position, ballRayDirection * 500, Color.white);
            if (hit.collider.gameObject.name == "BallPosition")
            {
                ballInplaceOuter.gameObject.SetActive(true);
                ToBallPosition(ballInplaceOuter.transform);
                isBallRightPlace = true;
                ballPlacedPosition = hit.point;
            }
            else
            {
                ballInplaceOuter.gameObject.SetActive(false);
                isBallRightPlace = false;
            }
        }
    }


    #endregion
    
    #region commands

    private void RegisterCommands()
    {
        DebugCommand IncreaseTime = new DebugCommand("increase_time",
            "increase timer of game",
            "increase_timer",
            () => { counterTime += 20;}
        );
        DebugController.commandList.Add(IncreaseTime);
        DebugCommand<int> SetTimerInSec = new DebugCommand<int>("set_timer", 
            "set count down timer in seconds",
            "set_timer <seconds>",
            (xx) => { counterTime = xx; });
        DebugController.commandList.Add(SetTimerInSec);

        DebugCommand<bool> ResetPos = new DebugCommand<bool>("reset_pos",
            "reset player position to vector zero",
            "reset_pos",
            (pos) => { Debug.Log(pos); });
        DebugController.commandList.Add(ResetPos);
    }

    #endregion
}
