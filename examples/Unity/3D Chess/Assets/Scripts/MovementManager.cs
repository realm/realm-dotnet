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
            // We need to elevate the piece slightly to make it sit on top of the cube.
            // For this example we'll just fix it to 0.5f as we know the height.
            var newPosition = position + new Vector3(0, 0.5f, 0);

            gameState.UpdatePiecePosition(activePiece, position);
            activePiece.Deselect();
            activePiece = null;
        }
    }
}
