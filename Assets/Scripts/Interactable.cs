using MoreLinq;

using System.Collections.Generic;
using System.Linq;

using TMPro;

using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

public class Interactable : MonoBehaviour
{
    [SerializeField] protected string interactionName;
    [SerializeField] protected TextMeshProUGUI tmp_interactionName;
    [SerializeField] protected Animator interactionAnimator;
    [SerializeField] protected UnityEvent onInteract;

    protected List<PlayerMovement> nearbyPlayers = new List<PlayerMovement>();

    private void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponentInParent(out PlayerMovement playerMovement))
        {
            nearbyPlayers.Add(playerMovement);
            tmp_interactionName.text = interactionName;
            interactionAnimator.SetBool("Enabled", true);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.TryGetComponentInParent(out PlayerMovement playerMovement))
        {
            nearbyPlayers.Remove(playerMovement);
            interactionAnimator.SetBool("Enabled", false);
        }
    }

    private void OnEnable() => GameInput.Register("Interaction", GameInput.ReferencePriorities.Environment, OnInteractionInput);

    private void OnDisable() => GameInput.Deregister("Interaction", GameInput.ReferencePriorities.Environment, OnInteractionInput);

    private bool OnInteractionInput(InputAction.CallbackContext ctx)
    {
        if (ctx.started && nearbyPlayers.Count > 0)
        {
            nearbyPlayers.Where(pm => pm).ForEach(pm => pm.Stop());
            onInteract?.Invoke();
            interactionAnimator.SetBool("Enabled", false);
            return true;
        }
        return false;
    }
}