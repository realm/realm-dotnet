using System.Linq;
using Realms;
using UnityEngine;

public class Piece : MonoBehaviour
{
    public PieceType type = default;

    private MovementManager movementManager = default;

    private Realm realm = default;
    private PieceEntity pieceEntity = default;

    public void Select()
    {
        gameObject.GetComponent<Renderer>().material.color = new Color(1, 0, 0, 1);
    }

    public void Deselect()
    {
        gameObject.GetComponent<Renderer>().material.color = new Color(1, 1, 1, 1);
    }

    public void MoveTo(Vector3 newPosition)
    {
        if (pieceEntity == null)
        {
            Debug.LogError("pieceEntity must not be null.");
        }
        if (realm == null)
        {
            Debug.LogError("realm must not be null.");
        }
        transform.position = newPosition;
        realm.Write(() =>
        {
            pieceEntity.SetPosition(newPosition);
        });
    }

    private void OnMouseDown()
    {
        if (movementManager == null)
        {
            Debug.LogError("movementManager must not be null");
        }
        movementManager.SetActivePiece(this);
    }

    private async void Awake()
    {
        movementManager = GameObject.FindObjectOfType<MovementManager>();

        if (SyncedRealmConfiguration.SyncConfiguration == null)
        {
            await SyncedRealmConfiguration.CreateSyncConfiguration();
        }
        realm = await Realm.GetInstanceAsync(SyncedRealmConfiguration.SyncConfiguration);
        pieceEntity = realm.All<PieceEntity>().FirstOrDefault(piece =>
                        piece.PositionX == transform.position.x &&
                        piece.PositionY == transform.position.y &&
                        piece.PositionZ == transform.position.z &&
                        piece.Type == (int)type);
        if (pieceEntity == null)
        {
            realm.Write(() =>
            {
                pieceEntity = new PieceEntity(type, transform.position);
                realm.Add(pieceEntity);
                pieceEntity.PropertyChanged += PropertyChanged;
            });
        }
    }

    private void Update()
    {
        if (transform.hasChanged && realm != null && pieceEntity != null)
        {
            realm.Write(() =>
            {
                pieceEntity.SetPosition(transform.position);
            });
            transform.hasChanged = false;
        }
    }

    private void OnDestroy()
    {
        if (pieceEntity == null)
        {
            Debug.LogError("pieceEntity must not be null.");
        }
        if (realm == null)
        {
            Debug.LogError("realm must not be null.");
        }
        realm.Write(() =>
        {
            realm.Remove(pieceEntity);
        });
    }

    private void PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        if (pieceEntity == null)
        {
            Debug.LogError("pieceEntity must not be null");
        }
        transform.position = pieceEntity.GetPosition();
    }
}
