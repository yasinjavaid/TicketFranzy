using System;

using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class SelectionManager : MonoBehaviour
{
    [Tooltip("This is the object that is selected by default when this is enabled")]
    public Selectable Selected;

    [Tooltip("If assigned, the selection frame will receive events when a selection has changed")]
    public SelectionFrame selectionFrame;

    [Tooltip("This will be invoked once whenever the selection changes")]
    public UnityEvent<Selectable> OnSelectionChanged;

    [SerializeField]
    protected MixedAudioClip navigationClip;

    public static SelectionManager Instance { get; protected set; }

    protected void Start() { if (selectionFrame) OnSelectionChanged?.AddListener(selectionFrame.Select); }

    protected virtual void OnEnable()
    {
        this.InvokeDelayed(new WaitForEndOfFrame(), () => Select(Selected));
        Instance = this;
    }

    protected virtual void OnDisable() { if (Instance == this) Instance = null; }

    public virtual void MoveSelection(Vector2 input)
    {
        if (!gameObject.activeInHierarchy) return;
        if (!Selected) throw new NullReferenceException("SelectionManager.Selected is null");

        if (Mathf.Abs(input.x) > Mathf.Abs(input.y))
        {
            if (input.x > 0) Select(Selected.navigation.selectOnRight);
            else if (input.x < 0) Select(Selected.navigation.selectOnLeft);
        }
        else
        {
            if (input.y > 0) Select(Selected.navigation.selectOnUp);
            else if (input.y < 0) Select(Selected.navigation.selectOnDown);
        }
    }

    /// <summary>
    /// Invoke this to explicitly select a component
    /// </summary>
    /// <param name="selectable">The selectable you want to select</param>
    public virtual void Select(Selectable selectable)
    {
        while (selectable && selectable.TryGetComponent(out LayoutGroupNavigation layoutGroupNavigation))
            selectable = layoutGroupNavigation.GetChild(Selected);
        if (selectable)
        {
            selectable.Select();
            OnSelected(selectable);
        }
    }

    /// <summary>
    /// Invoke this if the current selection was changed by another script. If possible prefer <seealso cref="Select(Selectable)"/>
    /// </summary>
    /// <param name="selectable">The newly selected component</param>
    /// <remarks>This skips any checks this script would normally do. If possible prefer <seealso cref="Select(Selectable)"/></remarks>
    public virtual void OnSelected(Selectable selectable)
    {
        navigationClip?.PlayOneShot(transform);
        OnSelectionChanged?.Invoke(Selected = selectable);
    }

    public virtual void OnSelected(BaseEventData baseEventData)
    {
        if (baseEventData.selectedObject && baseEventData.selectedObject.TryGetComponent(out Selectable selectable))
            OnSelected(selectable);
    }


    public virtual void InteractWithSelected()
    {
        if (gameObject.activeInHierarchy && Selected && Selected.IsActive() &&
            Selected.TryGetComponent(out ISubmitHandler submitHandler))
            submitHandler.OnSubmit(null);
    }
}
