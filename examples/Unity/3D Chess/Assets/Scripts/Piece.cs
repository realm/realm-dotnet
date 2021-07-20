using System.Linq;
using UnityEngine;

public class Piece : MonoBehaviour
{
    public enum Type : int
    {
        WhitePawn,
        WhiteBishop,
        WhiteKnight,
        WhiteRook,
        WhiteQueen,
        WhiteKing,
        BlackPawn,
        BlackBishop,
        BlackKnight,
        BlackRook,
        BlackQueen,
        BlackKing
    }

    public Type type = default;

    private PieceEntity pieceEntity = default;
    private MovementManager movementManager = default;

    public void Select()
    {
        gameObject.GetComponent<Renderer>().material.color = new Color(1, 0, 0, 1);
    }

    public void Deselect()
    {
        gameObject.GetComponent<Renderer>().material.color = new Color(1, 1, 1, 1);
    }

    public void Delete()
    {
        SyncedRealm.realm.Write(() =>
        {
            SyncedRealm.realm.Remove(pieceEntity);
        });
    }

    private void OnMouseDown()
    {
        movementManager.SetActivePiece(this);
    }

    private void Awake()
    {
        movementManager = GameObject.FindObjectOfType<MovementManager>();

        pieceEntity = SyncedRealm.realm.All<PieceEntity>().FirstOrDefault(piece =>
            piece.PositionX == transform.position.x &&
            piece.PositionY == transform.position.y &&
            piece.PositionZ == transform.position.z &&
            piece.Type == (int)type);
        if (pieceEntity == null)
        {
            SyncedRealm.realm.Write(() =>
            {
                pieceEntity = new PieceEntity(type, transform.position);
                SyncedRealm.realm.Add(pieceEntity);
            });
        }
        pieceEntity.PropertyChanged += PropertyChanged;
    }

    private void Update()
    {
        if (transform.hasChanged)
        {
            SyncedRealm.realm.Write(() =>
            {
                pieceEntity.PositionX = transform.position.x;
                pieceEntity.PositionY = transform.position.y;
                pieceEntity.PositionZ = transform.position.z;
            });
            transform.hasChanged = false;
        }
    }

    private void PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        var newPosition = new Vector3(pieceEntity.PositionX, pieceEntity.PositionY, pieceEntity.PositionZ);
        transform.position = newPosition;
    }
}
