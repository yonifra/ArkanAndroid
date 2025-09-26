using UnityEngine;

public class BrickController : MonoBehaviour
{
    private bool isDestroyed = false;
    private bool isBeingHit = false; // Prevents multiple simultaneous hits
    private float lastHitTime = -1f;
    private const float HIT_COOLDOWN = 0.1f; // 100ms cooldown between hits

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Ball") && !isDestroyed && !isBeingHit)
        {
            // Check cooldown to prevent rapid double-hits
            if (Time.fixedTime - lastHitTime > HIT_COOLDOWN)
            {
                HandleBallHit(collision.gameObject, collision.contacts[0].normal);
            }
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        // Fallback for trigger collisions
        if (other.CompareTag("Ball") && !isDestroyed && !isBeingHit)
        {
            // Check cooldown to prevent rapid double-hits
            if (Time.fixedTime - lastHitTime > HIT_COOLDOWN)
            {
                // Calculate collision normal based on positions
                Vector2 collisionNormal = (transform.position - other.transform.position).normalized;
                HandleBallHit(other.gameObject, collisionNormal);
            }
        }
    }

    private void HandleBallHit(GameObject ball, Vector2 normal)
    {
        if (isDestroyed || isBeingHit) return;

        // Lock this brick from further hits
        isBeingHit = true;
        isDestroyed = true;
        lastHitTime = Time.fixedTime;

        // Get the ball controller and handle the bounce immediately
        if (ball != null)
        {
            BallController ballController = ball.GetComponent<BallController>();
            if (ballController != null)
            {
                // Force the ball to bounce immediately using the collision normal
                ballController.HandleBrickCollision(normal);

                Debug.Log($"Brick hit ONCE - Normal: {normal}, Ball deflected and brick will be destroyed");
            }
            else
            {
                Debug.LogError("Ball doesn't have BallController component!");
            }
        }

        // Destroy the brick after ensuring physics are processed
        StartCoroutine(DestroyAfterDelay());
    }

    private System.Collections.IEnumerator DestroyAfterDelay()
    {
        // Wait for physics to process
        yield return new WaitForFixedUpdate();
        yield return new WaitForFixedUpdate(); // Wait 2 physics frames to be safe

        Debug.Log($"[BrickController] Brick {gameObject.name} being destroyed");

        // Spawn coins before destroying the brick
        SpawnCoins();

        // Notify the GameManager that a brick has been destroyed
        if (GameManager.Instance != null)
        {
            Debug.Log($"[BrickController] Notifying GameManager about brick {gameObject.name} destruction");
            GameManager.Instance.OnBrickDestroyed();
        }
        else
        {
            Debug.LogError("[BrickController] GameManager.Instance is NULL! Cannot notify about brick destruction!");
            Debug.LogError("[BrickController] SOLUTION: Create an empty GameObject in your scene and add the GameManager script to it!");

            // Try to find GameManager component in scene as fallback
            GameManager foundManager = FindObjectOfType<GameManager>();
            if (foundManager != null)
            {
                Debug.LogWarning("[BrickController] Found GameManager component but Instance is not set! This suggests Awake() hasn't run yet.");
                foundManager.OnBrickDestroyed();
            }
            else
            {
                Debug.LogError("[BrickController] No GameManager component found in the scene at all!");
            }
        }

        Debug.Log($"[BrickController] Destroying GameObject {gameObject.name}");
        Destroy(gameObject);
    }

    public void DestroyBrick()
    {
        if (!isDestroyed)
        {
            HandleBallHit(null, Vector2.up); // Fallback with upward normal
        }
    }

    private void SpawnCoins()
    {
        Debug.Log($"[BrickController] Attempting to spawn coins at brick position: {transform.position}");

        // Check if CoinSpawner exists before trying to spawn
        if (CoinSpawner.Instance == null)
        {
            Debug.LogError("[BrickController] CoinSpawner.Instance is NULL! No CoinSpawner GameObject found in scene!");
            Debug.LogError("[BrickController] SOLUTION: Create an empty GameObject named 'CoinSpawner' and add the CoinSpawner script to it!");
            return;
        }

        Debug.Log("[BrickController] CoinSpawner found, calling SpawnCoinsAtPosition...");

        // Spawn coins at this brick's position using the CoinSpawner
        CoinSpawner.SpawnCoinsAtPosition(transform.position);

        Debug.Log($"[BrickController] Coin spawning request sent for position: {transform.position}");
    }
}