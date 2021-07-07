using UnityEngine;

public class Square : MonoBehaviour
{
    [SerializeField] private EventManager eventManager = default;

    private void OnMouseDown()
    {
        eventManager.SquareClickedEvent.Invoke(transform.position);
    }
}
