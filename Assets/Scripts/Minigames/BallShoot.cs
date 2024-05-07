using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Serialization;
using UnityEngine.InputSystem;
using UnityEngine;

public abstract class BallShoot : ArcadeGame
{
     [Header("Ball Based Game")]

    [SerializeField, Min(float.Epsilon)] protected float reticleDistanceMultiplier;
    [SerializeField] protected Vector2 launchVelocitySensitivity;

    [FormerlySerializedAs("timeToRelease")]
    [SerializeField, Min(0)] protected float autoReleaseTime = 2f;
    
    [SerializeField] protected LineRenderer line_launchPreview;

    [SerializeField] protected Transform ballResetPosition;
    [SerializeField] protected Collider ballHolderCollider;
    [SerializeField] protected PhysicMaterial ballPhysicsMaterial;
    [SerializeField] protected PhysicMaterial tablePhysicsMaterial;
    [SerializeField] protected Ball balls;

    [SerializeField] protected int maxInputCapacity = 1000;
    [SerializeField] protected float maxIdleTime = .5f;
    [SerializeField] protected float gizmoSizeMultiplier = 3f;
    [SerializeField] protected Vector2 magnitudeCap = new Vector2(1f, .9f);
    [SerializeField, Range(0.5f, .95f)] protected float releasePoint;

    [SerializeField, Min(0)] protected int maxChevrons = 10;
    [SerializeField] protected float chevronDistance = 0.05f;
    [SerializeField] protected Vector3 chervronsOffset;

    protected DateTime inputCancelTime = DateTime.MaxValue;

    public static readonly TimeSpan LAUNCH_BALL_INPUT_MAX_AGE = TimeSpan.FromMilliseconds(100);

    public Ball PlayingBall { get; set; }

    protected virtual float GetCombinedBounciness() => ballPhysicsMaterial.CombinedBounciness(tablePhysicsMaterial);

    protected virtual float GetBallRadius() => balls.Radius;

    #region Events

    protected virtual void OnValidate() => releasePoint = Mathf.Clamp(releasePoint, magnitudeCap.y, 0.95f);

    public override void Reset()
    {
        base.Reset();
        if (ballHolderCollider) ballHolderCollider.enabled = false;
    }

    public override void OnScored(int value)
    {
        base.OnScored(value);
        if (OngoingGame && ballHolderCollider) ballHolderCollider.enabled = true;
    }

    #endregion

    #region Input

    protected override void OnEnable()
    {
        base.OnEnable();
        GameInput.Register("HoldBall", GameInput.ReferencePriorities.Character, OnInput_HoldBall);
    }

    protected override void OnDisable()
    {
        base.OnDisable();
        GameInput.Deregister("HoldBall", GameInput.ReferencePriorities.Character, OnInput_HoldBall);
    }

    protected virtual bool OnInput_LaunchBall(InputAction.CallbackContext ctx)
    {
        if (ctx.performed)
        {
            Vector2 newInput = ctx.ReadValue<Vector2>();
            newInput.x = Mathf.Clamp(newInput.x, -magnitudeCap.x, magnitudeCap.x);
            newInput.y = Mathf.Clamp(newInput.y, -magnitudeCap.y, magnitudeCap.y);
            if (newInput.y >= releasePoint) LaunchBall();
        }
        else if (ctx.canceled)
            inputCancelTime = DateTime.Now;
        return true;
    }

    protected virtual bool OnInput_HoldBall(InputAction.CallbackContext ctx) => true;

    #endregion
    
     #region Ball

    public virtual void WakeBallsUp()
    {
        
            balls.GetRigidbody.WakeUp();
    }

    public virtual void ResetBall(Collider collider)
    {
        if (collider.TryGetComponent(out Ball ball))
            ResetBall(ball);
    }

    protected virtual void ResetBall(Ball ball)
    {
        ball.GetRigidbody.MovePosition(ballResetPosition.position);
        ball.transform.GetChild(0).SetActive(true);
        ball.GetRigidbody.WakeUp();
    }

    protected virtual void GrabBall()
    {
        if (OngoingGame && !PlayingBall && TryGetBallAtReticle(out Ball ball))
            GrabBall(ball);
    }

    protected virtual void GrabBall(Ball ball)
    {
        PlayingBall = ball;
       // GrabbedBall.GetCollider.isTrigger = true;
       // GrabbedBall.GetRigidbody.isKinematic = true;
    }

    protected virtual bool TryRaycastBallPlacement(out RaycastHit hit)
    {
        RaycastHit[] hits = Physics.RaycastAll(Camera.main.ScreenPointToRay(Camera.main.WorldToScreenPoint(reticle.rectTransform.position)),
                100, LayerMask.GetMask("CapturePointer"), QueryTriggerInteraction.Collide);

        for (int i = 0; i < hits.Length; i++)
            if (hits[i].collider.name == "BallStartingArea")
            {
                hit = hits[i];
                return true;
            }

        hit = default;
        return false;
    }

    protected virtual void LaunchBall()
    {
        if (PlayingBall)
        {
            PlayingBall.transform.GetChild(0).SetActive(false);
            PlayingBall.GetCollider.isTrigger = false;
            PlayingBall.GetRigidbody.isKinematic = false;
            PlayingBall.GetRigidbody.velocity = GetLaunchMomentum();
            chargeBeginTime = default;
        }
    }

    protected virtual Vector2 GetLaunchInput() => ((Camera.main.WorldToScreenPoint(reticle.rectTransform.position) - Camera.main.WorldToScreenPoint(PlayingBall.transform.position)) * reticleDistanceMultiplier).normalized;

    protected virtual Vector2 GetLaunchInputWithCharge() => GetLaunchInput() * GetChargeMultiplier();

    protected virtual Vector3 GetLaunchMomentum() => (GetLaunchInputWithCharge() * launchVelocitySensitivity).ToVector3_XZ();

    protected virtual bool TryGetBallAtReticle(out Ball ball)
    {
        ball = default;
        Ray ray = Camera.main.ScreenPointToRay(Camera.main.WorldToScreenPoint(reticle.rectTransform.position));
        if (Physics.Raycast(ray, out RaycastHit hit, 10, LayerMask.GetMask("Ball", "Machine"), QueryTriggerInteraction.Ignore))
        {
            if (hit.collider.TryGetComponentInParentOrChildren(out ball))
            {
                Debug.DrawLine(ray.origin, hit.point, Color.green, 10);
                return true;
            }
            else
            {
                Debug.DrawLine(ray.origin, hit.point, Color.yellow, 10);
                return false;
            }
        }
        else
        {
            Debug.DrawRay(ray.origin, ray.direction, Color.red, 10);
            return false;
        }
    }

    #endregion
    
    #region Update

    protected override void Update()
    {
        
    }

    protected virtual void FixedUpdate()
    {
     
    }

    #endregion
    
}
