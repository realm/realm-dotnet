using System.Collections.Generic;
using UnityEngine;

public class GameState : MonoBehaviour
{
    [SerializeField] private SpawnManager spawnManager = default;
    [SerializeField] private GameObject piecesParent = default;

    private List<Piece> pieces = new List<Piece>();

    private void Start()
    {
        SetUpInitialBoard();
    }

    private void SetUpInitialBoard()
    {
        pieces.Add(spawnManager.createPiece(SpawnManager.PieceType.RookWhite, new Vector3(1, 0, 1)));
        pieces.Add(spawnManager.createPiece(SpawnManager.PieceType.KnightWhite, new Vector3(2, 0, 1)));
        pieces.Add(spawnManager.createPiece(SpawnManager.PieceType.BishopWhite, new Vector3(3, 0, 1)));
        pieces.Add(spawnManager.createPiece(SpawnManager.PieceType.QueenWhite, new Vector3(4, 0, 1)));
        pieces.Add(spawnManager.createPiece(SpawnManager.PieceType.KingWhite, new Vector3(5, 0, 1)));
        pieces.Add(spawnManager.createPiece(SpawnManager.PieceType.BishopWhite, new Vector3(6, 0, 1)));
        pieces.Add(spawnManager.createPiece(SpawnManager.PieceType.KnightWhite, new Vector3(7, 0, 1)));
        pieces.Add(spawnManager.createPiece(SpawnManager.PieceType.RookWhite, new Vector3(8, 0, 1)));

        pieces.Add(spawnManager.createPiece(SpawnManager.PieceType.PawnWhite, new Vector3(1, 0, 2)));
        pieces.Add(spawnManager.createPiece(SpawnManager.PieceType.PawnWhite, new Vector3(2, 0, 2)));
        pieces.Add(spawnManager.createPiece(SpawnManager.PieceType.PawnWhite, new Vector3(3, 0, 2)));
        pieces.Add(spawnManager.createPiece(SpawnManager.PieceType.PawnWhite, new Vector3(4, 0, 2)));
        pieces.Add(spawnManager.createPiece(SpawnManager.PieceType.PawnWhite, new Vector3(5, 0, 2)));
        pieces.Add(spawnManager.createPiece(SpawnManager.PieceType.PawnWhite, new Vector3(6, 0, 2)));
        pieces.Add(spawnManager.createPiece(SpawnManager.PieceType.PawnWhite, new Vector3(7, 0, 2)));
        pieces.Add(spawnManager.createPiece(SpawnManager.PieceType.PawnWhite, new Vector3(8, 0, 2)));

        pieces.Add(spawnManager.createPiece(SpawnManager.PieceType.PawnBlack, new Vector3(1, 0, 7)));
        pieces.Add(spawnManager.createPiece(SpawnManager.PieceType.PawnBlack, new Vector3(2, 0, 7)));
        pieces.Add(spawnManager.createPiece(SpawnManager.PieceType.PawnBlack, new Vector3(3, 0, 7)));
        pieces.Add(spawnManager.createPiece(SpawnManager.PieceType.PawnBlack, new Vector3(4, 0, 7)));
        pieces.Add(spawnManager.createPiece(SpawnManager.PieceType.PawnBlack, new Vector3(5, 0, 7)));
        pieces.Add(spawnManager.createPiece(SpawnManager.PieceType.PawnBlack, new Vector3(6, 0, 7)));
        pieces.Add(spawnManager.createPiece(SpawnManager.PieceType.PawnBlack, new Vector3(7, 0, 7)));
        pieces.Add(spawnManager.createPiece(SpawnManager.PieceType.PawnBlack, new Vector3(8, 0, 7)));

        pieces.Add(spawnManager.createPiece(SpawnManager.PieceType.RookBlack, new Vector3(1, 0, 8)));
        pieces.Add(spawnManager.createPiece(SpawnManager.PieceType.KnightBlack, new Vector3(2, 0, 8)));
        pieces.Add(spawnManager.createPiece(SpawnManager.PieceType.BishopBlack, new Vector3(3, 0, 8)));
        pieces.Add(spawnManager.createPiece(SpawnManager.PieceType.QueenBlack, new Vector3(4, 0, 8)));
        pieces.Add(spawnManager.createPiece(SpawnManager.PieceType.KingBlack, new Vector3(5, 0, 8)));
        pieces.Add(spawnManager.createPiece(SpawnManager.PieceType.BishopBlack, new Vector3(6, 0, 8)));
        pieces.Add(spawnManager.createPiece(SpawnManager.PieceType.KnightBlack, new Vector3(7, 0, 8)));
        pieces.Add(spawnManager.createPiece(SpawnManager.PieceType.RookBlack, new Vector3(8, 0, 8)));

        foreach (Piece piece in pieces)
        {
            piece.transform.SetParent(piecesParent.transform);
        }
    }
}
