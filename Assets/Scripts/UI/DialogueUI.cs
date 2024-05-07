using MoreLinq;

using System.Collections.Generic;
using System.Linq;

using TMPro;

using UnityEngine;
using UnityEngine.InputSystem;

public class DialogueUI : MonoBehaviour
{
    [SerializeField] protected Animator animator;
    [SerializeField] protected TextMeshProUGUI tmp_Title;
    [SerializeField] protected TextMeshProUGUI tmp_Dialogue;

    public string Title { get => tmp_Title.text; set => tmp_Title.text = value; }
    public bool Enabled { get; protected set; }

    public void DisplayText(string text)
    {
        tmp_Dialogue.text = text;
        SetEnabled(true);
    }

    public virtual void SetEnabled(bool enabled) => animator.SetBool("Enabled", Enabled = enabled);

    private void OnEnable()
    {
        GameInput.Register("Interaction", GameInput.ReferencePriorities.Screen, OnInteractionInput);
        GameInput.Register("Back", GameInput.ReferencePriorities.Screen, OnBackInput);
        GameInput.Register("CharacterMove", GameInput.ReferencePriorities.Screen, InputBlocker);
        GameInput.Register("CameraZoom", GameInput.ReferencePriorities.Screen, InputBlocker);
    }

    private void OnDisable()
    {
        GameInput.Deregister("Interaction", GameInput.ReferencePriorities.Screen, OnInteractionInput);
        GameInput.Deregister("Back", GameInput.ReferencePriorities.Screen, OnBackInput);
        GameInput.Deregister("CharacterMove", GameInput.ReferencePriorities.Screen, InputBlocker);
        GameInput.Deregister("CameraZoom", GameInput.ReferencePriorities.Screen, InputBlocker);
    }

    public bool InputBlocker(InputAction.CallbackContext _) => Enabled;

    private bool OnInteractionInput(InputAction.CallbackContext ctx)
    {
        if (Enabled && ctx.started)
        {
            SetEnabled(false);
            return true;
        }
        return Enabled;
    }

    private bool OnBackInput(InputAction.CallbackContext ctx)
    {
        if (Enabled && ctx.started)
        {
            SetEnabled(false);
            return true;
        }
        return Enabled;
    }
}
