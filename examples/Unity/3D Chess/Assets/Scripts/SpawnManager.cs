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

    public Piece SpawnPiece(Piece.Type pieceType, GameObject parent, Vector3 positionInPartent)
    {
        Piece piecePrefab = default;

        switch (pieceType)
        {
            case Piece.Type.WhitePawn:
                piecePrefab = prefabPawnWhite;
                break;
            case Piece.Type.WhiteRook:
                piecePrefab = prefabRookWhite;
                break;
            case Piece.Type.WhiteKnight:
                piecePrefab = prefabKnightWhite;
                break;
            case Piece.Type.WhiteBishop:
                piecePrefab = prefabBishopWhite;
                break;
            case Piece.Type.WhiteQueen:
                piecePrefab = prefabQueenWhite;
                break;
            case Piece.Type.WhiteKing:
                piecePrefab = prefabKingWhite;
                break;
            case Piece.Type.BlackPawn:
                piecePrefab = prefabPawnBlack;
                break;
            case Piece.Type.BlackRook:
                piecePrefab = prefabRookBlack;
                break;
            case Piece.Type.BlackKnight:
                piecePrefab = prefabKnightBlack;
                break;
            case Piece.Type.BlackBishop:
                piecePrefab = prefabBishopBlack;
                break;
            case Piece.Type.BlackQueen:
                piecePrefab = prefabQueenBlack;
                break;
            case Piece.Type.BlackKing:
                piecePrefab = prefabKingBlack;
                break;
        }

        var position = new Vector3(positionInPartent.x, positionInPartent.y, positionInPartent.z);
        Piece pieceInstance = Instantiate(piecePrefab, position, Quaternion.identity, parent.transform);

        return pieceInstance;
    }
}
