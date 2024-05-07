using MoreLinq;



using System;
using System.Linq;

using UnityEngine;
using UnityEngine.SceneManagement;

[Serializable]
[ExecuteInEditMode]
public class FauxVO : MonoBehaviour
{
    [SerializeField] protected CharacterVOInfo characterInfo;
    [SerializeField] protected AudioSource audioSource;
    [Range(0, 3)] protected float pitch;
    [Range(0, 3)] protected float amplitude;
    protected float evaluation;

    public float Frequency => 22.38f;
    public bool IsPlaying => audioSource.isPlaying;
    public float ElapsedTime { get; set; }

    public void Play() => audioSource.Play();

    public void Stop() => audioSource.Stop();

    protected void Update()
    {
        if (characterInfo)
        {
            pitch = characterInfo.FauxPitch;
            amplitude = characterInfo.FauxAmplitude;
        }

        if (audioSource)
        {
            if (Application.isPlaying && audioSource.isPlaying) ElapsedTime += Time.deltaTime;
            evaluation = Mathf.Lerp(pitch - amplitude, pitch + amplitude, Mathf.Abs(Mathf.Sin(Frequency * 2 * Mathf.PI * ElapsedTime)));
            audioSource.pitch = evaluation;
        }
    }

    public static FauxVO FindInScene()
    {
        GameObject sceneEssentials = SceneManager.GetActiveScene().GetRootGameObjects().FirstOrDefault(go => go.name == "SceneEssentials");
        if (sceneEssentials)
        {
            Transform t = sceneEssentials.transform.FindDeepChild("Audio/FauxVO");
            if (t && t.TryGetComponent(out FauxVO instance))
                return instance;
        }
        return FindObjectOfType<FauxVO>();
    }
}
