using System;
using System.Collections.Generic;
using System.Linq;

using UnityEngine;

public static class Auxiliar
{
    public static float GetRadAngleBetweenPoints(Vector3 p1, Vector3 p2) => Mathf.Atan2(p2.y - p1.y, p2.x - p1.x);

    public static float GetDegAngleBetweenPoints(Vector3 p1, Vector3 p2) => Mathf.Atan2(p2.y - p1.y, p2.x - p1.x) * Mathf.Rad2Deg;

    public static Vector2 GetAngleVectorBetweenPoints(Vector3 p1, Vector3 p2) => GetRadAngleBetweenPoints(p1, p2).RadToVector();

    public static float NaNToZero(float f) => float.IsNaN(f) ? 0 : f;

    public static Vector2 NaNToZero(Vector2 v)
    {
        v.x = NaNToZero(v.x);
        v.y = NaNToZero(v.y);
        return v;
    }

    public static Vector3 NaNToZero(Vector3 v)
    {
        v.x = NaNToZero(v.x);
        v.y = NaNToZero(v.y);
        v.z = NaNToZero(v.z);
        return v;
    }

    public static Vector4 NaNToZero(Vector4 v)
    {
        v.x = NaNToZero(v.x);
        v.y = NaNToZero(v.y);
        v.z = NaNToZero(v.z);
        v.w = NaNToZero(v.w);
        return v;
    }

    public static bool IsValid(Vector3 v) => !float.IsNaN(v.x) && !float.IsNaN(v.y) && !float.IsNaN(v.z);

    public static Vector2 InvertXY(Vector2 v) => new Vector2(v.y, v.x);

    public static float DistanceToCollider(this Vector2 point, Collider2D collider)
        => Vector2.Distance(collider.ClosestPoint(point), point);

    public static float DistanceToCollider(this Vector2 point, GameObject go)
        => go.GetComponentsInChildren<Collider2D>()
        .Min(c => point.DistanceToCollider(c));

    public static float DistanceToCollider(this Vector2 point, GameObject go, LayerMask layerMask)
        => go.GetComponentsInChildren<Collider2D>()
        .Where(c => layerMask.Includes(c.gameObject.layer))
        .Min(c => point.DistanceToCollider(c));

    public static string ReplaceCentralized(string s, int index, string text)
    {
        int startIndex = Mathf.RoundToInt(index - (text.Length / 2f));
        s = s.Remove(startIndex, text.Length);
        return s.Insert(startIndex, text);
    }

    public static string Replace(string s, int index, string text)
    {
        s = s.Remove(index, text.Length);
        return s.Insert(index, text);
    }

    public static GameObject PlayOneShot(this AudioClip audioClip)
        => PlayOneShot(audioClip, Camera.main.transform.position);
    public static GameObject PlayOneShot(this AudioClip audioClip, Vector2 worldPosition)
        => PlayOneShot(audioClip, new Vector3(worldPosition.x, worldPosition.y, Camera.main.transform.position.z));
    public static GameObject PlayOneShot(this AudioClip audioClip, Vector3 worldPosition, float volume = 1f)
    {
        if (audioClip == null) return null;
        GameObject go = new GameObject("Temporary Audio Source");
        go.transform.position = worldPosition;
        AudioSource audioSource = go.AddComponent<AudioSource>();
        audioSource.playOnAwake = false;
        audioSource.volume = volume;
        audioSource.PlayOneShot(audioClip);
        go.Destroy(audioClip.length);
        return go;
    }

    /// <summary>
    /// Stops playing or closes the game
    /// </summary>
    public static void QuitGame() =>
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#elif UNITY_WEBPLAYER
         Application.OpenURL(webplayerQuitURL);
#else
         Application.Quit();
#endif
}