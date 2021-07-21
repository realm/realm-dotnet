using UnityEngine;

public class PieceSpawner : MonoBehaviour
{
    [SerializeField] Piece prefabPawnWhite = default;
    [SerializeField] Piece prefabRookWhite = default;
    [SerializeField] Piece prefabKnightWhite = default;
    [SerializeField] Piece prefabBishopWhite = default;
    [SerializeField] Piece prefabQueenWhite = default;
    [SerializeField] Piece prefabKingWhite = default;
    [SerializeField] Piece prefabPawnBlack = default;
    [SerializeField] Piece prefabRookBlack = default;
    [SerializeField] Piece prefabKnightBlack = default;
    [SerializeField] Piece prefabBishopBlack = default;
    [SerializeField] Piece prefabQueenBlack = default;
    [SerializeField] Piece prefabKingBlack = default;

    public Piece SpawnPiece(PieceType pieceType, Vector3 position, GameObject parent, PieceMovement pieceMovement)
    {
        Piece piecePrefab = default;

        switch (pieceType)
        {
            case PieceType.WhitePawn:
                piecePrefab = prefabPawnWhite;
                break;
            case PieceType.WhiteRook:
                piecePrefab = prefabRookWhite;
                break;
            case PieceType.WhiteKnight:
                piecePrefab = prefabKnightWhite;
                break;
            case PieceType.WhiteBishop:
                piecePrefab = prefabBishopWhite;
                break;
            case PieceType.WhiteQueen:
                piecePrefab = prefabQueenWhite;
                break;
            case PieceType.WhiteKing:
                piecePrefab = prefabKingWhite;
                break;
            case PieceType.BlackPawn:
                piecePrefab = prefabPawnBlack;
                break;
            case PieceType.BlackRook:
                piecePrefab = prefabRookBlack;
                break;
            case PieceType.BlackKnight:
                piecePrefab = prefabKnightBlack;
                break;
            case PieceType.BlackBishop:
                piecePrefab = prefabBishopBlack;
                break;
            case PieceType.BlackQueen:
                piecePrefab = prefabQueenBlack;
                break;
            case PieceType.BlackKing:
                piecePrefab = prefabKingBlack;
                break;
        }

        Piece pieceInstance = Instantiate(piecePrefab, position, Quaternion.identity, parent.transform);
        pieceInstance.pieceMovement = pieceMovement;

        return pieceInstance;
    }

    public void SetUpInitialBoard(GameObject piecesParent, PieceMovement pieceMovement)
    {
        SpawnPiece(PieceType.WhiteRook, new Vector3(1, 0.5f, 1), piecesParent, pieceMovement);
        SpawnPiece(PieceType.WhiteKnight, new Vector3(2, 0.5f, 1), piecesParent, pieceMovement);
        SpawnPiece(PieceType.WhiteBishop, new Vector3(3, 0.5f, 1), piecesParent, pieceMovement);
        SpawnPiece(PieceType.WhiteQueen, new Vector3(4, 0.5f, 1), piecesParent, pieceMovement);
        SpawnPiece(PieceType.WhiteKing, new Vector3(5, 0.5f, 1), piecesParent, pieceMovement);
        SpawnPiece(PieceType.WhiteBishop, new Vector3(6, 0.5f, 1), piecesParent, pieceMovement);
        SpawnPiece(PieceType.WhiteKnight, new Vector3(7, 0.5f, 1), piecesParent, pieceMovement);
        SpawnPiece(PieceType.WhiteRook, new Vector3(8, 0.5f, 1), piecesParent, pieceMovement);

        SpawnPiece(PieceType.WhitePawn, new Vector3(1, 0.5f, 2), piecesParent, pieceMovement);
        SpawnPiece(PieceType.WhitePawn, new Vector3(2, 0.5f, 2), piecesParent, pieceMovement);
        SpawnPiece(PieceType.WhitePawn, new Vector3(3, 0.5f, 2), piecesParent, pieceMovement);
        SpawnPiece(PieceType.WhitePawn, new Vector3(4, 0.5f, 2), piecesParent, pieceMovement);
        SpawnPiece(PieceType.WhitePawn, new Vector3(5, 0.5f, 2), piecesParent, pieceMovement);
        SpawnPiece(PieceType.WhitePawn, new Vector3(6, 0.5f, 2), piecesParent, pieceMovement);
        SpawnPiece(PieceType.WhitePawn, new Vector3(7, 0.5f, 2), piecesParent, pieceMovement);
        SpawnPiece(PieceType.WhitePawn, new Vector3(8, 0.5f, 2), piecesParent, pieceMovement);

        SpawnPiece(PieceType.BlackPawn, new Vector3(1, 0.5f, 7), piecesParent, pieceMovement);
        SpawnPiece(PieceType.BlackPawn, new Vector3(2, 0.5f, 7), piecesParent, pieceMovement);
        SpawnPiece(PieceType.BlackPawn, new Vector3(3, 0.5f, 7), piecesParent, pieceMovement);
        SpawnPiece(PieceType.BlackPawn, new Vector3(4, 0.5f, 7), piecesParent, pieceMovement);
        SpawnPiece(PieceType.BlackPawn, new Vector3(5, 0.5f, 7), piecesParent, pieceMovement);
        SpawnPiece(PieceType.BlackPawn, new Vector3(6, 0.5f, 7), piecesParent, pieceMovement);
        SpawnPiece(PieceType.BlackPawn, new Vector3(7, 0.5f, 7), piecesParent, pieceMovement);
        SpawnPiece(PieceType.BlackPawn, new Vector3(8, 0.5f, 7), piecesParent, pieceMovement);

        SpawnPiece(PieceType.BlackRook, new Vector3(1, 0.5f, 8), piecesParent, pieceMovement);
        SpawnPiece(PieceType.BlackKnight, new Vector3(2, 0.5f, 8), piecesParent, pieceMovement);
        SpawnPiece(PieceType.BlackBishop, new Vector3(3, 0.5f, 8), piecesParent, pieceMovement);
        SpawnPiece(PieceType.BlackQueen, new Vector3(4, 0.5f, 8), piecesParent, pieceMovement);
        SpawnPiece(PieceType.BlackKing, new Vector3(5, 0.5f, 8), piecesParent, pieceMovement);
        SpawnPiece(PieceType.BlackBishop, new Vector3(6, 0.5f, 8), piecesParent, pieceMovement);
        SpawnPiece(PieceType.BlackKnight, new Vector3(7, 0.5f, 8), piecesParent, pieceMovement);
        SpawnPiece(PieceType.BlackRook, new Vector3(8, 0.5f, 8), piecesParent, pieceMovement);
    }
}
