using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Square : MonoBehaviour
{
    [SerializeField] private Board board = default;

    private void OnMouseDown()
    {
        board.SquareClickedEvent.Invoke((int)transform.position.x, (int)transform.position.z);
    }
}
