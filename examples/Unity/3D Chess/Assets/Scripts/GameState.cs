using System.Linq;
using Realms;
using UnityEngine;

public class GameState : MonoBehaviour
{
    [SerializeField] private PieceSpawner pieceSpawner = default;
    [SerializeField] private PieceMovement pieceMovement = default;
    [SerializeField] private GameObject piecesParent = default;

    private Realm realm = default;
    private IQueryable<PieceEntity> pieces = default;

    public void UpdatePiecePosition(Piece movedPiece, Vector3 newPosition)
    {
        // Check if there is already a piece at the new position and if so, destroy it.
        foreach (Piece attackedPiece in piecesParent.GetComponentsInChildren<Piece>())
        {
            if (attackedPiece.transform.position == newPosition)
            {
                var attackedPieceEntity = pieces.FirstOrDefault(piece =>
                            piece.PositionX == newPosition.x &&
                            piece.PositionY == newPosition.y &&
                            piece.PositionZ == newPosition.z);
                realm.Write(() =>
                {
                    realm.Remove(attackedPieceEntity);
                });
                Destroy(attackedPiece.gameObject);
            }
        }

        // Update the movedPiece's RealmObject.
        var oldPosition = movedPiece.transform.position;
        var movedPieceEntity = pieces.FirstOrDefault(piece =>
                            piece.PositionX == oldPosition.x &&
                            piece.PositionY == oldPosition.y &&
                            piece.PositionZ == oldPosition.z);
        realm.Write(() =>
        {
            movedPieceEntity.SetPosition(newPosition);
        });

        // Finally move the GameOject.
        movedPiece.transform.position = newPosition;
    }

    public void ResetGame()
    {
        // Destroy all GameObjects.
        foreach (Piece piece in piecesParent.GetComponentsInChildren<Piece>())
        {
            Destroy(piece.gameObject);
        }

        // Re-create all RealmObjects with their original position.
        pieces = Persistence.ResetDatabase().AsQueryable();

        // Recreate the board.
        pieceSpawner.SetUpInitialBoard(piecesParent, pieceMovement);
    }

    private void Awake()
    {
        realm = Realm.GetInstance();

        // Check if we already have PieceEntity's (which means we resume a game).
        pieces = realm.All<PieceEntity>();
        if (pieces.Count() > 0)
        {
            foreach (PieceEntity pieceEntity in pieces)
            {
                // Create the GameObjects for these RealmObjects.
                PieceType type = (PieceType)pieceEntity.Type;
                Vector3 position = pieceEntity.GetPosition();
                pieceSpawner.SpawnPiece(type, position, piecesParent, pieceMovement);
            }
        }
        else
        {
            // No game was saved, create a new board.
            pieces = Persistence.ResetDatabase().AsQueryable();
            pieceSpawner.SetUpInitialBoard(piecesParent, pieceMovement);
        }
    }

}
