using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using Utils;

namespace DebugConsole
{
    public class DebugController : MonoBehaviour
    {

        #region private variables

        private DebugCommand helpCommand;
        
        private bool showConsole;
        private bool showHelp;
        private string input;
        private Vector2 scroll;

        #endregion

        #region public variables

        public static List<object> commandList = new List<object>();

        #endregion

        #region monobehavior callbacks

        private void Awake()
        {
            helpCommand = new DebugCommand("help",
                "Show all commands",
                "help",
                () =>
                {
                    showHelp = true;
                });
            commandList.Add(helpCommand);
        }

        private void OnEnable()
        {
            GameInput.Register("DebugConsole",GameInput.ReferencePriorities.Character, OnToggleConsole);
            GameInput.Register("Return",GameInput.ReferencePriorities.Character, OnReturn);
        }
        private void OnDisable()
        {
            GameInput.Deregister("DebugConsole",GameInput.ReferencePriorities.Character, OnToggleConsole);
            GameInput.Deregister("Return",GameInput.ReferencePriorities.Character, OnReturn);
        }
        private void OnGUI()
        {
            if (!showConsole) { return; }
            float y = 0;
            if (showHelp)
            {
                 GUI.Box(new Rect( 0, y, Screen.width, 100), "");
                 Rect viewPort = new Rect(0, 0, Screen.width - 30, 20 * commandList.Count);
                 scroll = GUI.BeginScrollView(new Rect(0, y + 5, Screen.width, 90), scroll, viewPort);
                 for (int i = 0; i < commandList.Count; i++)
                 {
                     DebugCommandBase command = commandList[i] as DebugCommandBase;
                     string label = $"{command.commandFormate} - {command.commandDescription}";
                     Rect labelRect = new Rect(5, 20 * i, viewPort.width - 100, 20);
                     GUI.Label(labelRect, label);
                 }
                 GUI.EndScrollView();
                 y += 100;
                 
            }
           
         
            GUI.Box(new Rect(0,y,Screen.width,30),"");
            GUI.backgroundColor = new Color(0, 0, 0, 0);
            input = GUI.TextField(new Rect(10f, y + 5f, Screen.width - 20, 20), input);
        }

        #endregion

        #region  private methods

        #endregion
        private bool OnToggleConsole(InputAction.CallbackContext ctx)
        {
            if (ctx.performed)
            {
                showConsole = !showConsole;
            }
            return false;
        }  
        private bool OnReturn(InputAction.CallbackContext ctx)
        {
            if (ctx.performed && showConsole)
            {
               HandleInput();
               input = "";
            }
            return false;
        }

        private void HandleInput()
        {
            string[] properties = input.Split(' ');
            for (int i = 0; i < commandList.Count; i++)
            {
                DebugCommandBase commandBase = commandList[i] as DebugCommandBase;
                if (input.Contains(commandBase.commandId))
                {
                    
                    if (commandList[i] as DebugCommand != null)
                    {
                        (commandList[i] as DebugCommand)?.Invoke();
                    }
                    else if (commandList[i] as DebugCommand<bool> != null)
                    {
                        (commandList[i] as DebugCommand<bool>)?.Invoke(bool.Parse(properties[1]));
                    }
                    else if (commandList[i] as DebugCommand<int> != null)
                    {
                        (commandList[i] as DebugCommand<int>)?.Invoke(int.Parse(properties[1]));
                    }
                    else if (commandList[i] as DebugCommand<float> != null)
                    {
                        (commandList[i] as DebugCommand<float>)?.Invoke(float.Parse(properties[1]));
                    }
                    else if (commandList[i] as DebugCommand<string> != null)
                    {
                        (commandList[i] as DebugCommand<string>)?.Invoke(properties[1]);
                    }
                    else if (commandList[i] as DebugCommand<Vector2> != null)
                    {
                        (commandList[i] as DebugCommand<Vector2>)?.Invoke(
                            Vector2Extensions.GetVector2FromString(properties[1])
                        );
                    }
                    else if (commandList[i] as DebugCommand<Vector3> != null)
                    {
                        (commandList[i] as DebugCommand<Vector3>)?.Invoke(
                            Vector3Extensions.GetVector3FromString(properties[1])
                            );
                    }
                }
            }
        }
    }
}
