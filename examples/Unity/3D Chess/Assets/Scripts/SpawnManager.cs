using UnityEngine;

public class SpawnManager : MonoBehaviour
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

    public enum PieceType
    {
        PawnWhite,
        RookWhite,
        KnightWhite,
        BishopWhite,
        QueenWhite,
        KingWhite,
        PawnBlack,
        RookBlack,
        KnightBlack,
        BishopBlack,
        QueenBlack,
        KingBlack
    }

    public Piece createPiece(PieceType pieceType, Vector3 position)
    {
        Piece piecePrefab = default;

        switch (pieceType)
        {
            case PieceType.PawnWhite:
                piecePrefab = prefabPawnWhite;
                break;
            case PieceType.RookWhite:
                piecePrefab = prefabRookWhite;
                break;
            case PieceType.KnightWhite:
                piecePrefab = prefabKnightWhite;
                break;
            case PieceType.BishopWhite:
                piecePrefab = prefabBishopWhite;
                break;
            case PieceType.QueenWhite:
                piecePrefab = prefabQueenWhite;
                break;
            case PieceType.KingWhite:
                piecePrefab = prefabKingWhite;
                break;
            case PieceType.PawnBlack:
                piecePrefab = prefabPawnBlack;
                break;
            case PieceType.RookBlack:
                piecePrefab = prefabRookBlack;
                break;
            case PieceType.KnightBlack:
                piecePrefab = prefabKnightBlack;
                break;
            case PieceType.BishopBlack:
                piecePrefab = prefabBishopBlack;
                break;
            case PieceType.QueenBlack:
                piecePrefab = prefabQueenBlack;
                break;
            case PieceType.KingBlack:
                piecePrefab = prefabKingBlack;
                break;
        }

        Piece pieceInstance = Instantiate(piecePrefab);
        pieceInstance.transform.position = position;

        return pieceInstance;
    }
}
