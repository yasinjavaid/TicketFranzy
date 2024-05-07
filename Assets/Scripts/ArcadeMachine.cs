using UnityEngine;
using System.Collections.Generic;

public class ArcadeMachine : MonoBehaviour
{
    public static readonly Dictionary<string, ArcadeMachine> AllMachines = new Dictionary<string, ArcadeMachine>();
    public ENUMARCADEGAMES currentMachine;
    
    [SerializeField] Transform sittingPoint;
    [SerializeField, Range(0, 3)] private float waitingDistance = 1.5f;
    [SerializeField] private bool inUse = false;
    [SerializeField] private string gameSceneName = "";
    [SerializeField] private string _name;
    [SerializeField] private string _description;
    [SerializeField] private Sprite _spritePreview;
    [SerializeField] private int _price;
    
    
    public bool Spotted { get; set; }
    public bool InUse => inUse;
    public string UniqueID => uniqueID ??= gameObject.GetFullName();
    private string uniqueID;
    public Vector3 SittingPosition => sittingPoint.position;
    public Vector3 WaitingPosition => transform.position + (transform.forward * (sittingPoint.localPosition.z + waitingDistance));

    public string Name => _name;
    public string Description => _description;
    public Sprite SpritePreview => _spritePreview;
    public int Price => _price;

    public string GameSceneName => gameSceneName;

    private void Awake() => AllMachines.Add(UniqueID, this);

    private void OnDestroy() => AllMachines.Remove(UniqueID);

    public void UseMachine()
    {
        inUse = true;
        Spotted = false;
    }

    public void LeaveMachine() => inUse = false;

    private void OnDrawGizmosSelected()
    {
        Vector3 size = new Vector3(1, 1, 1);
        Vector3 pivot = new Vector3(0, size.y / 2, 0);
        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(SittingPosition + pivot, size);
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireCube(WaitingPosition + pivot, size);
    }
}