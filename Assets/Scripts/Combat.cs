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
    public EnemyController enemy;

    [Header("Turn Handler")]
    public bool enemyTurn = false; // enemies take turns when true, player takes turn when false
    public float turnDelay = 0.2f;

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

        if (HasTargetNeighbor())
        {
            Debug.Log("enemy attacks player!");
            EnemyAttacksPlayer(enemy.enemyDamage);

            if (loadMap.playerHealthSystemref.currentHealth <= 0)
            {
                Debug.Log("Player defeated!");
                return;
            }
        }
        enemy.MoveTowardsPlayer();
        enemyTurn = false;
    }

    // ---------- ATTACK HANDLERS ---------- //
    public void EnemyAttacksPlayer(int enemyDamage)
    {
        if (loadMap.playerHealthSystemref != null)
        {
            loadMap.playerHealthSystemref.TakeDamage(enemyDamage);
            Debug.Log("Enemy attacks! Player takes " + enemyDamage + " damage.");
        }
    }
    public void PlayerAttacksEnemy(int playerDamage)
    {
        if (enemy != null && enemy.healthSystemref != null)
        {
            if (NextToEnemy())
            {
                enemy.healthSystemref.TakeDamage(playerDamage);
                Debug.Log("Player attacks! Enemy takes " + playerDamage + " damage.");
                
                if (enemy.healthSystemref.currentHealth <= 0)
                {
                    Debug.Log("Enemy defeated!");
                    myTilemap.SetTile(loadMap.enemyPosition, null);
                    Destroy(enemy.gameObject);
                }
            } 
        }
    }

    // ---------- CHECK NEIGHBOR ---------- //
    private bool HasTargetNeighbor() 
    {
        Vector3Int playerTilePosition = myTilemap.WorldToCell(loadMap.movePlayerref.movePoint.position);

        for (int x = -1; x <= 1; x++)
        {
            for (int y = -1; y <= 1; y++)
            {
                if (x == 0 && y == 0)
                continue;

                Vector3Int checkPosition = enemyTurn 
                    ? playerTilePosition + new Vector3Int(x, y, 0) // enemy checks near the player
                    : loadMap.enemyPosition + new Vector3Int(x, y, 0); // player checks near enemy

                // Compare the target position to the adjacent positions
                if (checkPosition == playerTilePosition && !enemyTurn)
                {
                    Debug.Log($"Player found enemy at {checkPosition}");
                    return true;
                }
                if (checkPosition == loadMap.enemyPosition && enemyTurn)
                {
                    Debug.Log($"Enemy found player at {checkPosition}");
                    return true;
                }
            }
        }
        return false;
    }
    public bool NextToEnemy()
    {
        Vector3Int playerPosition = myTilemap.WorldToCell(loadMap.movePlayerref.movePoint.position);

        for (int x = -1; x <= 1; x++)
        {
            for (int y = -1; y <= 1; y++)
            {
                if (x == 0 && y == 0)
                continue;

                Vector3Int checkPosition = playerPosition + new Vector3Int(x, y, 0);
            
                TileBase tileAtPosition = myTilemap.GetTile(checkPosition);

                Debug.Log($"Player is checking tile at ({checkPosition.x}, {checkPosition.y}): {tileAtPosition}");

                // Check if this tile is an enemy tile
                if (tileAtPosition == loadMap._enemy)
                {
                    Debug.Log("Player found enemy");
                    //PlayerAttacksEnemy(enemy.healthSystemref.playerDamage);
                    return true; 
                }
            }
        }
        return false;
    }
}
