using UnityEngine;

public class BallController : MonoBehaviour
{
    public float moveSpeed = 10f; // speed of the ball
    public float maxSpeed = 15f; // maximum speed of the ball
    public float minAngle = 15f; // minimum angle of deflection when hitting the player
    public float maxAngle = 75f; // maximum angle of deflection when hitting the player
    public float randomAngleRange = 15f; // range of random angle added to the deflection angle when hitting the player

    private Rigidbody2D rb; // rigidbody component of the ball
    private Vector2 direction = Vector2.up; // current direction of the ball's movement
    private AudioSource audioSource; // audio source component

    void Start()
    {
        // get the rigidbody component
        rb = GetComponent<Rigidbody2D>();

        audioSource = GetComponent<AudioSource>();

        // Enhanced collision detection settings
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        rb.interpolation = RigidbodyInterpolation2D.Interpolate;

        // Ensure the ball doesn't lose velocity due to physics
        rb.gravityScale = 0f;
        rb.linearDamping = 0f;
        rb.angularDamping = 0f;
    }

    void FixedUpdate()  // Changed from Update to FixedUpdate for physics
    {
        // Move the ball using physics instead of direct position manipulation
        rb.linearVelocity = direction * moveSpeed;

        // Clamp the velocity to prevent excessive speed
        if (rb.linearVelocity.magnitude > maxSpeed)
        {
            rb.linearVelocity = rb.linearVelocity.normalized * maxSpeed;
        }

        // Check if the ball is gone (below a certain point)
        if (transform.position.y < -10)
        {
            RestartGame();
        }
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        // if the ball hits the player, deflect it based on the point of contact
        if (collision.gameObject.CompareTag("Player"))
        {
            // calculate the deflection angle based on the point of contact on the player
            Vector2 contactPoint = collision.contacts[0].point;
            Vector2 playerPosition = collision.transform.position;
            float relativeContactPoint = (contactPoint.x - playerPosition.x) / collision.collider.bounds.size.x;

            // calculate the new direction of the ball's movement based on the relative contact point
            float angle = relativeContactPoint * maxAngle;
            Vector2 newDirection = new Vector2(Mathf.Sin(angle * Mathf.Deg2Rad), Mathf.Cos(angle * Mathf.Deg2Rad)).normalized;

            // ensure the ball always moves upwards after hitting the paddle
            if (newDirection.y < 0)
            {
                newDirection.y = -newDirection.y;
            }

            direction = newDirection;
        }
        // if the ball hits a brick, destroy the brick and deflect the ball
        else if (collision.gameObject.CompareTag("Brick"))
        {
            // Destroy the brick
            BrickController brickController = collision.gameObject.GetComponent<BrickController>();
            brickController.DestroyBrick();

            // Deflect the ball based on the collision normal
            DeflectBall(collision.contacts[0].normal);
        }
        // if the ball hits any other object, deflect it based on the surface normal
        else
        {
            DeflectBall(collision.contacts[0].normal);
        }

        // play a sound effect when the ball hits something
        audioSource.Play();
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.CompareTag("BottomBoundry"))
        {
            // ball hit the bottom boundary, end the game
            Debug.Log("Game over!");
            audioSource.Play();
            // you can add additional game over logic here, such as showing a game over screen or resetting the game
        }
    }

    private void DeflectBall(Vector2 normal)
    {
        // reflect the direction of the ball's movement based on the surface normal
        direction = Vector2.Reflect(direction, normal).normalized;
    }

    void RestartGame()
    {
        // Reset the ball's position to the starting position
        transform.position = new Vector3(0, -4, 0);

        // Reset the ball's direction
        direction = Vector2.up;

        // Optionally, reset other game elements or variables as needed
    }
}

public class PaddleController : MonoBehaviour
{
    public float moveSpeed = 10f;
    public float leftBoundary = -8f; // TODO: Add the actual boundary of the box collider of the border here
    public float rightBoundary = 8f;

    private Rigidbody2D rb;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    void Update()
    {
        // Get horizontal input
        float horizontalInput = Input.GetAxis("Horizontal");

        // Calculate new position
        Vector2 newPosition = rb.position + Vector2.right * horizontalInput * moveSpeed * Time.deltaTime;

        // Clamp the new position within the boundaries
        newPosition.x = Mathf.Clamp(newPosition.x, leftBoundary, rightBoundary);

        // Update the paddle's position
        rb.MovePosition(newPosition);
    }
}
