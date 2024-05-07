using System;

using UnityEngine;

public enum AbsoluteDirectionType { None = 0b00, Cardinal = 0b01, Intercardinal = 0b10, Any = Cardinal | Intercardinal }

public enum AbsoluteDirectionEnum
{
    None = 0b0000, North = 0b1000, South = 0b0100, East = 0b0010, West = 0b0001,
    NorthEast = North | East, NorthWest = North | West,
    SouthEast = South | East, SouthWest = South | West
}

public enum CardinalDirectionEnum { None = AbsoluteDirectionEnum.None, North = AbsoluteDirectionEnum.North, South = AbsoluteDirectionEnum.South, East = AbsoluteDirectionEnum.East, West = AbsoluteDirectionEnum.West }

public enum IntercardinalDirectionEnum { None = AbsoluteDirectionEnum.None, NorthEast = AbsoluteDirectionEnum.NorthEast, NorthWest = AbsoluteDirectionEnum.NorthWest, SouthEast = AbsoluteDirectionEnum.SouthEast, SouthWest = AbsoluteDirectionEnum.SouthWest }

[Serializable]
public struct AbsoluteDirection : IEquatable<AbsoluteDirection>
{
    [SerializeField] private Vector2Int vector;
    public Vector2Int GetVector { get => vector; set => vector = Validate(value.x, value.y); }

    public bool IsCardinal => vector.x != 0 ^ vector.y != 0;
    public bool IsIntercardinal => vector.x != 0 && vector.y != 0;

    public AbsoluteDirection(int x, int y, AbsoluteDirectionType forceType = AbsoluteDirectionType.Any) => vector = Validate(x, y, forceType);
    public AbsoluteDirection(float x, float y, AbsoluteDirectionType forceType = AbsoluteDirectionType.Any) => vector = Validate(x, y, forceType);
    public AbsoluteDirection(Vector2Int v, AbsoluteDirectionType forceType = AbsoluteDirectionType.Any) => vector = Validate(v.x, v.y, forceType);
    public AbsoluteDirection(Vector2 v, AbsoluteDirectionType forceType = AbsoluteDirectionType.Any) => vector = Validate(v.x, v.y, forceType);
    public AbsoluteDirection(string s) => vector = ToVector(s);
    public AbsoluteDirection(AbsoluteDirectionEnum directionEnum) => vector = ToVector(directionEnum);
    public AbsoluteDirection(CardinalDirectionEnum directionEnum) => vector = ToVector(directionEnum);
    public AbsoluteDirection(IntercardinalDirectionEnum directionEnum) => vector = ToVector(directionEnum);

    public AbsoluteDirection RotateClockwise() => new AbsoluteDirection(vector.y, -vector.x);
    public AbsoluteDirection RotateCounterClockwise() => new AbsoluteDirection(-vector.y, vector.x);

    public AbsoluteDirection RotateRadians(float rad) => vector == default ? this : new AbsoluteDirection((vector.VectorToRad() + rad).RadToVector());
    public AbsoluteDirection RotateDegrees(float deg) => vector == default ? this : RotateRadians(deg * Mathf.Deg2Rad);

    public AbsoluteDirection FlipX() => new AbsoluteDirection(-vector.x, vector.y);
    public AbsoluteDirection FlipY() => new AbsoluteDirection(vector.x, -vector.y);
    public AbsoluteDirection FlipXY() => new AbsoluteDirection(-vector.x, -vector.y);

    public static AbsoluteDirection FromAngleDeg(float angleDeg) => new AbsoluteDirection(angleDeg.DegToVector());
    public static AbsoluteDirection FromAngleRad(float angleRad) => new AbsoluteDirection(angleRad.RadToVector());

    public float GetAngleDeg() => vector.GetAngleDeg();
    public float GetAngleRad() => vector.GetAngleRad();

    private static Vector2Int Validate(float x, float y, AbsoluteDirectionType forceType) => forceType switch
    {
        AbsoluteDirectionType.None => new Vector2Int(0, 0),
        AbsoluteDirectionType.Cardinal => ValidateToCardinal(x, y),
        AbsoluteDirectionType.Intercardinal => ValidateToIntercardinal(x, y),
        AbsoluteDirectionType.Any => Validate(x, y),
        _ => Validate(x, y),
    };
    private static Vector2Int Validate(float x, float y) => Validate(x, y, Mathf.PI / 4, 0);
    private static Vector2Int ValidateToCardinal(float x, float y) => Validate(x, y, Mathf.PI / 2, 0);
    private static Vector2Int ValidateToIntercardinal(float x, float y) => Validate(x, y, Mathf.PI / 2, Mathf.PI / 4);
    private static Vector2Int Validate(float x, float y, float step, float offset) => x == 0 && y == 0 ? Vector2Int.zero : InStepsOf(Mathf.Atan2(y, x), step, offset, true).RadToVector().RoundToInt();

    private static float InStepsOf(float value, float step, float offset, bool rounded = false) => rounded
            ? ((float)Math.Round((value + offset) / step) * step) - offset
            : ((float)Math.Floor((value + offset) / step) * step) - offset;

    public static Vector2Int ToVector(AbsoluteDirectionEnum e) => e switch
    {
        AbsoluteDirectionEnum.North => new Vector2Int(0, 1),
        AbsoluteDirectionEnum.NorthEast => new Vector2Int(1, 1),
        AbsoluteDirectionEnum.East => new Vector2Int(1, 0),
        AbsoluteDirectionEnum.SouthEast => new Vector2Int(1, -1),
        AbsoluteDirectionEnum.South => new Vector2Int(0, -1),
        AbsoluteDirectionEnum.SouthWest => new Vector2Int(-1, -1),
        AbsoluteDirectionEnum.West => new Vector2Int(-1, 0),
        AbsoluteDirectionEnum.NorthWest => new Vector2Int(-1, 1),
        _ => new Vector2Int(0, 0),
    };

    public static Vector2Int ToVector(CardinalDirectionEnum e) => e switch
    {
        CardinalDirectionEnum.North => new Vector2Int(0, 1),
        CardinalDirectionEnum.East => new Vector2Int(1, 0),
        CardinalDirectionEnum.South => new Vector2Int(0, -1),
        CardinalDirectionEnum.West => new Vector2Int(-1, 0),
        _ => new Vector2Int(0, 0),
    };

    public static Vector2Int ToVector(IntercardinalDirectionEnum e) => e switch
    {
        IntercardinalDirectionEnum.NorthEast => new Vector2Int(1, 1),
        IntercardinalDirectionEnum.SouthEast => new Vector2Int(1, -1),
        IntercardinalDirectionEnum.SouthWest => new Vector2Int(-1, -1),
        IntercardinalDirectionEnum.NorthWest => new Vector2Int(-1, 1),
        _ => new Vector2Int(0, 0),
    };

    public static Vector2Int ToVector(string s) => s switch
    {
        "N" => new Vector2Int(0, 1),
        "NE" => new Vector2Int(1, 1),
        "E" => new Vector2Int(1, 0),
        "SE" => new Vector2Int(1, -1),
        "S" => new Vector2Int(0, -1),
        "SW" => new Vector2Int(-1, -1),
        "W" => new Vector2Int(-1, 0),
        "NW" => new Vector2Int(-1, 1),
        _ => new Vector2Int(0, 0),
    };

    public override string ToString() => (vector == default) ? "C" :
        (vector.y > 0 ? "N" : vector.y < 0 ? "S" : "") +
        (vector.x > 0 ? "E" : vector.x < 0 ? "W" : "");


    public static Vector2Int NorthVector => new Vector2Int(0, 1);
    public static Vector2Int NorthEastVector => new Vector2Int(1, 1);
    public static Vector2Int NorthWestVector => new Vector2Int(-1, 1);
    public static Vector2Int EastVector => new Vector2Int(1, 0);
    public static Vector2Int WestVector => new Vector2Int(-1, 0);
    public static Vector2Int SouthVector => new Vector2Int(0, -1);
    public static Vector2Int SouthEastVector => new Vector2Int(1, -1);
    public static Vector2Int SouthWestVector => new Vector2Int(-1, -1);
    public static Vector2Int CenterVector => new Vector2Int(0, 0);

    public static AbsoluteDirection North => new AbsoluteDirection() { vector = NorthVector };
    public static AbsoluteDirection NorthEast => new AbsoluteDirection() { vector = NorthEastVector };
    public static AbsoluteDirection NorthWest => new AbsoluteDirection() { vector = NorthWestVector };
    public static AbsoluteDirection East => new AbsoluteDirection() { vector = EastVector };
    public static AbsoluteDirection West => new AbsoluteDirection() { vector = WestVector };
    public static AbsoluteDirection South => new AbsoluteDirection() { vector = SouthVector };
    public static AbsoluteDirection SouthEast => new AbsoluteDirection() { vector = SouthEastVector };
    public static AbsoluteDirection SouthWest => new AbsoluteDirection() { vector = SouthWestVector };
    public static AbsoluteDirection Center => new AbsoluteDirection() { vector = CenterVector };

    public override bool Equals(object obj) => obj is AbsoluteDirection direction && Equals(direction);
    public bool Equals(AbsoluteDirection other) => vector.Equals(other.vector);
    public override int GetHashCode() => 432591442 + vector.GetHashCode();

    public static bool operator ==(AbsoluteDirection left, AbsoluteDirection right) => left.Equals(right);
    public static bool operator !=(AbsoluteDirection left, AbsoluteDirection right) => !(left == right);


    public static implicit operator AbsoluteDirection(AbsoluteDirectionEnum directionEnum) => new AbsoluteDirection(directionEnum);

    public static implicit operator AbsoluteDirectionEnum(AbsoluteDirection direction) => direction.GetVector.x switch
    {
        -1 => direction.GetVector.y switch
        {
            -1 => AbsoluteDirectionEnum.SouthWest,
            0 => AbsoluteDirectionEnum.West,
            1 => AbsoluteDirectionEnum.NorthWest,
            _ => AbsoluteDirectionEnum.None,//Vector is in invalid state
        },
        0 => direction.GetVector.y switch
        {
            -1 => AbsoluteDirectionEnum.South,
            0 => AbsoluteDirectionEnum.None,
            1 => AbsoluteDirectionEnum.North,
            _ => AbsoluteDirectionEnum.None,//Vector is in invalid state
        },
        1 => direction.GetVector.y switch
        {
            -1 => AbsoluteDirectionEnum.SouthEast,
            0 => AbsoluteDirectionEnum.East,
            1 => AbsoluteDirectionEnum.NorthEast,
            _ => AbsoluteDirectionEnum.None,//Vector is in invalid state
        },
        _ => AbsoluteDirectionEnum.None,//Vector is in invalid state
    };


    public static implicit operator AbsoluteDirection(CardinalDirectionEnum directionEnum) => new AbsoluteDirection(directionEnum);

    public static explicit operator CardinalDirectionEnum(AbsoluteDirection direction)
        => direction.vector == NorthVector ? CardinalDirectionEnum.North
        : direction.vector == SouthVector ? CardinalDirectionEnum.South
        : direction.vector == EastVector ? CardinalDirectionEnum.East
        : direction.vector == WestVector ? CardinalDirectionEnum.West
        : default;


    public static implicit operator AbsoluteDirection(IntercardinalDirectionEnum directionEnum) => new AbsoluteDirection(directionEnum);

    public static explicit operator IntercardinalDirectionEnum(AbsoluteDirection direction)
        => direction.vector == NorthEastVector ? IntercardinalDirectionEnum.NorthEast
        : direction.vector == NorthWestVector ? IntercardinalDirectionEnum.NorthWest
        : direction.vector == SouthEastVector ? IntercardinalDirectionEnum.SouthEast
        : direction.vector == SouthWestVector ? IntercardinalDirectionEnum.SouthWest
        : default;


    public static explicit operator Vector2Int(AbsoluteDirection direction) => direction.GetVector;

    public static explicit operator Vector2(AbsoluteDirection direction) => direction.GetVector;

    public bool TryGetCardinalEnum(out CardinalDirectionEnum isoEnum) => (isoEnum = (CardinalDirectionEnum)this) != default;

    public bool TryGetIntercardinalEnum(out IntercardinalDirectionEnum isoEnum) => (isoEnum = (IntercardinalDirectionEnum)this) != default;
}
