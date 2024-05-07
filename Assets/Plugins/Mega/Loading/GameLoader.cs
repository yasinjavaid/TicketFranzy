using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

using Newtonsoft.Json;

using UnityEditor;

using UnityEngine;

public static class GameLoader
{
    private static readonly Queue<string> saveQueue = new Queue<string>();
    private volatile static bool saveIssues;
    private volatile static Task saveTask;
    private volatile static byte currentSaveSlot;

    public static byte CurrentSaveSlot { get => currentSaveSlot; set => currentSaveSlot = value; }

    public static float SavedPlaytimeThisSession { get; private set; }
    public static bool LoadingComplete { get; private set; }

    public static string GameLogPath => GetFolderPath() + "GameLog.txt";
    public static string SaveFolder => GetFolderPath();

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0022:Use expression body for methods", Justification = "<Pending>")]
    private static string GetFolderPath()
    {
#if UNITY_EDITOR_LINUX || UNITY_STANDALONE_LINUX
       return UnityEngine.Application.persistentDataPath;
#endif
        return Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "\\My Games\\Mega Cat\\Dev\\";
    }

    public static string GetSaveSlotLocation() => GetSaveSlotLocation(CurrentSaveSlot);
    public static string GetSaveSlotLocation(int saveSlot, int trailIndex = 0) => $"{SaveFolder}Save_{saveSlot:00}_{trailIndex:00}.dtsave";
    public static string GetSaveSlotBackupLocation(int saveSlot, int trailIndex = 0) => $"{SaveFolder}Save_{saveSlot:00}_{trailIndex:00}.dtsavebkp";

    public static bool SaveIssues => saveIssues;
    public static bool SaveRunning => saveTask != null && !saveTask.IsCompleted && !saveTask.IsFaulted;
    public static Dictionary<byte, GameSave> GameSaves { get; set; } = new Dictionary<byte, GameSave>() { { 0, new GameSave(Application.version) } };

    /// <summary>
    /// Saves current game state to CurrentSaveSlot
    /// </summary>
    /// <returns>A boolean indicating if the saving worked or failed</returns>
    public static void SaveGame()
    {
        SetInfo();

        saveQueue.Enqueue(JsonConvert.SerializeObject(GameSave.Current, Formatting.Indented));

        if (saveTask == null || saveTask.IsCompleted)
            saveTask = Task.Run(SaveGameTask);
    }

    /// <summary>
    /// Saves current game state to CurrentSaveSlot
    /// </summary>
    /// <param name="screenshot">A screenshot that is a visual representation of this save</param>
    /// <returns>A boolean indicating if the saving worked or failed</returns>
    public static void SaveGame(Texture2D screenshot)
    {
        SetInfo();

        GameSave.Current.Screenshot = screenshot.EncodeToPNG();

        saveQueue.Enqueue(JsonConvert.SerializeObject(GameSave.Current, Formatting.Indented));

        if (saveTask == null || saveTask.IsCompleted)
            saveTask = Task.Run(SaveGameTask);
    }

    /// <summary>
    /// Saves current game state to CurrentSaveSlot
    /// </summary>
    /// <param name="screenshotTaker">An instance of a ScreenshotTaker that can provide a visual representation of this save</param>
    /// <returns>A boolean indicating if the saving worked or failed</returns>
    public static void SaveGame(ScreenshotTaker screenshotTaker)
    {
        SetInfo();
        screenshotTaker.TakeScreenshot(OnScreenshotTaken);

        static void OnScreenshotTaken(Texture2D texture)
        {
            GameSave.Current.Screenshot = texture.EncodeToPNG();

            saveQueue.Enqueue(JsonConvert.SerializeObject(GameSave.Current, Formatting.Indented));

            if (saveTask == null || saveTask.IsCompleted)
                saveTask = Task.Run(SaveGameTask);
        }
    }

    private static void SetInfo()
    {
        GameSave.AddPlaytime(TimeSpan.FromSeconds(Time.realtimeSinceStartup - SavedPlaytimeThisSession), save: false);
        SavedPlaytimeThisSession = Time.realtimeSinceStartup;
        GameSave.SetSuccessful();
    }

    private static void SaveGameTask()
    {
        while (saveQueue.Count > 0)
        {
            string json = saveQueue.Dequeue();
            if (SaveGame(currentSaveSlot, 0, json)) continue;
            Thread.Sleep(100);
            if (SaveGame(currentSaveSlot, 0, json)) continue;
            saveIssues = true; break;
        }
    }

    private static bool SaveGame(int saveSlot, int trailIndex, string json)
    {
        if (SaveFile(json, GetSaveSlotLocation(saveSlot, trailIndex)))
        {
            SaveFile(json, GetSaveSlotBackupLocation(saveSlot, trailIndex));
            return true;
        }
        else
        {
            if (File.Exists(GetSaveSlotLocation(saveSlot, trailIndex)))
                File.Delete(GetSaveSlotLocation(saveSlot, trailIndex));
            return false;
        }
    }

    private static bool SaveFile(string json, string location)
    {
        using (StreamWriter sw = File.CreateText(location))
        {
            sw.Write(json);
            sw.Flush();
        }
        return File.Exists(location) && json == File.ReadAllText(location);
    }

    public static DateTime GetModifiedTime(byte key)
        => File.Exists(GetSaveSlotLocation(key))
            ? File.GetLastAccessTime(GetSaveSlotLocation(key))
            : File.Exists(GetSaveSlotBackupLocation(key))
                ? File.GetLastAccessTime(GetSaveSlotBackupLocation(key))
                : default;

    public static IEnumerator LoadAllAsync()
    {
        LoadingComplete = false;
        yield return null;

        GameSaves = new Dictionary<byte, GameSave>();
        List<byte> slots = Directory.GetFiles(SaveFolder, $"Save_??_00.dtsave")
            .Select(s => byte.TryParse(s.Substring(s.Length - 12, 2), out byte b) ? b : (byte)255)
            .Where(b => b < 100).ToList();
        yield return null;

        slots.AddRange(Directory.GetFiles(SaveFolder, $"Save_??_00.dtsavebkp")
            .Select(s => byte.TryParse(s.Substring(s.Length - 12, 2), out byte b) ? b : (byte)255)
            .Where(b => b < 100 && !slots.Contains(b)));
        yield return null;

        foreach (byte slot in slots)
        {
            TaskAwaiter awaiter = Task.Run(() => LoadGame(slot)).GetAwaiter();
            while (!awaiter.IsCompleted)
                yield return null;
        }
        LoadingComplete = true;
    }

    /// <summary>
    /// Attempts to load a save from disk using provided saveSlot
    /// </summary>
    /// <param name="saveSlot">The slot ID thet should be loaded from disk</param>
    /// <returns>A boolean indicating if the loading worked or failed</returns>
    private static void LoadGame(byte saveSlot)
    {
        try
        {
            if (TryReadAllText(GetSaveSlotLocation(saveSlot), out string saveText))
                GameSaves[saveSlot] = JsonConvert.DeserializeObject<GameSave>(saveText);
            if (!loadSuccessful() && TryReadAllText(GetSaveSlotBackupLocation(saveSlot), out string backupSaveText))
                GameSaves[saveSlot] = JsonConvert.DeserializeObject<GameSave>(backupSaveText);
        }
        catch (Exception e) { Debug.LogException(e); }

        bool loadSuccessful() => GameSaves.TryGetValue(saveSlot, out GameSave save) && save.Successful && AreCompatibleSaveVersions(save.GameVersion, Application.version);
    }

    private static bool TryReadAllText(string location, out string text)
    {
        if (File.Exists(location))
        {
            text = File.ReadAllText(location);
            return true;
        }
        else
        {
            text = null;
            return false;
        }
    }

    public static bool IsCompatibleSaveVersion(string version) => AreCompatibleSaveVersions(version, Application.version);


    [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0060:Remove unused parameter", Justification = "Method temporarily commented out")]
    public static bool AreCompatibleSaveVersions(string versionA, string versionB) => true;
    //=> Version.TryParse(versionA, out Version vA) && Version.TryParse(versionB, out Version vB) &&
    //vA.Major == vB.Major && vA.Minor == vB.Minor && vA.Build == vB.Build;
}
