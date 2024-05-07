using System.Collections;
using System.Collections.Generic;
using System.Linq;
using MetaData;
using Unity.Mathematics;
using UnityEngine;
using Random = UnityEngine.Random;

public class TileSpawnerNetwork : MonoBehaviour
{
    [SerializeField] private MetadataManager metadataManager;
    [SerializeField] private GameObject tile, whiteTile;
    [SerializeField] private RectTransform tilesContainer;
    [SerializeField] private Transform tileSpawnerPoint;

    [SerializeField] private float tileNextColumnDisplacement = 0.16f;

    [SerializeField] private float tileNextRowDisplacement = 0.307f;


    [SerializeField] private float tilesDroppingSpeed = 2;

    [SerializeField] private PianoTileNetwork dumpTile;
    public Queue<PianoTileNetwork> tiles = new Queue<PianoTileNetwork>();

    private List<int> tilesSpawnPointInRow = new List<int>();
    
    private PianoTileNetwork oldOne;
    
    private int countForTickets = 0;
    private int[] ticketsList = new int[4] {0, 1, 0, 2};

    [HideInInspector]
    public int actorNo;
    // Start is called before the first frame update

    private void OnEnable()
    {
    
    }

   

    private void OnDisable()
    {

    }

    public void Spawner(List<int> tileNoInRow)
    {


        List<string> soundsId = metadataManager.GetAuldlandSong();
        for (int i = 0; i < soundsId.Count; i++)
        {

            var colomn = tileNoInRow[i];
            var tileP = Instantiate(this.tile, Vector3.zero, quaternion.identity);
            var spawnedTileP = tileP.GetComponent<PianoTileNetwork>();
            CreateTile(spawnedTileP, i, colomn, colomn, false);
            for (int j = 0; j < 4; j++)
            {
                if (j == colomn)
                {
                    continue;
                }
                else
                {
                    var tileW = Instantiate(this.whiteTile, Vector3.zero, quaternion.identity);
                    var spawnedTileW = tileW.GetComponent<PianoTileNetwork>();
                    CreateTile(spawnedTileW, i, j, colomn, true);
                }
            }

            oldOne = spawnedTileP;
        }

        /*List<string> soundsId = metadataManager.GetAuldlandSong();
        for (int i = 0; i < soundsId.Count; i++)
        {
            var colomn = tileNoInRow[i];
            var tilePos = new Vector3(
                firstTileSpawnPoint.x + (colomn * tileNextColumnDisplacement),
                firstTileSpawnPoint.y + (i * tileNextRowDisplacement),
                firstTileSpawnPoint.z + (i * tileNextColumnDisplacementDepth)
            );
            
            var tile = Instantiate(this.tile, Vector3.zero, quaternion.identity);
            tile.transform.parent = tilesContainer.transform;
            tile.transform.rotation = Quaternion.Euler(new Vector3(8.5f,0,0));
            tile.transform.localPosition = tilePos;
            var spawnedTile = tile.GetComponent<PianoTileNetwork>();
            spawnedTile.columnNo = colomn;
            spawnedTile.soundId = soundsId[i];
            spawnedTile.previousNode = tiles.Count != 0 ? oldOne : dumpTile;
            spawnedTile.playerActorNumber = actorNo;
            spawnedTile.CustomOnEnable();
            tiles.Enqueue(spawnedTile);
            oldOne = spawnedTile;
        }*/
    }
    
    
    private void CreateTile(PianoTileNetwork pianoTile, int iterationA, int iterationB, int col,  bool wrongTile)
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
    
    
    
    
    
    
    
    
    
    
    
    
    
    
    
    
    
    

    /*private PianoTileNetwork GetCurrentTile()
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
    }*/
}
