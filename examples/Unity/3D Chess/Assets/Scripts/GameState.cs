using System.Linq;
using Realms;
using UnityEngine;

public class GameState : MonoBehaviour
{
    [SerializeField] private PieceSpawner pieceSpawner = default;
    [SerializeField] private PieceMovement pieceMovement = default;
    [SerializeField] private GameObject piecesParent = default;

    private Realm realm = default;
    private IQueryable<PieceEntity> pieceEntities = default;

    public void UpdatePiecePosition(Piece movedPiece, Vector3 newPosition)
    {
        // Check if there is already a piece at the new position and if so, destroy it.
        var attackedPiece = piecesParent.GetComponentsInChildren<Piece>()
                                .FirstOrDefault(piece => piece.transform.position == newPosition);
        if (attackedPiece != null)
        {
            var attackedPieceEntity = FindPieceEntityAtPosition(newPosition);
            realm.Write(() =>
            {
                realm.Remove(attackedPieceEntity);
            });
            Destroy(attackedPiece.gameObject);
        }

        // Update the movedPiece's RealmObject.
        var oldPosition = movedPiece.transform.position;
        var movedPieceEntity = FindPieceEntityAtPosition(oldPosition);
        realm.Write(() =>
        {
            movedPieceEntity.SetPosition(newPosition);
        });

        // Update the movedPiece's GameObject.
        movedPiece.transform.position = newPosition;
    }

    public void ResetGame()
    {
        // Destroy all GameObjects.
        foreach (Piece piece in piecesParent.GetComponentsInChildren<Piece>())
        {
            Destroy(piece.gameObject);
        }

        // Re-create all RealmObjects with their initial position.
        pieceEntities = Persistence.ResetDatabase();

        // Recreate the GameObjects.
        CreateGameObjects();
    }

    private void Awake()
    {
        realm = Realm.GetInstance();
        pieceEntities = realm.All<PieceEntity>();

        // Check if we already have PieceEntity's (which means we resume a game).
        if (pieceEntities.Count() == 0)
        {
            // No game was saved, create the necessary RealmObjects.
            pieceEntities = Persistence.ResetDatabase();
        }

        CreateGameObjects();
    }

    private void CreateGameObjects()
    {
        // Each RealmObject needs a corresponding GameObject to represent it.
        foreach (PieceEntity pieceEntity in pieceEntities)
        {
            PieceType type = (PieceType)pieceEntity.Type;
            Vector3 position = pieceEntity.GetPosition();
            pieceSpawner.SpawnPiece(type, position, piecesParent, pieceMovement);
        }
    }

    private PieceEntity FindPieceEntityAtPosition(Vector3 position)
    {
        return pieceEntities.FirstOrDefault(piece =>
                            piece.PositionX == position.x &&
                            piece.PositionY == position.y &&
                            piece.PositionZ == position.z);
    }

}
