using UnityEngine;
using UnityEngine.Events;

public class SquareClickedEvent : UnityEvent<Vector3>
{
}

public class EventManager : MonoBehaviour
{
    public UnityEvent NewPieceActivated = new UnityEvent();
    public SquareClickedEvent SquareClickedEvent = new SquareClickedEvent();
}
