# Coin Drop System Setup Instructions

## Overview

This system adds falling coins when bricks are destroyed in your Arkanoid game. Coins will spawn from destroyed bricks and fall down with physics.

## Setup Steps

### 1. Create CoinSpawner GameObject

1. In Unity, create an empty GameObject in your scene
2. Name it "CoinSpawner"
3. Add the `CoinSpawner` script component to it
4. In the inspector, assign the Coin prefab from `Assets/2DRPK/Prefabs/Coin.prefab` to the "Coin Prefab" field

### 2. Configure Coin Prefab

The coin prefab needs some components to work properly:

1. Open the Coin prefab from `Assets/2DRPK/Prefabs/Coin.prefab`
2. Make sure it has these components:
   - **Rigidbody2D**: For physics (gravity, falling)
     - Set Gravity Scale to 1
     - Set Collision Detection to Continuous
     - Enable Freeze Rotation (Z) to prevent spinning
   - **Collider2D**: For collision detection (BoxCollider2D or CircleCollider2D)
     - Set as Trigger if you want coins to pass through objects
   - **CoinController**: The script we created (should be added automatically)
   - **Animator**: Should already be there for the coin animation

### 3. Set Up Coin Collection (Optional)

If you want players to collect coins:

1. Make sure your Player paddle has the "Player" tag
2. The coins will automatically detect collision with the player and disappear
3. You can extend the `CollectCoin()` method in `CoinController.cs` to add:
   - Score increase
   - Sound effects
   - Particle effects
   - UI updates

### 4. Configure CoinSpawner Settings

In the CoinSpawner inspector, you can adjust:

- **Coin Drop Chance**: Probability (0-1) that a brick will drop coins
- **Min/Max Coins Per Brick**: How many coins each brick can drop
- **Coin Spawn Force**: Initial upward velocity of coins
- **Horizontal/Vertical Spread**: How much coins spread out when spawned
- **Spawn Offset**: Position offset from brick center

## How It Works

1. When a brick is hit by the ball, `BrickController.HandleBallHit()` is called
2. The brick starts its destruction sequence with `DestroyAfterDelay()`
3. Before destroying, it calls `SpawnCoins()` which uses `CoinSpawner.SpawnCoinsAtPosition()`
4. The CoinSpawner creates coin instances with random spread and physics
5. Each coin falls down due to gravity and can be collected by the player
6. Coins auto-destroy after a timeout or if they fall too far down

## Customization Options

### Coin Physics

Edit `CoinController.cs` to adjust:

- `fallSpeed`: How fast coins fall
- `gravityScale`: Gravity strength
- `horizontalSpread`: Random horizontal movement
- `lifeTime`: How long coins exist before auto-destruction

### Spawn Behavior

Edit `CoinSpawner.cs` to adjust:

- Drop chance percentage
- Number of coins per brick
- Initial spawn forces and spread
- Spawn position offsets

### Visual Effects

You can enhance the system by adding:

- Particle effects when coins spawn
- Sound effects for coin drops and collection
- Glowing or sparkle effects on coins
- Screen shake when coins drop

## Testing

1. Run the game in Unity
2. Hit bricks with the ball
3. Watch for coins to spawn and fall from destroyed bricks
4. Check the Console for debug messages about coin spawning

## Troubleshooting

**No coins appear:**

- Check that CoinSpawner exists in scene and has Coin Prefab assigned
- Verify Coin Drop Chance is > 0
- Check Console for error messages

**Coins don't fall:**

- Ensure Coin prefab has Rigidbody2D with Gravity Scale > 0
- Check that CoinController script is attached

**Coins spawn in wrong position:**

- Adjust Spawn Offset in CoinSpawner settings
- Check that brick positions are correct

**Performance issues:**

- Reduce Max Coins Per Brick
- Reduce Coin Drop Chance
- Decrease coin Life Time for faster cleanup
