using Cinemachine;

using System;

using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public abstract class ArcadeGame : MonoBehaviour
{
    [Header("Arcade Game")]
    [SerializeField] string Name;
    [SerializeField] protected TicketsReceivedUI ticketsReceivedUI;
    [SerializeField] TicketDispenser ticketsDispenser;
    [SerializeField] UnityEvent onGameEnd;
    [SerializeField, Range(0, 10)] protected float delayToTicketCamera = 3;
    [SerializeField, Range(0, 10)] protected float delayToEndGameEvents = 4;
    [SerializeField] protected Vector3 gravity = new Vector3(0, -9.81f, 0);

    [SerializeField] protected Transform virtualCamerasParent;

    [SerializeField] protected Image reticle;
    [SerializeField] protected RectTransform reticleBounds;
    [SerializeField] protected Image img_chargeFill;
    [SerializeField] protected CanvasFader chargeFader;
    [SerializeField] protected float moveReticleSensitivity;
    [SerializeField, Range(0, 1)] protected float minPowerInput = 0.1f;
    [SerializeField, Range(0, 1)] protected float minCharge = 0.1f;
    [SerializeField, Min(0)] protected float fullChargeTime = 1.5f;
    [SerializeField, Min(0)] protected float fixedTimeStep = 0.02f;
    [SerializeField, Min(0.0001f)] protected float defaultContactOffset = 0.01f;

    protected CinemachineVirtualCamera[] virtualCameras;

    public int Score = 0;

    protected DateTime chargeBeginTime;

    public virtual int Tickets => GetTicketFormulaProvider.GetTicketCount(Score);

    public Vector2 MoveReticleInput { get; set; }
    public Vector3 PointerPosition { get; set; }

    protected bool IsCharging => chargeBeginTime != default;

    protected float GetChargingTime() => IsCharging
        ? (float)(DateTime.Now - chargeBeginTime).TotalSeconds
        : 0f;

    protected virtual float GetChargeMultiplier(bool clampMinCharge = true) => Mathf.Clamp(GetChargingTime(), clampMinCharge ? minCharge : 0, fullChargeTime) / fullChargeTime;

    public TicketFormulaProvider GetTicketFormulaProvider => _ticketFormulaProvider ? _ticketFormulaProvider : _ticketFormulaProvider = GetComponent<TicketFormulaProvider>();
    protected TicketFormulaProvider _ticketFormulaProvider;

    public bool OngoingGame { get; set; }

    public bool IsOtherPlayer { get; set; }

    protected virtual void Awake()
    {
        virtualCameras = virtualCamerasParent.GetComponentsInChildren<CinemachineVirtualCamera>();
        Time.fixedDeltaTime = fixedTimeStep;
        Physics.defaultContactOffset = defaultContactOffset;
    }

    protected virtual void Start() => this.InvokeDelayed(0.1f, StartGame);

    protected virtual void OnEnable()
    {
        GameInput.Register("MoveReticle", GameInput.ReferencePriorities.Character, OnInput_MoveReticle);
        GameInput.Register("Pointer", GameInput.ReferencePriorities.Character, OnInput_Pointer);
    }

    protected virtual void OnDisable()
    {
        GameInput.Deregister("MoveReticle", GameInput.ReferencePriorities.Character, OnInput_MoveReticle);
        GameInput.Deregister("Pointer", GameInput.ReferencePriorities.Character, OnInput_Pointer);
    }

    public virtual void Reset()
    {
        Physics.gravity = gravity;
        Score = 0;
        OngoingGame = false;
        ticketsReceivedUI.Hide();
        ticketsDispenser.Clear();
    }
    
    public virtual void StartGame()
    {
        if (!OngoingGame)
        {
            Reset();
            OngoingGame = true;
            SetCamera("Start");
        }
    }

    public virtual void StopGame() => OngoingGame = false;

    public virtual void OnScored(int value) { if (OngoingGame) Score += value; }

    public virtual void OnGameEnd()
    {
        StopGame();
        this.InvokeDelayed(delayToTicketCamera, OnGameEnd_Camera);
        this.InvokeDelayed(delayToEndGameEvents, OnGameEnd_Delayed);
    }

    protected virtual void OnGameEnd_Camera() => SetCamera("Ticket");

    protected virtual void OnGameEnd_Delayed()
    {
        GameManager.AddTickets("Arcade Game", Name, Tickets);
        ticketsReceivedUI.gameObject.SetActive(true);
        ticketsReceivedUI.AnimateTickets(Tickets);
        ticketsDispenser.Print(Tickets);
        onGameEnd.Invoke();
    }

    protected virtual void SetCamera(string cameraName)
    {
        foreach (var vcam in virtualCameras)
            vcam.gameObject.SetActive(vcam.name.StartsWith(cameraName));
    }

    protected virtual void SetCamera(string cameraName, out CinemachineVirtualCamera camera)
    {
        camera = default;
        foreach (var vcam in virtualCameras)
            if (vcam.name.StartsWith(cameraName))
            {
                vcam.gameObject.SetActive(true);
                camera = vcam;
            }
            else vcam.gameObject.SetActive(false);
    }



    protected virtual bool OnInput_MoveReticle(InputAction.CallbackContext ctx)
    {
        MoveReticleInput = Vector2.ClampMagnitude(ctx.ReadValue<Vector2>(), 1);
        reticle.enabled = true;
        return true;
    }

    protected virtual bool OnInput_Pointer(InputAction.CallbackContext ctx)
    {
        if (ctx.performed && Physics.Raycast(Camera.main.ScreenPointToRay(ctx.ReadValue<Vector2>()), out RaycastHit hit, 100, LayerMask.GetMask("CapturePointer"), QueryTriggerInteraction.Collide))
        {
            PointerPosition = ctx.ReadValue<Vector2>();
            reticle.enabled = false;
        }
        return true;
    }

    protected virtual Vector2 PointerToReticlePosition(Vector2 pointerPosition)
        => Physics.Raycast(Camera.main.ScreenPointToRay(pointerPosition), out RaycastHit hit, 100,
            LayerMask.GetMask("CapturePointer"), QueryTriggerInteraction.Collide)
            ? (Vector2)hit.point
            : Vector2.zero;

    protected virtual void MoveReticleBy(Vector2 delta) => MoveReticleTo((Vector2)reticle.rectTransform.position + delta);

    protected virtual void MoveReticleTo(Vector2 position)
    {
        reticle.rectTransform.position = new Vector3(position.x, position.y, reticleBounds.position.z);
        if (img_chargeFill)
            img_chargeFill.rectTransform.position = reticle.rectTransform.position;
    }

    protected virtual void Update()
    {
        if (GameInput.Instance.GetCurrentControlScheme == "PC")
            MoveReticleTo(PointerToReticlePosition(PointerPosition));
        MoveReticleBy(MoveReticleInput * moveReticleSensitivity);
        UpdateUI();
    }

    protected virtual void UpdateUI() => UpdateImg_ChargeFill();

    protected virtual void UpdateImg_ChargeFill()
    {
        if (img_chargeFill)
            img_chargeFill.fillAmount = GetChargeMultiplier(false);
        if (chargeFader)
            chargeFader.IsVisible = chargeBeginTime != default;
    }
}