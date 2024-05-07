using System;
using ExitGames.Client.Photon;
using Photon.Realtime;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using Networking;
using Photon.Pun;
using UnityEngine;
using Random = UnityEngine.Random;

public class NetworkManager : MonoBehaviourPunCallbacks
{
    #region constants


    #region custom events

    //Custom Events
    public const byte LoadGameLevelEventCode = 1;
    public const byte BallPositionEventCode = 11;
    public const byte BallShotEventCode = 12;
    public const byte BallRespawnEventCode = 13;
    public const byte BallScoredEventCode = 14;
    
    
    public const byte GameEndScoreUpdateEventCode = 15;

    public const byte ScoreDisplayUpdateEventCode = 16;
    public const byte WheelRotateEventCode = 17;
    public const byte BallCountChangedEvent = 18;
    public const byte StartGameEvent = 19;


    public const byte PianoSpawnEventCode = 35;
    public const byte PianoButtonStateEventCode = 36;
    public const byte PianoWrongButtonPress = 37;


   
    public const byte CameraPanningEventCode = 76;


    
    //Custom Events for Mall `
    public const byte PlayerSpawnedEventCode = 50;
    public const byte PlayerPositionMallEventCode = 51;
    
    
    //Custom Events for UIs
    public const byte AllPlayersGoToMall = 70;
    

    #endregion
   
    
    //Custom keys
    public const string READYFORGAME = "READYFORGAME";
    public const string ISPLAYERFROMARCADEGAME = "ISPLAYERFROMARCADEGAME";
    public const string PLAYERPOSITION = "PLAYERPOSITION";
    public const string PLAYERROTATION = "PLAYERROTATION";
    public const string PLAYERCREATEDINMALL = "PLAYERCREATEDINMALL";
    public const string PLAYERSREADYFORREPLAY = "PLAYERSREADYFORREPLAY";
    public const string PLAYERREADYFORSCORE = "PLAYERREADYFORSCORE";


    #endregion

    #region serialize fields

    #endregion

    #region public variables

    public static NetworkManager Instance;
    public byte maxPlayersInRoom = 2;
    public Dictionary<int, Player> playerListEntries;
    public Player LocalPlayer => PhotonNetwork.LocalPlayer;

    public Player[] AllNetworkPlayers => PhotonNetwork.PlayerList;
    public ServerConnection Server => PhotonNetwork.Server;
    public Room CurrentRoom => PhotonNetwork.CurrentRoom;
    public Double Time => PhotonNetwork.Time;
    public int Ping => PhotonNetwork.GetPing();
    

    public bool IsOffline => PhotonNetwork.OfflineMode;

    public bool IsConnected => PhotonNetwork.IsConnected;
    

    public bool IsMasterClient => PhotonNetwork.IsMasterClient;
   
    #endregion
    #region private variables
    private Dictionary<string, RoomInfo> cachedRoomList;
    private Dictionary<string, GameObject> roomListEntries;
   

    private GameObject roomListContentView;
    private GameObject roomListPrefab;
    

    #endregion

    #region delegates, action and events

    public Action<int, Hashtable> onCustomPropertiesChange;
    public Action OnPlayerListUpdates;
    private Action OnConnectedToMasterAction;

    #endregion
    #region monobehaviour callbacks

    private void Awake()
    {
        //PhotonNetwork.
        Instance = this;
        PhotonNetwork.AutomaticallySyncScene = false;
        cachedRoomList = new Dictionary<string, RoomInfo>();
        roomListEntries = new Dictionary<string, GameObject>();
        DontDestroyOnLoad(this.gameObject);
    }
    
    #endregion

    #region photon callbacks

    public override void OnConnectedToMaster()
    {
        Debug.LogError(PhotonNetwork.LocalPlayer.UserId);
        OnConnectedToMasterAction?.Invoke();
    }

    public override void OnRoomListUpdate(List<RoomInfo> roomList)
    {
        ClearRoomListView();

        UpdateCachedRoomList(roomList);
        
        UpdateRoomListView();
    }

    public override void OnJoinedLobby()
    {
        // whenever this joins a new lobby, clear any previous room lists
        cachedRoomList.Clear();
        ClearRoomListView();
    }

    // note: when a client joins / creates a room, OnLeftLobby does not get called, even if the client was in a lobby before
    public override void OnLeftLobby()
    {
        cachedRoomList.Clear();
        ClearRoomListView();
    }

    public override void OnCreateRoomFailed(short returnCode, string message)
    {
     
    }

    public override void OnJoinRoomFailed(short returnCode, string message)
    {
        
    }

    public override void OnJoinRandomFailed(short returnCode, string message)
    {
        CreateRoom();
    }

    public override void OnJoinedRoom()
    {
        cachedRoomList.Clear();
        if (playerListEntries == null)
        {
            playerListEntries = new Dictionary<int, Player>();
        }

        foreach (Player playerInfo in PhotonNetwork.PlayerList)
        {
            playerListEntries.Add(playerInfo.ActorNumber, playerInfo);
        }
        OnPlayerListUpdates?.Invoke();
        PhotonNetwork.LoadLevel("MallNetwork");
    }

    public override void OnLeftRoom()
    {
        
        playerListEntries.Clear();
        playerListEntries = null;
        OnPlayerListUpdates?.Invoke();

    }

    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        playerListEntries.Add(newPlayer.ActorNumber, newPlayer);
        PlayerPrefs.SetInt(NetworkManager.ISPLAYERFROMARCADEGAME, 0);
        OnPlayerListUpdates?.Invoke();
    }

    public override void OnPlayerLeftRoom(Player otherPlayer)
    { 
        playerListEntries.Remove(otherPlayer.ActorNumber);
        OnPlayerListUpdates?.Invoke();
    }

    public override void OnMasterClientSwitched(Player newMasterClient)
    {
        if (PhotonNetwork.LocalPlayer.ActorNumber == newMasterClient.ActorNumber)
        {
            Debug.LogError("MasterClientChanged");
          //  StartGameButton.gameObject.SetActive(CheckPlayersReady());
        }
    }

    public override void OnPlayerPropertiesUpdate(Player player, Hashtable changedProps)
    {
        onCustomPropertiesChange?.Invoke(player.ActorNumber, changedProps);
    }


    #endregion

    #region public methods
    
    public void ConnectToMaster(string playerName, Action onComplete = null)
    {
        if (!playerName.Equals(""))
        {
            PhotonNetwork.LocalPlayer.NickName = playerName;
            PhotonNetwork.ConnectUsingSettings();
            OnConnectedToMasterAction = onComplete;
        }
        else
        {
            Debug.LogError("Player Name is invalid.");
        }
    }

    public void SetRoomListContent(GameObject scrollViewContent, GameObject prefabRoomEntry)
    {
        roomListContentView = scrollViewContent;
        roomListPrefab = prefabRoomEntry;
    }

    public void OnRoomListButtonClicked()
    {
        if (!PhotonNetwork.InLobby)
        {
            PhotonNetwork.JoinLobby();
        }
    }

    public void OnJoinedRoomClicked()
    {
        JoinRandomRoom();
    }

    public bool LeaveLobby()
    {
        if (PhotonNetwork.InLobby)
        { 
            return PhotonNetwork.LeaveLobby();
        }
        return false;
    }

    public void JoinLobby()
    {
        if (!PhotonNetwork.InLobby)
        {
            PhotonNetwork.JoinLobby();
        }
    }
    public bool CheckPlayersReadyArcadeGame(ENUMARCADEGAMES game)
    {
        if (!PhotonNetwork.IsMasterClient)
        {
            return false;
        }

        foreach (Player p in PhotonNetwork.PlayerList)
        {
            object ArcadeGame;
            if (p.CustomProperties.TryGetValue(READYFORGAME, out ArcadeGame))
            {
                if ((ENUMARCADEGAMES)ArcadeGame != game)
                {
                    return false;
                }
                else if ((ENUMARCADEGAMES)ArcadeGame == ENUMARCADEGAMES.None)
                {
                    return false;
                }
                
            }
            else
            {
                return false;
            }
        }

        return true;
    }

    public void LoadPhotonScene(string sceneName)
    {
        PhotonNetwork.LoadLevel(sceneName);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="dataToSent"></param>
    /// <param name="options"> 0 for OTHERS, 1 for ALL, 2 for MASTER CLIENT</param>
    /// <param name="eventCode"></param>
    public void RaisePhotonEvent(object[] dataToSent, int options, byte eventCode )
    {
        RaiseEventOptions eventOptions = new RaiseEventOptions()
        {
            Receivers = options == 0? ReceiverGroup.Others : 
                options == 1? ReceiverGroup.All : 
                options == 2? ReceiverGroup.MasterClient : ReceiverGroup.All
        };
        PhotonNetwork.RaiseEvent(eventCode, dataToSent, eventOptions, SendOptions.SendReliable);
    }

    /*public T GetValueFromhashTable<T>(T param, Hashtable hashtable, )
    {
        object wantToPlayAgain;
        if (hashtable.TryGetValue(NetworkManager.PLAYERSREADYFORREPLAY, out wantToPlayAgain))
        {
            if (!(bool) wantToPlayAgain)
            {
               // return false;
            }
        }
        return param;
    }*/
    #endregion

    #region private methods
    
    private void CreateRoom()
    {
        string roomName = string.Empty;
        roomName = (roomName.Equals(string.Empty)) ? "Room " + Random.Range(100, 1000) : roomName;
       
        RoomOptions options = new RoomOptions {MaxPlayers = maxPlayersInRoom, PlayerTtl = 10000 };

        PhotonNetwork.CreateRoom(roomName, options, null);
        
   //     PhotonNetwork.CreateRoom(roomName, options, null, new []{})
    }

    private void JoinRandomRoom()
    {
        PhotonNetwork.JoinRandomRoom();
    }
    private void UpdateCachedRoomList(List<RoomInfo> roomList)
    {
        foreach (RoomInfo info in roomList)
        {
            // Remove room from cached room list if it got closed, became invisible or was marked as removed
            if (!info.IsOpen || !info.IsVisible || info.RemovedFromList)
            {
                if (cachedRoomList.ContainsKey(info.Name))
                {
                    cachedRoomList.Remove(info.Name);
                }

                continue;
            }

            // Update cached room info
            if (cachedRoomList.ContainsKey(info.Name))
            {
                cachedRoomList[info.Name] = info;
            }
            // Add new room info to cache
            else
            {
                cachedRoomList.Add(info.Name, info);
            }
        }
    }
    private void UpdateRoomListView()
    {
        foreach (RoomInfo info in cachedRoomList.Values)
        {
            GameObject entry = Instantiate(roomListPrefab);
            entry.transform.SetParent(roomListContentView.transform);
            entry.transform.localScale = Vector3.one;
            entry.GetComponent<RoomListEntry>().Initialize(info.Name, (byte)info.PlayerCount, info.MaxPlayers);

            roomListEntries.Add(info.Name, entry);
        }
    }
    private void ClearRoomListView()
    {
        foreach (GameObject entry in roomListEntries.Values)
        {
            Destroy(entry.gameObject);
        }

        roomListEntries.Clear();
    }
    
    #endregion
}
