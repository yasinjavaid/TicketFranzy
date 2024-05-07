using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using Cinemachine;
using DebugConsole;
using ExitGames.Client.Photon;
using Unity.Mathematics;
using UnityEngine;
using TMPro;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using Photon.Realtime;
using Photon.Pun;
using UnityEngine.PlayerLoop;
using Hashtable = ExitGames.Client.Photon.Hashtable;

public class GhostPianoPlayer : MonoBehaviour
{
    #region serializeFields

    private CinemachineVirtualCamera[] virtualCameras;
    [SerializeField] private TextMeshProUGUI scoreTextUi;
    
    [SerializeField] private TileSpawnerNetwork tileSpawner;
    
    [SerializeField] protected Transform virtualCamerasParent;
    
    [SerializeField] private PianoButton[] pianoButtons;
    
    [SerializeField] private GameObject[] wrongTiles;
    
    [SerializeField] private Image timeMeterFill;
    
    [SerializeField] private Material[] tileMaterial;
    
    #endregion

    #region public fields

    
   

    #endregion
   
    #region private fields

  
    private bool isClientOwner = true;

    #endregion

    #region monobehaviour callbacks

    #region events action

    public static Action NetworkTileStart;
    
    public static Action<int> OnGameEndScoreUpdate;
    

    #endregion
    private void OnEnable()
    {
        PhotonNetwork.NetworkingClient.EventReceived += NetworkingClientOnEventReceived;
        virtualCameras = virtualCamerasParent.GetComponentsInChildren<CinemachineVirtualCamera>();
        foreach (var button in pianoButtons)
        {
            button.buttonRenderer.material = tileMaterial[0];
        }
    }

    private void OnDisable()
    {
        PhotonNetwork.NetworkingClient.EventReceived -= NetworkingClientOnEventReceived;
    }


    public void Start()
    {
        isClientOwner = true;
        
    }

    private void Update()
    {
      
        if (isClientOwner)
        {
            
        }
       
    }
    #endregion

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
        gameObject.SetActive(false);
    }
    private void NetworkingClientOnEventReceived(EventData obj)
    {
        byte eventCode = obj.Code;
        object[] data = (object[]) obj.CustomData;
        switch (eventCode)
        {
            case NetworkManager.PianoSpawnEventCode:
                if ((int) data[0] != NetworkManager.Instance.LocalPlayer.ActorNumber)
                {
                    SpawnEvent(data);
                }
                break;
            case NetworkManager.PianoButtonStateEventCode:
                if ((int) data[0] != NetworkManager.Instance.LocalPlayer.ActorNumber)
                {
                    StartTile(data);
                }

                break;
            case NetworkManager.PianoWrongButtonPress:
                if ((int) data[0] != NetworkManager.Instance.LocalPlayer.ActorNumber)
                {
                    WrongButtonPress(data);
                }
                break;
            case NetworkManager.GameEndScoreUpdateEventCode:
                if ((int) data[0] != NetworkManager.Instance.LocalPlayer.ActorNumber)
                {
                    SetCamera("Ticket");
                    OnGameEndScoreUpdate?.Invoke((int) data[1]);
                }
                break;
            default:
                break;
        }
    }

    public void StartTile(object[] data)
    {
        NetworkTileStart?.Invoke();
        scoreTextUi.text = data[3].ToString();
        SetButtonMaterial((int)data[1]);
    }

    private void SpawnEvent(object[] data)
    {
        if (NetworkManager.Instance.LocalPlayer.ActorNumber != (int)data[0])
        {
            tileSpawner.actorNo = (int) data[0];
            string str = (string) data[1];
            List<string> result = str.Split(',').ToList();
            List<int> intResult = result.Select(int.Parse).ToList();
            tileSpawner.Spawner(intResult);
            SetCamera("Play");
        }
    }

    private void SetCamera(string cameraName)
    {
        foreach (var vcam in virtualCameras)
            vcam.gameObject.SetActive(vcam.name.StartsWith(cameraName));
    }

    private void WrongButtonPress(object[] data)
    {
        var no = (int) data[1];
        StartCoroutine(BlinkGameObject(wrongTiles[no], 3, 0.1f));
        SetButtonMaterial(no);
    }

    private void SetButtonMaterial(int button)
    {
        for (int i = 0; i < pianoButtons.Length; i++)
        {
            pianoButtons[i].buttonRenderer.material = tileMaterial[i == button? button : i];
        }
    }
}
