using UnityEngine;

public class Target : MonoBehaviour
{

    [SerializeField] float moveSpeed = 5f;

    Vector2 movementInput = Vector2.zero;

    void Start()
    {

    }

    void Update()
    {
        Vector3 move = new Vector3(movementInput.x, 0, movementInput.y);
        transform.position += move * Time.deltaTime * moveSpeed;
    }

    public void SetMovementInput(Vector2 input)
    {
        movementInput = input;
    }
}
