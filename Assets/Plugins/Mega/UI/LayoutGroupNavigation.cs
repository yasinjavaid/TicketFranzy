using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
using MoreLinq;
using UnityEditor;

#if UNITY_EDITOR
[CustomEditor(typeof(LayoutGroupNavigation)), CanEditMultipleObjects]
public class LayoutGroupNavigationEditor : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        LayoutGroupNavigation group = target as LayoutGroupNavigation;
        EditorGUI.BeginDisabledGroup(true);
        EditorGUILayout.IntField("Visible Child Count", group.VisibleChildCount);
        EditorGUILayout.IntField("Active Child Count", group.ActiveChildCount);
        EditorGUI.EndDisabledGroup();
    }
}
#endif

[RequireComponent(typeof(Selectable))]
public class LayoutGroupNavigation : MonoBehaviour
{
    [SerializeField] protected float animationTime = 0.5f;
    [SerializeField] protected Selectable defaultSelectable;
    [SerializeField] protected bool loopAround;

    [Tooltip("The Layout Group this is attached to")]
    //[ValidateInput("ValidateLayoutGroup", "Only Vertical and Horizontal layout groups implemented yet", InfoMessageType.Error, ContinuousValidationCheck = false)]
    [SerializeField] protected LayoutGroup layoutGroup;
    [SerializeField] protected Selectable mySelectable;
    [SerializeField] protected SelectionManager selectionManager;
    [SerializeField] protected Scrollbar scrollbar;

    protected float elementSize, parentSize, padding, spacing;


    public int VisibleChildCount
    {
        get => _visibleChildCount;
        protected set
        {
            _visibleChildCount = value;
            if (scrollbar) scrollbar.size = MaxFirstVisibleChild > 0 ? 0.5f / MaxFirstVisibleChild : 1f;
        }
    }
    protected int _visibleChildCount;


    public int ActiveChildCount
    {
        get => _activeChildCount;
        protected set
        {
            _activeChildCount = value;
            if (scrollbar) scrollbar.size = MaxFirstVisibleChild > 0 ? 0.5f / MaxFirstVisibleChild : 1f;
        }
    }
    protected int _activeChildCount;


    public int FirstVisibleChild
    {
        get => _firstVisibleChild;
        protected set
        {
            _firstVisibleChild = value;
            if (scrollbar)
                LeanTween.value(scrollbar.gameObject, scrollbar.value, value / (float)MaxFirstVisibleChild, animationTime)
                    .setOnUpdate((float f) => scrollbar.value = f)
                    .setEaseInOutSine()
                    .setIgnoreTimeScale(true);
        }
    }
    protected int _firstVisibleChild;

    public int MaxFirstVisibleChild => ActiveChildCount - VisibleChildCount;

    protected int lastSelectedIndex = -1;
    protected Selectable lastSelected;
    protected bool isHorizontal, isVertical;
    protected Vector3 defaultPosition;

    public Selectable MySelectable => mySelectable;

    protected virtual void Awake()
    {
        if (!mySelectable) mySelectable = GetComponent<Selectable>();
        defaultPosition = this.GetRectTransform().localPosition;
        if (selectionManager) selectionManager.OnSelectionChanged.AddListener(OnSelectionChanged);
        else Debug.LogError($"{nameof(LayoutGroupNavigation)} on object {name} has no reference for {nameof(selectionManager)}");
        UpdateChildNavigation();
    }

    public virtual Selectable GetChild() => lastSelected && lastSelected.gameObject.activeSelf ? lastSelected :
            defaultSelectable && defaultSelectable.gameObject.activeSelf ? defaultSelectable :
            TryGetChildAtOrAfter(0, out Selectable child, false, false) ? child : null;

    public virtual Selectable GetChild(Selectable previousSelection)
    {
        if (previousSelection && previousSelection.transform != transform && previousSelection.transform.parent &&
            previousSelection.transform.parent.TryGetComponent(out LayoutGroupNavigation otherLayoutGroup) &&
            otherLayoutGroup.transform.parent == transform.parent && otherLayoutGroup.GetChild())
            for (int i = Mathf.Min(otherLayoutGroup.GetChild().transform.GetSiblingIndex(), transform.GetActiveChildCount() - 1); i >= 0; i--)
                if (transform.GetActiveChild(i).TryGetComponent(out Selectable child))
                    return child;
        return GetChild();
    }

    public virtual void OnSelectionChanged(Selectable selectable)
    {
        if (selectable.transform.IsChildOf(transform))
        {
            while (selectable.transform.parent != transform) selectable = selectable.transform.parent.GetComponent<Selectable>();

            lastSelected = selectable;
            lastSelectedIndex = selectable.transform.GetSiblingIndex();
            int activeSelectedIndex = selectable.transform.GetActiveSiblingIndex();

            if (activeSelectedIndex < FirstVisibleChild) MoveToIndex(activeSelectedIndex);
            else if (activeSelectedIndex > FirstVisibleChild + CalculateVisibleCount() - 1) MoveToIndex(activeSelectedIndex - CalculateVisibleCount() + 1);
        }

        if (lastSelected && !lastSelected.transform.parent == transform)
            UpdateChildNavigation();

    }

    protected virtual void MoveToIndex(int newIndex)
    {
        FirstVisibleChild = newIndex;

        LeanTween.cancel(gameObject);
        if (layoutGroup is HorizontalLayoutGroup)
            LeanTween.moveLocalX(gameObject, defaultPosition.x - (FirstVisibleChild * (elementSize + spacing)), animationTime).setEaseInOutSine().setIgnoreTimeScale(true);
        if (layoutGroup is VerticalLayoutGroup)
            LeanTween.moveLocalY(gameObject, defaultPosition.y + (FirstVisibleChild * (elementSize + spacing)), animationTime).setEaseInOutSine().setIgnoreTimeScale(true);
    }

    protected bool ValidateLayoutGroup(LayoutGroup layoutGroup) => layoutGroup is HorizontalLayoutGroup || layoutGroup is VerticalLayoutGroup;

    public int CalculateVisibleCount()
    {
        if (layoutGroup is HorizontalLayoutGroup horizontalLayoutGroup)
        {
            elementSize = TryGetChildAtOrAfter(0, out Selectable child, false, true) ? child.GetWidth() : 0;
            parentSize = this.GetRectTransform().GetWidth();
            padding = horizontalLayoutGroup.padding.left;
            spacing = horizontalLayoutGroup.spacing;
            return VisibleChildCount = Mathf.RoundToInt((parentSize - padding) / (elementSize + spacing));
        }
        else if (layoutGroup is VerticalLayoutGroup verticalLayoutGroup)
        {
            elementSize = TryGetChildAtOrAfter(0, out Selectable child, false, true) ? child.GetHeight() : 0;
            parentSize = this.GetRectTransform().GetHeight();
            padding = verticalLayoutGroup.padding.top;
            spacing = verticalLayoutGroup.spacing;
            return VisibleChildCount = Mathf.RoundToInt((parentSize - padding) / (elementSize + spacing));
        }
        return VisibleChildCount = -1;
    }

    public void ResetLastSelected()
    {
        lastSelectedIndex = -1;
        lastSelected = null;
    }

    /// <summary>
    /// Orders all child transforms by a given func.
    /// <br>If inactive children aren't included, they'll be naturally moved to the bottom in no defined order.</br>
    /// <para>See also: <seealso cref="OrderChildrenBy{T}(Func{T, int}, OrderByDirection, bool)"/></para>
    /// </summary>
    /// <param name="orderFunc">The function that gives values to the elements so they can be sorted</param>
    /// <param name="direction">Define whether the sorting is Ascending or Descending</param>
    /// <param name="includeInactive">If True, disabled objects will also be sorted.
    /// <br>If False, disabled objects will fall to the bottom.</br></param>
    public void OrderChildrenBy(Func<Transform, int> orderFunc, OrderByDirection direction, bool includeInactive)
    {
        IEnumerator<Transform> children = (includeInactive ? transform.GetChildren() : transform.GetActiveChildren()).OrderBy(orderFunc, direction).GetEnumerator();
        for (int i = 0; children.MoveNext(); i++)
            children.Current.SetSiblingIndex(i);
        UpdateChildNavigation();
    }

    /// <summary>
    /// Orders all child transforms by a given func.
    /// <br>If inactive children aren't included, they'll naturally fall to the bottom in no defined order.</br>
    /// <br>Children that don't have the component T will also be ignored and fall to the bottom.</br>
    /// <para>See also: <seealso cref="OrderChildrenBy(Func{Transform, int}, OrderByDirection, bool)"/></para>
    /// </summary>
    /// <typeparam name="T">The type of component that will be used as the base for the sorting function</typeparam>
    /// <param name="orderFunc">The function that gives values to the elements so they can be sorted</param>
    /// <param name="direction">Define whether the sorting is Ascending or Descending</param>
    /// <param name="includeInactive">If True, disabled objects will also be sorted.
    /// <br>If False, disabled objects will fall to the bottom.</br></param>
    public void OrderChildrenBy<T>(Func<T, int> orderFunc, OrderByDirection direction, bool includeInactive) where T : Component
    {
        IEnumerator<T> children = transform.GetComponentsInImmediateChildren<T>(includeInactive).OrderBy(orderFunc, direction).GetEnumerator();
        for (int i = 0; children.MoveNext(); i++)
            children.Current.transform.SetSiblingIndex(i);
        UpdateChildNavigation();
    }


    public void UpdateChildNavigation()
    {
        if (lastSelected != null && (lastSelected.transform.parent != transform || !lastSelected.gameObject.activeInHierarchy))
        {
            int index = Mathf.Min(lastSelectedIndex, transform.childCount - 1);
            if (TryGetPreviousChild(ref index, out Selectable previousChild, false, false))
            {
                lastSelectedIndex = index;
                lastSelected = previousChild;
            }
            else
            {
                lastSelectedIndex = -1;
                lastSelected = null;
            }
        }

        if (layoutGroup is HorizontalLayoutGroup)
        {
            for (int i = 0; i < transform.childCount; i++)
            {
                if (transform.GetChild(i).TryGetComponent(out Selectable selectable))
                {
                    Navigation navigation = MySelectable.navigation;
                    navigation.mode = Navigation.Mode.Explicit;

                    if (TryGetPreviousChild(i, out Selectable child, loopAround, false))
                        navigation.selectOnLeft = child;
                    if (TryGetNextChild(i, out child, loopAround, false))
                        navigation.selectOnRight = child;

                    selectable.navigation = navigation;
                    if (transform.GetChild(i).TryGetComponent(out LayoutGroupNavigation childLayoutGroupNavigation))
                        childLayoutGroupNavigation.UpdateChildNavigation();
                }
            }
        }
        else if (layoutGroup is VerticalLayoutGroup)
        {
            for (int i = 0; i < transform.childCount; i++)
            {
                if (transform.GetChild(i).TryGetComponent(out Selectable selectable))
                {
                    Navigation navigation = MySelectable.navigation;
                    navigation.mode = Navigation.Mode.Explicit;

                    if (TryGetPreviousChild(i, out Selectable child, loopAround, false))
                        navigation.selectOnUp = child;
                    if (TryGetNextChild(i, out child, loopAround, false))
                        navigation.selectOnDown = child;

                    selectable.navigation = navigation;
                    if (transform.GetChild(i).TryGetComponent(out LayoutGroupNavigation childLayoutGroupNavigation))
                        childLayoutGroupNavigation.UpdateChildNavigation();
                }
            }
        }

        CalculateVisibleCount();
        ActiveChildCount = transform.GetActiveChildCount();
    }

    protected virtual bool IsValidChild(Transform transform) =>
        transform != null &&
        transform.parent == transform &&
        transform.gameObject.activeInHierarchy &&
        transform.HasComponent<Selectable>();

    protected virtual bool IsValidChild(GameObject gameObject) =>
        gameObject != null &&
        gameObject.transform.parent == transform &&
        gameObject.activeInHierarchy &&
        gameObject.HasComponent<Selectable>();

    protected virtual bool IsValidChild(Selectable selectable) =>
        selectable != null &&
        selectable.transform.parent == transform &&
        selectable.gameObject.activeInHierarchy;

    protected virtual bool TryGetPreviousChild(int index, out Selectable selectable, bool loopAround, bool includeInactive, int minIndex = 0) => TryGetPreviousChild(ref index, out selectable, loopAround, includeInactive, minIndex);

    protected virtual bool TryGetPreviousChild(ref int index, out Selectable selectable, bool loopAround, bool includeInactive, int minIndex = 0)
    {
        int startIndex = index;
        minIndex = Mathf.Clamp(minIndex, 0, transform.childCount);
        while (--index >= minIndex)
            if (TryGetChildAt(index, out selectable, includeInactive))
                return true;

        if (loopAround && TryGetPreviousChild(transform.childCount, out selectable, false, includeInactive, startIndex + 1))
            return true;

        selectable = null;
        return false;
    }

    protected virtual bool TryGetNextChild(int index, out Selectable selectable, bool loopAround, bool includeInactive, int maxIndex = int.MaxValue) => TryGetNextChild(ref index, out selectable, loopAround, includeInactive, maxIndex);

    protected virtual bool TryGetNextChild(ref int index, out Selectable selectable, bool loopAround, bool includeInactive, int maxIndex = int.MaxValue)
    {
        int startIndex = index;
        maxIndex = Mathf.Clamp(maxIndex, 0, transform.childCount);
        while (++index < maxIndex)
            if (TryGetChildAt(index, out selectable, includeInactive))
                return true;

        if (loopAround && TryGetNextChild(0, out selectable, false, includeInactive, startIndex - 1))
            return true;

        selectable = null;
        return false;
    }

    protected virtual bool TryGetChildAtOrBefore(int index, out Selectable selectable, bool loopAround, bool includeInactive)
    {
        int startIndex = index;
        while (index >= 0)
            if (TryGetChildAt(index, out selectable, includeInactive))
                return true;
            else index--;

        if (loopAround && TryGetPreviousChild(transform.childCount, out selectable, false, includeInactive, startIndex + 1))
            return true;

        selectable = null;
        return false;
    }

    protected virtual bool TryGetChildAtOrAfter(int index, out Selectable selectable, bool loopAround, bool includeInactive)
    {
        int startIndex = index;
        while (index < transform.childCount)
            if (TryGetChildAt(index, out selectable, includeInactive))
                return true;
            else index++;

        if (loopAround && TryGetNextChild(0, out selectable, false, includeInactive, startIndex - 1))
            return true;

        selectable = null;
        return false;
    }

    protected virtual bool TryGetChildAt(int index, out Selectable selectable, bool includeInactive)
    {
        if (index >= 0 && index <= (transform.childCount - 1) && transform.GetChild(index).TryGetComponent(out selectable) && (includeInactive || selectable.gameObject.activeInHierarchy))
            return true;

        selectable = null;
        return false;
    }

#if UNITY_EDITOR
    protected virtual void OnValidate()
    {
        if (!layoutGroup) layoutGroup = GetComponent<LayoutGroup>();
        if (!mySelectable) mySelectable = GetComponent<Selectable>();
        UpdateChildNavigation();
    }
#endif
}
