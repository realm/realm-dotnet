using System.Collections.Generic;
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
            Destroy(attackedPiece.gameObject);
        }

        movedPiece.transform.position = new Vector3(endPosition.x, movedPiece.transform.position.y, endPosition.z);
    }

    private void Start()
    {
        SetUpInitialBoard();
    }

    private void SetUpInitialBoard()
    {
        pieces.Add(spawnManager.SpawnPiece(SpawnManager.PieceType.RookWhite, piecesParent, new Vector3(1, 0, 1)));
        pieces.Add(spawnManager.SpawnPiece(SpawnManager.PieceType.KnightWhite, piecesParent, new Vector3(2, 0, 1)));
        pieces.Add(spawnManager.SpawnPiece(SpawnManager.PieceType.BishopWhite, piecesParent, new Vector3(3, 0, 1)));
        pieces.Add(spawnManager.SpawnPiece(SpawnManager.PieceType.QueenWhite, piecesParent, new Vector3(4, 0, 1)));
        pieces.Add(spawnManager.SpawnPiece(SpawnManager.PieceType.KingWhite, piecesParent, new Vector3(5, 0, 1)));
        pieces.Add(spawnManager.SpawnPiece(SpawnManager.PieceType.BishopWhite, piecesParent, new Vector3(6, 0, 1)));
        pieces.Add(spawnManager.SpawnPiece(SpawnManager.PieceType.KnightWhite, piecesParent, new Vector3(7, 0, 1)));
        pieces.Add(spawnManager.SpawnPiece(SpawnManager.PieceType.RookWhite, piecesParent, new Vector3(8, 0, 1)));

        pieces.Add(spawnManager.SpawnPiece(SpawnManager.PieceType.PawnWhite, piecesParent, new Vector3(1, 0, 2)));
        pieces.Add(spawnManager.SpawnPiece(SpawnManager.PieceType.PawnWhite, piecesParent, new Vector3(2, 0, 2)));
        pieces.Add(spawnManager.SpawnPiece(SpawnManager.PieceType.PawnWhite, piecesParent, new Vector3(3, 0, 2)));
        pieces.Add(spawnManager.SpawnPiece(SpawnManager.PieceType.PawnWhite, piecesParent, new Vector3(4, 0, 2)));
        pieces.Add(spawnManager.SpawnPiece(SpawnManager.PieceType.PawnWhite, piecesParent, new Vector3(5, 0, 2)));
        pieces.Add(spawnManager.SpawnPiece(SpawnManager.PieceType.PawnWhite, piecesParent, new Vector3(6, 0, 2)));
        pieces.Add(spawnManager.SpawnPiece(SpawnManager.PieceType.PawnWhite, piecesParent, new Vector3(7, 0, 2)));
        pieces.Add(spawnManager.SpawnPiece(SpawnManager.PieceType.PawnWhite, piecesParent, new Vector3(8, 0, 2)));

        pieces.Add(spawnManager.SpawnPiece(SpawnManager.PieceType.PawnBlack, piecesParent, new Vector3(1, 0, 7)));
        pieces.Add(spawnManager.SpawnPiece(SpawnManager.PieceType.PawnBlack, piecesParent, new Vector3(2, 0, 7)));
        pieces.Add(spawnManager.SpawnPiece(SpawnManager.PieceType.PawnBlack, piecesParent, new Vector3(3, 0, 7)));
        pieces.Add(spawnManager.SpawnPiece(SpawnManager.PieceType.PawnBlack, piecesParent, new Vector3(4, 0, 7)));
        pieces.Add(spawnManager.SpawnPiece(SpawnManager.PieceType.PawnBlack, piecesParent, new Vector3(5, 0, 7)));
        pieces.Add(spawnManager.SpawnPiece(SpawnManager.PieceType.PawnBlack, piecesParent, new Vector3(6, 0, 7)));
        pieces.Add(spawnManager.SpawnPiece(SpawnManager.PieceType.PawnBlack, piecesParent, new Vector3(7, 0, 7)));
        pieces.Add(spawnManager.SpawnPiece(SpawnManager.PieceType.PawnBlack, piecesParent, new Vector3(8, 0, 7)));

        pieces.Add(spawnManager.SpawnPiece(SpawnManager.PieceType.RookBlack, piecesParent, new Vector3(1, 0, 8)));
        pieces.Add(spawnManager.SpawnPiece(SpawnManager.PieceType.KnightBlack, piecesParent, new Vector3(2, 0, 8)));
        pieces.Add(spawnManager.SpawnPiece(SpawnManager.PieceType.BishopBlack, piecesParent, new Vector3(3, 0, 8)));
        pieces.Add(spawnManager.SpawnPiece(SpawnManager.PieceType.QueenBlack, piecesParent, new Vector3(4, 0, 8)));
        pieces.Add(spawnManager.SpawnPiece(SpawnManager.PieceType.KingBlack, piecesParent, new Vector3(5, 0, 8)));
        pieces.Add(spawnManager.SpawnPiece(SpawnManager.PieceType.BishopBlack, piecesParent, new Vector3(6, 0, 8)));
        pieces.Add(spawnManager.SpawnPiece(SpawnManager.PieceType.KnightBlack, piecesParent, new Vector3(7, 0, 8)));
        pieces.Add(spawnManager.SpawnPiece(SpawnManager.PieceType.RookBlack, piecesParent, new Vector3(8, 0, 8)));
    }
}
