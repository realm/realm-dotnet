using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Piece : MonoBehaviour
{
    [SerializeField] private Board board = default;

    private bool isActive = false;

    private void OnEnable()
    {
        board.SquareClickedEvent.AddListener(SquareClickedListener);
        board.NewPieceActivated.AddListener(NewPieceActivated);
    }

    private void OnDisable()
    {
        board.SquareClickedEvent.RemoveListener(SquareClickedListener);
        board.NewPieceActivated.RemoveListener(NewPieceActivated);
    }

    private void OnMouseDown()
    {
        if (isActive)
        {
            Deactivate();
        }
        else
        {
            Activate();
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        // Another piece was moved to this position.
        Destroy(gameObject);
    }

    private void SquareClickedListener(int x, int z)
    {
        if (isActive)
        {
            // This piece was moved to a new square.
            transform.position = new Vector3(x, 0, z);
            Deactivate();
        }
    }

    private void NewPieceActivated()
    {
        Deactivate();
    }

    private void Activate()
    {
        board.NewPieceActivated.Invoke();
        isActive = true;
        gameObject.GetComponent<Renderer>().material.color = new Color(1, 0, 0, 1);
    }

    private void Deactivate()
    {
        isActive = false;
        gameObject.GetComponent<Renderer>().material.color = new Color(1, 1, 1, 1);
    }
}
