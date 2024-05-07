using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class PianoTileNetwork : MonoBehaviour
{
    
    private bool isDropping = false;
    
    private float dropeValue = Single.NaN;

    public int ticketsCount { get; set; }
    
    public GameObject startText;
    
    public Image[] tickets;
    
    public Transform Transform => transform;

    public RectTransform rectTransform;
 
    [HideInInspector]
    public int playerActorNumber = -1;
    [HideInInspector]
    public int columnNo = -1;
   
    public int id = -10;
    
    [HideInInspector]
    public PianoTileNetwork previousNode;
    
    [HideInInspector] 
    public bool isUsed = false;
    
    [HideInInspector]
    public string soundId = "c4";
    public void CustomOnEnable()
    {
            GhostPianoPlayer.NetworkTileStart += TileStart;
    }

    private void OnDisable()
    {
        if(GhostPianoPlayer.NetworkTileStart != null)GhostPianoPlayer.NetworkTileStart -= TileStart;
    }

    public void EnableTickets()
    {
        for (int i = 0; i < ticketsCount ; i++)
        {
            if (ticketsCount == 1) tickets[0].transform.localPosition = Vector3.zero;
            tickets[i].gameObject.SetActive(true);
        }
    }
    private void TileStop()
    {
        isDropping = false;
    }

    private void TileStart()
    {
        if (previousNode)
        {
            var localTileMove = previousNode.transform.localPosition;
            localTileMove.x = transform.localPosition.x;
            transform.DOLocalMove(localTileMove, 0.15f);
        }
    }
}
