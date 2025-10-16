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
    
    // Data-driven settings loaded from JSON
    private GameSettings gameSettings;

    void Start()
    {
        // Load JSON data
        gameSettings = JsonDataLoader.GameSettings;
        tileSize = gameSettings.mapSettings.tileSize;
        
        movePoint.parent = null;  // allows movepoint to dictate player's direction/ can be moved on it's own    
        ResetPosition();
        
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
        Debug.Log("MovePlayer: Data reloaded, updating settings...");
        gameSettings = JsonDataLoader.GameSettings;
        tileSize = gameSettings.mapSettings.tileSize;
        Debug.Log($"Updated tile size: {tileSize}");
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

        // Allow movement if the tile is null (empty) or is explicitly _none
        if (tileAtPosition == null || tileAtPosition == loadMap._none)
        {
            return true; 
        }

        // cannot move on wall, chest, door, or enemy tiles
        if (tileAtPosition == loadMap._wall ||
            tileAtPosition == loadMap._door ||
            tileAtPosition == loadMap._chest ||
            tileAtPosition == borderTile)
        {
            return false;
        }
        if (tileAtPosition == loadMap._enemy)
        {
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

        // Find the player spawn position from the tilemap (look for '$' character)
        Vector3Int playerSpawnTilePos = FindPlayerSpawnPosition();
        
        // Convert tilemap position to world position
        Vector3 worldPosition = myTilemap.CellToWorld(playerSpawnTilePos);
        
        // Set movePoint to the correct world position
        movePoint.position = new Vector3(
            worldPosition.x,
            worldPosition.y,
            movePoint.position.z
        );
       
        DrawPlayer(0, 0, playerSpawnTilePos.x, playerSpawnTilePos.y);
        Debug.Log($"Player spawn position set to tilemap position {playerSpawnTilePos} -> world position {worldPosition}");
    }
    
    // Find the player spawn position from the loaded map
    private Vector3Int FindPlayerSpawnPosition()
    {
        // Use the player spawn position stored by LoadMap
        return loadMap.playerSpawnPosition;
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
            
            // Check if player is now adjacent to an enemy and can attack
            CheckForEnemyAttack();
            
            combat.PlayerCompletedAction();
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
        
        myTilemap.RefreshAllTiles();
    }
    
    // ---------- CHECK FOR ENEMY ATTACK ---------- //
        void CheckForEnemyAttack()
        {
            Debug.Log("Checking for enemy attack...");
            // Check if player is adjacent to any enemy and can attack
            if (combat.NextToEnemy())
            {
                Debug.Log("Player is next to enemy, attacking!");
                // Player attacks enemy using damage from HealthSystem (which gets updated from JSON)
                int playerDamage = loadMap.playerHealthSystemref.playerDamage;
                combat.PlayerAttacksEnemy(playerDamage);
            }
            else
            {
                Debug.Log("Player is not next to any enemy");
            }
        }
}
