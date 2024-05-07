using System;
using System.Linq;

using ExitGames.Client.Photon;

using Photon.Pun;

using TMPro;

using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class MachineInteractionUI : SingletonMB<MachineInteractionUI>
{
    [SerializeField] protected Animator interactionAnimator;
    [SerializeField] protected Animator detailsAnimator;
    [FormerlySerializedAs("playerMovement")]
    public NetworkPlayerMovement networkPlayerMovement;
    [SerializeField] protected TextMeshProUGUI tmp_interactionMachineName;
    [SerializeField] protected TextMeshProUGUI tmp_detailsMachineName;
    [SerializeField] protected TextMeshProUGUI tmp_detailsMachineDescription;
    [SerializeField] protected TextMeshProUGUI tmp_detailsMachineCost;
    [SerializeField] protected Image img_detailsMachinePreview;
    [SerializeField] protected GameObject btn_play;
    ArcadeMachine interactingMachine;
    ArcadeMachine nearbyMachine;

    private void OnEnable()
    {
        GameInput.Register("Interaction", GameInput.ReferencePriorities.Environment, OnInteractionInput);
        GameInput.Register("Back", GameInput.ReferencePriorities.Environment, OnBackInput);
        GameInput.Register("CharacterMove", (int)GameInput.ReferencePriorities.Screen, InputBlocker);
        GameInput.Register("CharacterRun", (int)GameInput.ReferencePriorities.Screen, InputBlocker);
        GameInput.Register("CameraZoom", (int)GameInput.ReferencePriorities.Screen, InputBlocker);
    }

    private void OnDisable()
    {
        GameInput.Deregister("Interaction", GameInput.ReferencePriorities.Environment, OnInteractionInput);
        GameInput.Deregister("Back", GameInput.ReferencePriorities.Environment, OnBackInput);
        GameInput.Deregister("CharacterMove", (int)GameInput.ReferencePriorities.Screen, InputBlocker);
        GameInput.Deregister("CharacterRun", (int)GameInput.ReferencePriorities.Screen, InputBlocker);
        GameInput.Deregister("CameraZoom", (int)GameInput.ReferencePriorities.Screen, InputBlocker);
    }

    protected bool InputBlocker(InputAction.CallbackContext _) => interactingMachine;

    private bool OnInteractionInput(InputAction.CallbackContext ctx)
    {
        if (ctx.performed && networkPlayerMovement && nearbyMachine)
        {
            Hashtable props = new Hashtable() { { NetworkManager.READYFORGAME, nearbyMachine.currentMachine } };
            NetworkManager.Instance.LocalPlayer.SetCustomProperties(props);
        }
        if (!ctx.started) return false;
        if (interactingMachine)
        {
            SceneManager.LoadScene(interactingMachine.GameSceneName, LoadSceneMode.Single);
            return true;
        }
        else if (nearbyMachine)
        {
            PlayerMovement.LocalInstance.Stop();
            nearbyMachine.UseMachine();
            interactingMachine = nearbyMachine;
            tmp_detailsMachineName.text = interactingMachine.Name;
            tmp_detailsMachineDescription.text = interactingMachine.Description;

            if (PhotonNetwork.IsConnected && !NetworkManager.Instance.IsOffline)
            {
                tmp_detailsMachineCost.text = $"Ready for {interactingMachine.Name} Game.";
                btn_play.SetActive(false);
            }
            else tmp_detailsMachineCost.text = $"{interactingMachine.Price} Credits to Play";

            img_detailsMachinePreview.sprite = interactingMachine.SpritePreview;
            img_detailsMachinePreview.gameObject.SetActive(img_detailsMachinePreview.sprite);
            return true;
        }
        else return false;
    }

    private bool OnBackInput(InputAction.CallbackContext ctx)
    {
        if (networkPlayerMovement && ctx.performed)
        {
            Hashtable props = new Hashtable() { { NetworkManager.READYFORGAME, ENUMARCADEGAMES.None } };
            PhotonNetwork.LocalPlayer.SetCustomProperties(props);
            btn_play.SetActive(true);
        }
        if (!ctx.started) return false;
        if (interactingMachine)
        {
            interactingMachine.LeaveMachine();
            interactingMachine = null;
            return true;
        }
        else return false;
    }

    private void Update()
    {
        if (networkPlayerMovement)
        { // For online
            nearbyMachine = networkPlayerMovement.NearbyMachines.Where(m => m && !m.InUse)
                .OrderBy(m => Vector3.Distance(transform.position, m.SittingPosition))
                .FirstOrDefault();
            if (nearbyMachine) tmp_interactionMachineName.text = nearbyMachine.Name;
            interactionAnimator.SetBool("Enabled", nearbyMachine && !interactingMachine);
            detailsAnimator.SetBool("Enabled", interactingMachine);
        }
        else
        {//For single player
            nearbyMachine = PlayerMovement.LocalInstance.NearbyMachines.Where(m => m && !m.InUse)
                .OrderBy(m => Vector3.Distance(transform.position, m.SittingPosition))
                .FirstOrDefault();
            if (nearbyMachine) tmp_interactionMachineName.text = nearbyMachine.Name;
            interactionAnimator.SetBool("Enabled", nearbyMachine && !interactingMachine);
            detailsAnimator.SetBool("Enabled", interactingMachine);
        }


    }
}
