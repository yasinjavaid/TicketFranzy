
using TMPro;

using UnityEngine;
using UnityEngine.UI;

public class PrizeSlot : MonoBehaviour
{
    [SerializeField] protected TextMeshProUGUI tmp_Name;
    [SerializeField] protected TextMeshProUGUI tmp_Price;
    [SerializeField] protected TextMeshProUGUI tmp_Owned;
    [SerializeField] protected Image img_Preview;

    public Prize Prize { get => _prize; set => SetPrize(value); }
    protected Prize _prize;

    public void SetPrize(Prize prize)
    {
        _prize = prize;
        if (prize)
        {
            if (tmp_Name) tmp_Name.text = prize.Name;
            if (tmp_Price) tmp_Price.text = prize.Tickets.ToString("###,###,##0") + " tickets";
            if (tmp_Owned) tmp_Owned.text = prize.OwnedAmount.ToString("###,###,##0") + " owned";
            if (img_Preview) img_Preview.sprite = prize.GetSprite;
        }
    }

    public void Refresh() => SetPrize(Prize);
}
