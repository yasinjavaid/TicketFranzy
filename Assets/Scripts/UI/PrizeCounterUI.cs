using System.Collections.Generic;
using System;

using TMPro;

using UnityEngine;
using UnityEngine.UI;
using System.Linq;
using UnityEngine.InputSystem;

public class PrizeCounterUI : MonoBehaviour
{
    [SerializeField] protected Animator animator;
    [SerializeField] protected SelectionManager selectionManager;
    [SerializeField] protected TextMeshProUGUI tmp_SelectedTicketCost;
    [SerializeField] protected TextMeshProUGUI tmp_SelectedName;
    [SerializeField] protected TextMeshProUGUI tmp_SelectedDescription;
    [SerializeField] protected Image img_SelectedPreview;
    [SerializeField] protected LayoutGroupNavigation lgn_Vertical;
    [SerializeField] protected TMP_Dropdown drop_Order;
    [SerializeField] protected TMP_Dropdown drop_Filter;

    protected LayoutGroupNavigation lgn_HorizontalTemplate;
    protected PrizeSlot slot_Template;
    public bool Enabled { get; protected set; }

    public enum OrderingRules { Name, Price }

    protected List<Prize> prizes;

    protected virtual void Start()
    {
        drop_Order.onValueChanged.AddListener(OnOrderChanged);
        drop_Filter.onValueChanged.AddListener(OnFilterChanged);
        selectionManager.OnSelectionChanged.AddListener(OnSelectionChanged);

        drop_Order.AddOptionsFromEnum<OrderingRules>(true);
        drop_Filter.AddOptionsFromEnum<PrizeCategory>(true);

        lgn_HorizontalTemplate = lgn_Vertical.transform.GetChild(0).GetComponent<LayoutGroupNavigation>();
        slot_Template = lgn_HorizontalTemplate.transform.GetChild(0).GetComponent<PrizeSlot>();
    }

    public virtual void SetEnabled(bool enabled)
    {
        animator.SetBool("Enabled", enabled);
        CoreHUD.IsVisible = Enabled = enabled;

        if (Enabled)
        {
            UpdatePrizeList();
            OrderPrizeList();
            UpdatePrizeSlots();
        }
    }

    private void OnEnable()
    {
        GameInput.Register("Interaction", GameInput.ReferencePriorities.Screen, OnInteractionInput);
        GameInput.Register("Back", GameInput.ReferencePriorities.Screen, OnBackInput);
        GameInput.Register("CharacterMove", GameInput.ReferencePriorities.Screen, OnMoveInput);
        GameInput.Register("CameraZoom", GameInput.ReferencePriorities.Screen, OnZoomInput);
    }

    private void OnDisable()
    {
        GameInput.Deregister("Interaction", GameInput.ReferencePriorities.Screen, OnInteractionInput);
        GameInput.Deregister("Back", GameInput.ReferencePriorities.Screen, OnBackInput);
        GameInput.Deregister("CharacterMove", GameInput.ReferencePriorities.Screen, OnMoveInput);
        GameInput.Deregister("CameraZoom", GameInput.ReferencePriorities.Screen, OnZoomInput);
    }

    private bool OnInteractionInput(InputAction.CallbackContext ctx)
    {
        if (Enabled && ctx.started && selectionManager.Selected && selectionManager.Selected.TryGetComponent(out PrizeSlot prizeSlot) && GameManager.Tickets >= prizeSlot.Prize.Tickets)
        {
            GameManager.AddTickets("Prize Counter", prizeSlot.Prize.Name, -prizeSlot.Prize.Tickets);
            prizeSlot.Prize.OwnedAmount++;
            prizeSlot.Refresh();
        }
        return Enabled;
    }

    private bool OnBackInput(InputAction.CallbackContext ctx)
    {
        if (Enabled && ctx.started)
        {
            SetEnabled(false);
            return true;
        }
        return Enabled;
    }

    private bool OnMoveInput(InputAction.CallbackContext ctx)
    {
        if (Enabled && ctx.started)
        {
            selectionManager.MoveSelection(ctx.ReadValue<Vector2>());
            return true;
        }
        return Enabled;
    }

    private bool OnZoomInput(InputAction.CallbackContext ctx)
    {
        if (Enabled && ctx.started)
        {
            selectionManager.MoveSelection(Vector2.up * ctx.ReadValue<float>());
            return true;
        }
        return Enabled;
    }

    protected virtual void OnOrderChanged(int selection)
    {
        OrderPrizeList();
        UpdatePrizeSlots();
    }

    protected virtual void OnFilterChanged(int selection)
    {
        UpdatePrizeList();
        OrderPrizeList();
        UpdatePrizeSlots();
    }

    protected virtual void UpdatePrizeList()
        => prizes = (drop_Filter.value > 0
        ? PrizeList.Filtered((PrizeCategory)drop_Filter.value)
        : PrizeList.All)
        .ToList();

    protected virtual void OrderPrizeList()
    {
        switch ((OrderingRules)drop_Order.value)
        {
            case OrderingRules.Name: prizes.Sort((a, b) => a.Name.CompareTo(b.Name)); break;
            case OrderingRules.Price: prizes.Sort((a, b) => a.Tickets.CompareTo(b.Tickets)); break;
        }
    }

    protected virtual void UpdatePrizeSlots()
    {
        SetAllPrizeSlotsActive(false);

        for (int i = 0; i < prizes.Count; i++)
            GetPrizeSlot(i).SetPrize(prizes[i]);

        lgn_Vertical.UpdateChildNavigation();
    }

    protected virtual PrizeSlot GetPrizeSlot(int index)
    {
        if (index < 0) return null;

        int row = index / lgn_HorizontalTemplate.VisibleChildCount;
        int col = index % lgn_HorizontalTemplate.VisibleChildCount;

        while (lgn_Vertical.transform.childCount <= row)
            SetAllPrizeSlotsActive(Instantiate(lgn_HorizontalTemplate, lgn_Vertical.transform));

        LayoutGroupNavigation lgn_Horizontal = lgn_Vertical.transform.GetChild(row).GetComponent<LayoutGroupNavigation>();

        while (lgn_Horizontal.transform.childCount <= col)
            Instantiate(slot_Template, lgn_Horizontal.transform).gameObject.SetActive(false);

        PrizeSlot slot = lgn_Horizontal.transform.GetChild(col).GetComponent<PrizeSlot>();
        slot.gameObject.SetActive(true);

        return slot;
    }

    protected virtual void SetAllPrizeSlotsActive(bool active)
    {
        foreach (Transform group in lgn_Vertical.GetChildren())
            foreach (Transform slot in group.GetChildren())
                slot.gameObject.SetActive(active);
    }

    protected virtual void SetAllPrizeSlotsActive(LayoutGroupNavigation group, bool active)
    {
        foreach (Transform slot in group.GetChildren())
            slot.gameObject.SetActive(active);
    }

    protected virtual void OnSelectionChanged(Selectable selectable)
    {
        if (selectable.TryGetComponent(out PrizeSlot prizeSlot) && prizeSlot.Prize)
        {
            tmp_SelectedTicketCost.text = $"{prizeSlot.Prize.Tickets} Tickets";
            tmp_SelectedName.text = prizeSlot.Prize.Name;
            tmp_SelectedDescription.text = prizeSlot.Prize.Description;
            img_SelectedPreview.sprite = prizeSlot.Prize.GetSprite;
            img_SelectedPreview.gameObject.SetActive(img_SelectedPreview.sprite);
        }
    }
}
