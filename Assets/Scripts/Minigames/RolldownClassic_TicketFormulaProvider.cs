public class RolldownClassic_TicketFormulaProvider : TicketFormulaProvider
{
    public override int GetTicketCount(int score) =>
    (score == 36 || score == 6) ? 75 :
    (score == 35 || score == 7) ? 50 :
    score > 23 ? 1 + ((score - 23) * 3) :
    score < 19 ? 1 + ((19 - score) * 3) :
    1;
}
