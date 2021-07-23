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

    public void SpawnPiece(PieceType pieceType, Vector3 position, GameObject parent, PieceMovement pieceMovement)
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
    }
}
