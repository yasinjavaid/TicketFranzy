using System.Collections;

using UnityEngine;
using UnityEngine.Events;

public class TweenFloatCurve : MonoBehaviour
{
    public AnimationCurve tweenCurve;
    public float time = 1f;

    public FloatEvent valueEvent;
    public UnityEvent onComplete;

    [ContextMenu("Start Tween")]
    public void StartTween()
    {
        if(gameObject.activeInHierarchy)
            StartCoroutine(StartTweenRoutine());
    }

    IEnumerator StartTweenRoutine()
    {
        yield return null;

        float t = 0f;

        while(t < 1f)
        {
            t += Time.deltaTime / time;
            valueEvent.Invoke(tweenCurve.Evaluate(t));
            yield return null;
        }

        onComplete.Invoke();
    }
}

[System.Serializable]
public class FloatEvent : UnityEvent<float> { }