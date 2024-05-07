using MoreLinq;

using System.Collections.Generic;
using System.Linq;

using UnityEngine;
using UnityEngine.InputSystem;

public class Smartphone : MonoBehaviour
{
    [SerializeField] protected Animator phoneAnimator;
    [SerializeField] protected RectTransform buttonContainer;
    [SerializeField] protected GameObject appsGO;
    [SerializeField] protected App_Inventory app_Inventory;
    [SerializeField] protected App_Bank app_Bank;

    public bool Enabled { get; protected set; }

    private void Start() => appsGO.SetActive(true);

    private void OnEnable()
    {
        GameInput.Register("Back", int.MinValue, OnBackInput_Low);
        GameInput.Register("Back", GameInput.ReferencePriorities.Screen, OnBackInput);

        GameInput.Register("Interaction", (int)GameInput.ReferencePriorities.Screen, InputBlocker);
        GameInput.Register("CharacterMove", (int)GameInput.ReferencePriorities.Screen, InputBlocker);
        GameInput.Register("CharacterRun", (int)GameInput.ReferencePriorities.Screen, InputBlocker);
        GameInput.Register("CameraZoom", (int)GameInput.ReferencePriorities.Screen, InputBlocker);
    }

    private void OnDisable()
    {
        GameInput.Deregister("Back", int.MinValue, OnBackInput_Low);
        GameInput.Deregister("Back", GameInput.ReferencePriorities.Screen, OnBackInput);

        GameInput.Deregister("Interaction", (int)GameInput.ReferencePriorities.Screen, InputBlocker);
        GameInput.Deregister("CharacterMove", (int)GameInput.ReferencePriorities.Screen, InputBlocker);
        GameInput.Deregister("CharacterRun", (int)GameInput.ReferencePriorities.Screen, InputBlocker);
        GameInput.Deregister("CameraZoom", (int)GameInput.ReferencePriorities.Screen, InputBlocker);
    }

    protected bool InputBlocker(InputAction.CallbackContext _) => Enabled;

    public void OpenSmartphone()
    {
        PlayerMovement.LocalInstance.Stop();
        phoneAnimator.SetBool("Enabled", Enabled = true);
        buttonContainer.gameObject.SetActive(true);
        app_Inventory.SetEnabled(false);
        app_Bank.SetEnabled(false);
    }

    public void CloseSmartphone()
    {
        phoneAnimator.SetBool("Enabled", Enabled = false);
        buttonContainer.gameObject.SetActive(false);
        app_Inventory.SetEnabled(false);
        app_Bank.SetEnabled(false);
    }

    public void OpenInventory()
    {
        phoneAnimator.SetBool("Landscape", true);
        buttonContainer.gameObject.SetActive(false);
        app_Inventory.SetEnabled(true);
    }

    public void CloseInventory()
    {
        phoneAnimator.SetBool("Landscape", false);
        buttonContainer.gameObject.SetActive(true);
        app_Inventory.SetEnabled(false);
    }

    public void OpenBank()
    {
        phoneAnimator.SetBool("Landscape", true);
        buttonContainer.gameObject.SetActive(false);
        app_Bank.SetEnabled(true);
    }

    public void CloseBank()
    {
        phoneAnimator.SetBool("Landscape", false);
        buttonContainer.gameObject.SetActive(true);
        app_Bank.SetEnabled(false);
    }

    private bool OnBackInput(InputAction.CallbackContext ctx)
    {
        if (ctx.started && Enabled)
        {
            if (app_Inventory.Enabled)
                CloseInventory();
            else if (app_Bank.Enabled)
                CloseBank();
            else CloseSmartphone();
            return true;
        }
        return false;
    }

    private bool OnBackInput_Low(InputAction.CallbackContext ctx)
    {
        if (ctx.started && !Enabled)
        {
            OpenSmartphone();
            return true;
        }
        return false;
    }
}
