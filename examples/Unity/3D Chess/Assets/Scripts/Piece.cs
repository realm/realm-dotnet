using UnityEngine;

public class Piece : MonoBehaviour
{
    public PieceType type = default;
    public PieceMovement pieceMovement = default;

    public void Select()
    {
        gameObject.GetComponent<Renderer>().material.color = new Color(1, 0, 0, 1);
    }

    public void Deselect()
    {
        gameObject.GetComponent<Renderer>().material.color = new Color(1, 1, 1, 1);
    }

    private void OnMouseDown()
    {
        pieceMovement.SetActivePiece(this);
    }
}
