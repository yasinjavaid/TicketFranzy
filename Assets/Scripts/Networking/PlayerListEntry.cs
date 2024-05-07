
using UnityEngine;

using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Realtime;
using Photon.Pun.UtilityScripts;
using UnityEngine.UI;

public class PlayerListEntry : MonoBehaviour
{
    #region  public variables

    [Header("UI References")]
    public Text PlayerNameText;
    
    public Text ReadyForGameText;

    #endregion

    #region private variables
    private int ownerId;
    private bool isPlayerReady;
    
    

    #endregion

    #region monobehaviour callbacks

    private void Start()
    {
        
    }

    #endregion
    
    #region public methods

    public void Initialize(int playerId, string playerName)
    {
        ownerId = playerId;
        PlayerNameText.text = playerName;
        if (NetworkManager.Instance.LocalPlayer.ActorNumber == ownerId)
        {
            Hashtable initialProps = new Hashtable() {{NetworkManager.READYFORGAME, ENUMARCADEGAMES.None }};
            NetworkManager.Instance.LocalPlayer.SetCustomProperties(initialProps);
        }
    }

    public void OnPlayerReadyGameUpdate(ENUMARCADEGAMES enumArcadeGames)
    {
        ReadyForGameText.text = "Ready for > " + enumArcadeGames.ToString();
    }


    #endregion
}
