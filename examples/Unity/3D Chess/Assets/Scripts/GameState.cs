using System.Linq;
using Realms;
using UnityEngine;

public class GameState : MonoBehaviour
{
    [SerializeField] private PieceSpawner pieceSpawner = default;
    [SerializeField] private PieceMovement pieceMovement = default;
    [SerializeField] private GameObject piecesParent = default;

    private Realm realm = default;
    private IQueryable<PieceEntity> pieces = default;

    public void UpdatePiecePosition(Piece movedPiece, Vector3 newPosition)
    {
        foreach (Piece piece in piecesParent.GetComponentsInChildren<Piece>())
        {
            if (piece.transform.position == newPosition)
            {
                Destroy(piece);
                break;
            }
        }

        var oldPosition = movedPiece.transform.position;
        movedPiece.transform.position = newPosition;

        var pieceEntity = pieces.FirstOrDefault(piece =>
                            piece.PositionX == oldPosition.x &&
                            piece.PositionY == oldPosition.y &&
                            piece.PositionZ == oldPosition.z);
        realm.Write(() =>
        {
            pieceEntity.SetPosition(newPosition);
            Debug.Log("Position updated.");
        });
    }

    public async void ResetGame()
    {
        foreach (Piece piece in piecesParent.GetComponentsInChildren<Piece>())
        {
            Destroy(piece.gameObject);
        }
        realm.Write(() =>
        {
            realm.RemoveAll<PieceEntity>();
        });

        await Persistence.ResetDatabase();
        pieceSpawner.SetUpInitialBoard(piecesParent, pieceMovement);
    }

    private async void Awake()
    {
        await Persistence.CreateSyncConfiguration();
        //realm = await Realm.GetInstanceAsync(Persistence.SyncConfiguration);
        realm = Realm.GetInstance();

        pieces = realm.All<PieceEntity>();
        if (pieces.Count() > 0)
        {
            foreach (PieceEntity pieceEntity in pieces)
            {
                PieceType type = (PieceType)pieceEntity.Type;
                Vector3 position = pieceEntity.GetPosition();
                realm.Write(() =>
                {
                    var pieceEntity = new PieceEntity(type, position);
                    realm.Add(pieceEntity);
                });
                pieceSpawner.SpawnPiece(type, position, piecesParent, pieceMovement);
            }
        }
        else
        {
            await Persistence.ResetDatabase();
            pieces = realm.All<PieceEntity>();
            pieceSpawner.SetUpInitialBoard(piecesParent, pieceMovement);
        }
    }

}
