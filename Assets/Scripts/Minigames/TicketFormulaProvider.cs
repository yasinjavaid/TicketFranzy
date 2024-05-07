using UnityEngine;

public abstract class TicketFormulaProvider : MonoBehaviour
{
    public abstract int GetTicketCount(int score);
}
