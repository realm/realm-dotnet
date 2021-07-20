using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class GameState : MonoBehaviour
{
    [SerializeField] private SpawnManager spawnManager = default;
    [SerializeField] private GameObject piecesParent = default;

    private List<Piece> pieces = new List<Piece>();

    public void UpdatePiecePosition(Vector3 startPosition, Vector3 endPosition)
    {
        Piece movedPiece = pieces.Find(piece => piece.transform.position.x == startPosition.x && piece.transform.position.z == startPosition.z);
        if (!movedPiece)
        {
            Debug.Break();
        }

        Piece attackedPiece = pieces.Find(piece => piece.transform.position.x == endPosition.x && piece.transform.position.z == endPosition.z);
        if (attackedPiece)
        {
            pieces.Remove(attackedPiece);
            attackedPiece.Delete();
            Destroy(attackedPiece.gameObject);
        }

        movedPiece.transform.position = new Vector3(endPosition.x, movedPiece.transform.position.y, endPosition.z);
    }

    public void ResetGame()
    {
        foreach (Piece piece in pieces)
        {
            piece.Delete();
        }
        pieces.Clear();
        foreach (Transform piece in piecesParent.transform)
        {
            Destroy(piece.gameObject);
        }
        SetUpInitialBoard();
    }

    private async void Awake()
    {
        await SyncedRealm.OpenRealm();

        IQueryable<PieceEntity> pieceEntities = SyncedRealm.realm.All<PieceEntity>();
        if (pieceEntities.Count() > 0)
        {
            foreach (PieceEntity pieceEntity in pieceEntities)
            {
                Piece.Type type = (Piece.Type)pieceEntity.Type;
                Vector3 position = pieceEntity.GetPosition();
                pieces.Add(spawnManager.SpawnPiece(type, piecesParent, position));
            }
        }
        else
        {
            SetUpInitialBoard();
        }
    }

    private void SetUpInitialBoard()
    {
        pieces.Add(spawnManager.SpawnPiece(Piece.Type.WhiteRook, piecesParent, new Vector3(1, 0.5f, 1)));
        pieces.Add(spawnManager.SpawnPiece(Piece.Type.WhiteKnight, piecesParent, new Vector3(2, 0.5f, 1)));
        pieces.Add(spawnManager.SpawnPiece(Piece.Type.WhiteBishop, piecesParent, new Vector3(3, 0.5f, 1)));
        pieces.Add(spawnManager.SpawnPiece(Piece.Type.WhiteQueen, piecesParent, new Vector3(4, 0.5f, 1)));
        pieces.Add(spawnManager.SpawnPiece(Piece.Type.WhiteKing, piecesParent, new Vector3(5, 0.5f, 1)));
        pieces.Add(spawnManager.SpawnPiece(Piece.Type.WhiteBishop, piecesParent, new Vector3(6, 0.5f, 1)));
        pieces.Add(spawnManager.SpawnPiece(Piece.Type.WhiteKnight, piecesParent, new Vector3(7, 0.5f, 1)));
        pieces.Add(spawnManager.SpawnPiece(Piece.Type.WhiteRook, piecesParent, new Vector3(8, 0.5f, 1)));

        pieces.Add(spawnManager.SpawnPiece(Piece.Type.WhitePawn, piecesParent, new Vector3(1, 0.5f, 2)));
        pieces.Add(spawnManager.SpawnPiece(Piece.Type.WhitePawn, piecesParent, new Vector3(2, 0.5f, 2)));
        pieces.Add(spawnManager.SpawnPiece(Piece.Type.WhitePawn, piecesParent, new Vector3(3, 0.5f, 2)));
        pieces.Add(spawnManager.SpawnPiece(Piece.Type.WhitePawn, piecesParent, new Vector3(4, 0.5f, 2)));
        pieces.Add(spawnManager.SpawnPiece(Piece.Type.WhitePawn, piecesParent, new Vector3(5, 0.5f, 2)));
        pieces.Add(spawnManager.SpawnPiece(Piece.Type.WhitePawn, piecesParent, new Vector3(6, 0.5f, 2)));
        pieces.Add(spawnManager.SpawnPiece(Piece.Type.WhitePawn, piecesParent, new Vector3(7, 0.5f, 2)));
        pieces.Add(spawnManager.SpawnPiece(Piece.Type.WhitePawn, piecesParent, new Vector3(8, 0.5f, 2)));

        pieces.Add(spawnManager.SpawnPiece(Piece.Type.BlackPawn, piecesParent, new Vector3(1, 0.5f, 7)));
        pieces.Add(spawnManager.SpawnPiece(Piece.Type.BlackPawn, piecesParent, new Vector3(2, 0.5f, 7)));
        pieces.Add(spawnManager.SpawnPiece(Piece.Type.BlackPawn, piecesParent, new Vector3(3, 0.5f, 7)));
        pieces.Add(spawnManager.SpawnPiece(Piece.Type.BlackPawn, piecesParent, new Vector3(4, 0.5f, 7)));
        pieces.Add(spawnManager.SpawnPiece(Piece.Type.BlackPawn, piecesParent, new Vector3(5, 0.5f, 7)));
        pieces.Add(spawnManager.SpawnPiece(Piece.Type.BlackPawn, piecesParent, new Vector3(6, 0.5f, 7)));
        pieces.Add(spawnManager.SpawnPiece(Piece.Type.BlackPawn, piecesParent, new Vector3(7, 0.5f, 7)));
        pieces.Add(spawnManager.SpawnPiece(Piece.Type.BlackPawn, piecesParent, new Vector3(8, 0.5f, 7)));

        pieces.Add(spawnManager.SpawnPiece(Piece.Type.BlackRook, piecesParent, new Vector3(1, 0.5f, 8)));
        pieces.Add(spawnManager.SpawnPiece(Piece.Type.BlackKnight, piecesParent, new Vector3(2, 0.5f, 8)));
        pieces.Add(spawnManager.SpawnPiece(Piece.Type.BlackBishop, piecesParent, new Vector3(3, 0.5f, 8)));
        pieces.Add(spawnManager.SpawnPiece(Piece.Type.BlackQueen, piecesParent, new Vector3(4, 0.5f, 8)));
        pieces.Add(spawnManager.SpawnPiece(Piece.Type.BlackKing, piecesParent, new Vector3(5, 0.5f, 8)));
        pieces.Add(spawnManager.SpawnPiece(Piece.Type.BlackBishop, piecesParent, new Vector3(6, 0.5f, 8)));
        pieces.Add(spawnManager.SpawnPiece(Piece.Type.BlackKnight, piecesParent, new Vector3(7, 0.5f, 8)));
        pieces.Add(spawnManager.SpawnPiece(Piece.Type.BlackRook, piecesParent, new Vector3(8, 0, 8)));
    }
}
