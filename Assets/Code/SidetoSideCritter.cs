using UnityEngine;

public class SidetoSideCritter : MonoBehaviour
{
    public float moveDistance = 2f; // How far to move from the starting point
    public float moveSpeed = 2f;    // How fast to move

    private Vector2 startPos;
    private bool movingRight = true;
    private Rigidbody2D rb;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        startPos = rb.position;
    }

    // FixedUpdate is called every fixed framerate frame, use it for Rigidbody2D movement
    void FixedUpdate()
    {
        float direction = movingRight ? 1f : -1f;
        rb.linearVelocity = new Vector2(direction * moveSpeed, rb.linearVelocity.y);

        float distanceFromStart = rb.position.x - startPos.x;
        if (movingRight && distanceFromStart >= moveDistance)
        {
            movingRight = false;
        }
        else if (!movingRight && distanceFromStart <= -moveDistance)
        {
            movingRight = true;
        }
    }
}
