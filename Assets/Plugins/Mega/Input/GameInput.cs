using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;

using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(PlayerInput))]
public class GameInput : SingletonMB<GameInput>
{
    #region Public

    public enum ReferencePriorities { Environment = -1000, Default = 0, Character = 1000, UI = 10000, Screen = 100000, Debug = 1000000, Unstoppable = int.MaxValue }

    public delegate bool GameInputMethodDelegate(InputAction.CallbackContext ctx);

    public string CurrentActionMap
    {
        get => GetPlayerInput ? GetPlayerInput.currentActionMap.name : null;
        set { if (GetPlayerInput) GetPlayerInput.SwitchCurrentActionMap(value); }
    }

    public PlayerInput GetPlayerInput => _playerInput ? _playerInput : _playerInput = GetComponent<PlayerInput>();

    public string GetCurrentControlScheme => GetPlayerInput ? GetPlayerInput.currentControlScheme : null;

    public int GetHighestRegisteredValue() => inputEvents.Select(ie => ie.Value.Keys.Max()).Max();
    public int GetHighestRegisteredValue(string actionName) => inputEvents.TryGetValue(actionName, out InputEventList eventList) ? eventList.Keys.Max() : int.MinValue;

    public bool ActionNameExists(string actionName) => GetPlayerInput.actions.actionMaps.Any(map => map.actions.Any(action => action.name == actionName));

    public static bool InputBlocker(InputAction.CallbackContext _) => true;

    #region Registers

    public static void Register(string actionName, ReferencePriorities priority, GameInputMethodDelegate method)
        => Register(actionName, (int)priority, method);

    public static void Register(string actionName, int priority, GameInputMethodDelegate method)
    {
        if (Instance && !string.IsNullOrEmpty(actionName) && Instance.TryGetOrAddInputEventList(actionName, out InputEventList eventList))
            Instance.Register(priority, method, eventList);
        else if (!Instance) Debug.LogError($"GameInput.Register({actionName}, {priority}, {method}) failed. No GameInput instance found");
        else Debug.LogError($"GameInput.Register({actionName}, {priority}, {method}) failed. Does this action name exist?", Instance);

    }

    public static void RegisterCatchAll(ReferencePriorities priority, GameInputMethodDelegate method)
        => RegisterCatchAll((int)priority, method);

    public static void RegisterCatchAll(int priority, GameInputMethodDelegate method)
    {
        if (Instance) Instance.Register(priority, method, Instance.GetOrAddCatchAllInputEventList());
        else Debug.LogError($"GameInput.RegisterCatchAll({priority}, {method}) failed. No GameInput instance found");
    }

    public static void Deregister(string actionName, ReferencePriorities priority, GameInputMethodDelegate method)
        => Deregister(actionName, (int)priority, method);

    public static void Deregister(string actionName, int priority, GameInputMethodDelegate method)
        => _ = Instance && Instance.inputEvents.TryGetValue(actionName, out InputEventList eventList) && Instance.Deregister(priority, method, eventList);

    public static void DeregisterCatchAll(ReferencePriorities priority, GameInputMethodDelegate method)
        => DeregisterCatchAll((int)priority, method);

    public static void DeregisterCatchAll(int priority, GameInputMethodDelegate method)
        => _ = Instance && Instance.inputEvents.TryGetValue(CATCH_ALL_ACTION_NAME, out InputEventList eventList) && Instance.Deregister(priority, method, eventList);

    #endregion

    #endregion

    #region Protected

    #region SubClasses

    public delegate bool RegisterDeregisterDelegate(InputAction.CallbackContext ctx);

    protected class InputEventList : SortedList<int, List<GameInputMethodDelegate>>
    {
        public InputEventList(IComparer<int> comparer) : base(comparer) { }
    }

    protected class EventPriorityComparer : IComparer<int> { public int Compare(int a, int b) => b.CompareTo(a); }

    #endregion

    #region Fields

    protected const string CATCH_ALL_ACTION_NAME = "";

    protected bool inputRunning;
    protected PlayerInput _playerInput;
    protected Queue<Action> queuedActions = new Queue<Action>();
    protected Dictionary<string, InputEventList> inputEvents = new Dictionary<string, InputEventList>();
    #endregion

    #region Methods

    protected bool TryGetOrAddInputEventList(string actionName, out InputEventList eventList)
    {
        if (inputEvents.TryGetValue(actionName, out eventList))
            return true;
        else if (ActionNameExists(actionName))
        {
            inputEvents.Add(actionName, eventList = new InputEventList(new EventPriorityComparer()));
            return true;
        }
        else return false;
    }

    protected InputEventList GetOrAddCatchAllInputEventList()
    {
        if (!inputEvents.TryGetValue(CATCH_ALL_ACTION_NAME, out var eventList))
            inputEvents.Add(CATCH_ALL_ACTION_NAME, eventList = new InputEventList(new EventPriorityComparer()));
        return eventList;
    }

    protected override void Awake()
    {
        base.Awake();
        GetPlayerInput.onActionTriggered += onActionTriggered;
    }

    protected void onActionTriggered(InputAction.CallbackContext ctx)
    {
        if (inputEvents.TryGetValue(ctx.action.name, out InputEventList eventList))
            OnInput(ctx, eventList);
    }

    protected void Register(int priority, GameInputMethodDelegate method, InputEventList eventList)
    {
        if (inputRunning) queuedActions.Enqueue(Register);
        else Register();
        void Register()
        {
            if (!eventList.TryGetValue(priority, out List<GameInputMethodDelegate> methodList))
                eventList.Add(priority, methodList = new List<GameInputMethodDelegate>());
            methodList.Add(method);
        }
    }

    protected bool Deregister(int priority, GameInputMethodDelegate method, InputEventList eventList)
    {
        if (eventList.TryGetValue(priority, out List<GameInputMethodDelegate> methodList))
        {
            Deregister(method, methodList);
            return true;
        }
        return false;
    }

    protected void Deregister(GameInputMethodDelegate method, List<GameInputMethodDelegate> methodList)
    {
        if (inputRunning) queuedActions.Enqueue(Deregister);
        else Deregister();

        void Deregister() => methodList.Remove(method);
    }

    protected void OnInput(InputAction.CallbackContext ctx, InputEventList sortedList)
    {
        inputRunning = true;
        bool inputConsumed = false;
        IEnumerator<KeyValuePair<int, List<GameInputMethodDelegate>>> enumerator = AppendCatchAllList(sortedList);
        while (enumerator.MoveNext())
        {
            List<GameInputMethodDelegate> inputMethodList = enumerator.Current.Value;
            for (int i = 0; !inputConsumed && i < inputMethodList.Count; i++)
            {
                try
                {
                    GameInputMethodDelegate methodDelegate = inputMethodList[i];
                    if (methodDelegate.Method.IsStatic || (methodDelegate.Target is UnityEngine.Object unityObject ? unityObject != null : methodDelegate.Target != null)) //Casting to UnityObject is important because they have their own implementation of null comparison
                        inputConsumed = (bool)methodDelegate.DynamicInvoke(ctx);
                    else inputMethodList.RemoveAt(i--);
                }
                catch (Exception e) { Debug.LogError($"GameInput.OnInput(): {e.GetType()} thrown by {inputMethodList[i].Target.GetType()} with priority {enumerator.Current.Key} and index {i}"); }
            }
            if (inputConsumed) break;
        }
        inputRunning = false;
        while (queuedActions.Count > 0)
            queuedActions.Dequeue().Invoke();
    }

    protected IEnumerator<KeyValuePair<int, List<GameInputMethodDelegate>>> AppendCatchAllList(InputEventList inputEventsList)
    {
        if (inputEvents.TryGetValue(CATCH_ALL_ACTION_NAME, out InputEventList catchAllEventsList) && catchAllEventsList.Count > 0)
        {
            if (inputEventsList.Count == 0)
                foreach (KeyValuePair<int, List<GameInputMethodDelegate>> listKVP in catchAllEventsList)
                    yield return listKVP;
            else while (true)
                {
                    var inputEnumerator = inputEventsList.GetEnumerator();
                    var catchAllEnumerator = catchAllEventsList.GetEnumerator();

                    inputEnumerator.MoveNext();
                    catchAllEnumerator.MoveNext();

                    if (catchAllEnumerator.Current.Key > inputEnumerator.Current.Key)
                    {
                        yield return catchAllEnumerator.Current;
                        if (!catchAllEnumerator.MoveNext())
                        {
                            do { yield return inputEnumerator.Current; }
                            while (inputEnumerator.MoveNext());
                            break;
                        }
                    }
                    else
                    {
                        yield return inputEnumerator.Current;
                        if (!inputEnumerator.MoveNext())
                        {
                            do { yield return catchAllEnumerator.Current; }
                            while (catchAllEnumerator.MoveNext());
                            break;
                        }
                    }
                }
        }
        else foreach (KeyValuePair<int, List<GameInputMethodDelegate>> listKVP in inputEventsList)
                yield return listKVP;
    }

    #endregion

    #endregion
}