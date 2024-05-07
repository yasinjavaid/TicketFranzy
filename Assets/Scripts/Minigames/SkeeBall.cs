using Cinemachine;

using UnityEngine;

public class SkeeBall : BallRollGame
{
    [Header("SkeeBall")]
    [SerializeField] protected Ball[] extraBalls;

    [SerializeField, Range(0, 950)] protected int MinScore;
    [SerializeField, Range(0, 300)] protected int TicketSpanPoints;
    [SerializeField, Range(0, 100)] protected int TicketSpanAmount;
    [SerializeField, Range(0, 10)] protected int TicketMinimum;
    [SerializeField, Range(0, 99)] protected int TicketMaximum;
    [SerializeField, Range(0, 12)] protected int BallsPerGame;
    [SerializeField, Range(0, 900)] protected int ExtraBallPoints;
    [SerializeField, Range(1, 10)] protected int ExtraBallQuantity;

    protected bool extraBallsDelivered = false;
    protected Vector3[] extraBallsSpawnPosition;

    public override int MaxBallThrowCount => extraBallsDelivered ? balls.Length + extraBalls.Length : balls.Length;

    public override int Tickets => Score < MinScore ? TicketMinimum :
        Mathf.Clamp(Mathf.FloorToInt(Score * TicketSpanAmount / (float)TicketSpanPoints), TicketMinimum, TicketMaximum);

    public override void Reset()
    {
        base.Reset();
        extraBallsDelivered = false;
    }

    public override void OnScored(int value)
    {
        base.OnScored(value);

        if (OngoingGame && !extraBallsDelivered && Score >= ExtraBallPoints)
        {
            extraBallsDelivered = true;
            if (extraBallsSpawnPosition == null)
            {
                extraBallsSpawnPosition = new Vector3[extraBalls.Length];
                for (int i = 0; i < extraBalls.Length; i++)
                {
                    extraBallsSpawnPosition[i] = extraBalls[i].transform.position;
                    extraBalls[i].gameObject.SetActive(true);
                }
            }
            else for (int i = 0; i < extraBalls.Length; i++)
                {
                    extraBalls[i].transform.position = extraBallsSpawnPosition[i];
                    extraBalls[i].gameObject.SetActive(true);
                }
        }
    }
}
