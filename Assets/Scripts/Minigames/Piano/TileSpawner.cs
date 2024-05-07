using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using MetaData;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UI.Extensions.ColorPicker;
using Random = UnityEngine.Random;

public class TileSpawner : MonoBehaviour
{
    [SerializeField] private MetadataManager metadataManager;
    [SerializeField] private GameObject tile, whiteTile;
    [SerializeField] private RectTransform tilesContainer;
    [SerializeField] private Transform tileSpawnerPoint;

    [SerializeField] private float tileNextColumnDisplacement = 0.16f;

    [SerializeField] private float tileNextRowDisplacement = 0.307f;

    private float tileNextColumnDisplacementDepth = 0f;

    [SerializeField] private float tilesDroppingSpeed = 2;

    [SerializeField] private PianoTile dumpTile;
    public Queue<PianoTile> tiles = new Queue<PianoTile>();

    private List<int> tilesSpawnPointInRow = new List<int>();
    
    private PianoTile oldOne;
    // Start is called before the first frame update

    private int countForTickets = 0;
    private int[] ticketsList = new int[4] {0, 1, 0, 2};
        private void OnEnable()
    {
        PianoEvents.SpawnTiles += Spawner;
        PianoEvents.GetCurrentTile += GetCurrentTileSuccess;
        PianoEvents.GetTilesOrderLocalPlayer += PianoEventsOnGetTilesOrderLocalPlayer;
    }

   

    private void OnDisable()
    {
        PianoEvents.SpawnTiles -= Spawner;
        PianoEvents.GetCurrentTile -= GetCurrentTileSuccess;
        PianoEvents.GetTilesOrderLocalPlayer -= PianoEventsOnGetTilesOrderLocalPlayer;
    }

    private void Spawner()
    {
        for (int i = 0; i < metadataManager.GetAuldlandSong().Count; i++)
        {
            
            var colomn = Random.Range(0, 4);
            tilesSpawnPointInRow.Add(colomn);
           
            var tileP = Instantiate(this.tile, Vector3.zero, quaternion.identity);
            var spawnedTileP = tileP.GetComponent<PianoTile>();
            CreateTile(spawnedTileP, i, colomn,colomn,  false);
            for (int j = 0; j < 4; j++)
            {
                if (j == colomn)
                {
                  continue;
                }
                else
                {
                    var tileW = Instantiate(this.whiteTile, Vector3.zero, quaternion.identity);
                    var spawnedTileW = tileW.GetComponent<PianoTile>();
                    CreateTile(spawnedTileW, i, j,  colomn,  true);
                }
            }
            oldOne = spawnedTileP;
        }
    }

    private void CreateTile(PianoTile pianoTile, int iterationA, int iterationB, int col,  bool wrongTile)
    {
        var localPosition = tileSpawnerPoint.localPosition;
        var tilePos = new Vector3(
            localPosition.x + (iterationB * tileNextColumnDisplacement),
            localPosition.y + (iterationA * tileNextRowDisplacement),
            0
        );
        pianoTile.Transform.parent = tilesContainer.transform;
        pianoTile.rectTransform.localPosition = tilePos;
        pianoTile.rectTransform.localRotation = Quaternion.Euler(Vector3.zero);
        pianoTile.CustomOnEnable();
        if (!wrongTile)
        {
            pianoTile.ticketsCount = ticketsList[countForTickets];
            ++countForTickets;
            if (countForTickets == ticketsList.Length)
            {
                countForTickets = 0;
            }
            pianoTile.EnableTickets();
            pianoTile.startText.SetActive(iterationA == 0);
            pianoTile.columnNo = col;
            pianoTile.id = iterationA;
            pianoTile.soundId = metadataManager.GetAuldlandSong()[iterationA];
            pianoTile.previousNode = tiles.Count != 0 ? oldOne : dumpTile;
            pianoTile.playerActorNumber = -1;
            
            tiles.Enqueue(pianoTile);
        }
        else
        {
            pianoTile.previousNode = tiles.Count != 0 ? iterationA == 0 ? dumpTile : oldOne : dumpTile;
        }
    }
    private PianoTile GetCurrentTileSuccess()
    {
        if (tiles.Any())
        {
            return tiles.Dequeue();
        }
        return null;
    }
    private List<int> PianoEventsOnGetTilesOrderLocalPlayer()
    {
        return tilesSpawnPointInRow;
    }
}