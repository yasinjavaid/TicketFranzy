using System;
using System.Collections;
using System.Collections.Generic;
using MetaData;
using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;
using UnityEngine.UI;

public class PianoKeysNetwork : ArcadeGame
 { 
     [Header("PianoKeys")]

    #region serializeFields

    [SerializeField] private Camera cam;
    
    [SerializeField]
    private PianoEvents pianoEvents;
    
    [SerializeField] private PianoButton[] pianoButtons;
    
    [SerializeField] private GameObject[] wrongTiles;
    
    [SerializeField] private TextMeshPro scoreText;
    
    [SerializeField] private TextMeshProUGUI scoreTextUi;
    
    [SerializeField] private int scoreFactor;
    
    [SerializeField] private float counterTime; 
    
    [SerializeField] private MetadataManager metadataManager;
    
    [SerializeField] private Image timeMeterFill, timeMeterPlayerB;

    [SerializeField] private Material[] tileMaterial;
    #endregion

    #region Overrides variables 

    public override int Tickets => Score >= 10 ? Score * 2 : Score * 3;

    #endregion
    
    #region public fields

   

    #endregion
   
    #region private fields

    private PianoButton interactiveButton;

    private PianoTile currentTile;

    private bool IsCounterStarted { get; set; }
    private DateTime startedTime;
    
    private Mouse mouse => Mouse.current;
   
    
    private bool isTilePressAvailable = false;

    private bool isWaitStarted = false;

    private bool StartTimer = false;
    
    private bool playIT = true;
    
    private int buttonID = -1;

    private float totalTime; 
    
    #endregion

    #region delegates, action and events
    
   

    #endregion


    #region inputs

    private bool TileA(InputAction.CallbackContext ctx)
    {
        if (ctx.started)
        {
            TilePressDown(0);
            return true;
        }
        else if (ctx.canceled)
        {
            TilePressUp(0);
        }
        return false;
    }
    private bool TileB(InputAction.CallbackContext ctx)
    {
        if (ctx.started)
        {
            TilePressDown(1);

            return true;
        }
        else if (ctx.canceled)
        {
            TilePressUp(1);
            return true;
        }
        return false;
    }
    private bool TileC(InputAction.CallbackContext ctx)
    {
        if (ctx.started)
        {
            TilePressDown(2);

            return true;
        }
        else if (ctx.canceled)
        {
            TilePressUp(2);
            return true;
        }
        return false;
    }
    private bool TileD(InputAction.CallbackContext ctx)
    {
        if (ctx.started)
        {
            TilePressDown(3);
            return true;
        }
        else if (ctx.canceled)
        {
            TilePressUp(3);
            return true;
        }
        return false;
    }
    private bool Press(InputAction.CallbackContext ctx)
    {
        if (ctx.started)
        {
            if (CastRayOnScreenPoint())
            {
                TilePressDown(interactiveButton.buttonNo);
            }
        }
        else if (ctx.canceled)
        {
            if (interactiveButton)
            {
                TilePressUp(interactiveButton.buttonNo);
                interactiveButton = null;
            }
        }
        return false;
    }

    #endregion

    #region monobehaviour callbacks


    public void IsFromGame()
    {
        PlayerPrefs.SetInt(NetworkManager.ISPLAYERFROMARCADEGAME, 1);
    }
    public override void StartGame()
    {
        base.StartGame();
        totalTime = counterTime;
        metadataManager.Init();
        pianoEvents.SpawnTileAction();
        GettingCurrentTile();
        isTilePressAvailable = true;
        SetCamera("Play");
        this.InvokeDelayed(1, SendTilesSpawnEvent);
        StartedGame();
    }
    
    protected override void OnEnable()
    {
        GameInput.Register("Press",GameInput.ReferencePriorities.Character, Press);
        GameInput.Register("TileA",GameInput.ReferencePriorities.Character, TileA);
        GameInput.Register("TileB",GameInput.ReferencePriorities.Character, TileB);
        GameInput.Register("TileC",GameInput.ReferencePriorities.Character, TileC);
        GameInput.Register("TileD",GameInput.ReferencePriorities.Character, TileD);
        GhostPianoPlayer.OnGameEndScoreUpdate += OnGameEndScoreUpdate;
        foreach (var button in pianoButtons)
        {
            button.buttonRenderer.material = tileMaterial[0];
        }
    }

    protected override void Update()
    {
        base.Update();
        CounterTime();
    }

   
    protected override void OnDisable()
    {
        UnsubscribeEvents();
        GhostPianoPlayer.OnGameEndScoreUpdate -= OnGameEndScoreUpdate;
        SetWhiteMaterialToAllButtons();
    }
    
    public override void OnGameEnd()
    {
        OngoingGame = false;
        isTilePressAvailable = false;
        UnsubscribeEvents();
        isTilePressAvailable = false;
        base.OnGameEnd();
    }

    private void UnsubscribeEvents()
    {
        GameInput.Deregister("Press",GameInput.ReferencePriorities.Character, Press);
        GameInput.Deregister("TileA",GameInput.ReferencePriorities.Character, TileA);
        GameInput.Deregister("TileB",GameInput.ReferencePriorities.Character, TileB);
        GameInput.Deregister("TileC",GameInput.ReferencePriorities.Character, TileC);
        GameInput.Deregister("TileD",GameInput.ReferencePriorities.Character, TileD);
       
    }

    #endregion
    

    
    #region cast rays
    private bool CastRayOnScreenPoint()
    {
        Vector3 coor = mouse.position.ReadValue();
        RaycastHit hit; 
        if (Physics.Raycast(cam.ScreenPointToRay(coor), out hit) ) 
        {
            if (hit.collider.gameObject.TryGetComponent(out PianoButton pButton))
            {
                interactiveButton = pButton;
                return true;
            }
        }

        return false;
    }

    #endregion

    #region tile

    
    private void TilePressDown(int tileNo)
    { 
        if (!isTilePressAvailable || !playIT) return;
        if (currentTile && currentTile.columnNo == tileNo)
        {
            playIT = false;
            buttonID = tileNo;
            //correct match
            PlayPianoButtonSound();
            StartDroppingTiles();
            CorrectMatch(currentTile.ticketsCount, tileNo);
            currentTile.isUsed = true;
            GettingCurrentTile();
        //    if (!IsCounterStarted) StartedGame();
        }
        else
        {
            //wrong match
            isTilePressAvailable = false;
            StartCoroutine(BlinkGameObject(wrongTiles[tileNo],3,0.1f));
            SendWrongPiano(tileNo);
        }
        SetWhiteMaterialToAllButtons();
        pianoButtons[tileNo].ButtonDownAnim();
        pianoButtons[tileNo].buttonRenderer.material = tileMaterial[1];
    }

    private void TilePressUp(int tileNo)
    {
        pianoButtons[tileNo].ButtonUpAnim();
        if (!isTilePressAvailable) return;
    }
    private void StartedGame()
    {
        StartTimeCounter();
    }


    private void StartDroppingTiles()
    {
        pianoEvents.TileStartAction();
        this.InvokeDelayed(0.1f, () => playIT = true);
    }

    private void GettingCurrentTile()
    {
        currentTile = pianoEvents.GetPianoTileDelegate();  //GetCurrentTile?.Invoke();
        if (currentTile == null)
        {
            //GameEnd
            OnGameEnd();
            SendEndTicketsEvent();
        }
    }

    private void PlayPianoButtonSound()
    {
        pianoEvents.PlayPianoSound(currentTile.soundId);
    }
    
    private void CorrectMatch(int tiles , int button)
    {
        Score += tiles;
        scoreTextUi.text = Score.ToString();
        SendPianoDownEvent(button, true);
    }
    
    private void SetWhiteMaterialToAllButtons()
    {
        foreach (var button in pianoButtons)
        {
            button.buttonRenderer.material = tileMaterial[0];
        }
    }
    private IEnumerator BlinkGameObject(GameObject gameObject, int numBlinks, float seconds)
    {
        gameObject.SetActive(true);
        // In this method it is assumed that your game object has a SpriteRenderer component attached to it
        Image renderer = gameObject.GetComponent<Image>();
        // disable animation if any animation is attached to the game object
        //      Animator animator = gameObject.GetComponent<Animator>();
        //      animator.enabled = false; // stop animation for a while
        for (int i = 0; i < numBlinks * 2; i++)
        {
            //toggle renderer
            renderer.enabled = !renderer.enabled;
            //wait for a bit
            yield return new WaitForSeconds(seconds);
        }
        //make sure renderer is enabled when we exit
        renderer.enabled = true;
        //    animator.enabled = true; // enable animation again, if it was disabled before
     
    
        gameObject.SetActive(false);
        yield return new WaitForEndOfFrame();
        isTilePressAvailable = true;
    }

    #endregion
  
    
    #region timers
    private string CounterTime() => IsCounterStarted ? 
        CheckIsTimerFinished(counterTime - StartedTime()).ToString("00")
        : "";

    

    private float StartedTime() => (float) (DateTime.Now - startedTime).TotalSeconds;

    private void StartTimeCounter()
    {
        IsCounterStarted = true;
        startedTime = DateTime.Now;
    }
    private float CheckIsTimerFinished(float runningTime)
    {
        var filamount = runningTime / counterTime;
        timeMeterFill.fillAmount = filamount;
        timeMeterPlayerB.fillAmount = filamount;
        if (runningTime <= 0.0f)
        {
            IsCounterStarted = false;
      
            OnGameEnd();
            SendEndTicketsEvent();
        }
        return runningTime;
    }
    //  private float CountCounterTime
    #endregion

    
    private void OnGameEndScoreUpdate(int obj)
    {
        ticketsReceivedUI.ShowTicketsForPlayerB(obj);
    }
    #region network events

    private void SendTilesSpawnEvent()
    {
        var list =  pianoEvents.OnGetTilesOrderLocalPlayer();
        string listData = string.Join(",", list.ToArray());
  
    
        object[] dataToSend = new object[]
        {
            NetworkManager.Instance.LocalPlayer.ActorNumber,
            listData                 
        };
        NetworkManager.Instance.RaisePhotonEvent(dataToSend, 1, NetworkManager.PianoSpawnEventCode);
    }

    private void SendPianoDownEvent(int pianoButtonNo, bool isHold)
    {
        object[] dataToSend = new object[]
        {
            NetworkManager.Instance.LocalPlayer.ActorNumber,
            pianoButtonNo,
            isHold,
            Score
        };
        NetworkManager.Instance.RaisePhotonEvent(dataToSend, 1, NetworkManager.PianoButtonStateEventCode);
    }

    private void SendWrongPiano(int pianoNumber)
    {
        object[] dataToSend = new object[]
        {
            NetworkManager.Instance.LocalPlayer.ActorNumber,
            pianoNumber
        };
        NetworkManager.Instance.RaisePhotonEvent(dataToSend, 1, NetworkManager.PianoWrongButtonPress);
    }
    private void SendEndTicketsEvent()
    {
        object[] dataToSend = new object[]
        {
            NetworkManager.Instance.LocalPlayer.ActorNumber,
            Tickets
        };
        NetworkManager.Instance.RaisePhotonEvent(dataToSend, 1, NetworkManager.GameEndScoreUpdateEventCode);
    }

    #endregion
}