using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.Profiling;
using UnityEngine.UI;

public class PianoTile : MonoBehaviour
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
    //[HideInInspector]
    public int columnNo = -1;

    public int id = -10;
    [HideInInspector]
    public PianoTile previousNode;

    [HideInInspector] 
    public bool isUsed = false;
    
    [HideInInspector]
    public string soundId = "c4";
    public void CustomOnEnable()
    {
        PianoEvents.TileStart += TileStart;
        PianoEvents.TileStop += TileStop;
    }

    private void OnDisable()
    {
        if (PianoEvents.TileStart != null) PianoEvents.TileStart -= TileStart;
        if (PianoEvents.TileStop != null) PianoEvents.TileStop -= TileStop;
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
        Profiler.BeginSample("_PianoDropCode");
        if (previousNode)
        {
            var localTileMove = previousNode.transform.localPosition;
            localTileMove.x = transform.localPosition.x;
            transform.DOLocalMove(localTileMove, 0.15f);
        }
        Profiler.EndSample();
    }
}
