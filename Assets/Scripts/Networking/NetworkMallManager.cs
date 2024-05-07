using System;
using System.Collections;
using System.Collections.Generic;
using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Realtime;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Serialization;
using Hashtable = ExitGames.Client.Photon.Hashtable;
using Random = UnityEngine.Random;

public enum ENUMARCADEGAMES
{
    None,
    TestGame,
    HotShot, 
    QuickDrop,
    PianoKeys,
    FullTilt,
    Squiggle
}
public class NetworkMallManager : MonoBehaviour
{
    #region public variables

    public Transform[] spawnPoints;
    public GameObject player;

    public GameObject playerlistPanel;
    public GameObject playerListContentView;

    public GameObject playerListPrefab;

    [FormerlySerializedAs("StartGameButton")] public GameObject startGameButton;

    public List<GhostMallPlayer> ghostPlayers;

    #endregion

    #region private variables

    private Dictionary<int, GameObject> playerlist;
    
    #endregion


    #region PlayerCustomProps

    
    

    

  

    #endregion
    
    #region monobehaviour callbacks

 
    private void OnEnable()
    {
        NetworkManager.Instance.OnPlayerListUpdates += OnPlayerListUpdates;
        NetworkManager.Instance.onCustomPropertiesChange += ONCustomPropertiesChange;
        PhotonNetwork.NetworkingClient.EventReceived += NetworkingClientOnEventReceived;
    }

    void Start()
    {
        MachineInteractionUI.Instance.networkPlayerMovement = player.GetComponent<NetworkPlayerMovement>();
        
        playerlist = new Dictionary<int, GameObject>();
        OnPlayerListUpdates();
        SetPlayerOldValues();
    }

    private void OnDisable()
    {
        NetworkManager.Instance.OnPlayerListUpdates -= OnPlayerListUpdates;
        NetworkManager.Instance.onCustomPropertiesChange -= ONCustomPropertiesChange;
        PhotonNetwork.NetworkingClient.EventReceived -= NetworkingClientOnEventReceived;
    }
    #endregion

    #region private methods

    private void SetPlayerOldValues()
    {
        // from arcade game scenes
        if (PlayerPrefs.GetInt(NetworkManager.ISPLAYERFROMARCADEGAME) == 1)
        {
            PlayerPrefs.SetInt(NetworkManager.ISPLAYERFROMARCADEGAME, 0);
            player.transform.position = Utils.Vector3Extensions.GetVector3FromString(PlayerPrefs.GetString(NetworkManager.PLAYERPOSITION));
            player.transform.eulerAngles = Utils.Vector3Extensions.GetVector3FromString(PlayerPrefs.GetString(NetworkManager.PLAYERROTATION));
        }
        //From menu
        else
        {
            player.transform.position = spawnPoints[NetworkManager.Instance.LocalPlayer.ActorNumber-1].position;
        }

        if (NetworkManager.Instance.LocalPlayer.IsLocal)
        {
            Hashtable props = new Hashtable()
            {
                { NetworkManager.PLAYERCREATEDINMALL, true},
                {NetworkManager.PLAYERPOSITION, player.transform.position}
            };
            NetworkManager.Instance.LocalPlayer.SetCustomProperties(props);  
        }
        SendCustomEventForPlayerSpawned();
    }

    private void OnPlayerListUpdates()
    {
        ClearPlayerList();
        playerlistPanel.SetActive(true);
        foreach (KeyValuePair<int, Player> playerinfo in NetworkManager.Instance.playerListEntries)
        {
            GameObject playerEntry = Instantiate(playerListPrefab);
            playerEntry.transform.SetParent(playerListContentView.transform);
            playerEntry.transform.localScale = Vector3.one;
            playerEntry.GetComponent<PlayerListEntry>().
                Initialize(playerinfo.Value.ActorNumber, 
                    playerinfo.Value.NickName);
            playerlist.Add(playerinfo.Value.ActorNumber, playerEntry);
        }
    }

    private void ClearPlayerList()
    {
        foreach (GameObject go in playerlist.Values)
        {
            Destroy(go.gameObject);
        }
        playerlist.Clear();
        playerlist = null;
        playerlist = new Dictionary<int, GameObject>();
    }

    private void NetworkingClientOnEventReceived(EventData obj)
    {
        byte eventCode = obj.Code;
        switch (eventCode)
        {
            case NetworkManager.LoadGameLevelEventCode:
                object[] data = (object[]) obj.CustomData;
                string levelName = (string) data[0];
                if (NetworkManager.Instance.LocalPlayer.IsLocal)
                {
                    Hashtable props = new Hashtable()
                    {
                        { NetworkManager.PLAYERCREATEDINMALL, false}
                    };
                    NetworkManager.Instance.LocalPlayer.SetCustomProperties(props);  
                }
                NetworkManager.Instance.LoadPhotonScene(levelName);
                break;
            case NetworkManager.PlayerSpawnedEventCode:
                OnJoinGhostPlayer((object[])obj.CustomData);
                break;
            default:
                break;
        }
    }

    private void ONCustomPropertiesChange(int actorNumber, Hashtable props)
    {
        GameObject player;
        if (playerlist.TryGetValue(actorNumber, out player))
        {
            object arcadeGame;
            if (props.TryGetValue(NetworkManager.READYFORGAME, out arcadeGame))
            {
                player.GetComponent<PlayerListEntry>().OnPlayerReadyGameUpdate((ENUMARCADEGAMES) arcadeGame);
                startGameButton.SetActive(NetworkManager.Instance.CheckPlayersReadyArcadeGame((ENUMARCADEGAMES) arcadeGame));
            }
        }
    }

    private void OnJoinGhostPlayer(object[] data)
    {
        foreach (Player p in NetworkManager.Instance.AllNetworkPlayers)
        {
            object isPlayerJoined;
            if (p.CustomProperties.TryGetValue(NetworkManager.PLAYERCREATEDINMALL, out isPlayerJoined))
            {
                if (!(bool)isPlayerJoined)
                {
                    return;
                }
                if (NetworkManager.Instance.LocalPlayer.ActorNumber != p.ActorNumber)
                {
                    object pos;
                    if (p.CustomProperties.TryGetValue(NetworkManager.PLAYERPOSITION, out pos))
                    {
                        ghostPlayers[0].gameObject.transform.position = (Vector3)pos;
                    }
                    ghostPlayers[0].gameObject.SetActive(true);
                }
            }
            else
            {
            }
        }
    }

    #endregion

    #region public methods

    

    public void OnStartGameButtonClicked()
    {
        NetworkManager.Instance.CurrentRoom.IsOpen = false;
        NetworkManager.Instance.CurrentRoom.IsVisible = false;
        PlayerPrefs.SetString(NetworkManager.PLAYERPOSITION, player.transform.position.ToString());
        PlayerPrefs.SetString(NetworkManager.PLAYERROTATION, player.transform.eulerAngles.ToString());
        Destroy(player);

        // PhotonNetwork
        SendCustomEventForLoadGameScene();
    }
    
    #endregion

    #region PhotonCustomEvents

    private void SendCustomEventForLoadGameScene()
    {
        object[] content = new object[]
        {
            GetSceneNameFromArcadeMachineOnline((ENUMARCADEGAMES)
                NetworkManager.Instance.LocalPlayer.CustomProperties[NetworkManager.READYFORGAME
                ])
        };
        NetworkManager.Instance.RaisePhotonEvent(content, 1, NetworkManager.LoadGameLevelEventCode ); //(NetworkManager.LoadGameLevelEventCode, content, 1, SendOptions.SendReliable);
    }

    private void SendCustomEventForPlayerSpawned()
    {
        object[] content = new object[]
        {
            NetworkManager.Instance.LocalPlayer.ActorNumber
        };
        NetworkManager.Instance.RaisePhotonEvent(content, 1, NetworkManager.PlayerSpawnedEventCode);
    }

    #endregion
    
    #region static methods

    public static string GetSceneNameFromArcadeMachineOnline(ENUMARCADEGAMES machine)
    {
        string sceneName = "None";
        switch (machine)
        {
            case ENUMARCADEGAMES.TestGame:
                sceneName = "TestGame";
                break;
            case ENUMARCADEGAMES.HotShot:
                sceneName = "HotShotNetwork";
                break;
            case ENUMARCADEGAMES.QuickDrop:
                sceneName = "QuickDropNetwork";
                break;
            case ENUMARCADEGAMES.PianoKeys:
                sceneName = "PianoKeysNetwork";
                break;
            case ENUMARCADEGAMES.FullTilt:
                sceneName = "FullTiltNetwork";
                break;
            case ENUMARCADEGAMES.Squiggle:
                sceneName = "SquiggleNetwork";
                break;

            default:
                throw new ArgumentOutOfRangeException(nameof(machine), machine, null);
        }

        return sceneName;
    }

    #endregion
}

