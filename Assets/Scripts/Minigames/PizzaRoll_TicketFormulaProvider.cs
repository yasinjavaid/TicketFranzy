public class PizzaRoll_TicketFormulaProvider : TicketFormulaProvider
{
    public override int GetTicketCount(int score)
        => score == 36 ? 75
        : score == 35 ? 50
        : score > 20 ? 1 + ((score - 20) * 3)
        : 1;
}
