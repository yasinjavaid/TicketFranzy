using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

public class Titlescreen : MonoBehaviour
{
    [SerializeField] protected UnityEvent onStartInput;

    private void OnEnable() => GameInput.Register("Start", GameInput.ReferencePriorities.Screen, OnStartInput);

    private void OnDisable() => GameInput.Deregister("Start", GameInput.ReferencePriorities.Screen, OnStartInput);

    private bool OnStartInput(InputAction.CallbackContext ctx)
    {
        if (ctx.started)
            onStartInput?.Invoke();
        return true;
    }
}
