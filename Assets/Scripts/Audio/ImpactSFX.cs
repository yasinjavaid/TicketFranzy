
using UnityEngine;

public class ImpactSFX : MonoBehaviour
{
    [SerializeField] protected AudioSource audioSource;
    [SerializeField] protected AnimationCurve volumeCurve = new AnimationCurve(new Keyframe(0, 0), new Keyframe(5, 1));
    [SerializeField] protected AnimationCurve pitchCurve = new AnimationCurve(new Keyframe(0, 1), new Keyframe(5, 1));

    public void Play(float relativeVelocity)
    {
        if (volumeCurve != null) audioSource.volume = volumeCurve.Evaluate(relativeVelocity);
        if (pitchCurve != null) audioSource.pitch = pitchCurve.Evaluate(relativeVelocity);
        audioSource.Play();
    }

    private void OnCollisionEnter(Collision collision) => Play(collision.relativeVelocity.magnitude);

    private void OnCollisionEnter2D(Collision2D collision) => Play(collision.relativeVelocity.magnitude);
}
