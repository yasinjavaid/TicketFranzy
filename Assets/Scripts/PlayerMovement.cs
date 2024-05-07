using System.Collections.Generic;

using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;

public class PlayerMovement : MonoBehaviour
{
    [FormerlySerializedAs("speed")]
    [SerializeField] protected float walkSpeed;
    [SerializeField] protected float runSpeed;
    [SerializeField] protected float accelerationTime;
    [SerializeField] protected float rotationMaxSpeed;
    [SerializeField] protected float rotationSmoothTime;
    [SerializeField] protected float rotationOffset;
    [SerializeField] protected Rigidbody myRigidbody;
    [SerializeField] protected Animator myAnimator;

    public static PlayerMovement LocalInstance { get; set; }

    public virtual Vector2 MoveInput { get; protected set; }

    public virtual bool RunInput { get; protected set; }

    public Vector3 CurrentVelocity { get; protected set; }

    public Vector3 CurrentAcceleration => _currentAcceleration;
    protected Vector3 _currentAcceleration;

    protected float targetAngleDeg;
    protected float currentRotationVelocity;

    protected List<ArcadeMachine> nearbyMachines = new List<ArcadeMachine>();

    public IEnumerable<ArcadeMachine> NearbyMachines => nearbyMachines;

    protected static Dictionary<int, Vector3> scenePositions = new Dictionary<int, Vector3>();

    public static bool LoadLastPos { get; set; } = true;

    private void Awake()
    {
        LocalInstance = this;
        if (LoadLastPos && scenePositions.TryGetValue(SceneManager.GetActiveScene().buildIndex, out Vector3 pos))
            transform.position = pos;
    }

    protected virtual void OnEnable()
    {
        GameInput.Register("CharacterMove", GameInput.ReferencePriorities.Character, OnMoveInput);
        GameInput.Register("CharacterRun", GameInput.ReferencePriorities.Character, OnRunInput);
    }

    protected virtual void OnDisable()
    {
        GameInput.Deregister("CharacterMove", GameInput.ReferencePriorities.Character, OnMoveInput);
        GameInput.Deregister("CharacterRun", GameInput.ReferencePriorities.Character, OnRunInput);
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

    protected virtual bool OnRunInput(InputAction.CallbackContext ctx)
    {
        RunInput = !ctx.canceled;
        return true;
    }

    public void Stop()
    {
        MoveInput = Vector2.zero;
        RunInput = false;
    }

    private void FixedUpdate()
    {
        Vector3 targetVelocity = MoveInput.ToVector3_XZ() * (RunInput ? runSpeed : walkSpeed);
        CurrentVelocity = Vector3.SmoothDamp(CurrentVelocity, targetVelocity, ref _currentAcceleration, accelerationTime, float.MaxValue, Time.deltaTime);
        if (myRigidbody)
        {
            myRigidbody.MovePosition(myRigidbody.position + (CurrentVelocity * Time.deltaTime));
            float newAngle = Mathf.SmoothDampAngle(myRigidbody.transform.localEulerAngles.y, targetAngleDeg, ref currentRotationVelocity, rotationSmoothTime, rotationMaxSpeed, Time.deltaTime * 4);
            myRigidbody.MoveRotation(Quaternion.AngleAxis(newAngle, Vector3.up));

            myAnimator.SetBool("Walking", !RunInput && MoveInput != Vector2.zero);
            myAnimator.SetBool("Running", RunInput && MoveInput != Vector2.zero);
        }
    }

    private void OnDestroy() => scenePositions[SceneManager.GetActiveScene().buildIndex] = transform.position;
}
