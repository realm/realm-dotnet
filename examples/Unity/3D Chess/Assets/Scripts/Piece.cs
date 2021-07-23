using UnityEngine;

public class Piece : MonoBehaviour
{
    public PieceType type = default;
    public PieceMovement pieceMovement = default;

    // This flag is necessary to make sure notifications work correctly.
    // When detroying a GameObject it will not be destroyed immediately
    // (see https://docs.unity3d.com/ScriptReference/Object.Destroy.html).
    // We need to check for the existence of Pieces when inserting new one's though
    // and this flag helps us check the status correctly.
    public bool willBeDeleted = false;

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
