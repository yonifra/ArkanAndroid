using UnityEngine;

public class CoinSpawner : MonoBehaviour
{
    [Header("Coin Prefab")]
    public GameObject coinPrefab; // Assign the Coin prefab from 2DRPK/Prefabs/Coin.prefab

    [Header("Spawn Settings")]
    [Range(0f, 1f)]
    public float coinDropChance = 0.7f; // 70% chance to drop a coin

    [Range(1, 5)]
    public int minCoinsPerBrick = 1;
    [Range(1, 5)]
    public int maxCoinsPerBrick = 3;

    [Header("Physics Settings")]
    public float coinSpawnForce = 2f; // Initial upward force when coin spawns
    public float horizontalSpreadRange = 1.5f; // How much coins spread horizontally
    public float verticalSpreadRange = 0.5f; // How much coins spread vertically

    [Header("Spawn Position")]
    public Vector2 spawnOffset = Vector2.zero; // Offset from brick position

    // Singleton instance for easy access
    public static CoinSpawner Instance { get; private set; }

    void Awake()
    {
        // Implement singleton pattern
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Debug.LogWarning("Multiple CoinSpawner instances detected. Using the first one.");
        }
    }

    void Start()
    {
        // Try to find coin prefab if not assigned
        if (coinPrefab == null)
        {
            Debug.LogWarning("Coin prefab not assigned to CoinSpawner. Attempting to load from Resources...");
            // Try to load from the 2DRPK/Prefabs folder
            coinPrefab = Resources.Load<GameObject>("2DRPK/Prefabs/Coin");

            if (coinPrefab == null)
            {
                Debug.LogError("Could not find Coin prefab! Please assign it manually in the inspector.");
                Debug.LogError("Path should be: Assets/2DRPK/Prefabs/Coin.prefab");
            }
            else
            {
                Debug.Log("Successfully loaded Coin prefab from Resources");
            }
        }
    }

    /// <summary>
    /// Spawns coins at the specified position (usually a destroyed brick's position)
    /// </summary>
    /// <param name="spawnPosition">World position where coins should spawn</param>
    public void SpawnCoins(Vector3 spawnPosition)
    {
        // Check if we should drop coins at all
        if (Random.Range(0f, 1f) > coinDropChance)
        {
            Debug.Log("No coins dropped this time (chance roll failed)");
            return;
        }

        // Check if coin prefab is available
        if (coinPrefab == null)
        {
            Debug.LogError("Cannot spawn coins: Coin prefab is not assigned!");
            return;
        }

        // Determine how many coins to spawn
        int coinsToSpawn = Random.Range(minCoinsPerBrick, maxCoinsPerBrick + 1);
        Debug.Log($"Spawning {coinsToSpawn} coins at position {spawnPosition}");

        // Spawn each coin with some variation
        for (int i = 0; i < coinsToSpawn; i++)
        {
            SpawnSingleCoin(spawnPosition, i, coinsToSpawn);
        }
    }

    /// <summary>
    /// Spawns a single coin with physics and variation
    /// </summary>
    /// <param name="basePosition">Base spawn position</param>
    /// <param name="coinIndex">Index of this coin (for variation)</param>
    /// <param name="totalCoins">Total number of coins being spawned</param>
    private void SpawnSingleCoin(Vector3 basePosition, int coinIndex, int totalCoins)
    {
        // Calculate spawn position with offset and some randomization
        Vector3 spawnPos = basePosition + (Vector3)spawnOffset;

        // Add some random spread so coins don't spawn in exactly the same spot
        float horizontalSpread = Random.Range(-horizontalSpreadRange, horizontalSpreadRange);
        float verticalSpread = Random.Range(-verticalSpreadRange, verticalSpreadRange);
        spawnPos += new Vector3(horizontalSpread, verticalSpread, 0);

        // Instantiate the coin
        GameObject newCoin = Instantiate(coinPrefab, spawnPos, Quaternion.identity);

        // Get the coin controller and configure it
        CoinController coinController = newCoin.GetComponent<CoinController>();
        if (coinController == null)
        {
            // Add CoinController if the prefab doesn't have it
            Debug.LogWarning("Coin prefab doesn't have CoinController. Adding one...");
            coinController = newCoin.AddComponent<CoinController>();
        }

        // Configure coin physics
        Rigidbody2D coinRb = newCoin.GetComponent<Rigidbody2D>();
        if (coinRb == null)
        {
            // Add Rigidbody2D if the prefab doesn't have it
            Debug.LogWarning("Coin prefab doesn't have Rigidbody2D. Adding one...");
            coinRb = newCoin.AddComponent<Rigidbody2D>();

            // Configure the rigidbody
            coinRb.gravityScale = 1f;
            coinRb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
            coinRb.interpolation = RigidbodyInterpolation2D.Interpolate;
            coinRb.freezeRotation = true;
        }

        // Add initial force to make coins "pop out" of the brick
        Vector2 initialForce = Vector2.zero;
        initialForce.y = coinSpawnForce;
        initialForce.x = Random.Range(-horizontalSpreadRange * 0.5f, horizontalSpreadRange * 0.5f);

        coinRb.AddForce(initialForce, ForceMode2D.Impulse);

        // Set coin name for debugging
        newCoin.name = $"Coin_{coinIndex}";

        Debug.Log($"Spawned coin {coinIndex + 1}/{totalCoins} at {spawnPos} with force {initialForce}");
    }

    /// <summary>
    /// Public method for bricks to call when they're destroyed
    /// </summary>
    /// <param name="brickPosition">Position of the destroyed brick</param>
    public static void SpawnCoinsAtPosition(Vector3 brickPosition)
    {
        if (Instance != null)
        {
            Instance.SpawnCoins(brickPosition);
        }
        else
        {
            Debug.LogError("CoinSpawner.Instance is null! Make sure CoinSpawner is in the scene.");
        }
    }

    /// <summary>
    /// Method to test coin spawning (for debugging)
    /// </summary>
    [System.Obsolete("Debug method only")]
    public void DEBUG_SpawnTestCoins()
    {
        Debug.Log("DEBUG: Spawning test coins at (0, 0, 0)");
        SpawnCoins(Vector3.zero);
    }
}