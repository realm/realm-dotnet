using System;
using System.Linq;
using Realms;
using UnityEngine;

public class GameState : MonoBehaviour
{
    [SerializeField] private PieceSpawner pieceSpawner = default;
    [SerializeField] private PieceMovement pieceMovement = default;
    [SerializeField] private GameObject pieces = default;

    private Realm realm = default;
    private IQueryable<PieceEntity> pieceEntities = default;
    private IDisposable notificationToken = default;

    public void UpdatePieceToPosition(Piece movedPiece, Vector3 newPosition)
    {
        // Check if there is already a piece at the new position and if so, destroy it.
        var attackedPiece = FindPieceAtPosition(newPosition);
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
        foreach (Piece piece in pieces.GetComponentsInChildren<Piece>())
        {
            Destroy(piece.gameObject);
        }

        // We need to unsubscribe from notification while resetting to surpress them.
        notificationToken.Dispose();
        notificationToken = null;

        // Re-create all RealmObjects with their initial position.
        pieceEntities = Persistence.ResetDatabase(realm);

        // Recreate the GameObjects.
        CreateGameObjects();

        SubscribeFotNotifications();
    }

    private async void Awake()
    {
        realm = await Persistence.CreateRealmAsync();
        pieceEntities = realm.All<PieceEntity>();

        // Check if we already have PieceEntity's (which means we resume a game).
        if (pieceEntities.Count() == 0)
        {
            // No game was saved, create the necessary RealmObjects.
            pieceEntities = Persistence.ResetDatabase(realm);
        }
        CreateGameObjects();

        SubscribeFotNotifications();
    }

    private void OnDestroy()
    {
        notificationToken.Dispose();
        realm.Dispose();
    }

    private void CreateGameObjects()
    {
        // Each RealmObject needs a corresponding GameObject to represent it.
        foreach (PieceEntity pieceEntity in pieceEntities)
        {
            PieceType type = (PieceType)pieceEntity.Type;
            Vector3 position = pieceEntity.GetPosition();
            pieceSpawner.SpawnPiece(type, position, pieces, pieceMovement);
        }
    }

    private Piece FindPieceAtIndex(int i)
    {
        return pieces.GetComponentsInChildren<Piece>()[i];
    }

    private Piece FindPieceAtPosition(Vector3 position)
    {
        return pieces.GetComponentsInChildren<Piece>()
            .FirstOrDefault(piece => piece.transform.position == position);
    }

    private PieceEntity FindPieceEntityAtPosition(Vector3 position)
    {
        return pieceEntities.FirstOrDefault(piece =>
                            piece.PositionX == position.x &&
                            piece.PositionY == position.y &&
                            piece.PositionZ == position.z);
    }

    private void SubscribeFotNotifications()
    {
        notificationToken = pieceEntities.SubscribeForNotifications((sender, changes, error) =>
        {
            if (changes != null)
            {
                foreach (var i in changes.DeletedIndices)
                {
                    var deletedPiece = FindPieceAtIndex(i);
                    // The `willBeDeleted` flag needs to be set here to make sure when checking
                    // if a Piece already exists in this location during the `InsertedIndices` loop
                    // is correct.
                    // The Destroy will not be executed right here but instead at the end of the Update
                    // loop (see https://docs.unity3d.com/ScriptReference/Object.Destroy.html).
                    deletedPiece.willBeDeleted = true;
                    Destroy(deletedPiece.gameObject);
                }
                foreach (var i in changes.InsertedIndices)
                {
                    var insertedPieceEntity = sender.ElementAt(i);
                    var insertedPiece = FindPieceAtPosition(insertedPieceEntity.GetPosition());
                    if (insertedPiece == null || insertedPiece.willBeDeleted == true)
                    {
                        PieceType type = (PieceType)insertedPieceEntity.Type;
                        Vector3 position = insertedPieceEntity.GetPosition();
                        pieceSpawner.SpawnPiece(type, position, pieces, pieceMovement);
                    }
                }
                foreach (var i in changes.NewModifiedIndices)
                {
                    var modifiedPieceEntity = sender.ElementAt(i);
                    var modifiedPiece = FindPieceAtIndex(i);
                    modifiedPiece.transform.position = modifiedPieceEntity.GetPosition();
                }
            }
        });
    }

}
