using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Runtime.InteropServices.WindowsRuntime;
using UnityEngine;
using UnityEngine.InputSystem;

public class HotShotTest : MonoBehaviour
{
    public float minimumDistance = .2f;
    public float maxTime = 1;
    public GameControls gameControls = new GameControls();
    public float zDepth = 15;
    public delegate void StartTouch(Vector2 position, float time);
    public event StartTouch OnStartTouch;
    public delegate void EndTouch(Vector2 position, float time);
    public event EndTouch OnEndTouch;

    private Vector2 startPoint;

    private Vector2 endPoint;

    private float startTime;
    private float endTime;
    
    // Start is called before the first frame update
    private void OnEnable()
    {
      //  GameInput.Register("Press",GameInput.ReferencePriorities.Character, Method);
      //  OnStartTouch += OnOnStartTouch;
      //  OnEndTouch += OnOnEndTouch;
    }

  
    private void OnOnStartTouch(Vector2 position, float time)
    {
        endPoint = position;
        endTime = time;
    }
    private void OnOnEndTouch(Vector2 position, float time)
    {
        startPoint = position;
        startTime = time;
        DetectSwipe();
    }

    public void DetectSwipe()
    {
        if (Vector3.Distance(startPoint, endPoint) >= minimumDistance 
            && (endTime - startTime) <= maxTime)
        {
            Debug.Log("Swipe Detec");
        }
    }


    private bool Method(InputAction.CallbackContext ctx)
    {
        /*if (ctx.started)
        {
            if (OnStartTouch != null)
            {
                OnStartTouch(ScreenToWorldPosition(Camera.main, 
                    gameControls.HotShot.Position.ReadValue<Vector2>(), zDepth), (float)ctx.startTime);
            }
            Debug.Log("PressedDown");
        }
        else if(ctx.canceled)
        {
            if (OnEndTouch != null)
            {
                OnEndTouch(ScreenToWorldPosition(Camera.main, 
                    gameControls.HotShot.Position.ReadValue<Vector2>(), zDepth), (float)ctx.startTime);
            }
            Debug.Log("PressedUp");

        }*/
        return true;
    }

    private void OnDisable()
    {
        GameInput.Deregister("Position",GameInput.ReferencePriorities.Character, Method);
      //  OnStartTouch += OnOnStartTouch;
      //  OnEndTouch += OnOnEndTouch;
    }

    public Vector3 ScreenToWorldPosition(Camera cam, Vector2 screenPoint, float zDepth = default)
    {
        Vector3 cameraPoint = new Vector3(screenPoint.x, screenPoint.y, zDepth);
        return cam.ScreenToWorldPoint(cameraPoint);
    }

}
