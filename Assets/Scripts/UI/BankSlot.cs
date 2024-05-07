using TMPro;

using UnityEngine;

public class BankSlot : MonoBehaviour
{
    [SerializeField] protected TextMeshProUGUI tmp_Title;
    [SerializeField] protected TextMeshProUGUI tmp_Desc;
    [SerializeField] protected TextMeshProUGUI tmp_Value;
    [SerializeField] protected TextMeshProUGUI tmp_Quantity;
    [SerializeField] protected TextMeshProUGUI tmp_Subtotal;
    [SerializeField] protected TextMeshProUGUI tmp_Balance;
    [SerializeField] protected TMP_FontAsset positiveFont;
    [SerializeField] protected TMP_FontAsset negativeFont;

    Transaction transaction;

    public void SetTransaction(Transaction transaction, ref int currentBalance)
    {
        this.transaction = transaction;
        TMP_FontAsset fontAsset = transaction.Value >= 0 ? positiveFont : negativeFont;
        UpdateTMP(tmp_Title, transaction.Title, fontAsset);
        UpdateTMP(tmp_Desc, transaction.Description, fontAsset);
        UpdateTMP(tmp_Value, transaction.Value.ToString("###,###,##0"), fontAsset);
        UpdateTMP(tmp_Quantity, transaction.Quantity.ToString("###,###,##0"), fontAsset);
        UpdateTMP(tmp_Subtotal, transaction.Subtotal.ToString("###,###,##0"), fontAsset);
        UpdateTMP(tmp_Balance, (currentBalance += transaction.Subtotal).ToString("###,###,##0"), fontAsset);

        static void UpdateTMP(TextMeshProUGUI tmp, string text, TMP_FontAsset font)
        {
            if (tmp)
            {
                tmp.text = text;
                tmp.font = font;
                tmp.UpdateFontAsset();
            }
        }
    }
}