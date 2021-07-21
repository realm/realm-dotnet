using System.Linq;
using Realms;
using UnityEngine;

public class GameState : MonoBehaviour
{
    [SerializeField] private SpawnManager spawnManager = default;
    [SerializeField] private GameObject piecesParent = default;

    public void UpdatePiecePosition(Piece movedPiece, Vector3 endPosition)
    {
        for (int i = 0; i < piecesParent.transform.childCount; i++)
        {
            var piece = piecesParent.transform.GetChild(i).gameObject.GetComponent<Piece>();
            if (piece.transform.position == endPosition)
            {
                Destroy(piece);
                break;
            }
        }

        movedPiece.MoveTo(endPosition);
    }

    public void ResetGame()
    {
        for (int i = 0; i < piecesParent.transform.childCount; i++)
        {
            var piece = piecesParent.transform.GetChild(i).gameObject.GetComponent<Piece>();
            if (piece != null)
            {
                Destroy(piece.gameObject);
            }
        }
        SetUpInitialBoard();
    }

    private async void Awake()
    {
        await SyncedRealmConfiguration.CreateSyncConfiguration();
        var realm = await Realm.GetInstanceAsync(SyncedRealmConfiguration.SyncConfiguration);

        if (realm == null)
        {
            Debug.LogError("realm must not be null");
        }

        IQueryable<PieceEntity> pieceEntities = realm.All<PieceEntity>();
        if (pieceEntities.Count() > 0)
        {
            foreach (PieceEntity pieceEntity in pieceEntities)
            {
                PieceType type = (PieceType)pieceEntity.Type;
                Vector3 position = pieceEntity.GetPosition();
                spawnManager.SpawnPiece(type, piecesParent, position);
            }
        }
        else
        {
            SetUpInitialBoard();
        }
    }

    private void SetUpInitialBoard()
    {
        spawnManager.SpawnPiece(PieceType.WhiteRook, piecesParent, new Vector3(1, 0.5f, 1));
        spawnManager.SpawnPiece(PieceType.WhiteKnight, piecesParent, new Vector3(2, 0.5f, 1));
        spawnManager.SpawnPiece(PieceType.WhiteBishop, piecesParent, new Vector3(3, 0.5f, 1));
        spawnManager.SpawnPiece(PieceType.WhiteQueen, piecesParent, new Vector3(4, 0.5f, 1));
        spawnManager.SpawnPiece(PieceType.WhiteKing, piecesParent, new Vector3(5, 0.5f, 1));
        spawnManager.SpawnPiece(PieceType.WhiteBishop, piecesParent, new Vector3(6, 0.5f, 1));
        spawnManager.SpawnPiece(PieceType.WhiteKnight, piecesParent, new Vector3(7, 0.5f, 1));
        spawnManager.SpawnPiece(PieceType.WhiteRook, piecesParent, new Vector3(8, 0.5f, 1));

        spawnManager.SpawnPiece(PieceType.WhitePawn, piecesParent, new Vector3(1, 0.5f, 2));
        spawnManager.SpawnPiece(PieceType.WhitePawn, piecesParent, new Vector3(2, 0.5f, 2));
        spawnManager.SpawnPiece(PieceType.WhitePawn, piecesParent, new Vector3(3, 0.5f, 2));
        spawnManager.SpawnPiece(PieceType.WhitePawn, piecesParent, new Vector3(4, 0.5f, 2));
        spawnManager.SpawnPiece(PieceType.WhitePawn, piecesParent, new Vector3(5, 0.5f, 2));
        spawnManager.SpawnPiece(PieceType.WhitePawn, piecesParent, new Vector3(6, 0.5f, 2));
        spawnManager.SpawnPiece(PieceType.WhitePawn, piecesParent, new Vector3(7, 0.5f, 2));
        spawnManager.SpawnPiece(PieceType.WhitePawn, piecesParent, new Vector3(8, 0.5f, 2));

        spawnManager.SpawnPiece(PieceType.BlackPawn, piecesParent, new Vector3(1, 0.5f, 7));
        spawnManager.SpawnPiece(PieceType.BlackPawn, piecesParent, new Vector3(2, 0.5f, 7));
        spawnManager.SpawnPiece(PieceType.BlackPawn, piecesParent, new Vector3(3, 0.5f, 7));
        spawnManager.SpawnPiece(PieceType.BlackPawn, piecesParent, new Vector3(4, 0.5f, 7));
        spawnManager.SpawnPiece(PieceType.BlackPawn, piecesParent, new Vector3(5, 0.5f, 7));
        spawnManager.SpawnPiece(PieceType.BlackPawn, piecesParent, new Vector3(6, 0.5f, 7));
        spawnManager.SpawnPiece(PieceType.BlackPawn, piecesParent, new Vector3(7, 0.5f, 7));
        spawnManager.SpawnPiece(PieceType.BlackPawn, piecesParent, new Vector3(8, 0.5f, 7));

        spawnManager.SpawnPiece(PieceType.BlackRook, piecesParent, new Vector3(1, 0.5f, 8));
        spawnManager.SpawnPiece(PieceType.BlackKnight, piecesParent, new Vector3(2, 0.5f, 8));
        spawnManager.SpawnPiece(PieceType.BlackBishop, piecesParent, new Vector3(3, 0.5f, 8));
        spawnManager.SpawnPiece(PieceType.BlackQueen, piecesParent, new Vector3(4, 0.5f, 8));
        spawnManager.SpawnPiece(PieceType.BlackKing, piecesParent, new Vector3(5, 0.5f, 8));
        spawnManager.SpawnPiece(PieceType.BlackBishop, piecesParent, new Vector3(6, 0.5f, 8));
        spawnManager.SpawnPiece(PieceType.BlackKnight, piecesParent, new Vector3(7, 0.5f, 8));
        spawnManager.SpawnPiece(PieceType.BlackRook, piecesParent, new Vector3(8, 0.5f, 8));
    }
}
