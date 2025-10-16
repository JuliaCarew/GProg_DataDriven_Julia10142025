using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.Tilemaps;

public class EnemyController : MonoBehaviour 
{
    [Header("References")]
    public LoadMap loadMap;    
    public HealthSystem healthSystemref;

    [Header("Enemy Stats")]
    public int maxHealth = 30;
    public int currentHealth;
    public int enemyDamage = 5; 
    public float tileSize = 0.08f;
    private int direction;

    [Header("Enemy Position / Tiles")]
    public TileBase enemyTile;
    public TileBase playerTile;
    public Vector3Int enemyPosition; 

    void Start()
    {
        if (loadMap.healthSystemref != null)
        {
            loadMap.healthSystemref.currentHealth = maxHealth; 
            currentHealth = maxHealth;
        }
        else
        {
            Debug.LogWarning("HealthSystem is not assigned to the EnemyController!");
        }
    }

    // ---------- INITIALIZE ENEMY ---------- //
    public void Initialize(Vector3Int position, Tilemap tilemap, LoadMap loadMapRef)
    {
        enemyPosition = position;
        loadMap.myTilemap = tilemap;
        loadMap = loadMapRef;

        // create or find HealthSystem component
        loadMap.healthSystemref = GetComponent<HealthSystem>();
        if (loadMap.healthSystemref == null)
        {
            Debug.LogError("Missing HealthSystem on EnemyController.");
        }
        
    }

    // ---------- ENEMY AI MOVEMENT ---------- //
    public bool CanMove(int x, int y) 
    {
        Vector3Int targetPosition = new Vector3Int(x, y, 0);

        // Check if the target tile is walkable (null or _none)
        TileBase tileAtTarget = loadMap.myTilemap.GetTile(targetPosition);
        if (tileAtTarget == null || tileAtTarget == loadMap._none)
        {
            return true; 
        }

        // Block movement on walls, chests, doors, and other enemy tiles
        if (tileAtTarget == loadMap._wall ||
            tileAtTarget == loadMap._door ||
            tileAtTarget == loadMap._chest ||
            tileAtTarget == enemyTile) 
        {
            return false;
        }

        // Block movement onto the player's tile
        if (tileAtTarget == playerTile)
        {
            Debug.Log("Enemy is adjacent to player.");
            return false;
        }
        return false;
    }
    public void MoveTowardsPlayer() 
    {
        Vector3 enemyWorldPos = transform.position;
        Vector3Int enemyTilePos = loadMap.myTilemap.WorldToCell(enemyWorldPos);

        Vector3 playerWorldPos = loadMap.movePlayerref.movePoint.position;
        Vector3Int playerTilePos = loadMap.myTilemap.WorldToCell(playerWorldPos);

        int deltaX = playerTilePos.x - enemyTilePos.x;
        int deltaY = playerTilePos.y - enemyTilePos.y;

        int moveX = deltaX != 0 ? (deltaX > 0 ? 1 : -1) : 0;
        int moveY = deltaY != 0 ? (deltaY > 0 ? 1 : -1) : 0;

        Vector3Int targetTilePos = enemyTilePos + new Vector3Int(moveX, moveY, 0);

        if (targetTilePos == playerTilePos)
        {
            Debug.Log("Enemy attacks the player!");
            loadMap.combatref.EnemyAttacksPlayer(enemyDamage, this);
        }
        else if (CanMove(targetTilePos.x, targetTilePos.y))
        {
            DrawEnemy(enemyTilePos.x, enemyTilePos.y, targetTilePos.x, targetTilePos.y);
        }
    }

    // ---------- DRAW ENEMY ---------- //
    void DrawEnemy(int previousX, int previousY, int targetX, int targetY)
    {
        Vector3Int previousPosition = new Vector3Int(previousX, previousY, 0);
        Vector3Int currentPosition = new Vector3Int(targetX, targetY, 0);

        // Clear the previous tile
        if (loadMap.myTilemap.HasTile(previousPosition))
        {
            loadMap.myTilemap.SetTile(previousPosition, null);
        }
        
        // Set the new tile
        loadMap.myTilemap.SetTile(currentPosition, enemyTile);
        
        // Update enemy position tracking
        enemyPosition = currentPosition;
        
        // Update GameObject transform to match the new tile position
        transform.position = loadMap.myTilemap.CellToWorld(currentPosition);
    }

    // ---------- HEALTH SYSTEM ---------- //
    public void TakeDamage(int damage)
    {
        if (healthSystemref != null)
        {
            healthSystemref.TakeDamage(damage);
            // Sync the EnemyController's currentHealth with the HealthSystem's currentHealth
            currentHealth = healthSystemref.currentHealth;
            healthSystemref.UpdateHealthUI();
            Debug.Log($"Enemy health synced: EnemyController.currentHealth = {currentHealth}, HealthSystem.currentHealth = {healthSystemref.currentHealth}");
        }
    }
    public void Die()
    {
        if (healthSystemref != null)
        {
            healthSystemref.Die(enemyPosition);
        }
        Destroy(gameObject);
    }

}
