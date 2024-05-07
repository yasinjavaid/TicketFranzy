using MoreLinq;

using System;
using System.Collections.Generic;
using System.Linq;

using UnityEngine;

public class GameSave
{
    /// <summary>
    /// Convenience property to get currently running GameSave
    /// </summary>
    public static GameSave Current => GameLoader.GameSaves != null && GameLoader.GameSaves.TryGetValue(GameLoader.CurrentSaveSlot, out GameSave save) ? save : null;

    public static bool TryGetCurrent(out GameSave current) => (current = Current) != null;

    public GameSave() { }

    public GameSave(string version) => GameVersion = version;

    public GameSave(string version, string sceneName, bool save = true)
    {
        GameVersion = version;
        SceneName = sceneName;
        if (save) GameLoader.SaveGame();
    }

    /// <summary>
    /// PLEASE USE STATIC VERSIONS INSTEAD
    /// </summary>
    public string GameVersion;

    public bool IsCompatibleVersion => GameLoader.IsCompatibleSaveVersion(GameVersion);



    /// <summary>
    /// PLEASE USE STATIC VERSIONS INSTEAD
    /// </summary>
    public int Cash = 100;

    public static int GetCash() => Current?.Cash ?? 0;

    /// <summary>
    /// Sets the current amount of Cash. Saves only if value is different and save == true
    /// </summary>
    /// <param name="amount"></param>
    /// <param name="save"></param>
    public static void SetCash(int amount, bool save = true)
    {
        if (Current != null && Current.Cash != amount)
        {
            Current.Cash = amount;
            if (save) GameLoader.SaveGame();
        }
    }

    public static void AddCash(int addAmount) => SetCash(GetCash() + addAmount);



    /// <summary>
    /// PLEASE USE STATIC VERSIONS INSTEAD
    /// </summary>
    public string SceneName = "";

    public static string GetSceneName() => Current?.SceneName;

    /// <summary>
    /// Sets the currently open scene name. Saves only if value is different and save == true
    /// </summary>
    /// <param name="name"></param>
    /// <param name="save"></param>
    public static void SetSceneName(string name, bool save = false)
    {
        if (Current != null && Current.SceneName != name)
        {
            Current.SceneName = name;
            if (save) GameLoader.SaveGame();
        }
    }



    /// <summary>
    /// PLEASE USE STATIC VERSIONS INSTEAD
    /// </summary>
    public string PreviousSceneName = "";

    public static string GetPreviousSceneName() => Current?.PreviousSceneName;

    /// <summary>
    /// Sets the previously open scene name. Saves only if value is different and save == true
    /// </summary>
    /// <param name="name"></param>
    /// <param name="save"></param>
    public static void SetPreviousSceneName(string name, bool save = false)
    {
        if (Current != null && Current.PreviousSceneName != name)
        {
            Current.PreviousSceneName = name;
            if (save) GameLoader.SaveGame();
        }
    }



    /// <summary>
    /// PLEASE USE STATIC VERSIONS INSTEAD
    /// </summary>
    public HashSet<string> VisitedScenes = new HashSet<string>();

    public static bool GetVisitedScene(string key) => Current?.VisitedScenes?.Contains(key) ?? false;

    /// <summary>
    /// Sets whether an object has been interacted with. Saves only if value is different and save == true
    /// </summary>
    /// <param name="key"></param>
    /// <param name="value"></param>
    /// <param name="save"></param>
    public static void SetVisitedScene(string key, bool value = true, bool save = true)
    {
        if (Current == null) return;
        if (Current.VisitedScenes == null) Current.VisitedScenes = new HashSet<string>();
        bool changed = value ? Current.VisitedScenes.Add(key) : Current.VisitedScenes.Remove(key);
        if (changed && save) GameLoader.SaveGame();
    }



    /// <summary>
    /// PLEASE USE STATIC VERSIONS INSTEAD
    /// </summary>
    public long Playtime;

    public static TimeSpan GetPlaytime() => TimeSpan.FromTicks(Current?.Playtime ?? 0);

    /// <summary>
    /// Sets the total Playtime. Saves only if value is different and save == true
    /// </summary>
    /// <param name="playtime"></param>
    /// <param name="save"></param>
    public static void SetPlaytime(TimeSpan playtime, bool save = false)
    {
        if (Current != null && Current.Playtime != playtime.Ticks)
        {
            Current.Playtime = playtime.Ticks;
            if (save) GameLoader.SaveGame();
        }
    }

    public static void AddPlaytime(TimeSpan addPlaytime, bool save = true) => SetPlaytime(GetPlaytime() + addPlaytime, save);



    /// <summary>
    /// PLEASE USE STATIC VERSIONS INSTEAD
    /// </summary>
    public HashSet<string> InteractedObjects = new HashSet<string>();

    public static bool GetInteractedWith(string key) => Current?.InteractedObjects?.Contains(key) ?? false;

    /// <summary>
    /// Sets whether an object has been interacted with. Saves only if value is different and save == true
    /// </summary>
    /// <param name="key"></param>
    /// <param name="value"></param>
    /// <param name="save"></param>
    public static void SetInteractedWith(string key, bool value = true, bool save = true)
    {
        if (Current == null) return;
        if (Current.InteractedObjects == null) Current.InteractedObjects = new HashSet<string>();
        bool changed = value ? Current.InteractedObjects.Add(key) : Current.InteractedObjects.Remove(key);
        if (changed && save) GameLoader.SaveGame();
    }



    /// <summary>
    /// PLEASE USE STATIC VERSIONS INSTEAD
    /// </summary>
    public HashSet<string> EventsCompleted = new HashSet<string>();

    public static bool GetEventCompleted(string key) => Current?.EventsCompleted?.Contains(key) ?? false;

    /// <summary>
    /// Sets whether an event has been completed. Saves only if value is different and save == true
    /// </summary>
    /// <param name="key"></param>
    /// <param name="value"></param>
    /// <param name="save"></param>
    public static void SetEventCompleted(string key, bool value = true, bool save = true)
    {
        if (Current == null) return;
        if (Current.EventsCompleted == null) Current.EventsCompleted = new HashSet<string>();
        bool changed = value ? Current.EventsCompleted.Add(key) : Current.EventsCompleted.Remove(key);
        if (changed && save) GameLoader.SaveGame();
    }



    /// <summary>
    /// PLEASE USE STATIC VERSIONS INSTEAD
    /// </summary>
    public HashSet<string> TasksCompleted = new HashSet<string>();

    public static bool GetTaskCompleted(string key) => Current?.TasksCompleted?.Contains(key) ?? false;

    /// <summary>
    /// Sets whether a task has been completed. Saves only if value is different and save == true
    /// </summary>
    /// <param name="key"></param>
    /// <param name="value"></param>
    /// <param name="save"></param>
    public static void SetTaskCompleted(string key, bool value = true, bool save = true)
    {
        if (Current == null) return;
        if (Current.TasksCompleted == null) Current.TasksCompleted = new HashSet<string>();
        bool changed = value ? Current.TasksCompleted.Add(key) : Current.TasksCompleted.Remove(key);
        if (changed && save) GameLoader.SaveGame();
    }


    /// <summary>
    /// PLEASE USE STATIC VERSIONS INSTEAD
    /// </summary>
    public HashSet<string> TutorialsCompleted = new HashSet<string>();


    public static bool GetTutorialCompleted(string key) => Current?.TutorialsCompleted?.Contains(key) ?? false;

    /// <summary>
    /// Sets whether a tutorial has been completed. Saves only if value is different and save == true
    /// </summary>
    /// <param name="key"></param>
    /// <param name="value"></param>
    /// <param name="save"></param>
    public static void SetTutorialCompleted(string key, bool value = true, bool save = true)
    {
        if (Current == null) return;
        if (Current.TutorialsCompleted == null) Current.TutorialsCompleted = new HashSet<string>();
        bool changed = value ? Current.TutorialsCompleted.Add(key) : Current.TutorialsCompleted.Remove(key);
        if (changed && save) GameLoader.SaveGame();
    }



    /// <summary>
    /// PLEASE USE STATIC VERSIONS INSTEAD
    /// </summary>
    public Dictionary<string, int> QuestSteps = new Dictionary<string, int>();

    public static Dictionary<string, int> GetQuestSteps() => Current?.QuestSteps?.ToDictionary();

    public static int GetQuestStep(string key) => Current?.QuestSteps != null && Current.QuestSteps.TryGetValue(key, out int step) ? step : 0;

    /// <summary>
    /// Sets the current step on a quest. Saves only if value is different and save == true
    /// </summary>
    /// <param name="key"></param>
    /// <param name="step"></param>
    public static void SetQuestStep(string key, int step, bool save = true)
    {
        if (Current == null) return;
        if (Current.QuestSteps == null) Current.QuestSteps = new Dictionary<string, int>();
        if (GetQuestStep(key) != step)
        {
            Current.QuestSteps[key] = step;
            if (save) GameLoader.SaveGame();
        }
    }



    /// <summary>
    /// PLEASE USE STATIC VERSIONS INSTEAD
    /// </summary>
    public Dictionary<string, int> QuestBranches = new Dictionary<string, int>();

    public static Dictionary<string, int> GetQuestBranches() => Current?.QuestBranches?.ToDictionary();

    public static int GetQuestBranch(string key) => Current?.QuestBranches != null && Current.QuestBranches.TryGetValue(key, out int branch) ? branch : 0;

    /// <summary>
    /// Sets the current branch on a quest. Saves only if value is different and save == true
    /// </summary>
    /// <param name="key"></param>
    /// <param name="branch"></param>
    public static void SetQuestBranch(string key, int branch, bool save = true)
    {
        if (Current == null) return;
        if (Current.QuestBranches == null) Current.QuestBranches = new Dictionary<string, int>();
        if (GetQuestBranch(key) != branch)
        {
            Current.QuestBranches[key] = branch;
            if (save) GameLoader.SaveGame();
        }
    }



    /// <summary>
    /// PLEASE USE STATIC VERSIONS INSTEAD
    /// </summary>
    public Dictionary<string, int> OwnedItems = new Dictionary<string, int>();

    protected static void SetOwnedItems(Dictionary<string, int> ownedItems)
    {
        if (Current != null)
        {
            Current.OwnedItems = ownedItems;
            GameLoader.SaveGame();
        }
    }

    public static int GetOwnedAmount(string key) => Current?.OwnedItems != null && Current.OwnedItems.TryGetValue(key, out int amount) ? amount : 0;

    public static void SetOwnedAmount(string key, int amount, bool save = true)
    {
        if (Current == null) return;
        if (Current.OwnedItems == null) Current.OwnedItems = new Dictionary<string, int>();
        Current.OwnedItems[key] = amount;
        if (save) GameLoader.SaveGame();
    }

    public static void AddOwnedAmount(string key, int addAmount, bool save = true) => SetOwnedAmount(key, GetOwnedAmount(key) + addAmount, save);



    /// <summary>
    /// PLEASE USE STATIC VERSIONS INSTEAD
    /// </summary>
    public Dictionary<string, int> HighScores = new Dictionary<string, int>();

    public static int GetHighScore(string key)
    {
        if (Current == null) return default;
        if (Current.HighScores == null) Current.HighScores = new Dictionary<string, int>();
        return Current.HighScores.TryGetValue(key, out int score) ? score : default;
    }

    public static void SetHighScore(string key, int score, bool save = true)
    {
        if (Current == null) return;
        if (save && Current.HighScores.TryGetValue(key, out int oldScore) && oldScore == score) save = false;
        Current.HighScores[key] = score;
        if (save) GameLoader.SaveGame();
    }



    /// <summary>
    /// PLEASE USE STATIC VERSIONS INSTEAD
    /// </summary>
    public string TrackingQuest;

    public static string GetTrackingQuest() => Current?.TrackingQuest;

    public static void SetTrackingQuest(string key) { if (Current != null) Current.TrackingQuest = key; }



    public Dictionary<string, SaveVector3> ScenePositions;

    public static bool TryGetPositionInScene(string sceneName, out Vector3 position)
    {
        if (Current?.ScenePositions != null && Current.ScenePositions.TryGetValue(sceneName, out SaveVector3 pos))
        {
            position = pos;
            return true;
        }
        else
        {
            position = default;
            return false;
        }
    }

    public static void SetPositionInScene(string sceneName, Vector3 position)
    {
        if (Current == null) return;
        if (Current.ScenePositions == null) Current.ScenePositions = new Dictionary<string, SaveVector3>();
        Current.ScenePositions[sceneName] = position;
    }



    /// <summary>
    /// PLEASE USE STATIC VERSIONS INSTEAD
    /// </summary>
    public byte[] Screenshot;



    //This should always be the last thing to save/load
    /// <summary>
    /// PLEASE USE STATIC VERSIONS INSTEAD
    /// </summary>
    public bool Successful;

    public static void SetSuccessful() { if (Current != null) Current.Successful = true; }



    [Serializable]
    public class SaveVector2
    {
        public float x, y;

        public SaveVector2(float x, float y) { this.x = x; this.y = y; }

        public static implicit operator Vector2(SaveVector2 v) => new Vector2(v.x, v.y);
        public static implicit operator SaveVector2(Vector2 v) => new SaveVector2(v.x, v.y);
    }

    [Serializable]
    public class SaveVector3
    {
        public float x, y, z;

        public SaveVector3(float x, float y, float z) { this.x = x; this.y = y; this.z = z; }

        public static implicit operator Vector3(SaveVector3 v) => new Vector3(v.x, v.y, v.z);
        public static implicit operator SaveVector3(Vector3 v) => new SaveVector3(v.x, v.y, v.z);
    }
}