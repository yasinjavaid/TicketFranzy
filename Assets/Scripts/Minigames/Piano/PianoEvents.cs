using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PianoEvents : MonoBehaviour
{
    public static Action TileStart;
    public static Action TileStop;
    public static Action SpawnTiles;
    public static Action<string> playPianoButtonSound;
    public delegate PianoTile CurrentTile();
    public static event CurrentTile GetCurrentTile;

    public delegate List<int> GetTilesOrder();

    public static event GetTilesOrder GetTilesOrderLocalPlayer;


    public void SpawnTileAction()
    {
        SpawnTiles?.Invoke();
    }
    public void TileStartAction()
    {
        TileStart?.Invoke();
    }
    public void TileStopAction()
    {
        TileStop?.Invoke();
    }

    public void PlayPianoSound(string sound)
    {
        playPianoButtonSound?.Invoke(sound);
    }
    
    public PianoTile GetPianoTileDelegate()
    {
        var tile = GetCurrentTile?.Invoke();
        return tile;
    }

    public List<int> OnGetTilesOrderLocalPlayer()
    {
        return GetTilesOrderLocalPlayer?.Invoke();
    }
}
