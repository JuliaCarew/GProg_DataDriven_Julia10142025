using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Tilemaps;

public class Combat : MonoBehaviour 
{
    public GameObject player;

    [Header("References")]
    public LoadMap loadMap;
    public Tilemap myTilemap;

    [Header("Turn Handler")]
    public bool enemyTurn = false; // enemies take turns when true, player takes turn when false
    public float turnDelay = 0.2f;
    
    // Data-driven settings loaded from JSON
    private GameSettings gameSettings;

    void Start()
    {
        // Load JSON data
        gameSettings = JsonDataLoader.GameSettings;
        turnDelay = gameSettings.combatSettings.turnDelay;
        
        // Subscribe to data reload events
        JsonDataLoader.OnDataReloaded += OnDataReloaded;
    }
    
    void OnDestroy()
    {
        // Unsubscribe from data reload events
        JsonDataLoader.OnDataReloaded -= OnDataReloaded;
    }
    
    void OnDataReloaded()
    {
        Debug.Log("Combat: Data reloaded, updating settings...");
        gameSettings = JsonDataLoader.GameSettings;
        turnDelay = gameSettings.combatSettings.turnDelay;
        Debug.Log($"Updated turn delay: {turnDelay}");
    }
    
    // ---------- PLAYER TURN ---------- // 
    public void PlayerCompletedAction()
    {
        if (!enemyTurn)
        {
            EnemyTurn(); // Trigger enemy turn immediately after player's action
        }
    }

    // ---------- ENEMY TURN ---------- //
    public void EnemyTurn() 
    {
        Debug.Log("Enemy's turn");

        // Get all enemies in the scene
        EnemyController[] enemies = FindObjectsOfType<EnemyController>();
        
        foreach (EnemyController enemy in enemies)
        {
            if (enemy != null)
            {
                // Check if this specific enemy can attack the player
                if (HasTargetNeighbor(enemy))
                {
                    Debug.Log("enemy attacks player!");
                    EnemyAttacksPlayer(enemy.enemyDamage, enemy);

                    if (loadMap.playerHealthSystemref.currentHealth <= 0)
                    {
                        Debug.Log("Player defeated!");
                        enemyTurn = false;
                        return;
                    }
                }
                else
                {
                    // Enemy moves towards player
                    enemy.MoveTowardsPlayer();
                }
            }
        }
        
        enemyTurn = false;
    }

    // ---------- ATTACK HANDLERS ---------- //
    public void EnemyAttacksPlayer(int enemyDamage, EnemyController attackingEnemy)
    {
        if (loadMap.playerHealthSystemref != null)
        {
            loadMap.playerHealthSystemref.TakeDamage(enemyDamage);
            Debug.Log($"Enemy {attackingEnemy.name} attacks! Player takes {enemyDamage} damage.");
        }
    }
    public void PlayerAttacksEnemy(int playerDamage)
    {
        // Get all enemies in the scene
        EnemyController[] enemies = FindObjectsOfType<EnemyController>();
        
        foreach (EnemyController enemy in enemies)
        {
            if (enemy != null && enemy.healthSystemref != null)
            {
                if (NextToEnemy(enemy))
                {
                    enemy.healthSystemref.TakeDamage(playerDamage);
                    // Update both player and enemy health UI
                    loadMap.playerHealthSystemref.UpdateHealthUI();
                    enemy.healthSystemref.UpdateHealthUI();
                    Debug.Log("Player attacks! Enemy takes " + playerDamage + " damage. Enemy health: " + enemy.healthSystemref.currentHealth);
                    
                    if (enemy.healthSystemref.currentHealth <= 0)
                    {
                        Debug.Log("Enemy defeated!");
                        // Clear the enemy tile from the tilemap
                        Vector3Int enemyTilePos = myTilemap.WorldToCell(enemy.transform.position);
                        myTilemap.SetTile(enemyTilePos, null);
                        Destroy(enemy.gameObject);
                    }
                    break; // Only attack one enemy per turn
                }
            }
        }
    }

    // ---------- CHECK NEIGHBOR ---------- //
    private bool HasTargetNeighbor(EnemyController enemy) 
    {
        Vector3Int playerTilePosition = myTilemap.WorldToCell(loadMap.movePlayerref.movePoint.position);
        Vector3Int enemyTilePosition = myTilemap.WorldToCell(enemy.transform.position);

        // Check if enemy is directly adjacent to player (not diagonal)
        int deltaX = Mathf.Abs(playerTilePosition.x - enemyTilePosition.x);
        int deltaY = Mathf.Abs(playerTilePosition.y - enemyTilePosition.y);
        
        // Check adjacency based on JSON settings
        bool isAdjacent = false;
        if (gameSettings.combatSettings.allowDiagonalAttacks)
        {
            // Allow diagonal attacks
            isAdjacent = (deltaX <= 1 && deltaY <= 1) && (deltaX + deltaY > 0);
        }
        else
        {
            // Only allow direct attacks (not diagonal)
            isAdjacent = (deltaX == 1 && deltaY == 0) || (deltaX == 0 && deltaY == 1);
        }
        
        if (isAdjacent)
        {
            // Check if there's a wall between enemy and player
            if (!IsWallBetween(enemyTilePosition, playerTilePosition))
            {
                Debug.Log($"Enemy found player at {playerTilePosition}");
                return true;
            }
        }
        
        return false;
    }
    
    // Check if there's a wall between two positions
    private bool IsWallBetween(Vector3Int pos1, Vector3Int pos2)
    {
        Vector3Int direction = pos2 - pos1;
        
        // If they're not in a straight line, no wall can be between them
        if (direction.x != 0 && direction.y != 0)
            return false;
            
        // Normalize direction
        if (direction.x != 0) direction.x = direction.x / Mathf.Abs(direction.x);
        if (direction.y != 0) direction.y = direction.y / Mathf.Abs(direction.y);
        
        // Check each tile between the two positions
        Vector3Int currentPos = pos1 + direction;
        while (currentPos != pos2)
        {
            TileBase tile = myTilemap.GetTile(currentPos);
            if (tile == loadMap._wall)
            {
                return true; // Found a wall between them
            }
            currentPos += direction;
        }
        
        return false; // No wall found
    }
    public bool NextToEnemy(EnemyController enemy)
    {
        Vector3Int playerPosition = myTilemap.WorldToCell(loadMap.movePlayerref.movePoint.position);
        Vector3Int enemyTilePosition = myTilemap.WorldToCell(enemy.transform.position);

        Debug.Log($"Checking if player at {playerPosition} is next to enemy at {enemyTilePosition}");

        // Check if player is directly adjacent to enemy (not diagonal)
        int deltaX = Mathf.Abs(playerPosition.x - enemyTilePosition.x);
        int deltaY = Mathf.Abs(playerPosition.y - enemyTilePosition.y);
        
        Debug.Log($"Delta: X={deltaX}, Y={deltaY}");
        
        // Check adjacency based on JSON settings
        bool isAdjacent = false;
        if (gameSettings.combatSettings.allowDiagonalAttacks)
        {
            // Allow diagonal attacks
            isAdjacent = (deltaX <= 1 && deltaY <= 1) && (deltaX + deltaY > 0);
        }
        else
        {
            // Only allow direct attacks (not diagonal)
            isAdjacent = (deltaX == 1 && deltaY == 0) || (deltaX == 0 && deltaY == 1);
        }
        
        if (isAdjacent)
        {
            Debug.Log("Player is adjacent to enemy, checking for walls...");
            // Check if there's a wall between player and enemy
            if (!IsWallBetween(playerPosition, enemyTilePosition))
            {
                Debug.Log("No wall between player and enemy - can attack!");
                return true;
            }
            else
            {
                Debug.Log("Wall between player and enemy - cannot attack");
            }
        }
        else
        {
            Debug.Log("Player is not adjacent to enemy");
        }
        
        return false;
    }
    
    // Overloaded method for backward compatibility with MovePlayer.cs
    public bool NextToEnemy()
    {
        // Get all enemies in the scene
        EnemyController[] enemies = FindObjectsOfType<EnemyController>();
        Debug.Log($"Found {enemies.Length} enemies to check");
        
        foreach (EnemyController enemy in enemies)
        {
            if (enemy != null)
            {
                Debug.Log($"Checking enemy: {enemy.name}");
                if (NextToEnemy(enemy))
                {
                    return true;
                }
            }
        }
        
        return false;
    }
}
