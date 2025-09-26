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
    private float lastBrickHitTime = -1f; // Track when we last hit a brick

    void Start()
    {
        // get the rigidbody component
        rb = GetComponent<Rigidbody2D>();

        audioSource = GetComponent<AudioSource>();

        // Enhanced collision detection settings for fast-moving objects
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        rb.interpolation = RigidbodyInterpolation2D.Interpolate;

        // Ensure the ball doesn't lose velocity due to physics
        rb.gravityScale = 0f;
        rb.linearDamping = 0f;
        rb.angularDamping = 0f;

        // Ensure the ball doesn't go to sleep during gameplay
        rb.sleepMode = RigidbodySleepMode2D.NeverSleep;

        Debug.Log("Ball initialized with Continuous collision detection");
    }

    void FixedUpdate()  // Changed from Update to FixedUpdate for physics
    {
        // Check if the game is still active
        if (GameManager.Instance != null && !GameManager.Instance.IsGameActive())
        {
            return; // Don't update ball physics if game is not active
        }

        // Ensure the ball maintains consistent speed and direction
        Vector2 currentVelocity = rb.linearVelocity;

        // If velocity is significantly different from intended direction, correct it
        if (Vector2.Angle(currentVelocity.normalized, direction) > 5f ||
            Mathf.Abs(currentVelocity.magnitude - moveSpeed) > 0.5f)
        {
            rb.linearVelocity = direction * moveSpeed;
        }

        // Clamp the velocity to prevent excessive speed (lower max speed to prevent boundary pass-through)
        float safeMaxSpeed = Mathf.Min(maxSpeed, 12f); // Reduce max speed for better collision detection
        if (rb.linearVelocity.magnitude > safeMaxSpeed)
        {
            direction = rb.linearVelocity.normalized;
            rb.linearVelocity = direction * safeMaxSpeed;
        }

        // Prevent the ball from getting stuck by ensuring minimum speed
        if (rb.linearVelocity.magnitude < moveSpeed * 0.8f)
        {
            rb.linearVelocity = direction * moveSpeed;
        }

        // Safety check: Detect if ball might be stuck in an object
        // Cast a ray in the direction of movement to detect potential collisions
        RaycastHit2D hit = Physics2D.Raycast(transform.position, direction, 0.5f);
        if (hit.collider != null && hit.collider.CompareTag("Brick"))
        {
            // If we're about to hit a brick but moving too fast, we might pass through
            if (rb.linearVelocity.magnitude > moveSpeed * 1.2f)
            {
                Debug.Log("High speed detected near brick - correcting velocity");
                rb.linearVelocity = direction * moveSpeed;
            }
        }

        // Aggressive boundary clamping - absolutely prevent ball from leaving area
        ClampBallPosition();

        // Check for boundary violations and correct position
        CheckBoundaryViolations();

        // Predictive boundary checking to catch fast-moving balls
        CheckPredictiveBoundaryCollision();

        // Final failsafe - absolutely prevent ball from escaping horizontally
        Vector3 currentPos = transform.position;
        if (currentPos.x < -8.2f || currentPos.x > 9.5f)
        {
            Debug.LogWarning($"EMERGENCY: Ball escaped boundaries at {currentPos.x}! Forcing back inside.");
            currentPos.x = Mathf.Clamp(currentPos.x, -8.2f, 9.5f);
            transform.position = currentPos;

            // Force ball to bounce away from boundary
            if (currentPos.x <= -8.2f && direction.x < 0)
                direction.x = Mathf.Abs(direction.x);
            else if (currentPos.x >= 9.5f && direction.x > 0)
                direction.x = -Mathf.Abs(direction.x);

            rb.linearVelocity = direction * moveSpeed;
        }

        // Check if the ball is gone (below a certain point)
        if (transform.position.y < -10)
        {
            RestartGame();
        }
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        // Ensure we have valid collision data
        if (collision.contacts == null || collision.contacts.Length == 0)
            return;

        Vector2 collisionNormal = collision.contacts[0].normal;

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
            // Apply the new velocity immediately to ensure responsive physics
            rb.linearVelocity = direction * moveSpeed;
        }
        // Handle left and right boundary collisions specifically
        else if (collision.gameObject.CompareTag("LeftBoundary") || collision.gameObject.CompareTag("RightBoundary"))
        {
            // Stop the ball's horizontal movement and bounce it back
            HandleBoundaryCollision(collisionNormal);
        }
        // Brick collisions are COMPLETELY handled by BrickController
        // Ball ignores brick collisions to prevent double processing
        else if (!collision.gameObject.CompareTag("Brick"))
        {
            // Handle collisions with walls, boundaries, and other objects
            DeflectBall(collisionNormal);
            rb.linearVelocity = direction * moveSpeed;
        }
        // Note: Brick collisions are ignored here - they're handled by BrickController only

        // play a sound effect when the ball hits something
        if (audioSource != null)
        {
            audioSource.Play();
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.CompareTag("BottomBoundry"))
        {
            // Check if the game is still active (don't trigger game over if level is complete)
            if (GameManager.Instance != null && GameManager.Instance.IsGameActive())
            {
                // ball hit the bottom boundary, end the game
                Debug.Log("Game over!");
                audioSource.Play();
                // you can add additional game over logic here, such as showing a game over screen or resetting the game
            }
        }
        else if (other.gameObject.CompareTag("LeftBoundary") || other.gameObject.CompareTag("RightBoundary"))
        {
            // Handle boundary collision for trigger-based boundaries
            Vector2 normal;
            if (other.gameObject.CompareTag("LeftBoundary"))
                normal = Vector2.right; // Left boundary, bounce right
            else
                normal = Vector2.left;  // Right boundary, bounce left

            HandleBoundaryCollision(normal);
        }
    }

    private void DeflectBall(Vector2 normal)
    {
        // Ensure the normal is valid and normalized
        if (normal.magnitude < 0.1f)
        {
            // Fallback to a default upward normal if collision normal is invalid
            normal = Vector2.up;
        }
        else
        {
            normal = normal.normalized;
        }

        // reflect the direction of the ball's movement based on the surface normal
        Vector2 newDirection = Vector2.Reflect(direction, normal).normalized;

        // Ensure the new direction isn't too shallow (prevents near-horizontal bounces)
        if (Mathf.Abs(newDirection.y) < 0.1f)
        {
            newDirection.y = newDirection.y > 0 ? 0.1f : -0.1f;
            newDirection = newDirection.normalized;
        }

        direction = newDirection;
    }

    private void HandleBoundaryCollision(Vector2 normal)
    {
        // Immediately stop the ball to prevent pass-through
        rb.linearVelocity = Vector2.zero;

        // Ensure the normal is valid and normalized
        if (normal.magnitude < 0.1f)
        {
            // If we don't have a good normal, determine it from ball position
            if (transform.position.x > 0)
                normal = Vector2.left;  // Right boundary, bounce left
            else
                normal = Vector2.right; // Left boundary, bounce right
        }
        else
        {
            normal = normal.normalized;
        }

        // Move ball away from boundary immediately
        Vector3 pos = transform.position;
        if (normal.x > 0) // Left boundary collision
        {
            pos.x = -8.2f; // Move to safe position inside left boundary
            direction.x = Mathf.Abs(direction.x); // Ensure rightward movement
        }
        else if (normal.x < 0) // Right boundary collision
        {
            pos.x = 9.5f;  // Move to safe position inside right boundary
            direction.x = -Mathf.Abs(direction.x); // Ensure leftward movement
        }

        // Ensure we have proper vertical movement
        if (Mathf.Abs(direction.y) < 0.3f)
        {
            direction.y = direction.y > 0 ? 0.3f : -0.3f;
        }

        // Normalize and apply the new direction
        direction = direction.normalized;
        transform.position = pos;

        // Apply velocity after a small delay to ensure position is set
        rb.linearVelocity = direction * moveSpeed;

        Debug.Log($"Boundary collision: Ball moved to {pos.x}, new direction: {direction}");
    }    // Public method for brick to call when handling collision
    public void HandleBrickCollision(Vector2 collisionNormal)
    {
        // Prevent rapid successive brick hits (should not happen with new system, but extra safety)
        if (Time.fixedTime - lastBrickHitTime < 0.05f)
        {
            Debug.Log("Ignoring rapid successive brick hit");
            return;
        }

        lastBrickHitTime = Time.fixedTime;

        // Store previous direction for comparison
        Vector2 previousDirection = direction;

        // Immediately deflect the ball
        DeflectBall(collisionNormal);

        // Apply the new velocity immediately to ensure the ball bounces
        rb.linearVelocity = direction * moveSpeed;

        // Play sound if available
        if (audioSource != null)
        {
            audioSource.Play();
        }

        Debug.Log($"Ball bounced: Previous dir = {previousDirection}, New dir = {direction}, Velocity = {rb.linearVelocity}");
    }

    private void ClampBallPosition()
    {
        // Hard boundaries that the ball absolutely cannot cross
        float leftBoundary = -8.2f;   // Well inside the Left Wall
        float rightBoundary = 9.5f;   // Well inside the Right Wall

        Vector3 pos = transform.position;
        bool positionChanged = false;

        // Clamp horizontal position
        if (pos.x < leftBoundary)
        {
            pos.x = leftBoundary;
            positionChanged = true;
            // Force ball to bounce right if moving left
            if (direction.x < 0)
            {
                direction.x = -direction.x;
                rb.linearVelocity = direction * moveSpeed;
            }
            Debug.Log("Ball clamped at left boundary");
        }
        else if (pos.x > rightBoundary)
        {
            pos.x = rightBoundary;
            positionChanged = true;
            // Force ball to bounce left if moving right
            if (direction.x > 0)
            {
                direction.x = -direction.x;
                rb.linearVelocity = direction * moveSpeed;
            }
            Debug.Log("Ball clamped at right boundary");
        }

        if (positionChanged)
        {
            transform.position = pos;
        }
    }

    private void CheckBoundaryViolations()
    {
        // Define the game area boundaries based on actual wall positions
        float leftBoundary = -8.3f;   // Slightly inside the Left Wall position (-8.57)
        float rightBoundary = 9.6f;   // Slightly inside the Right Wall position (9.9)

        Vector3 pos = transform.position;
        bool correctionNeeded = false;

        // Check left boundary - be more aggressive about catching violations
        if (pos.x <= leftBoundary)
        {
            pos.x = leftBoundary + 0.1f; // Move ball inside the boundary
            if (direction.x < 0) // Only reverse if moving toward boundary
            {
                direction.x = Mathf.Abs(direction.x); // Force rightward movement
            }
            correctionNeeded = true;
            Debug.Log("Ball position corrected at left boundary");
        }

        // Check right boundary - be more aggressive about catching violations
        if (pos.x >= rightBoundary)
        {
            pos.x = rightBoundary - 0.1f; // Move ball inside the boundary
            if (direction.x > 0) // Only reverse if moving toward boundary
            {
                direction.x = -Mathf.Abs(direction.x); // Force leftward movement
            }
            correctionNeeded = true;
            Debug.Log("Ball position corrected at right boundary");
        }

        if (correctionNeeded)
        {
            transform.position = pos;
            direction = direction.normalized;
            rb.linearVelocity = direction * moveSpeed;
        }
    }

    private void CheckPredictiveBoundaryCollision()
    {
        // Cast a ray to predict where the ball will be in the next frame
        Vector2 currentPos = transform.position;
        Vector2 predictedPos = currentPos + (rb.linearVelocity * Time.fixedDeltaTime);

        float leftBoundary = -8.3f;
        float rightBoundary = 9.6f;

        // Check if ball will cross left boundary
        if (currentPos.x > leftBoundary && predictedPos.x <= leftBoundary && rb.linearVelocity.x < 0)
        {
            Debug.Log("Predicted left boundary collision - forcing bounce");
            HandleBoundaryCollision(Vector2.right);
        }
        // Check if ball will cross right boundary
        else if (currentPos.x < rightBoundary && predictedPos.x >= rightBoundary && rb.linearVelocity.x > 0)
        {
            Debug.Log("Predicted right boundary collision - forcing bounce");
            HandleBoundaryCollision(Vector2.left);
        }
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
