using System.Linq;
using Realms;
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

    private Realm realm = default;
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
        realm.Write(() =>
        {
            realm.Remove(pieceEntity);
        });
    }

    private void OnMouseDown()
    {
        movementManager.SetActivePiece(this);
    }

    private void Awake()
    {
        realm = Realm.GetInstance();
        pieceEntity = realm.All<PieceEntity>().FirstOrDefault(piece =>
            piece.PositionX == transform.position.x &&
            piece.PositionY == transform.position.y &&
            piece.PositionZ == transform.position.z &&
            piece.Type == (int)type);
        if (pieceEntity == null)
        {
            realm.Write(() =>
            {
                pieceEntity = new PieceEntity(type, transform.position);
                realm.Add(pieceEntity);
            });
        }
    }

    private void Start()
    {
        movementManager = GameObject.FindObjectOfType<MovementManager>();
    }

    private void Update()
    {
        if (transform.hasChanged)
        {
            realm.Write(() =>
            {
                pieceEntity.PositionX = transform.position.x;
                pieceEntity.PositionY = transform.position.y;
                pieceEntity.PositionZ = transform.position.z;
            });
            transform.hasChanged = false;
        }
    }
}
