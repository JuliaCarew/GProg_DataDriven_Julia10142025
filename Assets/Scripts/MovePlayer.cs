using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class MovePlayer : MonoBehaviour
{
    [Header("References")]
    public LoadMap loadMap;
    public Combat combat;
    public Tilemap myTilemap;
    public SceneLoader sceneLoader;

    [Header("Transform & GameObjects")]
    public Transform movePoint;
    public GameObject playerSpawnPoint;
    public TileBase borderTile;

    public float tileSize = 0.08f;
    public TileBase playerTile; // updating the player sprite
    public TileBase enemyTile;

    void Start()
    {
        movePoint.parent = null;  // allows movepoint to dictate player's direction/ can be moved on it's own    
        ResetPosition();
    }
    void Update()
    {
        if (combat.enemyTurn == true)
        {
            return;
        }
        MovePosition();
    }

    // ---------- CHECK MOVE TILE ---------- //
    public bool CanMove(int x, int y)
    {
        // setting new variable to determine current position
        Vector3Int cellPosition = new Vector3Int(x, y, 0);      
        // Get the tile at the specified grid position
        TileBase tileAtPosition = myTilemap.GetTile(cellPosition);

        Debug.Log($"Player is checking tile at ({x}, {y}): {tileAtPosition}");

        // Allow movement if the tile is null (empty) or is explicitly _none
        if (tileAtPosition == null || tileAtPosition == loadMap._none)
        {
            Debug.Log("Player can walk");
            return true; 
        }

        // cannot move on wall, chest, door, or enemy tiles
        if (tileAtPosition == loadMap._wall ||
            tileAtPosition == loadMap._door ||
            tileAtPosition == loadMap._chest ||
            tileAtPosition == borderTile)
        {
            Debug.Log($"Player is blocked at ({x}, {y}): {tileAtPosition}");
            return false;
        }
        if (tileAtPosition == loadMap._enemy)
        {
            Debug.Log($"Player is blocked by ENEMY at ({x}, {y}): {tileAtPosition}");
            return false;
        }
        if (tileAtPosition == loadMap._win)
        {
            LevelComplete();
            return false;
        }
        return false;
    }

    // ---------- RESET PLAYER ---------- // 
    public void ResetPosition() // set player to start position whenever completing a level
    {
        // Before moving, clear any previous player tile
        int previousX = Mathf.RoundToInt(movePoint.position.x / tileSize);
        int previousY = Mathf.RoundToInt(movePoint.position.y / tileSize);
        Vector3Int previousPosition = new Vector3Int(previousX, previousY, 0);
        
        if (myTilemap.HasTile(previousPosition)) 
        {
            myTilemap.SetTile(previousPosition, null);  // Remove the player tile from the old position
        }

        Vector3 spawnPosition = playerSpawnPoint.transform.position;
        movePoint.position = new Vector3(
            Mathf.Round(spawnPosition.x / tileSize) * tileSize,
            Mathf.Round(spawnPosition.y / tileSize) * tileSize,
            movePoint.position.z
        );
       
        DrawPlayer(0, 0, 
            Mathf.RoundToInt(spawnPosition.x / tileSize), 
            Mathf.RoundToInt(spawnPosition.y / tileSize)
        );
        Debug.Log($"Player spawn position set to {spawnPosition}");
    }

    // ---------- MOVE PLAYER ---------- //
    void MovePosition()
    {
        // set player's current pos using movePoint & tileSize
        int playerX = Mathf.RoundToInt(movePoint.position.x / tileSize); 
        int playerY = Mathf.RoundToInt(movePoint.position.y / tileSize); 
        
        int inputX = 0, inputY = 0;
        if (Input.GetKeyDown(KeyCode.W)) inputY = 1;  // Move up
        else if (Input.GetKeyDown(KeyCode.S)) inputY = -1; // Move down
        else if (Input.GetKeyDown(KeyCode.A)) inputX = -1; // Move left
        else if (Input.GetKeyDown(KeyCode.D)) inputX = 1;  // Move right

        // increment target based on player pos
        int targetX = playerX + inputX;
        int targetY = playerY + inputY;
        
        if (inputX == 0 && inputY == 0)
        {
            return;
        }

        if (CanMove(targetX, targetY)) // Check if the target tile is walkable
        {   
            // Update the move point's position using targetX,Y var previously selected
            movePoint.position = new Vector3(
                targetX * tileSize,
                targetY * tileSize,
                movePoint.position.z
            );     
            DrawPlayer(playerX, playerY, targetX, targetY);  // Draw the player at the new position
            Debug.Log($"Player moved to new position: {targetX}, {targetY}");

            combat.PlayerCompletedAction();
        }
        else
        {
            Debug.Log($"Player cannot move to position: {targetX}, {targetY}");
        }
    }

    // ---------- RNEXT LV / YOU WIN ---------- //   
    void LevelComplete() // if player detects _winTile, reset position + get next map
    {
        Debug.Log("Level complete! Getting a new map...");
        loadMap.LoadPremadeMap();
        ResetPosition();
        sceneLoader.WinGame();
    }

    // ---------- DRAW PLAYER ---------- //
    // setting previous position to null, curent position to playertile
    void DrawPlayer(int previousX, int previousY, int currentX, int currentY)
    {
        Vector3Int previousPosition = new Vector3Int(previousX, previousY, 0);
        Vector3Int currentPosition = new Vector3Int(currentX, currentY, 0);

        if (myTilemap.HasTile(previousPosition)) 
        {
            myTilemap.SetTile(previousPosition, null);
        }
        // Place the player tile at the new position
        myTilemap.SetTile(currentPosition, playerTile);

        foreach (EnemyController enemyref in FindObjectsOfType<EnemyController>())
        {
            enemyref.MoveTowardsPlayer();
        }
        myTilemap.RefreshAllTiles();
    }
}
