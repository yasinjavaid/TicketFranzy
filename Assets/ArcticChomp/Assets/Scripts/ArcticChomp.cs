using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using UnityEngine.InputSystem;
using System.Linq;

using DebugConsole;
using TMPro;


public class ArcticChomp : ArcadeGame
{

    private Mouse mouse => Mouse.current;
    private Camera cam => Camera.main;


    [Header("Mouth")]
    [SerializeField] private GameObject upperMouth;
    [SerializeField] private GameObject lowerMouth;
    [SerializeField] private float mouthOpenTime;
    [SerializeField] private float maxMouthAngle;
    private bool moveMouth;

    [Header("BallSpawner")]
    [SerializeField] private Transform startBallPosition;
    [SerializeField] private Transform endBallPosition;
    [SerializeField] private GameObject spawnedBalls;
    private bool allowBallSpawn;

    [Header("Time")]
    [SerializeField] private float gameTime;
    [SerializeField] private float bonusTime;
    [SerializeField] private float minSpawnDelay;
    [SerializeField] private float maxSpawnDelay;
    private float SpawnDelay;

    private float remainingTime;
    

    [Header("Score")]
    [SerializeField] private TextMeshPro scoreText;
    private int points;
    private int Tics;


    [Header("Lever")]
    [SerializeField] public ArcticChompPaddle paddle;
    [SerializeField] protected Transform leverTransform;
    [SerializeField] protected float leverReturnDegreesPerSecond;
    [SerializeField] protected RectTransform grabBallRegion;
    [SerializeField, Range(0, 1)] protected float rotationCaptureTimeframe;
    private Queue<(float time, float rotation)> previousRotations = new Queue<(float, float)>();
    private Quaternion defaultHandleRotation;
    [SerializeField] private float minHandleAngle;
    [SerializeField] private float maxHandleAngle;
    [SerializeField] private float handleKeySpeed;
    private float handleAngle;
    private float keyHandleValue;
    private float stickHandleValue;




    //Lever variables
    public float a = 24.8963f;
    public float b = -16.4591f;
    public float c = 16.4591f;
    public float d = 0.524898f;
    public float k = 40;


    //private float a = 20f;
    //private float b = -15f;
    //private float c = 15f;
    //private float d = 0.3f;
    //private float k = 0;


    private bool isHoldingHandle;
    private bool inputFinished;
    private float defaultLeverRotation;
    private float currentLeverRotation;





    // Start is called before the first frame update
    protected override void Start()
    {
        moveMouth = false;
        allowBallSpawn = false;
        base.Start();
        defaultHandleRotation = leverTransform.rotation;
        keyHandleValue = 0;
    }

    public override void StartGame()
    {
        Invoke("startBallSpawn", 2.0f);
        paddle.applyForce = false;
        SpawnDelay = getRandomSpawnTime(minSpawnDelay, maxSpawnDelay);
        base.StartGame();
        StartMouthMovement();
        currentLeverRotation = defaultLeverRotation = leverTransform.localEulerAngles.x;

        inputFinished = false;
        isHoldingHandle = false;
        remainingTime = gameTime;
        points = 0;
    }

    private void startBallSpawn()
    {
        allowBallSpawn = true;
    }


    protected override void OnEnable()
    {
        base.OnEnable();
        GameInput.Register("HoldBall", GameInput.ReferencePriorities.Character, OnInput_HoldBall);
        GameInput.Register("KeyValue", GameInput.ReferencePriorities.Character, keyInput);
        GameInput.Register("LeftStickValue", GameInput.ReferencePriorities.Character, LeftStickInput);
    }

    protected override void OnDisable()
    {
        base.OnDisable();
        GameInput.Deregister("HoldBall", GameInput.ReferencePriorities.Character, OnInput_HoldBall);
        GameInput.Deregister("KeyValue", GameInput.ReferencePriorities.Character, keyInput);
        GameInput.Register("LeftStickValue", GameInput.ReferencePriorities.Character, LeftStickInput);

    }

    #region input
    protected virtual bool OnInput_HoldBall(InputAction.CallbackContext ctx)
    {
        if (ctx.started && !inputFinished && OngoingGame && CheckHandleCollision())
            //reticle.transform.position.y > grabBallRegion.TransformPoint(grabBallRegion.rect.min).y &&
            //reticle.transform.position.y < grabBallRegion.TransformPoint(grabBallRegion.rect.max).y)
        {
            Debug.Log("Press Handle");
            isHoldingHandle = true;
        }
        else if (ctx.canceled)
            isHoldingHandle = false;
        return true;
    }

    public bool keyInput(InputAction.CallbackContext ctx)
    {
        if (ctx.performed)
        {
            keyHandleValue = ctx.ReadValue<float>();
        }

        else if (ctx.canceled)
        {
            keyHandleValue = ctx.ReadValue<float>();
        }


        return true;
    }

    private bool LeftStickInput(InputAction.CallbackContext ctx)
    {
        if (ctx.performed)
        {
            stickHandleValue = ctx.ReadValue<float>();
        }
        else if (ctx.canceled)
        {
            stickHandleValue = ctx.ReadValue<float>();
        }


        return true;
    }

    private void setPaddleAngle()
    {
        float oldAngle = Gear.InspectorAngles(paddle.gameObject.transform.localEulerAngles.x);

        if (Gear.InspectorAngles(leverTransform.localEulerAngles.x) > 0)
        {
            paddle.gameObject.transform.localEulerAngles = (leverTransform.localEulerAngles) * -1.5f;

        }
        else
        {
            paddle.gameObject.transform.localEulerAngles = Quaternion.Euler(Gear.InspectorAngles(leverTransform.localEulerAngles.x) / 2 * -1, 0, 0).eulerAngles;
        }

        float newAngle = Gear.InspectorAngles(paddle.gameObject.transform.localEulerAngles.x);

        if (newAngle < oldAngle)
        {
            paddle.applyForce = true;
        }
        else
        {
            paddle.applyForce = false;
        }

    }

    private bool CheckHandleCollision()
    {
        Vector3 coor = mouse.position.ReadValue();
        RaycastHit hit;
        Physics.Raycast(cam.ScreenPointToRay(coor), out hit);

        if (Physics.Raycast(cam.ScreenPointToRay(coor), out hit))
        {
            if (hit.collider.gameObject.CompareTag("handle"))
            {
                return true;
            }

        }

        return false;
    }


    #endregion



    protected virtual void FixedUpdate()
    {
        handleAngle = Gear.InspectorAngles(leverTransform.localEulerAngles.x);
        if (!OngoingGame) 
        {
            if(handleAngle != 0)
            {
                leverTransform.localEulerAngles = Vector3.right * (currentLeverRotation = Mathf.MoveTowards(handleAngle, defaultLeverRotation, leverReturnDegreesPerSecond * Time.deltaTime));
                setPaddleAngle();
            }

            return;
        }
        
        if (isHoldingHandle)
        {
            float temp = GetRelativeReticlePosition();
            float temp1 = Mathf.Clamp01(temp);
            float temp2 = GetRelativeLeverRotation(temp1);


            Vector3 test = Vector3.right * (currentLeverRotation = GetRelativeLeverRotation(Mathf.Clamp01(GetRelativeReticlePosition())));
//            leverTransform.DORotate(test, 0);
            leverTransform.localEulerAngles = Vector3.right * (currentLeverRotation = GetRelativeLeverRotation(Mathf.Clamp01(GetRelativeReticlePosition())));

            setPaddleAngle();


            previousRotations.Enqueue((Time.timeSinceLevelLoad, leverTransform.localEulerAngles.x));
            while (previousRotations.Peek().time < Time.timeSinceLevelLoad - rotationCaptureTimeframe)
                previousRotations.Dequeue();
            if (previousRotations.Peek().time < Time.timeSinceLevelLoad - rotationCaptureTimeframe / 2)
            {
                var last = previousRotations.Last();
                var first = previousRotations.Peek();
                float rotationVelocity = (last.rotation - first.rotation) / (last.time - first.time);
                //if (rotationVelocity > maxRotationVelocity)
                //    maxRotationVelocity = rotationVelocity;
            }
            //rb_wheel.angularVelocity = Vector3.right * -maxRotationVelocity * rotationFactor;
        }

        if (keyHandleValue != 0)
        {
            handleAngle = handleAngle + (-1 * keyHandleValue * handleKeySpeed);
            if (keyHandleValue > 0 && handleAngle < minHandleAngle)
            {
                    handleAngle = minHandleAngle;
            }
            else if (keyHandleValue < 0 && handleAngle > maxHandleAngle)
            {
                handleAngle = maxHandleAngle;
            }

            leverTransform.localEulerAngles = Vector3.right * handleAngle;

            setPaddleAngle();

            previousRotations.Enqueue((Time.timeSinceLevelLoad, leverTransform.localEulerAngles.x));
            while (previousRotations.Peek().time < Time.timeSinceLevelLoad - rotationCaptureTimeframe)
                previousRotations.Dequeue();
            if (previousRotations.Peek().time < Time.timeSinceLevelLoad - rotationCaptureTimeframe / 2)
            {
                var last = previousRotations.Last();
                var first = previousRotations.Peek();
                float rotationVelocity = (last.rotation - first.rotation) / (last.time - first.time);
                //if (rotationVelocity > maxRotationVelocity)
                //    maxRotationVelocity = rotationVelocity;
            }



        }

        if (stickHandleValue != 0)
        {
            if(stickHandleValue > 0)
            {
                handleAngle = minHandleAngle * Mathf.Abs(stickHandleValue); ;
            }
            else if(stickHandleValue < 0)
            {
                handleAngle = maxHandleAngle * Mathf.Abs(stickHandleValue);
            }

            leverTransform.localEulerAngles = Vector3.right * handleAngle;

            setPaddleAngle();

            previousRotations.Enqueue((Time.timeSinceLevelLoad, leverTransform.localEulerAngles.x));
            while (previousRotations.Peek().time < Time.timeSinceLevelLoad - rotationCaptureTimeframe)
                previousRotations.Dequeue();
            if (previousRotations.Peek().time < Time.timeSinceLevelLoad - rotationCaptureTimeframe / 2)
            {
                var last = previousRotations.Last();
                var first = previousRotations.Peek();
                float rotationVelocity = (last.rotation - first.rotation) / (last.time - first.time);
                //if (rotationVelocity > maxRotationVelocity)
                //    maxRotationVelocity = rotationVelocity;
            }

        }



        if (leverTransform.localEulerAngles.x != 0 && !isHoldingHandle && keyHandleValue == 0 && stickHandleValue == 0)
        {
            leverTransform.localEulerAngles = Vector3.right * (currentLeverRotation = Mathf.MoveTowards(handleAngle, defaultLeverRotation, leverReturnDegreesPerSecond * Time.deltaTime));
            setPaddleAngle();

        }



        if(SpawnDelay < 0 && allowBallSpawn)
        {
            spawnBall();
            SpawnDelay = getRandomSpawnTime(minSpawnDelay, maxSpawnDelay);
        }

        SpawnDelay -= Time.deltaTime;

        if(remainingTime > 0 && allowBallSpawn)
        {
            remainingTime -= Time.deltaTime;
            if(remainingTime < 0)
            {
                OnGameEnd();
            }
            
        }

        //if (inputFinished && rb_wheel.IsSleeping())
        //    OnGameEnd();
    }

    private float GetRelativeLeverRotation(float x)
    {
        return (a * Mathf.Pow((b * x) + c, d)) + k;
    }


    #region MouthCode
    public void StartMouthMovement()
    {
        moveMouth = true;
        OpenMouthUpper();
    }

    public void TriggerMouthMovement()
    {
        if (!moveMouth)
        {
            StartMouthMovement();
        }
        else
        {
            moveMouth = false;
        }
    }


    private void OpenMouthUpper()
    {
        if (moveMouth)
        {
            Quaternion UpperMouthRot = Quaternion.Euler(-maxMouthAngle, 0, 180);
            upperMouth.transform.DORotateQuaternion(UpperMouthRot, mouthOpenTime).OnComplete(CloseMouthUpper);
            OpenMouthLower();
        }
    }

    private void OpenMouthLower()
    {
        if (moveMouth)
        {
            Quaternion LowerMouthRot = Quaternion.Euler(-90 - maxMouthAngle, 0, 180);
            lowerMouth.transform.DORotateQuaternion(LowerMouthRot, mouthOpenTime).OnComplete(CloseMouthLower);
        }
    }

    private void CloseMouthUpper()
    {
        Quaternion UpperMouthRot = Quaternion.Euler(-90, 0, 180);
        upperMouth.transform.DORotateQuaternion(UpperMouthRot, mouthOpenTime).OnComplete(OpenMouthUpper);
    }

    private void CloseMouthLower()
    {
        Quaternion LowerMouthRot = Quaternion.Euler(-90, 0, 180);
        lowerMouth.transform.DORotateQuaternion(LowerMouthRot, mouthOpenTime).OnComplete(OpenMouthLower);
    }

    #endregion


    #region LeverQueue
    private float GetRelativeReticlePosition() => ToRelative(reticle.transform.localPosition.y, reticleBounds.rect.yMin, reticleBounds.rect.yMax);

    private static float ToRelative(float i, float min, float max)
    => (i - min) / (max - min);

    public static float ToAbsolute(float i, float min, float max)
        => ((max - min) * i) + min;

    #endregion


    #region ballSpawner

    private Vector3 getRandomSpawnPosition(Vector3 startBallPosition, Vector3 endBallPosition)
    {
        float x = Random.Range(startBallPosition.x, endBallPosition.x);
        float z = Random.Range(startBallPosition.z, endBallPosition.z);
        Vector3 spawnPosition = new Vector3(x, startBallPosition.y, z);
        return spawnPosition;
    }


    private float getRandomSpawnTime(float minTime, float maxTime)
    {
        return Random.Range(minTime, maxTime);
    }

    public void returnBall(Collider other)
    {
        if (other.gameObject.CompareTag("ball") || other.gameObject.CompareTag("ScoreBall"))
        {
            Ball ball = other.GetComponent<Ball>();
            ball.Velocity = Vector3.zero;
            ball.GetRigidbody.angularVelocity = Vector3.zero;

            BallSpawner.sharedInstance.ReturnToPool("ball", other.gameObject);
        }
        else if (other.gameObject.CompareTag("ScoreBall"))
        {
            Ball ball = other.GetComponent<Ball>();
            ball.Velocity = Vector3.zero;
            ball.GetRigidbody.angularVelocity = Vector3.zero;
            BallSpawner.sharedInstance.ReturnToPool("ScoreBall", other.gameObject);
        }
    }


    private void spawnBall()
    {
        int rand = Random.Range(0, 3);

        if(rand < 2)
        {
            if (BallSpawner.sharedInstance.isPoolContain("ball"))
            {
                GameObject ball = BallSpawner.sharedInstance.SpawnFromPool("ball", getRandomSpawnPosition(startBallPosition.position, endBallPosition.position), Quaternion.identity);
                ball.transform.parent = spawnedBalls.transform;
            }
            else if(BallSpawner.sharedInstance.isPoolContain("ScoreBall"))
            {
                GameObject ball = BallSpawner.sharedInstance.SpawnFromPool("ScoreBall", getRandomSpawnPosition(startBallPosition.position, endBallPosition.position), Quaternion.identity);
                ball.transform.parent = spawnedBalls.transform;
            }
        }
        else
        {
            if (BallSpawner.sharedInstance.isPoolContain("ScoreBall"))
            {
                GameObject ball = BallSpawner.sharedInstance.SpawnFromPool("ScoreBall", getRandomSpawnPosition(startBallPosition.position, endBallPosition.position), Quaternion.identity);
                ball.transform.parent = spawnedBalls.transform;
            }
            else if (BallSpawner.sharedInstance.isPoolContain("ball"))
            {
                GameObject ball = BallSpawner.sharedInstance.SpawnFromPool("ball", getRandomSpawnPosition(startBallPosition.position, endBallPosition.position), Quaternion.identity);
                ball.transform.parent = spawnedBalls.transform;
            }
        }

    }


    public void clearBalls()
    {
        int childCount = spawnedBalls.transform.childCount;
        for (int i = childCount - 1; i >= 0; i--)
        {
            GameObject ball = spawnedBalls.GetChild(i);
            ball.GetComponent<Rigidbody>().velocity = Vector3.zero;
            BallSpawner.sharedInstance.ReturnToPool(ball.gameObject.tag, ball.gameObject);
        }
    }

    #endregion


    #region points

    public void AddPoints()
    {
        if (!OngoingGame)
        {
            return;
        }

        if(remainingTime > 0)
        {
            ++points;
            if(remainingTime < bonusTime)
            {
                ++points;
            }
            scoreText.text = points.ToString("000");
            Debug.Log("Points:" + points);

        }
    }

    private int getTickets(int points)
    {
        int tics = 0;

        if(points >= 0 && points < 20)
        {
            tics = 14;
        }
        else if(points >= 20 && points < 50)
        {
            tics = 16;
        }
        else if (points >= 50 && points < 75)
        {
            tics = 18;
        }
        else if (points >= 75 && points < 100)
        {
            tics = 20;
        }
        else if (points >= 100 && points < 130)
        {
            tics = 25;
        }
        else if (points >= 130)
        {
            tics = 50;
        }

        return tics;
    }

    public override int Tickets => Tics;

    #endregion


    #region EndAndRestart

    public override void OnGameEnd()
    {
        Tics = getTickets(points);
        base.OnGameEnd();
        allowBallSpawn = false;
        moveMouth = false;
        inputFinished = true;
        paddle.applyForce = false;
        //leverTransform.DORotate(defaultHandleRotation.eulerAngles, 2.0f);
        //paddle.transform.DORotate(Vector3.zero, 2.0f);
    }

    public override void Reset()
    {
        base.Reset();
        clearBalls();
        inputFinished = false;
        isHoldingHandle = false;
        scoreText.text = "000";
        ticketsReceivedUI.gameObject.SetActive(false);
        leverTransform.localEulerAngles = new Vector3(defaultLeverRotation,0,0);
        paddle.gameObject.transform.localEulerAngles = leverTransform.localEulerAngles;
    }

    #endregion
}
