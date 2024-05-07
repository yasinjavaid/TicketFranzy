using Cinemachine;

using TMPro;

using UnityEngine;

public class RolldownGame : BallRollGame
{
    [Header("Rolldown Game")]
    [SerializeField] protected TextMeshPro ballsTMP;
    [SerializeField] protected Animator animator;

    public override void Reset()
    {
        base.Reset();
        ballsTMP.text = "0";
        if (animator) animator.SetBool("Victory", false);
    }

    public override void OnBallScored(Ball ball)
    {
        base.OnBallScored(ball);
        this.InvokeDelayed(f_tmpUpdateStartDelay, () => ballsTMP.text = BallsScored.Count.ToString("0"));
    }

    public override void OnGameEnd()
    {
        base.OnGameEnd();
        if (animator) animator.SetBool("Victory", true);
    }
}