using System.Collections;

using UnityEngine;
using UnityEngine.UI.Extensions;

public class AnimationEvents : MonoBehaviour
{
    public AudioSource menuAudio;
    public AudioSource menuMusicLoop;
    public GameObject MainMenuCanvas;
    public GameObject TitlescreenCanvas;
    public GameObject CharCustomization;
    public float delayBeforeMusicStarts = 4.0f;

    public void Start() => StartCoroutine(DelayedMusicStart());

    IEnumerator DelayedMusicStart()
    {
        yield return new WaitForSecondsRealtime(delayBeforeMusicStarts);
        if (menuMusicLoop) menuMusicLoop.Play();
    }

    public void MakeMainMenuActive() => MainMenuCanvas.SetActive(true);

    public void MakeTitlescreenInactive() => TitlescreenCanvas.SetActive(false);

    public void MakeMainMenuInactive() => MainMenuCanvas.SetActive(false);

    public void MakeCustomizationActive() => CharCustomization.SetActive(true);

    public void MakeCustomizationInactive() => CharCustomization.SetActive(false);

    public void playSoundEffect(AudioClip clip) => menuAudio.PlayOneShot(clip);

    public UIParticleSystem Sparks;

    public void playSparks() => Sparks.StartParticleEmission();

    public UIParticleSystem ConfettiRed;

    public void playParticleELeft() => ConfettiRed.StartParticleEmission();

    public UIParticleSystem ConfettiYellow;

    public void playParticleSparks() => ConfettiYellow.StartParticleEmission();

    public UIParticleSystem ConfettiGreen;

    public void playParticleElectricOne() => ConfettiGreen.StartParticleEmission();

    public GameObject Object;

    public void SetOn() => Object.SetActive(true);

    public ParticleSystem Lines;

    public void playLines() => Lines.Play();

    public ParticleSystem Lines2;

    public void playLines2() => Lines2.Play();

    public ParticleSystem Spheres;

    public void playSpheres() => Spheres.Play();
}
