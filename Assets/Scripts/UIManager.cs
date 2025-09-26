using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    [Header("Level Complete UI")]
    public GameObject levelCompletePanel;
    public Text levelCompleteText;
    public Text congratulationsText;
    public Button restartButton;
    public Button nextLevelButton;

    [Header("Game UI")]
    public Text bricksRemainingText;
    public GameObject gameOverPanel;

    void Start()
    {
        SetupUI();
    }

    void SetupUI()
    {
        // Create UI elements if they don't exist
        if (levelCompletePanel == null)
        {
            CreateLevelCompleteUI();
        }

        // Hide panels at start
        if (levelCompletePanel != null)
        {
            levelCompletePanel.SetActive(false);
        }

        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(false);
        }

        // Setup button functionality
        SetupButtons();
    }

    void CreateLevelCompleteUI()
    {
        Debug.Log("[UIManager] CreateLevelCompleteUI started");

        // Find or create Canvas
        Canvas canvas = FindObjectOfType<Canvas>();
        if (canvas == null)
        {
            Debug.Log("[UIManager] No Canvas found, creating new one");
            GameObject canvasGO = new GameObject("Canvas");
            canvas = canvasGO.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvasGO.AddComponent<CanvasScaler>();
            canvasGO.AddComponent<GraphicRaycaster>();
        }
        else
        {
            Debug.Log($"[UIManager] Using existing Canvas: {canvas.gameObject.name}");
        }

        // Create main panel
        GameObject panel = new GameObject("LevelCompletePanel");
        panel.transform.SetParent(canvas.transform, false);

        Image panelImage = panel.AddComponent<Image>();
        panelImage.color = new Color(0, 0, 0, 0.8f); // Semi-transparent black

        RectTransform panelRect = panel.GetComponent<RectTransform>();
        panelRect.anchorMin = Vector2.zero;
        panelRect.anchorMax = Vector2.one;
        panelRect.offsetMin = Vector2.zero;
        panelRect.offsetMax = Vector2.zero;

        levelCompletePanel = panel;
        Debug.Log("[UIManager] Level complete panel created successfully");

        // Create title text
        GameObject titleTextGO = new GameObject("TitleText");
        titleTextGO.transform.SetParent(panel.transform, false);

        Text titleText = titleTextGO.AddComponent<Text>();
        titleText.text = "LEVEL COMPLETE!";
        titleText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        titleText.fontSize = 48;
        titleText.color = Color.white;
        titleText.alignment = TextAnchor.MiddleCenter;
        titleText.fontStyle = FontStyle.Bold;

        RectTransform titleRect = titleTextGO.GetComponent<RectTransform>();
        titleRect.anchorMin = new Vector2(0, 0.7f);
        titleRect.anchorMax = new Vector2(1, 0.9f);
        titleRect.offsetMin = Vector2.zero;
        titleRect.offsetMax = Vector2.zero;

        levelCompleteText = titleText;

        // Create congratulations text
        GameObject congratsTextGO = new GameObject("CongratulationsText");
        congratsTextGO.transform.SetParent(panel.transform, false);

        Text congratsText = congratsTextGO.AddComponent<Text>();
        congratsText.text = "All bricks destroyed!\nWell done!";
        congratsText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        congratsText.fontSize = 24;
        congratsText.color = Color.yellow;
        congratsText.alignment = TextAnchor.MiddleCenter;

        RectTransform congratsRect = congratsTextGO.GetComponent<RectTransform>();
        congratsRect.anchorMin = new Vector2(0.1f, 0.5f);
        congratsRect.anchorMax = new Vector2(0.9f, 0.7f);
        congratsRect.offsetMin = Vector2.zero;
        congratsRect.offsetMax = Vector2.zero;

        congratulationsText = congratsText;

        // Create Restart button
        CreateButton("RestartButton", "RESTART LEVEL", new Vector2(0.2f, 0.2f), new Vector2(0.45f, 0.35f), panel.transform);

        // Create Next Level button
        CreateButton("NextLevelButton", "NEXT LEVEL", new Vector2(0.55f, 0.2f), new Vector2(0.8f, 0.35f), panel.transform);
    }

    void CreateButton(string name, string text, Vector2 anchorMin, Vector2 anchorMax, Transform parent)
    {
        GameObject buttonGO = new GameObject(name);
        buttonGO.transform.SetParent(parent, false);

        Image buttonImage = buttonGO.AddComponent<Image>();
        buttonImage.color = new Color(0.2f, 0.6f, 1f, 0.8f); // Light blue

        Button button = buttonGO.AddComponent<Button>();

        RectTransform buttonRect = buttonGO.GetComponent<RectTransform>();
        buttonRect.anchorMin = anchorMin;
        buttonRect.anchorMax = anchorMax;
        buttonRect.offsetMin = Vector2.zero;
        buttonRect.offsetMax = Vector2.zero;

        // Create button text
        GameObject textGO = new GameObject("Text");
        textGO.transform.SetParent(buttonGO.transform, false);

        Text buttonText = textGO.AddComponent<Text>();
        buttonText.text = text;
        buttonText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        buttonText.fontSize = 18;
        buttonText.color = Color.white;
        buttonText.alignment = TextAnchor.MiddleCenter;
        buttonText.fontStyle = FontStyle.Bold;

        RectTransform textRect = textGO.GetComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = Vector2.zero;
        textRect.offsetMax = Vector2.zero;

        // Store button references
        if (name == "RestartButton")
        {
            restartButton = button;
        }
        else if (name == "NextLevelButton")
        {
            nextLevelButton = button;
        }
    }

    void SetupButtons()
    {
        if (restartButton != null)
        {
            restartButton.onClick.RemoveAllListeners();
            restartButton.onClick.AddListener(() => {
                if (GameManager.Instance != null)
                {
                    GameManager.Instance.RestartLevel();
                }
            });
        }

        if (nextLevelButton != null)
        {
            nextLevelButton.onClick.RemoveAllListeners();
            nextLevelButton.onClick.AddListener(() => {
                if (GameManager.Instance != null)
                {
                    GameManager.Instance.NextLevel();
                }
            });
        }
    }

    public void ShowLevelComplete()
    {
        Debug.Log($"[UIManager] ShowLevelComplete called. Panel exists: {levelCompletePanel != null}");

        if (levelCompletePanel != null)
        {
            Debug.Log("[UIManager] Activating level complete panel");
            levelCompletePanel.SetActive(true);
            Debug.Log($"[UIManager] Panel activated. Active state: {levelCompletePanel.activeInHierarchy}");
        }
        else
        {
            Debug.LogError("[UIManager] Level complete panel is NULL! UI may not have been created properly.");
        }
    }

    public void HideLevelComplete()
    {
        if (levelCompletePanel != null)
        {
            levelCompletePanel.SetActive(false);
        }
    }

    public void UpdateBricksRemaining(int remaining, int total)
    {
        if (bricksRemainingText != null)
        {
            bricksRemainingText.text = $"Bricks: {remaining}/{total}";
        }
    }

    void Update()
    {
        // Update bricks remaining counter
        if (GameManager.Instance != null && bricksRemainingText != null)
        {
            UpdateBricksRemaining(GameManager.Instance.remainingBricks, GameManager.Instance.totalBricks);
        }
    }
}