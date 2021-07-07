using System.Collections.Generic;
using System.Linq;
using Realms;
using UnityEngine;

public class GameState : MonoBehaviour
{
    [SerializeField] private SpawnManager spawnManager = default;
    [SerializeField] private GameObject piecesParent = default;

    private List<Piece> pieces = new List<Piece>();
    private Realm realm = default;

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

    private void Awake()
    {
        realm = Realm.GetInstance();
    }

    private void Start()
    {
        IQueryable<PieceEntity> pieceEntities = realm.All<PieceEntity>();
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

    private void OnApplicationQuit()
    {
        realm.Write(() =>
        {
            realm.RemoveAll<PieceEntity>();
            foreach (Piece piece in pieces)
            {
                PieceEntity pieceEntity = new PieceEntity(piece.PieceType, piece.transform.position);
                realm.Add(pieceEntity);
            }
        });
        realm.Dispose();
    }

    private void SetUpInitialBoard()
    {
        pieces.Add(spawnManager.SpawnPiece(Piece.Type.WhiteRook, piecesParent, new Vector3(1, 0, 1)));
        pieces.Add(spawnManager.SpawnPiece(Piece.Type.WhiteKnight, piecesParent, new Vector3(2, 0, 1)));
        pieces.Add(spawnManager.SpawnPiece(Piece.Type.WhiteBishop, piecesParent, new Vector3(3, 0, 1)));
        pieces.Add(spawnManager.SpawnPiece(Piece.Type.WhiteQueen, piecesParent, new Vector3(4, 0, 1)));
        pieces.Add(spawnManager.SpawnPiece(Piece.Type.WhiteKing, piecesParent, new Vector3(5, 0, 1)));
        pieces.Add(spawnManager.SpawnPiece(Piece.Type.WhiteBishop, piecesParent, new Vector3(6, 0, 1)));
        pieces.Add(spawnManager.SpawnPiece(Piece.Type.WhiteKnight, piecesParent, new Vector3(7, 0, 1)));
        pieces.Add(spawnManager.SpawnPiece(Piece.Type.WhiteRook, piecesParent, new Vector3(8, 0, 1)));

        pieces.Add(spawnManager.SpawnPiece(Piece.Type.WhitePawn, piecesParent, new Vector3(1, 0, 2)));
        pieces.Add(spawnManager.SpawnPiece(Piece.Type.WhitePawn, piecesParent, new Vector3(2, 0, 2)));
        pieces.Add(spawnManager.SpawnPiece(Piece.Type.WhitePawn, piecesParent, new Vector3(3, 0, 2)));
        pieces.Add(spawnManager.SpawnPiece(Piece.Type.WhitePawn, piecesParent, new Vector3(4, 0, 2)));
        pieces.Add(spawnManager.SpawnPiece(Piece.Type.WhitePawn, piecesParent, new Vector3(5, 0, 2)));
        pieces.Add(spawnManager.SpawnPiece(Piece.Type.WhitePawn, piecesParent, new Vector3(6, 0, 2)));
        pieces.Add(spawnManager.SpawnPiece(Piece.Type.WhitePawn, piecesParent, new Vector3(7, 0, 2)));
        pieces.Add(spawnManager.SpawnPiece(Piece.Type.WhitePawn, piecesParent, new Vector3(8, 0, 2)));

        pieces.Add(spawnManager.SpawnPiece(Piece.Type.BlackPawn, piecesParent, new Vector3(1, 0, 7)));
        pieces.Add(spawnManager.SpawnPiece(Piece.Type.BlackPawn, piecesParent, new Vector3(2, 0, 7)));
        pieces.Add(spawnManager.SpawnPiece(Piece.Type.BlackPawn, piecesParent, new Vector3(3, 0, 7)));
        pieces.Add(spawnManager.SpawnPiece(Piece.Type.BlackPawn, piecesParent, new Vector3(4, 0, 7)));
        pieces.Add(spawnManager.SpawnPiece(Piece.Type.BlackPawn, piecesParent, new Vector3(5, 0, 7)));
        pieces.Add(spawnManager.SpawnPiece(Piece.Type.BlackPawn, piecesParent, new Vector3(6, 0, 7)));
        pieces.Add(spawnManager.SpawnPiece(Piece.Type.BlackPawn, piecesParent, new Vector3(7, 0, 7)));
        pieces.Add(spawnManager.SpawnPiece(Piece.Type.BlackPawn, piecesParent, new Vector3(8, 0, 7)));

        pieces.Add(spawnManager.SpawnPiece(Piece.Type.BlackRook, piecesParent, new Vector3(1, 0, 8)));
        pieces.Add(spawnManager.SpawnPiece(Piece.Type.BlackKnight, piecesParent, new Vector3(2, 0, 8)));
        pieces.Add(spawnManager.SpawnPiece(Piece.Type.BlackBishop, piecesParent, new Vector3(3, 0, 8)));
        pieces.Add(spawnManager.SpawnPiece(Piece.Type.BlackQueen, piecesParent, new Vector3(4, 0, 8)));
        pieces.Add(spawnManager.SpawnPiece(Piece.Type.BlackKing, piecesParent, new Vector3(5, 0, 8)));
        pieces.Add(spawnManager.SpawnPiece(Piece.Type.BlackBishop, piecesParent, new Vector3(6, 0, 8)));
        pieces.Add(spawnManager.SpawnPiece(Piece.Type.BlackKnight, piecesParent, new Vector3(7, 0, 8)));
        pieces.Add(spawnManager.SpawnPiece(Piece.Type.BlackRook, piecesParent, new Vector3(8, 0, 8)));
    }
}
