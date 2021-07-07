using UnityEngine;

public class Piece : MonoBehaviour
{
    [SerializeField] private EventManager eventManager = default;

    private bool isActive = false;

    private void OnEnable()
    {
        eventManager.SquareClickedEvent.AddListener(SquareClickedListener);
        eventManager.NewPieceActivated.AddListener(NewPieceActivated);
    }

    private void OnDisable()
    {
        eventManager.SquareClickedEvent.RemoveListener(SquareClickedListener);
        eventManager.NewPieceActivated.RemoveListener(NewPieceActivated);
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
        eventManager.NewPieceActivated.Invoke();
        isActive = true;
        gameObject.GetComponent<Renderer>().material.color = new Color(1, 0, 0, 1);
    }

    private void Deactivate()
    {
        isActive = false;
        gameObject.GetComponent<Renderer>().material.color = new Color(1, 1, 1, 1);
    }
}
