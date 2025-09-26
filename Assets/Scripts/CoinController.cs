using UnityEngine;

public class CoinController : MonoBehaviour
{
    [Header("Physics Settings")]
    public float fallSpeed = 3f;
    public float gravityScale = 1f;
    public float horizontalSpread = 1f; // Random horizontal movement when spawned

    [Header("Lifetime Settings")]
    public float lifeTime = 10f; // How long before auto-destruction
    public float destroyBelowY = -15f; // Y position to destroy coin if it falls too far

    private Rigidbody2D rb;
    private float spawnTime;

    void Start()
    {
        // Get components
        rb = GetComponent<Rigidbody2D>();

        // Record spawn time
        spawnTime = Time.time;

        // Setup physics
        SetupPhysics();

        // Add some random horizontal velocity for natural spread
        AddInitialVelocity();
    }

    void SetupPhysics()
    {
        if (rb == null) return;

        // Configure rigidbody for falling coins
        rb.gravityScale = gravityScale;
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        rb.interpolation = RigidbodyInterpolation2D.Interpolate;

        // Prevent rotation for clean falling animation
        rb.freezeRotation = true;

        // Apply downward velocity
        rb.linearVelocity = Vector2.down * fallSpeed;
    }

    void AddInitialVelocity()
    {
        if (rb == null) return;

        // Add random horizontal spread
        float horizontalVelocity = Random.Range(-horizontalSpread, horizontalSpread);
        Vector2 currentVelocity = rb.linearVelocity;
        rb.linearVelocity = new Vector2(horizontalVelocity, currentVelocity.y);
    }

    void Update()
    {
        // Check if coin should be destroyed
        CheckDestruction();

        // Ensure coin keeps falling at proper speed
        MaintainFallSpeed();
    }

    void CheckDestruction()
    {
        // Destroy if too old
        if (Time.time - spawnTime > lifeTime)
        {
            DestroyCoin();
            return;
        }

        // Destroy if fallen too far
        if (transform.position.y < destroyBelowY)
        {
            DestroyCoin();
            return;
        }
    }

    void MaintainFallSpeed()
    {
        if (rb == null) return;

        // Ensure coin maintains minimum fall speed
        if (rb.linearVelocity.y > -fallSpeed * 0.5f)
        {
            Vector2 velocity = rb.linearVelocity;
            velocity.y = -fallSpeed;
            rb.linearVelocity = velocity;
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        // Handle collision with player paddle (optional - for collecting coins)
        if (other.CompareTag("Player"))
        {
            CollectCoin();
        }
        // Handle collision with bottom boundary
        else if (other.CompareTag("BottomBoundry"))
        {
            DestroyCoin();
        }
    }

    void CollectCoin()
    {
        Debug.Log("Coin collected!");

        // Play collection sound if available
        AudioSource audioSource = GetComponent<AudioSource>();
        if (audioSource != null)
        {
            audioSource.Play();
        }

        // TODO: Integrate with scoring system
        // Example: GameManager.Instance.AddScore(10);
        // Example: UIManager.Instance.UpdateCoinCount(1);

        // TODO: Add particle effect for coin collection
        // Example: Instantiate collection particle effect

        DestroyCoin();
    }

    void DestroyCoin()
    {
        // Add destruction effect here if desired
        // Example: fade out animation, particle effect

        Destroy(gameObject);
    }

    // Public method to set custom fall speed when spawning
    public void SetFallSpeed(float speed)
    {
        fallSpeed = speed;
        if (rb != null)
        {
            Vector2 velocity = rb.linearVelocity;
            velocity.y = -fallSpeed;
            rb.linearVelocity = velocity;
        }
    }

    // Public method to set custom horizontal spread
    public void SetHorizontalSpread(float spread)
    {
        horizontalSpread = spread;
        AddInitialVelocity();
    }
}