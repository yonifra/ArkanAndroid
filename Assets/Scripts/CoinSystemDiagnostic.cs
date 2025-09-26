using UnityEngine;

/// <summary>
/// Diagnostic script to test and troubleshoot the coin spawning system.
/// Add this to any GameObject in your scene and press the keys listed below to test.
/// Remove this script once the coin system is working properly.
/// </summary>
public class CoinSystemDiagnostic : MonoBehaviour
{
    [Header("Diagnostic Controls")]
    [SerializeField] private KeyCode testSpawnKey = KeyCode.C;
    [SerializeField] private KeyCode diagnosticInfoKey = KeyCode.D;

    [Header("Test Settings")]
    [SerializeField] private Vector3 testSpawnPosition = Vector3.zero;

    void Update()
    {
        // Test coin spawning
        if (Input.GetKeyDown(testSpawnKey))
        {
            TestCoinSpawning();
        }

        // Show diagnostic information
        if (Input.GetKeyDown(diagnosticInfoKey))
        {
            ShowDiagnosticInfo();
        }
    }

    void Start()
    {
        Debug.Log("=== COIN SYSTEM DIAGNOSTIC LOADED ===");
        Debug.Log($"Press '{testSpawnKey}' to test coin spawning");
        Debug.Log($"Press '{diagnosticInfoKey}' to show diagnostic info");
        Debug.Log("=========================================");

        // Run initial diagnostic
        Invoke(nameof(ShowDiagnosticInfo), 1f); // Delay to let everything initialize
    }

    public void TestCoinSpawning()
    {
        Debug.Log("=== TESTING COIN SPAWNING ===");

        if (CoinSpawner.Instance == null)
        {
            Debug.LogError("❌ CoinSpawner.Instance is NULL!");
            Debug.LogError("SOLUTION: Create a GameObject named 'CoinSpawner' and add the CoinSpawner script component.");
            return;
        }

        Debug.Log("✅ CoinSpawner instance found, attempting to spawn coins...");
        CoinSpawner.SpawnCoinsAtPosition(testSpawnPosition);
        Debug.Log($"Coin spawn test completed at position: {testSpawnPosition}");
    }

    public void ShowDiagnosticInfo()
    {
        Debug.Log("=== COIN SYSTEM DIAGNOSTIC INFO ===");

        // Check CoinSpawner existence
        bool spawnerExists = CoinSpawner.Instance != null;
        Debug.Log($"CoinSpawner.Instance exists: {(spawnerExists ? "✅ YES" : "❌ NO")}");

        if (spawnerExists)
        {
            CoinSpawner spawner = CoinSpawner.Instance;

            // Check coin prefab assignment
            bool prefabAssigned = spawner.coinPrefab != null;
            Debug.Log($"Coin Prefab assigned: {(prefabAssigned ? "✅ YES" : "❌ NO")}");

            if (prefabAssigned)
            {
                Debug.Log($"Coin Prefab name: {spawner.coinPrefab.name}");
            }
            else
            {
                Debug.LogError("❌ Coin Prefab is not assigned to CoinSpawner!");
                Debug.LogError("SOLUTION: Drag 'Assets/2DRPK/Prefabs/Coin.prefab' to the 'Coin Prefab' field in CoinSpawner inspector.");
            }

            // Check drop chance
            Debug.Log($"Coin Drop Chance: {spawner.coinDropChance * 100f}%");
            if (spawner.coinDropChance <= 0)
            {
                Debug.LogWarning("⚠️ Coin Drop Chance is 0%! No coins will ever spawn.");
                Debug.LogWarning("SOLUTION: Set 'Coin Drop Chance' to a value between 0.1 and 1.0 in CoinSpawner inspector.");
            }

            // Check coin count settings
            Debug.Log($"Coins per brick: {spawner.minCoinsPerBrick}-{spawner.maxCoinsPerBrick}");

            // Check physics settings
            Debug.Log($"Spawn force: {spawner.coinSpawnForce}");
            Debug.Log($"Horizontal spread: {spawner.horizontalSpreadRange}");
        }
        else
        {
            Debug.LogError("❌ Cannot get detailed info - CoinSpawner instance not found!");

            // Try to find CoinSpawner component in scene
            CoinSpawner[] spawners = FindObjectsOfType<CoinSpawner>();
            Debug.Log($"CoinSpawner components found in scene: {spawners.Length}");

            if (spawners.Length == 0)
            {
                Debug.LogError("❌ No CoinSpawner component found in the scene!");
                Debug.LogError("SOLUTION: Create an empty GameObject, name it 'CoinSpawner', and add the CoinSpawner script component.");
            }
            else if (spawners.Length > 1)
            {
                Debug.LogWarning($"⚠️ Multiple CoinSpawner components found ({spawners.Length})! Only one should exist.");
            }
            else
            {
                Debug.Log("✅ CoinSpawner component found, but Instance is not set. This may be a timing issue.");
                Debug.Log("The CoinSpawner's Awake() method should set the Instance. Check for errors during startup.");
            }
        }

        // Check for coin prefab in project
        GameObject coinPrefab = Resources.Load<GameObject>("2DRPK/Prefabs/Coin");
        if (coinPrefab != null)
        {
            Debug.Log("✅ Coin prefab found in Resources folder");
        }
        else
        {
            Debug.LogWarning("⚠️ Coin prefab not found via Resources.Load. Manual assignment required.");
        }

        // Check for existing bricks
        BrickController[] bricks = FindObjectsOfType<BrickController>();
        Debug.Log($"Brick controllers found in scene: {bricks.Length}");

        Debug.Log("=================================");
    }
}

/// <summary>
/// Alternative version that extends CoinSpawner with a manual test button.
/// Add this to your CoinSpawner GameObject if you prefer inspector buttons.
/// </summary>
[System.Serializable]
public class CoinSpawnerDebugExtension : MonoBehaviour
{
    [Header("Debug Controls")]
    [SerializeField] private bool enableDebugButtons = true;

    void OnValidate()
    {
        // This runs in the editor when values change
        if (enableDebugButtons)
        {
            // Add debug info to the inspector
        }
    }

    [ContextMenu("Test Spawn Coins")]
    public void DEBUG_TestSpawnCoins()
    {
        if (Application.isPlaying)
        {
            CoinSpawner spawner = GetComponent<CoinSpawner>();
            if (spawner != null)
            {
                Debug.Log("Manual coin spawn test triggered from inspector");
                spawner.SpawnCoins(transform.position);
            }
            else
            {
                Debug.LogError("No CoinSpawner component found on this GameObject!");
            }
        }
        else
        {
            Debug.LogWarning("This test only works during Play mode");
        }
    }

    [ContextMenu("Show Diagnostic Info")]
    public void DEBUG_ShowInfo()
    {
        CoinSystemDiagnostic diagnostic = FindObjectOfType<CoinSystemDiagnostic>();
        if (diagnostic != null)
        {
            diagnostic.ShowDiagnosticInfo();
        }
        else
        {
            Debug.LogWarning("No CoinSystemDiagnostic found. Add the CoinSystemDiagnostic script to any GameObject.");
        }
    }
}