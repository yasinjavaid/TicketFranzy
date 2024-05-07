using System.Collections.Generic;
using EasyUnityInternals;
using UnityEngine.SceneManagement;
using System;

public class ScenesTime : EasySingleton
{
    public static Dictionary<int, DateTime> SceneLoadedTimes { get; } = new Dictionary<int, DateTime>();

    public static Dictionary<int, DateTime> SceneUnloadedTimes { get; } = new Dictionary<int, DateTime>();

    private void OnEnable() => SceneLoadedTimes[SceneManager.GetActiveScene().buildIndex] = DateTime.Now;

    private void OnDisable() => SceneUnloadedTimes[SceneManager.GetActiveScene().buildIndex] = DateTime.Now;
}
