using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public float speed = 10f;   // player speed
    private Rigidbody2D rb2d;   // rigidbody component
    private Vector2 touchPos;   // touch position
    private bool isDragging;    // is the player being dragged?
    private Vector2 playerSize; // player size
    private Vector3 screenBounds; // screen bounds

    void Start()
    {
        // get the Rigidbody2D component
        rb2d = GetComponent<Rigidbody2D>();
        // get the size of the player sprite
        playerSize = GetComponent<SpriteRenderer>().bounds.size;
        
        // get the screen bounds
        screenBounds = Camera.main.ScreenToWorldPoint(new Vector3(Screen.width, Screen.height, Camera.main.transform.position.z));
    }

    void Update()
    {
        // move the player based on touch input or keyboard input
        if (Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);
            if (touch.phase == TouchPhase.Moved)
            {
                // get the touch position and convert to world space
                Vector2 touchPosition = Camera.main.ScreenToWorldPoint(touch.position);
                // set the player position to the touch position
                transform.position = new Vector3(touchPosition.x, transform.position.y, transform.position.z);
            }
        }
        else
        {
            // move the player using keyboard input
            float move = Input.GetAxis("Horizontal");
            transform.position += new Vector3(move * speed * Time.deltaTime, 0, 0);
        }

    
        
        
        // limit the player's position to within the screen bounds
        // calculate the minimum and maximum x coordinates of the screen bounds
        float screenMinX = -screenBounds.x;
        float screenMaxX = screenBounds.x;

        float xPosition = Mathf.Clamp(transform.position.x, screenMinX + playerSize.x / 2, screenMaxX - playerSize.x / 2);
        transform.position = new Vector3(xPosition, transform.position.y, transform.position.z);
        
        // print out the player's position for debugging
        // Debug.Log("Player position: " + transform.position);
    }


    void FixedUpdate()
    {
        // move the player based on touch input
        if (isDragging)
        {
            Vector2 newPos = Vector2.MoveTowards(rb2d.position, touchPos, speed * Time.fixedDeltaTime);
            rb2d.MovePosition(newPos);
        }
    }
}
