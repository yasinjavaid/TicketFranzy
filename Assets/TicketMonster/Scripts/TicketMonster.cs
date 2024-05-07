using System;
using System.Collections.Generic;
using System.Linq;

using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class TicketMonster : ArcadeGame
{
    
    [SerializeField] protected Rigidbody rb_wheel;
    [SerializeField] protected Transform leverTransform;
    [SerializeField] protected float rotationFactor;
    [SerializeField] protected float leverReturnDegreesPerSecond;
    [SerializeField] private float rotationVelocityMin = 1;
    [SerializeField] protected float rotationCorrectionSpeed;
    [SerializeField] protected RectTransform grabBallRegion;
    [SerializeField, Range(0, 1)] protected float rotationCaptureTimeframe;

    private float maxRotationVelocity;

    private bool isHoldingHandle;
    private bool inputFinished;
    private float defaultLeverRotation;
    private float currentLeverRotation;
    protected int currentValue;

    [Header("LeverVariables")]
    public float a = 24.8963f;
    public float b = -16.4591f;
    public float c = 16.4591f;
    public float d = 0.524898f;
    public  float k = 40;



    private Queue<(float time, float rotation)> previousRotations = new Queue<(float, float)>();

    public override int Tickets => currentValue;

    private float GetRelativeReticlePosition() => ToRelative(reticle.transform.localPosition.y, reticleBounds.rect.yMin, reticleBounds.rect.yMax);

    protected override void Start()
    {
        base.Start();
        currentLeverRotation = defaultLeverRotation = leverTransform.localEulerAngles.x;
    }

    public override void Reset()
    {
        base.Reset();
        ticketsReceivedUI.gameObject.SetActive(false);

    }

    public override void StartGame()
    {
        base.StartGame();
        inputFinished = false;
        isHoldingHandle = false;
    }

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

    protected virtual bool OnInput_HoldBall(InputAction.CallbackContext ctx)
    {
        if (ctx.started && !inputFinished && OngoingGame &&
            reticle.transform.position.y > grabBallRegion.TransformPoint(grabBallRegion.rect.min).y &&
            reticle.transform.position.y < grabBallRegion.TransformPoint(grabBallRegion.rect.max).y)
            isHoldingHandle = true;
        else if (ctx.canceled)
            isHoldingHandle = false;
        return true;
    }

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0042:Deconstruct variable declaration", Justification = "It's more organized as is")]
    protected virtual void FixedUpdate()
    {
        if (!OngoingGame) return;
        if (isHoldingHandle)
        {
            leverTransform.localEulerAngles = Vector3.right * (currentLeverRotation = GetRelativeLeverRotation(Mathf.Clamp01(GetRelativeReticlePosition())));
            previousRotations.Enqueue((Time.timeSinceLevelLoad, leverTransform.localEulerAngles.x));
            while (previousRotations.Peek().time < Time.timeSinceLevelLoad - rotationCaptureTimeframe)
                previousRotations.Dequeue();
            if (previousRotations.Peek().time < Time.timeSinceLevelLoad - rotationCaptureTimeframe / 2)
            {
                var last = previousRotations.Last();
                var first = previousRotations.Peek();
                float rotationVelocity = (last.rotation - first.rotation) / (last.time - first.time);
                if (rotationVelocity > maxRotationVelocity)
                    maxRotationVelocity = rotationVelocity;
            }
            rb_wheel.angularVelocity = Vector3.right * -maxRotationVelocity * rotationFactor;
        }
        else
        {
            if (maxRotationVelocity != 0)
            {
                inputFinished = true;
                if (maxRotationVelocity < rotationVelocityMin)
                {
                    maxRotationVelocity += rotationCorrectionSpeed * Time.deltaTime;
                    rb_wheel.angularVelocity = Vector3.right * -maxRotationVelocity * rotationFactor;
                }
                else maxRotationVelocity = 0;
            }
            leverTransform.localEulerAngles = Vector3.right * (currentLeverRotation = Mathf.MoveTowards(currentLeverRotation, defaultLeverRotation, leverReturnDegreesPerSecond * Time.deltaTime));
        }

        if (inputFinished && rb_wheel.IsSleeping())
        {
            inputFinished = false;
            OnGameEnd();
        }
    }

    private float GetRelativeLeverRotation(float x)
    {
        return (a * Mathf.Pow((b * x) + c, d)) + k;
    }

    public void OnWheelTriggerEnter(int value)
    {
            currentValue = value;
    }

    private static float ToRelative(float i, float min, float max)
        => (i - min) / (max - min);

    public static float ToAbsolute(float i, float min, float max)
        => ((max - min) * i) + min;
}
