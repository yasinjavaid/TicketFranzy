using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

using UnityEngine;
using UnityEngine.InputSystem;

public class SinkIt : BallBasedGame
{
    [Header("Sink It")]

    [SerializeField, Range(10, 60)] protected int ballsPerGame = 30;

    [SerializeField, Min(0)] protected float scoreCamTime = 5;
    [SerializeField, Min(0)] protected float ballFeederInterval = 1;
    [SerializeField, Min(0)] protected float maxBallsOnTray = 5;
    [SerializeField, Min(0)] protected float throwVelocityMultiplier = 1f;
    [SerializeField, Min(0)] protected float bounceVelocityMultiplier = 1f;

    [SerializeField, Range(0, 1)] protected float bounceTossThreshold = .5f;
    [SerializeField, Range(0, 1)] protected float tooLowThreshold = .2f;
    [SerializeField, Range(0, 1)] protected float gizmoOffset = 0f;
    [SerializeField, Range(0, -90)] protected float throwLaunchAngle = 0f;
    [SerializeField, Range(0, 90)] protected float bounceLaunchAngle = 45f;

    [SerializeField] protected Transform ballFeedTransform;
    [SerializeField] protected RectTransform ballHitReticle;
    [SerializeField] protected GameObject tossIndicator;
    [SerializeField] protected GameObject bounceIndicator;
    [SerializeField, Range(0, 10)] protected int gizmoRows = 1;
    [SerializeField, Range(0, 10)] protected int gizmoCols = 1;

    private int launchedBallsCount = 0;

    protected EasyCoroutine ballFeederCoroutine;

    protected HashSet<GameObject> clearedCups = new HashSet<GameObject>();
    protected List<Ball> ballsOnFeeder = new List<Ball>();
    protected List<Ball> ballsOnTray = new List<Ball>();

    public HashSet<int> CompletedCups = new HashSet<int>();
    public int BallsPerGame => ballsPerGame;
    public int TotalShots => launchedBallsCount;
    public int SecondsRemainig => 90;

    protected (Vector3, Vector3) lastLaunchVector;

    #region Gameplay (may include calls to Feedback methods)

    public override int Tickets
        => clearedCups.Count >= 10 ? 32 + ballsPerGame - launchedBallsCount
        : clearedCups.Count >= 7 ? 20
        : clearedCups.Count >= 4 ? 14
        : 10;

    public override void Reset()
    {
        base.Reset();
        clearedCups.Clear();
        CompletedCups.Clear();
    }

    public override void StartGame()
    {
        base.StartGame();
        SetCamera("Grab");
        EasyCoroutine.StartNew(ref ballFeederCoroutine, BallFeederCoroutine());
    }

    protected override bool OnInput_HoldBall(InputAction.CallbackContext ctx)
    {
        if (ctx.started)
        {
            if (GrabbedBall && IsValidLaunchPos(reticle.rectTransform.position)) chargeBeginTime = DateTime.Now;
            else GrabBall();
        }
        if (ctx.canceled && chargeBeginTime != default)
            LaunchBall();
        return true;
    }

    protected override Vector2 GetLaunchInput() => Vector2.up;

    protected override Vector3 GetLaunchMomentum()
        => GetLaunchDirection(GetLaunchInputWithCharge(), GetLaunchAngle(reticle.rectTransform.position))
          * launchVelocitySensitivity.y * (IsThrow(reticle.rectTransform.position) ? throwVelocityMultiplier : bounceVelocityMultiplier);

    protected override void LaunchBall()
    {
        if (IsValidLaunchPos(reticle.rectTransform.position))
        {
            base.LaunchBall();
            lastLaunchVector = (PlayingBall.transform.position, PlayingBall.GetRigidbody.velocity);
            PlayingBall = null;
        }
        else chargeBeginTime = default;
    }

    protected virtual Vector2 GetRelativeLaunchPos(Vector3 worldLaunchPos) => (Vector2)reticleBounds.InverseTransformPoint(worldLaunchPos) - reticleBounds.rect.min;

    protected virtual float GetLaunchAngle(Vector3 worldLaunchPos) => IsThrow(worldLaunchPos) ? throwLaunchAngle : bounceLaunchAngle;

    protected virtual bool IsThrow(Vector3 worldLaunchPos) => GetRelativeLaunchPos(worldLaunchPos).y > (reticleBounds.rect.height * bounceTossThreshold);

    protected virtual bool IsValidLaunchPos(Vector3 worldLaunchPos)
    {
        Vector2 relativePos = GetRelativeLaunchPos(worldLaunchPos);
        return relativePos.y > (reticleBounds.rect.height * tooLowThreshold) &&
            relativePos.y < reticleBounds.rect.height &&
            relativePos.x < reticleBounds.rect.width &&
            relativePos.x > 0;
    }

    protected virtual Vector3 GetLaunchDirection(Vector2 launchInput, float launchAngle)
        => launchAngle > 0
            ? Vector3.RotateTowards(launchInput.ToVector3_XZ(), Vector3.down, launchAngle * Mathf.Deg2Rad, 0)
            : Vector3.RotateTowards(launchInput.ToVector3_XZ(), Vector3.up, -launchAngle * Mathf.Deg2Rad, 0);

    protected virtual Vector3 GetLaunchDirection(Vector3 launchInput, float launchAngle)
        => launchAngle > 0
        ? Vector3.RotateTowards(launchInput, Vector3.down, launchAngle * Mathf.Deg2Rad, 0)
        : Vector3.RotateTowards(launchInput, Vector3.up, -launchAngle * Mathf.Deg2Rad, 0);

    protected virtual IEnumerator BallFeederCoroutine()
    {
        while (true)
        {
            if (OngoingGame && ballsOnTray.Count < maxBallsOnTray && ballsOnFeeder.Count > 0)
                FeedABallToTray();
            yield return new WaitForSeconds(ballFeederInterval);
        }
    }


    public void BallThroughCup(OnTriggerEvents triggerEvents)
    {
        GameObject cup = triggerEvents.gameObject.transform.parent.gameObject;
        if (clearedCups.Add(cup))
        {
            //turn off cup light

            CompletedCups.Add(int.Parse(cup.name.Split('(').Last()[0].ToString()));

            if (clearedCups.Count >= 10)
                OnGameEnd();
        }
    }

    public void OnBallEnterCounterTrigger(Collider collider)
    {
        if (OngoingGame && collider.HasComponent<Ball>())
        {
            launchedBallsCount++;
            if (launchedBallsCount >= ballsPerGame)
                OnGameEnd();
        }
    }

    public void OnBallEnterFeedTrigger(Collider collider)
    {
        if (collider.TryGetComponent(out Ball ball))
            ballsOnFeeder.Add(ball);
    }

    public void OnBallExitFeedTrigger(Collider collider)
    {
        if (collider.TryGetComponent(out Ball ball))
            ballsOnFeeder.Remove(ball);
    }

    public void OnBallEnterTrayTrigger(Collider collider)
    {
        if (collider.TryGetComponent(out Ball ball))
            ballsOnTray.Add(ball);
    }

    public void OnBallExitTrayTrigger(Collider collider)
    {
        if (collider.TryGetComponent(out Ball ball))
            ballsOnTray.Remove(ball);
    }

    public bool FeedABallToTray()
    {
        if (ballsOnFeeder.Count > 0)
        {
            Ball ball = ballsOnFeeder[0];
            ball.transform.position = ballFeedTransform.position;
            WakeBallsUp();
            return true;
        }
        return false;
    }

    #endregion

    #region Feedback (UI, Camera...)

    protected override void UpdateUI()
    {
        base.UpdateUI();
        UpdateBallHitReticle();
        UpdateTossBounceIndicators();
    }

    protected virtual void UpdateBallHitReticle()
    {
        if (!IsThrow(reticle.rectTransform.position) && GetChargingTime() > 0)
        {
            ballHitReticle.gameObject.SetActive(true);
            ballHitReticle.position = PredictBallTrajectory(reticle.rectTransform.position, GetLaunchMomentum(), gravity, GetCombinedBounciness(), GetBallRadius(), GetBallDrag(), 1, false)
                                     .FirstOrDefault(tuple => tuple.pos.y < ballHitReticle.position.y).pos.Set(y: ballHitReticle.position.y);
            Debug.DrawLine(reticle.rectTransform.position, ballHitReticle.position, Color.magenta, 5f);
        }
        else ballHitReticle.gameObject.SetActive(false);
    }

    protected virtual void UpdateTossBounceIndicators()
    {
        bool display = GrabbedBall && IsValidLaunchPos(reticle.rectTransform.position);
        bool isThrow = IsThrow(reticle.rectTransform.position);
        tossIndicator.SetActive(display && isThrow);
        bounceIndicator.SetActive(display && !isThrow);
    }

    protected static void DrawGizmoLine(Color color, Vector3 pos, Vector3 velocity, Vector3 gravity, float bounciness, float ballRadius, float ballDrag)
    {
        Gizmos.color = color;

        foreach ((Vector3 pos, Vector3 vel) cur in PredictBallTrajectory(pos, velocity, gravity, bounciness, ballRadius, ballDrag, 2, true))
        {
            if (cur.vel.z < 0 || Mathf.Abs(cur.vel.x) > 0.1f) break;
            Gizmos.DrawLine(pos, pos = cur.pos);
        }
    }

    protected static IEnumerable<(Vector3 pos, Vector3 vel)> PredictBallTrajectory(Vector3 position, Vector3 velocity, Vector3 gravity, float bounciness, float ballRadius, float ballDrag, float duration, bool reflectCollision)
    {
        for (float time = 0; time < duration; time += Time.fixedDeltaTime)
        {
            velocity = (velocity + (gravity * Time.fixedDeltaTime)) * (1 - (ballDrag * Time.fixedDeltaTime));
            Vector3 frameMovement = velocity * Time.fixedDeltaTime;
            if (reflectCollision && Physics.SphereCast(position, ballRadius, frameMovement.normalized, out RaycastHit hit, frameMovement.magnitude, LayerMask.GetMask("Machine"), QueryTriggerInteraction.Ignore))
                velocity = Vector3.Reflect(velocity, hit.normal) * bounciness;
            yield return (position += frameMovement, velocity);
        }
    }

    #endregion

#if UNITY_EDITOR

    protected virtual void OnDrawGizmos()
    {
        DrawGizmoLine(Color.cyan, reticle.transform.position, GetLaunchMomentum(), gravity, GetCombinedBounciness(), GetBallRadius(), GetBallDrag());
        if (lastLaunchVector != default)
            DrawGizmoLine(Color.green, lastLaunchVector.Item1, lastLaunchVector.Item2, gravity, GetCombinedBounciness(), GetBallRadius(), GetBallDrag());

        Gizmos.color = Color.magenta;
        Gizmos.DrawLine(
            reticleBounds.TransformPoint(new Vector3(reticleBounds.rect.xMin, reticleBounds.rect.yMin + (reticleBounds.rect.height * bounceTossThreshold), 0)),
            reticleBounds.TransformPoint(new Vector3(reticleBounds.rect.xMax, reticleBounds.rect.yMin + (reticleBounds.rect.height * bounceTossThreshold), 0)));
        Gizmos.DrawLine(
            reticleBounds.TransformPoint(new Vector3(reticleBounds.rect.xMin, reticleBounds.rect.yMin + (reticleBounds.rect.height * tooLowThreshold), 0)),
            reticleBounds.TransformPoint(new Vector3(reticleBounds.rect.xMax, reticleBounds.rect.yMin + (reticleBounds.rect.height * tooLowThreshold), 0)));
        Gizmos.DrawLine(
            reticleBounds.TransformPoint(new Vector3(reticleBounds.rect.xMin, reticleBounds.rect.yMin, 0)),
            reticleBounds.TransformPoint(new Vector3(reticleBounds.rect.xMax, reticleBounds.rect.yMin, 0)));
        Gizmos.DrawLine(
            reticleBounds.TransformPoint(new Vector3(reticleBounds.rect.xMin, reticleBounds.rect.yMax, 0)),
            reticleBounds.TransformPoint(new Vector3(reticleBounds.rect.xMax, reticleBounds.rect.yMax, 0)));
        Gizmos.DrawLine(
            reticleBounds.TransformPoint(new Vector3(reticleBounds.rect.xMin, reticleBounds.rect.yMin, 0)),
            reticleBounds.TransformPoint(new Vector3(reticleBounds.rect.xMin, reticleBounds.rect.yMax, 0)));
        Gizmos.DrawLine(
            reticleBounds.TransformPoint(new Vector3(reticleBounds.rect.xMax, reticleBounds.rect.yMin, 0)),
            reticleBounds.TransformPoint(new Vector3(reticleBounds.rect.xMax, reticleBounds.rect.yMax, 0)));
    }

    protected virtual void OnDrawGizmosSelected()
    {
        if (reticle)
        {
            float bounciness = GetCombinedBounciness();
            float ballRadius = GetBallRadius();
            float ballDrag = GetBallDrag();
            Rect rect = reticleBounds.rect;
            float minVelocity = launchVelocitySensitivity.y * minCharge;
            float maxVelocity = launchVelocitySensitivity.y;
            float deltaVelocity = maxVelocity - minVelocity;
            (Color color, float velocity)[] trajectoryData = new (Color, float)[]
            {
                (Color.red, minVelocity),
                (Color.yellow, minVelocity + (deltaVelocity * .33f)),
                (Color.green,  minVelocity + (deltaVelocity * .66f)),
                (Color.cyan, maxVelocity),
            };

            if (rect.width > 0 && rect.height > 0)
                for (float y = rect.yMin + (rect.height * (tooLowThreshold + gizmoOffset)); y < rect.yMax; y += rect.height / gizmoRows)
                {
                    Vector3 launchDirection = GetLaunchDirection(Vector3.forward, GetLaunchAngle(reticleBounds.TransformPoint(Vector3.up * y)));
                    float yVelocityMultiplier = IsThrow(reticleBounds.TransformPoint(Vector3.up * y)) ? throwVelocityMultiplier : bounceVelocityMultiplier;
                    for (float x = rect.xMin; x <= rect.xMax; x += rect.width / gizmoCols)
                    {
                        Vector3 pos = reticleBounds.TransformPoint(new Vector3(x, y, 0));
                        for (int i = 0; i < trajectoryData.Length; i++)
                            DrawGizmoLine(
                                trajectoryData[i].color, pos,
                                trajectoryData[i].velocity * launchDirection * yVelocityMultiplier,
                                gravity, bounciness, ballRadius, ballDrag);
                    }
                }
        }
    }

#endif
}
