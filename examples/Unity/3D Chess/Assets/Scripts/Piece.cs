using UnityEngine;

public class Piece : MonoBehaviour
{
    public enum Type : int
    {
        WhitePawn,
        WhiteBishop,
        WhiteKnight,
        WhiteRook,
        WhiteQueen,
        WhiteKing,
        BlackPawn,
        BlackBishop,
        BlackKnight,
        BlackRook,
        BlackQueen,
        BlackKing
    }

    public Type PieceType => type;

    [SerializeField] private Type type = default;

    private MovementManager movementManager = default;

    public void Select()
    {
        gameObject.GetComponent<Renderer>().material.color = new Color(1, 0, 0, 1);
    }

    public void Deselect()
    {
        gameObject.GetComponent<Renderer>().material.color = new Color(1, 1, 1, 1);
    }

    private void Start()
    {
        movementManager = GameObject.FindObjectOfType<MovementManager>();
    }

    private void OnMouseDown()
    {
        movementManager.SetActivePiece(this);
    }
}
