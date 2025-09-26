using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class GameManager : MonoBehaviour
{
    [Header("Game State")]
    public int totalBricks = 0;
    public int remainingBricks = 0;
    public bool isGameActive = true;
    public bool isLevelComplete = false;

    [Header("UI References")]
    public UIManager uiManager;

    [Header("Game Objects")]
    public BallController ballController;

    // Singleton instance for easy access
    public static GameManager Instance { get; private set; }

    void Awake()
    {
        // Implement singleton pattern
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        InitializeGame();
    }

    void InitializeGame()
    {
        Debug.Log("[GameManager] InitializeGame started");

        // Count all bricks in the scene
        CountBricks();

        // Find UI manager if not assigned
        if (uiManager == null)
        {
            Debug.Log("[GameManager] UIManager not assigned, searching scene...");
            uiManager = FindObjectOfType<UIManager>();
            if (uiManager != null)
            {
                Debug.Log($"[GameManager] Found UIManager: {uiManager.gameObject.name}");
            }
            else
            {
                Debug.LogWarning("[GameManager] No UIManager found in scene! Level completion UI will not work properly.");
            }
        }
        else
        {
            Debug.Log($"[GameManager] UIManager already assigned: {uiManager.gameObject.name}");
        }

        // Find ball controller if not assigned
        if (ballController == null)
        {
            Debug.Log("[GameManager] BallController not assigned, searching scene...");
            ballController = FindObjectOfType<BallController>();
            if (ballController != null)
            {
                Debug.Log($"[GameManager] Found BallController: {ballController.gameObject.name}");
            }
            else
            {
                Debug.LogWarning("[GameManager] No BallController found in scene!");
            }
        }
        else
        {
            Debug.Log($"[GameManager] BallController already assigned: {ballController.gameObject.name}");
        }

        isGameActive = true;
        isLevelComplete = false;

        Debug.Log($"[GameManager] Game initialized - Total bricks: {totalBricks}, UIManager: {uiManager != null}, BallController: {ballController != null}");
    }

    void CountBricks()
    {
        // Find all brick controllers in the scene
        BrickController[] bricks = FindObjectsOfType<BrickController>();
        totalBricks = bricks.Length;
        remainingBricks = totalBricks;

        Debug.Log($"[GameManager] CountBricks: Found {totalBricks} bricks in the scene");

        // Log each brick for debugging
        for (int i = 0; i < bricks.Length; i++)
        {
            Debug.Log($"[GameManager] Brick {i + 1}: {bricks[i].gameObject.name} at position {bricks[i].transform.position}");
        }

        if (totalBricks == 0)
        {
            Debug.LogWarning("[GameManager] WARNING: No bricks found! Make sure bricks have BrickController component.");
        }
    }

    public void OnBrickDestroyed()
    {
        Debug.Log($"[GameManager] OnBrickDestroyed called. Game active: {isGameActive}, Level complete: {isLevelComplete}");

        if (!isGameActive || isLevelComplete)
        {
            Debug.Log("[GameManager] OnBrickDestroyed ignored - game not active or level already complete");
            return;
        }

        remainingBricks--;
        Debug.Log($"[GameManager] Brick destroyed! Remaining: {remainingBricks}/{totalBricks}");

        // Check if all bricks are destroyed
        if (remainingBricks <= 0)
        {
            Debug.Log("[GameManager] All bricks destroyed! Calling CompleteLevel()");
            CompleteLevel();
        }
        else
        {
            Debug.Log($"[GameManager] Still {remainingBricks} bricks remaining");
        }
    }

    void CompleteLevel()
    {
        if (isLevelComplete)
        {
            Debug.Log("[GameManager] CompleteLevel called but level already complete - ignoring");
            return; // Prevent multiple calls
        }

        Debug.Log("[GameManager] *** LEVEL COMPLETING NOW! ***");

        isLevelComplete = true;
        isGameActive = false;

        Debug.Log($"[GameManager] Level completed! Game state - Active: {isGameActive}, Complete: {isLevelComplete}");

        // Stop the ball
        if (ballController != null)
        {
            Debug.Log("[GameManager] Starting ball slowdown coroutine");
            StartCoroutine(StopBallGradually());
        }
        else
        {
            Debug.LogWarning("[GameManager] No ball controller found - cannot stop ball");
        }

        // Show level complete UI
        Debug.Log("[GameManager] Calling ShowLevelCompleteUI()");
        ShowLevelCompleteUI();
    }

    IEnumerator StopBallGradually()
    {
        // Gradually slow down the ball for a nice effect
        Rigidbody2D ballRb = ballController.GetComponent<Rigidbody2D>();
        if (ballRb != null)
        {
            Vector2 originalVelocity = ballRb.linearVelocity;
            float slowdownTime = 1.5f;
            float elapsed = 0f;

            while (elapsed < slowdownTime)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / slowdownTime;
                ballRb.linearVelocity = Vector2.Lerp(originalVelocity, Vector2.zero, t);
                yield return null;
            }

            ballRb.linearVelocity = Vector2.zero;
        }
    }

    void ShowLevelCompleteUI()
    {
        Debug.Log($"[GameManager] ShowLevelCompleteUI called. UIManager found: {uiManager != null}");

        if (uiManager != null)
        {
            Debug.Log("[GameManager] Calling uiManager.ShowLevelComplete()");
            uiManager.ShowLevelComplete();
        }
        else
        {
            Debug.LogWarning("[GameManager] UIManager not found! Creating basic UI message...");
            // Fallback: Create a simple on-screen message
            GameObject textGO = new GameObject("LevelCompleteText");
            var text = textGO.AddComponent<UnityEngine.UI.Text>();
            text.text = "LEVEL COMPLETE!\nAll bricks destroyed!";
            text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            text.fontSize = 36;
            text.color = Color.white;
            text.alignment = TextAnchor.MiddleCenter;

            // Position it in the center of the screen
            var canvas = FindObjectOfType<Canvas>();
            if (canvas == null)
            {
                Debug.Log("[GameManager] Creating new Canvas for fallback UI");
                var canvasGO = new GameObject("Canvas");
                canvas = canvasGO.AddComponent<Canvas>();
                canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            }
            else
            {
                Debug.Log("[GameManager] Using existing Canvas for fallback UI");
            }
            textGO.transform.SetParent(canvas.transform, false);
            Debug.Log("[GameManager] Fallback UI message created successfully");
        }
    }

    public void RestartLevel()
    {
        Debug.Log("Restarting level...");

        // Reset game state
        isGameActive = true;
        isLevelComplete = false;

        // Hide UI
        if (uiManager != null)
        {
            uiManager.HideLevelComplete();
        }

        // Reload the current scene
        UnityEngine.SceneManagement.SceneManager.LoadScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene().name);
    }

    public void NextLevel()
    {
        Debug.Log("Loading next level...");
        // For now, just restart the same level
        // In a full game, you would load the next scene/level here
        RestartLevel();
    }

    // Method to pause/resume the game
    public void PauseGame()
    {
        Time.timeScale = 0f;
        isGameActive = false;
    }

    public void ResumeGame()
    {
        Time.timeScale = 1f;
        isGameActive = true;
    }

    // Method to check if game should continue (useful for ball controller)
    public bool IsGameActive()
    {
        return isGameActive && !isLevelComplete;
    }

    // Debug method to manually trigger level completion
    [System.Obsolete("Debug method only")]
    public void DEBUG_CompleteLevel()
    {
        Debug.Log("[GameManager] DEBUG_CompleteLevel called manually");
        remainingBricks = 0;
        CompleteLevel();
    }

    // Debug method to check current game state
    public void DEBUG_LogGameState()
    {
        Debug.Log($"[GameManager] DEBUG - Total bricks: {totalBricks}, Remaining: {remainingBricks}, Game active: {isGameActive}, Level complete: {isLevelComplete}");
        Debug.Log($"[GameManager] DEBUG - UIManager found: {uiManager != null}, BallController found: {ballController != null}");
    }

    void OnDestroy()
    {
        // Cleanup is handled by UIManager
    }
}