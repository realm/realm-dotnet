using UnityEngine;

public class SpawnManager : MonoBehaviour
{
    [SerializeField] GameObject prefabPawnWhite = default;
    [SerializeField] GameObject prefabRookWhite = default;
    [SerializeField] GameObject prefabKnightWhite = default;
    [SerializeField] GameObject prefabBishopWhite = default;
    [SerializeField] GameObject prefabQueenWhite = default;
    [SerializeField] GameObject prefabKingWhite = default;
    [SerializeField] GameObject prefabPawnBlack = default;
    [SerializeField] GameObject prefabRookBlack = default;
    [SerializeField] GameObject prefabKnightBlack = default;
    [SerializeField] GameObject prefabBishopBlack = default;
    [SerializeField] GameObject prefabQueenBlack = default;
    [SerializeField] GameObject prefabKingBlack = default;

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

    public void createPiece(PieceType pieceType, Vector3 position)
    {
        GameObject piecePrefab = default;

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

        GameObject pieceInstance = Instantiate(piecePrefab);
        pieceInstance.transform.position = position;
    }
}
