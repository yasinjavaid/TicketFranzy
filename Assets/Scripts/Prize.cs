using UnityEngine;

public enum PrizeCategory { None, Test }

[CreateAssetMenu(fileName = "Prize", menuName = "Scriptable Objects/Prize")]
public class Prize : ScriptableObject
{
    public string Name => _name;
    [SerializeField] protected string _name;

    public PrizeCategory Category => _category;
    [SerializeField] protected PrizeCategory _category;

    public string Description => _description;
    [SerializeField] protected string _description;

    public int Tickets => _tickets;
    [SerializeField] protected int _tickets;

    public Sprite GetSprite => _sprite;
    [SerializeField] protected Sprite _sprite;

    public int OwnedAmount { get => GameSave.GetOwnedAmount(Name); set => GameSave.SetOwnedAmount(Name, value); }
}