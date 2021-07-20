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
    public PieceEntity pieceEntity { get; private set; }

    private Realm realm = default;
    private MovementManager movementManager = default;

    public void Select()
    {
        gameObject.GetComponent<Renderer>().material.color = new Color(1, 0, 0, 1);
    }

    public void Deselect()
    {
        gameObject.GetComponent<Renderer>().material.color = new Color(1, 1, 1, 1);
    }

    private void OnMouseDown()
    {
        if (movementManager == null)
        {
            Debug.LogError("movementManager must not be null");
        }
        movementManager.SetActivePiece(this);
    }

    private async void Awake()
    {
        movementManager = GameObject.FindObjectOfType<MovementManager>();

        realm = await Realm.GetInstanceAsync(SyncedRealmConfiguration.SyncConfiguration);
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
        pieceEntity.PropertyChanged += PropertyChanged;
    }

    private void Update()
    {
        if (transform.hasChanged)
        {
            if (realm != null)
            {
                realm.Write(() =>
                {
                    pieceEntity.PositionX = transform.position.x;
                    pieceEntity.PositionY = transform.position.y;
                    pieceEntity.PositionZ = transform.position.z;
                });
            }
            transform.hasChanged = false;
        }
    }

    private void OnDestroy()
    {
        pieceEntity.PropertyChanged -= PropertyChanged;
    }

    private void PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        if (pieceEntity == null)
        {
            Debug.LogError("pieceEntity must not be null");
        }
        //var newPosition = new Vector3(pieceEntity.PositionX, pieceEntity.PositionY, pieceEntity.PositionZ);
        //transform.position = newPosition;
    }
}
