using System;
using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using UnityEngine;
using UnityEngine.InputSystem;

public class PianoSondData
{
    public string name;
    public string[] soundIds;
    public List<string> sounds = new List<string>();
}

public class Test : MonoBehaviour
{
    public GameObject cube;
    private Gamepad Gamepad => Gamepad.current;
    
    PianoSondData pianosoundData = new PianoSondData();

    private void OnEnable()
    {
        GameInput.Register("LeftStick", GameInput.ReferencePriorities.Character, Method);
        GameInput.Register("DpadUpperButton", GameInput.ReferencePriorities.Character, Method1);
        GameInput.Register("Button", GameInput.ReferencePriorities.Character, Method2);
    }

    private bool Method2(InputAction.CallbackContext ctx)
    {
        if (ctx.performed)
        {
            //ctx.ReadValue<>()
            Debug.Log("Button Pressed");
        }

        return true;
    }

    private bool Method1(InputAction.CallbackContext ctx)
    {
        if (ctx.performed)
        {
            //ctx.ReadValue<>()
            Debug.Log("D-PadUpperButtonPressed");
        }

        return true;
    }

    private void OnDisable()
    {
        GameInput.Deregister("LeftStick", GameInput.ReferencePriorities.Character, Method);
        GameInput.Deregister("DpadUpperButton", GameInput.ReferencePriorities.Character, Method1);
        GameInput.Deregister("Button", GameInput.ReferencePriorities.Character, Method2);
    }

    private bool Method(InputAction.CallbackContext ctx)
    {
        if (ctx.performed)
        {
            //ctx.ReadValue<>()
            Debug.Log("Performed");
        }

        return false;
    }

    private void Start()
    {
        pianosoundData.name = "TestSond";
        pianosoundData.soundIds = new string[]{"a","b","c","d","e","f"};
        pianosoundData.sounds.Add("a");
        pianosoundData.sounds.Add("a");
        pianosoundData.sounds.Add("a");
        pianosoundData.sounds.Add("a");
        pianosoundData.sounds.Add("a");
        string output =   JsonConvert.SerializeObject(pianosoundData);
        // Debug.Log(output);
    }

    private void Update()
    {
       // Debug.Log(Gamepad.leftStick.ReadValue() );
       float angle = Mathf.Atan2(Gamepad.leftStick.y.ReadValue(), Gamepad.leftStick.x.ReadValue()) * Mathf.Rad2Deg;
       cube.transform.eulerAngles = new Vector3(0, -angle, 0);
    //   Debug.Log(angle);
    }
}
