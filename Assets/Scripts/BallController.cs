using UnityEngine;

public class BallController : MonoBehaviour
{
    public float speed = 10f;                   // ball speed
    public Vector2 startDirection = Vector2.up; // starting direction of the ball
    private Rigidbody2D rb2d;                   // rigidbody component
    public float hitFactor = 0.5f;              // hit factor

    void Start()
    {
        // get the Rigidbody2D component
        rb2d = GetComponent<Rigidbody2D>();
        
        // set the ball's velocity to the starting direction
        startDirection.Normalize();
        rb2d.velocity = startDirection * speed;
    }

    void FixedUpdate()
    {
        // limit the ball's maximum speed
        if (rb2d.velocity.magnitude > speed)
        {
            rb2d.velocity = rb2d.velocity.normalized * speed;
        }
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        // calculate the new direction for the ball
        Vector2 newDirection = Vector2.zero;
        if (collision.gameObject.CompareTag("Player"))
        {
            // ball hit the player, bounce up
            newDirection = Vector2.up;
            // add horizontal velocity based on where the ball hit the paddle
            float hitPosition = (collision.transform.position.x - transform.position.x) / collision.collider.bounds.size.x;
            newDirection.x = hitPosition * hitFactor;
        }
        else if (collision.gameObject.CompareTag("Brick"))
        {
            // ball hit a brick, bounce off
            newDirection = rb2d.velocity.normalized;
            Destroy(collision.gameObject);
        }
        else
        {
            // ball hit a wall, bounce off
            newDirection = Vector2.Reflect(rb2d.velocity.normalized, collision.contacts[0].normal);
        }

        // apply the new direction to the ball's velocity
        rb2d.velocity = newDirection * speed;
    }
    
    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.CompareTag("BottomBoundry"))
        {
            // ball hit the bottom boundary, end the game
            Debug.Log("Game over!");
            // you can add additional game over logic here, such as showing a game over screen or resetting the game
        }
    }

}