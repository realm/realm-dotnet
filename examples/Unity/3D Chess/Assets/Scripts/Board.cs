using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class SquareClickedEvent : UnityEvent<int, int>
{
}

public class Board : MonoBehaviour
{
    public UnityEvent NewPieceActivated = new UnityEvent();
    public SquareClickedEvent SquareClickedEvent = new SquareClickedEvent();
}
