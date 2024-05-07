using System;
using System.Linq;

using TMPro;

using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

public class TicketsReceivedUI : MonoBehaviour
{
    [SerializeField] protected TextMeshProUGUI tmp_Tickets;
    [SerializeField] protected AnimationCurve curve_Tickets;
    [SerializeField] protected Animator animator;

    [SerializeField] protected TextMeshProUGUI tmp_TicketsPlayerB;
    
    [SerializeField] protected UnityEvent onInteractInput;
    [SerializeField] protected UnityEvent onBackInput;

    protected int targetAmount;
    protected float elapsedTime;
    protected float curveTime;

    public static Action OnTicketsShowComplete;
    public bool onTicketActioninvokedFlag = false;

    private void OnEnable()
    {
        GameInput.Register("Interact", GameInput.ReferencePriorities.Screen, OnInteractInput);
        GameInput.Register("Back", GameInput.ReferencePriorities.Screen, OnBackInput);
    }

    private void OnDisable()
    {
        GameInput.Deregister("Interact", GameInput.ReferencePriorities.Screen, OnInteractInput);
        GameInput.Deregister("Back", GameInput.ReferencePriorities.Screen, OnBackInput);
    }

    protected bool OnInteractInput(InputAction.CallbackContext ctx)
    {
        if (ctx.performed)
        {
            if (animator.GetBool("Show"))
            {
                onInteractInput?.Invoke();
            }
        
        }
        return true;
    }

    protected bool OnBackInput(InputAction.CallbackContext ctx)
    {
        if (ctx.performed)
        {
            if (animator.GetBool("Show"))
            {
                onBackInput?.Invoke();
            }
        }
        return true;
    }

    public void AnimateTickets(int amount)
    {
        targetAmount = amount;
        elapsedTime = 0;
        curveTime = curve_Tickets.keys.Max(key => key.time);
        ShowUp();
    }

    public void ShowTicketsForPlayerB(int ticketsPlayerB)
    {
        tmp_TicketsPlayerB.text = "Player B gets " + ticketsPlayerB + " Tickets";
    }

    public void ShowUp() => SetVisible(true);

    public void Hide() => SetVisible(false);

    public void SetVisible(bool value) => animator.SetBool("Show", value);

    private void Update()
    {
       

        if (elapsedTime < curveTime)
        {
            elapsedTime = Mathf.Clamp(elapsedTime + Time.deltaTime, 0, curveTime);
            tmp_Tickets.text = Mathf.RoundToInt(Mathf.Clamp(curve_Tickets.Evaluate(elapsedTime), 0, 1) * targetAmount).ToString();
        }
        else
        {
            if (onTicketActioninvokedFlag) return;
            onTicketActioninvokedFlag = true;
            OnTicketsShowComplete?.Invoke() ;
        }
    }
}
