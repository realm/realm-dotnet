using UnityEngine;

public class ResetButton : MonoBehaviour
{
    [SerializeField] private GameState gameState = default;

    public void ResetGame()
    {
        gameState.ResetGame();
    }
}
