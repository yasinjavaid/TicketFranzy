using System;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

namespace Networking
{
   public class LobbyManager : MonoBehaviour
   {
      #region serializeFields

   

      #endregion

      #region public fields

      public InputField playerNameInput;
      public GameObject connectMasterPanel;
      public GameObject joinGamePanel;
      public GameObject roomListPanel;
      public GameObject roomListContent;
      public GameObject roomEntryListPrefab;
      
      #endregion
   
      #region private fields

   

      #endregion

      #region monobehaviour callbacks

      private void Awake()
      {
         playerNameInput.text = "Player " + Random.Range(1000, 100000);
      }

      #endregion

      #region public methods

      public void OnLoginButtonClicked()
      {
         string playerName = playerNameInput.text;

         if (!playerName.Equals(""))
         {
            NetworkManager.Instance.ConnectToMaster(playerName, ShowRoomCreationPanel);
         }
         else
         {
            Debug.LogError("Player Name is invalid.");
         }
      }

      public void OnJoinGameClicked()
      {
         NetworkManager.Instance.OnJoinedRoomClicked();
      }

      public void ShowRoomListPanel()
      { 
         NetworkManager.Instance.JoinLobby();
         roomListPanel.SetActive(true);
      }

      public void HideRoomListPanel()
      {
         NetworkManager.Instance.LeaveLobby();
         roomListPanel.SetActive(false);
      }

      #endregion

      #region private methods

   
      private void ShowRoomCreationPanel()
      {
         connectMasterPanel.SetActive(false);
         joinGamePanel.SetActive(true);
         NetworkManager.Instance.SetRoomListContent(roomListContent, roomEntryListPrefab);
      }

      #endregion
   }
}
