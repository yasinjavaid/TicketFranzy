using UnityEngine;

using UnityEngine.UI;

public abstract class SelectionFrame : MonoBehaviour
{
    [SerializeField] protected Vector2 borderSizes;
    [SerializeField, Min(0)] protected float transitionTime;
    public float TransitionTime => transitionTime;


    protected void MoveTo(RectTransform rect)
    {
        this.GetRectTransform().position = rect.position;
        this.GetRectTransform().SetWidth(rect.rect.size.x + (borderSizes.x * 2));
        this.GetRectTransform().SetHeight(rect.rect.size.y + (borderSizes.y * 2));
    }

    public virtual void Select(Selectable selectable) => Select(selectable ? selectable.GetRectTransform() : default);

    public virtual void Select(RectTransform rect)
    {
        if (!rect) return;

        LeanTween.size(this.GetRectTransform(), new Vector2(rect.rect.size.x + (borderSizes.x * 2), rect.rect.size.y + (borderSizes.y * 2)), transitionTime).setEaseInOutSine().setIgnoreTimeScale(true);
        LeanTween.value(gameObject, transform.position - rect.position, Vector3.zero, transitionTime).setEaseInOutSine().setIgnoreTimeScale(true)
            .setOnUpdate((Vector3 distance) => transform.position = rect.position + distance);
    }

    public virtual void Select(Vector3 position, Vector2 size)
    {
        LeanTween.move(gameObject, position, transitionTime).setEaseInOutSine().setIgnoreTimeScale(true);
        LeanTween.size(this.GetRectTransform(), new Vector2(size.x + (borderSizes.x * 2), size.y + (borderSizes.y * 2)), transitionTime).setEaseInOutSine().setIgnoreTimeScale(true);
    }

}