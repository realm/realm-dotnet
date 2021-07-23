using Realms;
using UnityEngine;

public class PieceEntity : RealmObject
{
    public int Type { get; set; }

    public float PositionX { get; set; }
    public float PositionY { get; set; }
    public float PositionZ { get; set; }

    public PieceEntity()
    {

    }

    public PieceEntity(PieceType type, Vector3 position)
    {
        Type = (int)type;
        PositionX = position.x;
        PositionY = position.y;
        PositionZ = position.z;
    }

    public Vector3 GetPosition()
    {
        return new Vector3(PositionX, PositionY, PositionZ);
    }

    public void SetPosition(Vector3 position)
    {
        PositionX = position.x;
        PositionY = position.y;
        PositionZ = position.z;
    }
}
