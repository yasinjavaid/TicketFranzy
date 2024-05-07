using System;
using System.Collections.Generic;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;
using UnityEngine.InputSystem;

public class NetworkPlayerMovement : MonoBehaviour
{
    [SerializeField] protected float speed;
    [SerializeField] protected float accelerationTime;
    [SerializeField] protected float rotationMaxSpeed;
    [SerializeField] protected float rotationSmoothTime;
    [SerializeField] protected float rotationOffset;
    [SerializeField] protected Rigidbody myRigidbody;
    [SerializeField] protected Animator myAnimator;
    [SerializeField] private float thresholdChange = 0.05f;

    public virtual Vector2 MoveInput { get; protected set; }

    public Vector3 CurrentVelocity { get; protected set; }

    public Vector3 CurrentAcceleration => _currentAcceleration;
    protected Vector3 _currentAcceleration;

    protected float targetAngleDeg;
    protected float currentRotationVelocity;
    
    protected List<ArcadeMachine> nearbyMachines = new List<ArcadeMachine>();

    public IEnumerable<ArcadeMachine> NearbyMachines => nearbyMachines;
    
    private Vector3 lastTransformPos;


    protected void Start()
    {
        lastTransformPos = transform.position;
    }

    protected virtual void OnEnable()
    {
        if (NetworkManager.Instance.LocalPlayer.IsLocal)
        {
            GameInput.Register("CharacterMove", GameInput.ReferencePriorities.Character, OnMoveInput);
        }
    }
    
    protected virtual void OnDisable()
    {
        if (NetworkManager.Instance.LocalPlayer.IsLocal)
        {
            GameInput.Deregister("CharacterMove", GameInput.ReferencePriorities.Character, OnMoveInput);
        }
    }


    private void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponentInParent(out ArcadeMachine arcadeMachine))
            nearbyMachines.Add(arcadeMachine);
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.TryGetComponentInParent(out ArcadeMachine arcadeMachine))
            nearbyMachines.Remove(arcadeMachine);
    }

    protected virtual bool OnMoveInput(InputAction.CallbackContext ctx)
    {
        MoveInput = ctx.ReadValue<Vector2>();
        if (MoveInput != Vector2.zero)
            targetAngleDeg = (Mathf.Atan2(MoveInput.y, -MoveInput.x) * Mathf.Rad2Deg) + rotationOffset;
        return true;
    }

    public void Stop() => MoveInput = Vector2.zero;

    private void Update()
    {
        var playerOffset = transform.position - lastTransformPos;
        if (playerOffset.magnitude >= thresholdChange)
        {
            lastTransformPos = transform.position;
            SendCustomEventForPlayerTransform();
        }
      
    }

    private void FixedUpdate()
    {
        if (!NetworkManager.Instance.LocalPlayer.IsLocal)
        {
            return;
        }
        Vector3 targetVelocity = MoveInput.ToVector3_XZ() * speed;
        CurrentVelocity = Vector3.SmoothDamp(CurrentVelocity, targetVelocity, ref _currentAcceleration, accelerationTime, float.MaxValue, Time.deltaTime);
        if (myRigidbody)
        {
            myRigidbody.MovePosition(myRigidbody.position + (CurrentVelocity * Time.deltaTime));
            float newAngle = Mathf.SmoothDampAngle(myRigidbody.transform.localEulerAngles.y,
            targetAngleDeg == 0? 
                myRigidbody.transform.localEulerAngles.y :
                targetAngleDeg, 
                ref currentRotationVelocity, 
                rotationSmoothTime, 
                rotationMaxSpeed, 
                Time.deltaTime * 4);
            myRigidbody.MoveRotation(Quaternion.AngleAxis(newAngle, Vector3.up));

            myAnimator.SetBool("Walking", MoveInput != Vector2.zero);
        }
    }

    private void SendCustomEventForPlayerTransform()
    {
        if (NetworkManager.Instance.LocalPlayer.IsLocal)
        {
            object[] dataToSend = new object[]
            {
                NetworkManager.Instance.LocalPlayer.ActorNumber,
                transform.position,
                transform.rotation,
                myAnimator.GetBool("Walking")
            };
            NetworkManager.Instance.RaisePhotonEvent(dataToSend, 1, NetworkManager.PlayerPositionMallEventCode);
        }
    }
}
