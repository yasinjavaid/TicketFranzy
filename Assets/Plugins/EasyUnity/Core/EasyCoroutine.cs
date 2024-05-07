using EasyUnityInternals;

using System;
using System.Collections;

using UnityEngine;

public class EasyCoroutine
{
    protected IEnumerator enumerator;
    public bool IsRunning { get; protected set; }

    public EasyCoroutine(IEnumerator enumerator)
    { this.enumerator = enumerator ?? throw new ArgumentNullException(nameof(enumerator)); Start(); }
    public EasyCoroutine(YieldInstruction yieldInstruction, Action action)
        : this(action.EnumerateAfter(yieldInstruction)) { }
    public EasyCoroutine(CustomYieldInstruction yieldInstruction, Action action)
        : this(action.EnumerateAfter(yieldInstruction)) { }


    /// <summary>
    /// Starts this coroutine if it's not already running
    /// </summary>
    /// <returns>True if the coroutine has just been started, false if it was already running</returns>
    protected bool Start()
    {
        if (!IsRunning)
        {
            EasySingleton.GetInstance.StartCoroutine(enumerator);
            return IsRunning = true;
        }
        return false;
    }

    /// <summary>
    /// Starts or restarts this coroutine, whether it was running or not, with a new enumerator
    /// </summary>
    /// <param name="enumerator">The new enumerator to be used</param>
    /// <returns>True if the coroutine was already running and had to be reset, false otherwise</returns>
    protected bool Restart(IEnumerator enumerator)
    {
        bool b = Stop();
        this.enumerator = enumerator;
        Start();
        return b;
    }

    /// <summary>
    /// Stops the coroutine
    /// </summary>
    /// <returns>True if the coroutine was running and has just been stopped, false if it wasn't running already</returns>
    public bool Stop()
    {
        if (IsRunning)
        {
            EasySingleton.GetInstance.StopCoroutine(enumerator);
            IsRunning = false;
            return true;
        }
        return false;
    }

    ~EasyCoroutine() => Stop();


    /// <summary>
    /// Restarts an EasyCoroutine with a new enumerable, or starts a new coroutine if instance was null.
    /// </summary>
    /// <param name="instance">A reference to the instance field</param>
    /// <param name="enumerable">The enumerable to be used by the instance</param>
    /// <returns>True if the coroutine was running and had to be stopped, false otherwise</returns>
    public static bool StartNew(ref EasyCoroutine instance, IEnumerable enumerable)
        => StartNew(ref instance, enumerable.GetEnumerator());

    /// <summary>
    /// Restarts an EasyCoroutine with a new enumerator, or starts a new coroutine if instance was null.
    /// </summary>
    /// <param name="instance">A reference to the instance field</param>
    /// <param name="enumerator">The new enumerator to be used by the instance</param>
    /// <returns>True if the coroutine was running and had to be stopped, false otherwise</returns>
    public static bool StartNew(ref EasyCoroutine instance, IEnumerator enumerator)
    {
        if (instance != null) return instance.Restart(enumerator);
        instance = new EasyCoroutine(enumerator);
        return false;
    }

    /// <summary>
    /// Restarts an EasyCoroutine with a new enumerator, or starts a new coroutine if instance was null.
    /// </summary>
    /// <param name="instance">A reference to the instance field</param>
    /// <param name="yieldInstruction">The instruction to be awaited before the action is invoked</param>
    /// <param name="action">The action to be invoked once the instruction is complete</param>
    /// <returns>True if the coroutine was running and had to be stopped, false otherwise</returns>
    public static bool StartNew(ref EasyCoroutine instance, YieldInstruction yieldInstruction, Action action)
        => StartNew(ref instance, action.EnumerateAfter(yieldInstruction));

    /// <summary>
    /// Restarts an EasyCoroutine with a new enumerator, or starts a new coroutine if instance was null.
    /// </summary>
    /// <param name="instance">A reference to the instance field</param>
    /// <param name="yieldInstruction">The instruction to be awaited before the action is invoked</param>
    /// <param name="action">The action to be invoked once the instruction is complete</param>
    /// <returns>True if the coroutine was running and had to be stopped, false otherwise</returns>
    public static bool StartNew(ref EasyCoroutine instance, CustomYieldInstruction yieldInstruction, Action action)
        => StartNew(ref instance, action.EnumerateAfter(yieldInstruction));

    /// <summary>
    /// Stops a coroutine
    /// </summary>
    /// <returns>True if the coroutine was running and has just been stopped, false if it wasn't running already</returns>
    public static bool Stop(ref EasyCoroutine instance) => instance?.Stop() ?? false;

    /// <summary>
    /// Stops and disposes an EasyCoroutine
    /// </summary>
    /// <param name="instance">An instance to be stopped and disposed</param>
    /// <returns>True if the instance was not null, false otherwise</returns>
    public static bool Dispose(ref EasyCoroutine instance)
    {
        if (instance != null)
        {
            instance.Stop();
            instance = null;
            return true;
        }
        return false;
    }
}