using System;
using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using Photon.Realtime;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using Hashtable = ExitGames.Client.Photon.Hashtable;

public class RestartTimer : MonoBehaviour
{
    #region serializeFields

    [SerializeField] private double playerPlayAgainTime = 10;
    [SerializeField] private TextMeshProUGUI timeCounterText;
    [SerializeField] private TextMeshProUGUI iWantToPlayText;
    
    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private PhotonView photonView;

    [SerializeField] protected UnityEvent onPlayAgain;
    [SerializeField] protected UnityEvent onBackInput;

    #endregion

    #region public fields



    #endregion

    #region private fields

    private bool iWantsToPlayAgain = false;
    private bool IsCounterStarted { get; set; }

    

    private double startedTime;

    #endregion

    #region monobehaviour callbacks

    private void OnEnable()
    {
        GameInput.Register("Interact", GameInput.ReferencePriorities.Screen, OnInteractInput);
        GameInput.Register("Back", GameInput.ReferencePriorities.Screen, OnBackInput);
        TicketsReceivedUI.OnTicketsShowComplete += OnTicketsShowComplete;
        NetworkManager.Instance.onCustomPropertiesChange += ONCustomPropertiesChange;
        SetMyCustomProperties(false);
    }



    private void OnDisable()
    {
        GameInput.Deregister("Interact", GameInput.ReferencePriorities.Screen, OnInteractInput);
        GameInput.Deregister("Back", GameInput.ReferencePriorities.Screen, OnBackInput);
        TicketsReceivedUI.OnTicketsShowComplete -= OnTicketsShowComplete;
        NetworkManager.Instance.onCustomPropertiesChange -= ONCustomPropertiesChange;

    }



    private void Update()
    {
        if (!IsCounterStarted) return;
        timeCounterText.text = CounterTime();
    }

    #endregion

    #region EvntsBindings

    private bool OnInteractInput(InputAction.CallbackContext ctx)
    {
        
            if (!ctx.performed) return false;
            SetMyCustomProperties(true);
            iWantToPlayText.gameObject.SetActive(true);
            iWantsToPlayAgain = true;
            return true;
      
    }

    private bool OnBackInput(InputAction.CallbackContext ctx)
    {
        if (ctx.performed)
        {
            photonView.RPC("DontWantToPlayAgainByAnyPlayer", RpcTarget.All);
            onBackInput?.Invoke();
            return true;
        }

        return false;

    }

    private void OnTicketsShowComplete()
    {
        ShowUp();
        this.InvokeDelayed(0.5f, StartTimer);
      //  StartTimer();
    }

    private void ONCustomPropertiesChange(int playerActorNumber, Hashtable hashtable)
    {
        if (!IsCounterStarted) return;
        if (CheckIfBothPlayerWantsToPlay())
        {
            onPlayAgain?.Invoke();
        }
    }

    #endregion

    #region public methods

    public void ShowUp() => SetVisible(true);

    public void Hide() => SetVisible(false);

    public void SetTimerStarter(bool value) => IsCounterStarted = value;

    public void SetMyCustomProperties(bool value)
    {
        if (NetworkManager.Instance.LocalPlayer.IsLocal)
        {
            Hashtable props = new Hashtable()
            {
                {NetworkManager.PLAYERSREADYFORREPLAY, value}
            };
            NetworkManager.Instance.LocalPlayer.SetCustomProperties(props);
        }
    }


    #endregion

    #region private methods


    private void StartTimer()
    {
        IsCounterStarted = true;
        iWantToPlayText.gameObject.SetActive(false);
        startedTime = NetworkManager.Instance.Time;
    }

    private string CounterTime() => IsCounterStarted
        ? CheckIsTimerFinished(playerPlayAgainTime - StartedTime()).ToString("00")
        : "";



    private double StartedTime() => (NetworkManager.Instance.Time - startedTime);


    private double CheckIsTimerFinished(double runningTime)
    {
        if (runningTime <= 0.0f)
        {
            IsCounterStarted = false;
            //Gameover
            OnTimerEnd();

        }

        return runningTime;
    }

    private void OnTimerEnd()
    {
        if (CheckIfBothPlayerWantsToPlay())
        {
            onPlayAgain?.Invoke();
        }
        else
        {
            onBackInput?.Invoke();
        }
    }

    private bool CheckIfBothPlayerWantsToPlay()
    {
        foreach (Player p in NetworkManager.Instance.AllNetworkPlayers)
        {
            object wantToPlayAgain;
            if (p.CustomProperties.TryGetValue(NetworkManager.PLAYERSREADYFORREPLAY, out wantToPlayAgain))
            {
                if (!(bool) wantToPlayAgain)
                {
                    return false;
                }
            }
            else
            {
                return false;
            }
        }

        //all players ready for Play
        return true;
    }

    private void SetVisible(bool value)
    {
        canvasGroup.alpha = value ? 1 : 0;
    }



    [PunRPC]
    void DontWantToPlayAgainByAnyPlayer()
    {
        onBackInput?.Invoke();
    }

    #endregion
}
