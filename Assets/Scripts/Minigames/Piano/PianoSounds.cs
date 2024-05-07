using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Hellmade.Sound;

public class PianoSounds : MonoBehaviour
{
    [SerializeField] public AudioClip[] sounds;
    public Dictionary<string, AudioClip> pianoSounds = new Dictionary<string, AudioClip>();
    // Start is called before the first frame update

    private void Awake()
    {
        for (int i = 0; i < sounds.Length; i++)
        {
            pianoSounds.Add(sounds[i].name, sounds[i]);
        }
    }

    private void OnEnable()
    {
        PianoEvents.playPianoButtonSound += PlaySound;
    }

    private void OnDisable()
    {
        PianoEvents.playPianoButtonSound -= PlaySound;
    }

    public void PlaySound(string key)
    {
        AudioClip sound = null; 
        if(pianoSounds.TryGetValue(key.ToUpper(), out sound))
        {
            EazySoundManager.PlayMusic(sound, 1, false, false);
        }
 
    }
}
