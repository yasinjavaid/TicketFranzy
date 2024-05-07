using System.Collections;

using UnityEngine;

public class SceneFader : SingletonMB<SceneFader>
{
    [SerializeField] protected CanvasFader canvasFader;

    public static CanvasFader CanvasFader => Instance ? Instance.canvasFader : null;

    private IEnumerator Start()
    {
        yield return null;
        yield return null;
        if (canvasFader)
            canvasFader.FadeOut();
    }
}
