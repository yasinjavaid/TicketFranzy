using Cinemachine;

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

using TMPro;

using UnityEngine;
using UnityEngine.InputSystem;

using Random = UnityEngine.Random;

public abstract class BallRollGame : BallBasedGame
{
    [Header("Ball Roll Game")]
    [SerializeField] TimingMeter launchTiming;
    [SerializeField] protected CanvasFader launchTimingFader;
    [SerializeField, Range(0, 90)] protected float worstCaseLaunchAngle = 45f;
    [SerializeField] protected TextMeshPro scoreTMP;
    [SerializeField] protected float scoreCamTime = 5;
    [SerializeField, Range(0, 5)] protected float f_tmpUpdateStartDelay = 1f;
    [SerializeField, Range(0, 1)] protected float f_tmpUpdateTime = 1f;

    protected readonly HashSet<Ball> BallsThrown = new HashSet<Ball>();
    protected readonly HashSet<Ball> BallsScored = new HashSet<Ball>();

    public virtual int MaxBallThrowCount => balls.Length;

    public virtual Ball GetPlacedBall() => PlayingBall && !GrabbedBall && !BallsThrown.Contains(PlayingBall) ? PlayingBall : null;

    protected override bool ShowLaunchPreviewLine => GetPlacedBall();

    #region Gameplay (may include calls to Feedback methods)

    protected override bool OnInput_HoldBall(InputAction.CallbackContext ctx)
    {
        if (ctx.started)
        {
            if (GetPlacedBall())
            {
                chargeBeginTime = DateTime.Now;
                launchTimingFader.IsVisible = launchTiming.enabled = false;
            }
            else if (GrabbedBall) PlaceBall();
            else GrabBall();
        }
        if (ctx.canceled && chargeBeginTime != default) LaunchBall();
        return true;
    }

    protected override void ResetBall(Ball ball)
    {
        base.ResetBall(ball);
        BallsThrown.Remove(ball);
        if (PlayingBall == ball)
        {
            PlayingBall = null;
            GrabbedBall = null;
            SetCamera("Grab");
        }
    }

    protected override void FixedUpdate()
    {
        base.FixedUpdate();

        foreach (Ball ball in BallsThrown.Where(ball => ball.transform.position.y < 0 || (ball.GetRigidbody.IsSleeping() && !BallsScored.Contains(ball))).ToList())
            ResetBall(ball);
    }

    protected override void LaunchBall()
    {
        if (GetPlacedBall())
        {
            base.LaunchBall();
            if (PlayingBall)
            {
                BallsThrown.Add(PlayingBall);
                SetCamera("Follow", out CinemachineVirtualCamera followCam);
                followCam.transform.position = Camera.main.transform.position;
                followCam.InternalUpdateCameraState(Vector3.up, 1);
                followCam.Follow = PlayingBall.transform;
            }
        }
    }

    protected override void GrabBall(Ball ball)
    {
        if (BallsThrown.Contains(ball)) return;
        base.GrabBall(ball);
        if (GrabbedBall) SetCamera("Throw");
    }

    protected override void UpdateGrabbedBall()
    {
        if (GrabbedBall)
        {
            Color color = GrabbedBall.GetMeshRenderer.material.color;
            if (TryRaycastBallPlacement(out RaycastHit raycastHit))
            {
                color.a = 1;
                GrabbedBall.transform.GetChild(0).SetActive(true);
                GrabbedBall.transform.position = GetBallPlacementPosition(raycastHit);
            }
            else
            {
                color.a = .5f;
                GrabbedBall.transform.GetChild(0).SetActive(false);
                GrabbedBall.transform.position = reticle.rectTransform.position;
            }
            GrabbedBall.GetMeshRenderer.material.color = color;
        }
    }

    protected override Vector2 GetLaunchInput()
    {
        float timing = (launchTiming.value * 2) - 1; //converts (0..1) to (-1..1)
        float angle = ((-timing * worstCaseLaunchAngle) + 90) * Mathf.Deg2Rad;
        return new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));
    }

    protected virtual void PlaceBall()
    {
        if (GrabbedBall && TryRaycastBallPlacement(out RaycastHit hit))
        {
            GrabbedBall.transform.position = GetBallPlacementPosition(hit);
            GrabbedBall = null;
            launchTimingFader.IsVisible = launchTiming.enabled = true;
            launchTiming.value = Random.value;
        }
    }

    protected virtual Vector3 GetBallPlacementPosition(RaycastHit hit) => hit.point + (new Vector3(0, 1, -1) * GrabbedBall.GetCollider.bounds.extents.y);

    public virtual void OnBallScored(Collider ballCollider)
    {

        if (ballCollider.TryGetComponent(out Ball ball) && BallsScored.Add(ball))
        {
            OngoingGame = BallsScored.Count < MaxBallThrowCount;
            OnBallScored(ball);
        }
    }

    public virtual void OnBallScored(Ball ball)
    {
        PlayingBall = null;
        SetCamera("Score");
        if (OngoingGame) this.InvokeDelayed(scoreCamTime, () => SetCamera("Grab"));
        else this.InvokeDelayed(scoreCamTime, OnGameEnd);
    }

    public virtual void OnColliderEnterStartingArea(Collider collider)
    {
        if (collider.TryGetComponentInParent(out Ball ball))
            OnBallEnterStartingArea(ball);
    }

    public virtual void OnBallEnterStartingArea(Ball ball)
    {
        if (ball == PlayingBall && !GrabbedBall && BallsThrown.Contains(ball) && !BallsScored.Contains(ball))
        {
            ball.GetRigidbody.velocity = Vector3.zero;
            ball.GetRigidbody.angularVelocity = Vector3.zero;
            BallsThrown.Remove(PlayingBall);
            GrabBall(ball);
        }
    }

    #endregion

    #region Feedback (UI, Camera...)

    public override void StartGame()
    {
        base.StartGame();
        SetCamera("Grab");
    }

    public override void Reset()
    {
        base.Reset();
        scoreTMP.text = Score.ToString("00");
        BallsThrown.Clear();
        BallsScored.Clear();
    }

    public override void OnScored(int value)
    {
        base.OnScored(value);
        StartCoroutine(UpdateScoreTextCoroutine());
    }

    protected virtual IEnumerator UpdateScoreTextCoroutine()
    {
        yield return new WaitForSeconds(f_tmpUpdateStartDelay);
        int startingScore = int.Parse(scoreTMP.text);
        float elapsedTime = 0;

        do
        {
            yield return null;
            elapsedTime += Time.deltaTime;
            scoreTMP.text = Mathf.RoundToInt(Mathf.Lerp(startingScore, Score, Mathf.Clamp(elapsedTime / f_tmpUpdateTime, 0, 1))).ToString("00");
        } while (elapsedTime < f_tmpUpdateTime);

        scoreTMP.text = Score.ToString("00");
    }

    #endregion
}