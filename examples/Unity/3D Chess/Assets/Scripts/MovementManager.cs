using UnityEngine;

public class MovementManager : MonoBehaviour
{
    [SerializeField] private EventManager eventManager = default;
    [SerializeField] private GameState gameState = default;

    private Piece activePiece = default;

    public void SetActivePiece(Piece piece)
    {
        if (activePiece)
        {
            activePiece.Deselect();
        }
        activePiece = piece;
        activePiece.Select();
    }

    private void OnEnable()
    {
        eventManager.SquareClickedEvent.AddListener(SquareClickedListener);
    }

    private void OnDisable()
    {
        eventManager.SquareClickedEvent.RemoveListener(SquareClickedListener);
    }

    private void SquareClickedListener(Vector3 position)
    {
        if (activePiece)
        {
            gameState.UpdatePiecePosition(activePiece.transform.position, position);
            activePiece.Deselect();
            activePiece = null;
        }
    }
}
