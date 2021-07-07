using UnityEngine;

public class Square : MonoBehaviour
{
    [SerializeField] private EventManager eventManager = default;

    private void OnMouseDown()
    {
        int x = (int)Mathf.Round(transform.position.x);
        int z = (int)Mathf.Round(transform.position.z);

        eventManager.SquareClickedEvent.Invoke(x, z);
    }
}
