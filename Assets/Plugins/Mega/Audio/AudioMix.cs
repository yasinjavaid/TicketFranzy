using UnityEngine;
using System.Linq;
using System.IO;
using UnityEngine.Audio;
#if UNITY_EDITOR
using UnityEditor;
#endif

[System.Serializable]
[CreateAssetMenu(fileName = "AudioMix", menuName = "Audio/AudioMix")]
public class AudioMix : ScriptableObject
{
    [SerializeField, Range(0, 1)] protected float volume = 1;
    [SerializeField, Range(-3, 3)] protected float pitch = 1;
    [SerializeField, Range(-1, 1)] protected float stereoPan = 0;
    [SerializeField] protected AudioMixerGroup mixerGroup = null;

    public float Volume => volume;
    public float Pitch => pitch;
    public float StereoPan => stereoPan;
    public AudioMixerGroup MixerGroup => mixerGroup;

#if UNITY_EDITOR
    [MenuItem("Assets/Audio/CreateMixFile", false)]
    protected static void CreateMixFile()
    {
        foreach (Object o in Selection.objects)
        {
            string path = Path.ChangeExtension(AssetDatabase.GetAssetPath(o), "asset");
            UnityEditorExtensions.LoadOrCreate<AudioMix>(path);
        }
    }

    [MenuItem("Assets/Audio/CreateMixFile", true)]
    protected static bool CreateMixFileValidation() => Selection.objects.All(o => o is AudioClip);
#endif
}
