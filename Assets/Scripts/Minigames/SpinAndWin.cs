using DebugConsole;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;


public class SpinAndWin : ArcadeGame
{
    [SerializeField] protected Transform leverTransform;
    [SerializeField] protected float leverReturnDegreesPerSecond;
    [SerializeField] protected RectTransform grabBallRegion;
    [SerializeField] protected RectTransform stopButton;
    [SerializeField, Range(0, 1)] protected float rotationCaptureTimeframe;

    [Header("Light")]
    [Range(0.0f, 10.0f)]
    [SerializeField] private float lightSpeed;
    private float lightDelay;
    [SerializeField] private List<SpinLights> Lights;
    public float blinkDelay = 0.4f;
    private float blinkTimer;


    [Header("GUI")]
    [SerializeField] private GameObject Panel;
    [SerializeField] private TextMeshPro ticketText;

    private bool isHoldingHandle;
    private bool inputFinished;
    private float defaultLeverRotation;
    private float currentLeverRotation;
    protected int currentValue;

    private int currentLight;
    private bool rotateLights;
    private bool enableBlink;

    public override int Tickets => currentValue;


    private float GetRelativeReticlePosition() => ToRelative(reticle.transform.localPosition.y, reticleBounds.rect.yMin, reticleBounds.rect.yMax);


    private Queue<(float time, float rotation)> previousRotations = new Queue<(float, float)>();

    //Lever variables
    private float a = 20f;
    private float b = -15f;
    private float c = 15f;
    private float d = 0.3f;
    private float k = 0;

    protected override void Awake()
    {
        base.Awake();
        setDebugCommands();
    }



    protected override void Start()
    {
        StopGame();
        enableBlink = false;
        base.Start();
    }

    public override void StartGame()
    {
        ticketsReceivedUI.gameObject.SetActive(false);
        Panel.SetActive(false);
        base.StartGame();
        enableBlink = true;
        blinkTimer = 0;
        currentLight = 0;
        currentLeverRotation = defaultLeverRotation = leverTransform.localEulerAngles.x;
        stopAllLights();
        rotateLights = false;
        lightDelay = getLightDelay();
    }

    public override void Reset()
    {
        base.Reset();
        inputFinished = false;
        isHoldingHandle = false;
        enableBlink = false;
        stopAllLights();
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

        if(ctx.started && OngoingGame && inputFinished && reticle.transform.position.y > stopButton.TransformPoint(stopButton.rect.min).y && reticle.transform.position.y < stopButton.TransformPoint(stopButton.rect.max).y && reticle.transform.position.x > stopButton.TransformPoint(stopButton.rect.min).x && reticle.transform.position.x < stopButton.TransformPoint(stopButton.rect.max).x)
        {
            rotateLights = false;
            currentValue = Lights[GetLightIndex(currentLight)].getPoints();
            Lights[GetLightIndex(currentLight - 2)].DisableEmission();
            ticketText.text = currentValue.ToString();
            Panel.SetActive(true);
            enableBlink = true;
            OnGameEnd();
            //call stop light function here
//            FinishInput();
        }

        return true;
    }

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0042:Deconstruct variable declaration", Justification = "It's more organized as is")]
    protected virtual void FixedUpdate()
    {
        if (!OngoingGame)
        {
            if (enableBlink)
            {
                if(blinkTimer <= 0)
                {
                    Lights[GetLightIndex(currentLight - 1)].blinkLight();
                    blinkTimer = blinkDelay;
                }
                blinkTimer -= Time.deltaTime;
                
            }
            return;
        }
        if(!enableBlink && !rotateLights)
        {
            Lights[GetLightIndex(currentLight - 1)].DisableEmission();
        }


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
                float changeRotation = (last.rotation - first.rotation);



                if (changeRotation != 0 && !rotateLights)
                {
                    rotateLights = true;
                    currentLight = 0;
                }
            }
        }
        else
        {
            if (rotateLights)
            {
                if (!inputFinished)
                {
                    inputFinished = true;
                }
            }
            if (inputFinished)
            {
                leverTransform.localEulerAngles = Vector3.right * (currentLeverRotation = Mathf.MoveTowards(currentLeverRotation, defaultLeverRotation, leverReturnDegreesPerSecond * Time.deltaTime));
            }
            //if (inputFinished && rb_wheel.IsSleeping())
            //    OnGameEnd();
        }

        lightDelay = lightDelay - Time.deltaTime;
        if (lightDelay < 0)
        {
            SpinLights(rotateLights);
            lightDelay = getLightDelay();
        }

    }


    private float GetRelativeLeverRotation(float x)
    {
        //float a = 24.8963f;
        //float b = -16.4591f;
        //float c = 16.4591f;
        //float d = 0.524898f;
        //float k = 0;
        return (a * Mathf.Pow((b * x) + c, d)) + k;
    }



    private static float ToRelative(float i, float min, float max)
        => (i - min) / (max - min);

    public static float ToAbsolute(float i, float min, float max)
        => ((max - min) * i) + min;

    #region lights

    public void stopAllLights()
    {
        foreach(SpinLights light in Lights) 
        {
            light.DisableEmission();
        }
    }
    
    private void SpinLights(bool isRotating)
    {
        if (isRotating)
        {
            currentLight = GetLightIndex(currentLight + 1);
            Lights[GetLightIndex(currentLight)].EnableEmission();
            Lights[GetLightIndex(currentLight - 1)].EnableEmission();
            Lights[GetLightIndex(currentLight - 2)].EnableEmission();
            Lights[GetLightIndex(currentLight - 3)].DisableEmission();
        }
    }

    private int GetLightIndex(int index)
    {
        int val = index;
        if(val >= Lights.Count)
        {
            val = val - Lights.Count;
        }

        if(val < 0)
        {
            val = Lights.Count + val;
        }


        return val;
    }

    private float getLightDelay()
    {
        return (10 - lightSpeed) * 0.01f;
    }

    #endregion

    #region Debug
    private void setDebugCommands()
    {
        DebugCommand<float> SetRotationSpeed = new DebugCommand<float>("set_speed",
        "set speed of light around the wheel",
        "set_speed <seconds>",
        (xx) => { lightSpeed = xx; });

        DebugController.commandList.Add(SetRotationSpeed);
    }

    #endregion

}
