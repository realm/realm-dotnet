using UnityEngine;

public class GameState : MonoBehaviour
{
    [SerializeField] private SpawnManager spawnManager = default;

    private Piece[] pieces = default;

    private void Start()
    {
        SetUpInitialBoard();
    }

    private void SetUpInitialBoard()
    {
        spawnManager.createPiece(SpawnManager.PieceType.RookWhite, new Vector3(4, 0, 4));
    }
}
