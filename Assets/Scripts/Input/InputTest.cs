
using System;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.InputSystem;

public class InputTest : MonoBehaviour
{
    [SerializeField] private int maxInputCapacity = 1000;
    [SerializeField] private float maxIdleTime = .5f;
    [SerializeField] private float gizmoSizeMultiplier = 3f;
    [SerializeField] private float yMagnitudeCap = .8f;
    protected readonly List<Vector2> inputList = new List<Vector2>();
    Vector2 lowestPoint;
    Vector2 highestPoint;
    private DateTime inputCancelTime = DateTime.MaxValue;

    private void OnEnable() => GameInput.Register("LaunchBall", GameInput.ReferencePriorities.Character, OnInput_LaunchBall);

    private void OnDisable() => GameInput.Deregister("LaunchBall", GameInput.ReferencePriorities.Character, OnInput_LaunchBall);


    protected virtual bool OnInput_LaunchBall(InputAction.CallbackContext ctx)
    {
        if (ctx.performed)
        {
            Vector2 newInput = ctx.ReadValue<Vector2>();
            newInput.y = Mathf.Clamp(newInput.y, -yMagnitudeCap, yMagnitudeCap);
            inputList.Add(newInput);
            while (inputList.Count > maxInputCapacity)
                inputList.RemoveAt(0);
            if (newInput.y <= lowestPoint.y)
                lowestPoint = newInput;
            if (newInput.y >= highestPoint.y)
                highestPoint = newInput;
        }
        else if (ctx.canceled)
            inputCancelTime = DateTime.Now;
        else if (ctx.started && inputCancelTime < DateTime.Now - TimeSpan.FromSeconds(maxIdleTime))
        {
            inputList.Clear();
            lowestPoint = Vector2.zero;
            highestPoint = Vector2.zero;
        }
        return true;
    }

    private void OnDrawGizmos()
    {
        for (int i = 1; i < inputList.Count; i++)
            Gizmos.DrawLine(inputList[i - 1] * gizmoSizeMultiplier, inputList[i] * gizmoSizeMultiplier);
        Gizmos.color = Color.red;
        Gizmos.DrawLine(lowestPoint * gizmoSizeMultiplier, highestPoint * gizmoSizeMultiplier);
    }
}
