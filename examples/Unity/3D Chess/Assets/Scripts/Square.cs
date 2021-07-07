using UnityEngine;

public class Square : MonoBehaviour
{
    [SerializeField] private EventManager eventManager = default;

    private void OnMouseDown()
    {
        eventManager.SquareClickedEvent.Invoke((int)transform.position.x, (int)transform.position.z);
    }
}
