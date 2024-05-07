using System.Linq;

using UnityEngine;

public class TimingMeter : MonoBehaviour
{
    public AnimationCurve positionOverTime = new AnimationCurve(new Keyframe[] { new Keyframe(0, 0), new Keyframe(1, 1), });
    [HideInInspector] public float value;
    [HideInInspector] public float time;
    public float velocity = 1;

    public float GetMinTime() => positionOverTime.keys.Min(frame => frame.time);
    public float GetMaxTime() => positionOverTime.keys.Max(frame => frame.time);

    protected virtual void Update()
    {
        float minTime = GetMinTime();
        float maxTime = GetMaxTime();

        time += velocity * Time.deltaTime;

        if (velocity > 0 && time > maxTime)
        {
            time = maxTime - (time - maxTime);
            velocity = -velocity;
        }
        else if (velocity < 0 && time < minTime)
        {
            time = minTime - (time - minTime);
            velocity = -velocity;
        }

        value = positionOverTime.Evaluate(time);
    }
}
