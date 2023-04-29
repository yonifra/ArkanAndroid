using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BallController : MonoBehaviour
{
    public float moveSpeed = 10f; // speed of the ball
    public float maxSpeed = 20f; // maximum speed of the ball
    public float minAngle = 15f; // minimum angle of deflection when hitting the player
    public float maxAngle = 75f; // maximum angle of deflection when hitting the player
    public float randomAngleRange = 15f; // range of random angle added to the deflection angle when hitting the player

    private Rigidbody2D rb; // rigidbody component of the ball
    private Vector2 direction; // current direction of the ball's movement

    void Start()
    {
        // get the rigidbody component
        rb = GetComponent<Rigidbody2D>();

        // set the initial direction of the ball to move up and to the right
        direction = new Vector2(1, 1).normalized;
    }

    void Update()
    {
        // move the ball based on its direction and speed
        Vector2 velocity = direction * moveSpeed;
        rb.velocity = velocity;

        // clamp the speed of the ball to the maximum speed
        if (rb.velocity.magnitude > maxSpeed)
        {
            rb.velocity = rb.velocity.normalized * maxSpeed;
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
            float angle = Mathf.Abs(contactPoint.x - playerPosition.x) / collision.collider.bounds.size.x * (maxAngle - minAngle) + minAngle;

            // add a random angle to the deflection angle
            float randomAngle = Random.Range(-randomAngleRange, randomAngleRange);
            angle += randomAngle;

            // calculate the new direction of the ball's movement based on the deflection angle
            direction = new Vector2(Mathf.Cos(angle * Mathf.Deg2Rad), Mathf.Sin(angle * Mathf.Deg2Rad)).normalized;
        }
        // if the ball hits a brick, destroy the brick and deflect the ball
        else if (collision.gameObject.CompareTag("Brick"))
        {
            Vector2 contactPoint = collision.GetContact(0).point;
            Vector2 ballPosition = transform.position;
            float collisionAngle = Vector2.Angle(contactPoint - ballPosition, Vector2.up);
            float xDirection = (contactPoint.x > ballPosition.x) ? 1 : -1;

            // Adjust the direction of the ball based on the angle of the collision
            Vector2 newDirection = new Vector2(Mathf.Sin(collisionAngle * Mathf.Deg2Rad), Mathf.Cos(collisionAngle * Mathf.Deg2Rad));
            if (newDirection.x > 0)
            {
                newDirection.x = Mathf.Clamp(newDirection.x, 0.5f, 1);
            }
            else
            {
                newDirection.x = Mathf.Clamp(newDirection.x, -1, -0.5f);
            }

            // Update the velocity of the ball
            GetComponent<Rigidbody2D>().velocity = newDirection * moveSpeed * xDirection;
        
            // Destroy the brick
            BrickController brickController = collision.gameObject.GetComponent<BrickController>();
            brickController.DestroyBrick();
            DeflectBall(collision.contacts[0].normal);
        }
        // if the ball hits any other object, deflect it based on the surface normal
        else
        {
            DeflectBall(collision.contacts[0].normal);
        }

        // play a sound effect when the ball hits something
        // AudioManager.Instance.PlaySoundEffect("Hit");
    }

    private void DeflectBall(Vector2 normal)
    {
        // reflect the direction of the ball's movement based on the surface normal
        direction = Vector2.Reflect(direction, normal).normalized;
    }
}
