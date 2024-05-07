

using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.Serialization;

public class CanvasFader : MonoBehaviour
{
    [SerializeField] protected FloatRange fadeAlpha = new FloatRange(0, 1);
    [SerializeField] protected bool updateGameObjectActive;
    [FormerlySerializedAs("fadeTime")]
    [SerializeField] protected float fadeInTime;
    [FormerlySerializedAs("fadeTime")]
    [SerializeField] protected float fadeOutTime;
    [SerializeField] protected bool useScaledTime;

    [SerializeField] protected CanvasGroup canvasGroup;

    protected int change;
    bool isVisible = false;
    protected bool fadingIn;
    protected bool fadingOut;
    public bool IsVisible { get => isVisible; set { if (value) FadeIn(); else FadeOut(); } }
    public bool IsFullyVisible => IsVisible && change == 0;
    public bool IsFullyHidden => !IsVisible && change == 0;

    public void FadeIn()
    {
        change = 1;
        isVisible = true;
        if (updateGameObjectActive)
            gameObject.SetActive(true);
    }

    public void FadeOut()
    {
        change = -1;
        isVisible = false;
    }

    private void Update()
    {
        if (change == 0) return;

        float target = change > 0 ? fadeAlpha.Max : fadeAlpha.Min;
        float deltaTime = useScaledTime ? Time.deltaTime : Time.unscaledDeltaTime;

        canvasGroup.alpha = Mathf.MoveTowards(canvasGroup.alpha, target, deltaTime / (change > 0 ? fadeInTime : fadeOutTime));

        if (canvasGroup.alpha.Approximately(target, 0.01f))
        {
            canvasGroup.alpha = target;
            if (updateGameObjectActive)
                gameObject.SetActive(change > 0);
            change = 0;
        }
    }

    private void OnValidate() { if (!canvasGroup) canvasGroup = GetComponent<CanvasGroup>(); }
}
