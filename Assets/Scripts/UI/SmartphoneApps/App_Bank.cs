using System.Collections.Generic;

using TMPro;

using UnityEngine;

public class App_Bank : MonoBehaviour
{
    [SerializeField] protected CanvasFader canvasFader;
    [SerializeField] protected LayoutGroupNavigation lgn_Vertical;
    [SerializeField] protected BankSlot slot_Template;
    [SerializeField] protected TextMeshProUGUI tmp_IconBalance;
    [SerializeField] protected TextMeshProUGUI tmp_FinalBalance;

    public bool Enabled { get; protected set; }

    private void Update() => tmp_IconBalance.text = GameManager.Tickets.ToString("###,###,##0");

    public virtual void SetEnabled(bool enabled)
    {
        canvasFader.IsVisible = enabled;

        if (Enabled = enabled)
            UpdateSlots();
    }

    protected virtual void UpdateSlots()
    {
        SetAllSlotsActive(false);

        int balance = 0;
        IEnumerator<Transaction> transactions = GameManager.GetTransactions.GetEnumerator();
        for (int i = 0; transactions.MoveNext(); i++)
            GetSlot(i).SetTransaction(transactions.Current, ref balance);

        tmp_FinalBalance.text = balance.ToString("###,###,##0");

        if (balance != GameManager.Tickets)
            Debug.LogWarning("Bank got a different balance result than current ticket count!");
    }

    protected virtual BankSlot GetSlot(int index)
    {
        if (index < 0) return null;

        while (lgn_Vertical.transform.childCount <= index)
            Instantiate(slot_Template, lgn_Vertical.transform).gameObject.SetActive(false);

        BankSlot slot = lgn_Vertical.transform.GetChild(index).GetComponent<BankSlot>();
        slot.gameObject.SetActive(true);

        return slot;
    }

    protected virtual void SetAllSlotsActive(bool active)
    {
        foreach (Transform slot in lgn_Vertical.GetChildren())
            slot.gameObject.SetActive(active);
    }
}
