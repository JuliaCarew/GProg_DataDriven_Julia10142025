using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using System.IO;

public class LoadMap : MonoBehaviour
{
    [Header("References")]
    public HealthSystem healthSystemref;
    public HealthSystem playerHealthSystemref;

    public EnemyController enemyref;
    public Combat combatref;
    public MovePlayer movePlayerref;

    [Header("Transform & GameObjects")]
    public Transform mapCenter;
    public Vector3Int enemyPosition;

    [Header("Tilemap & Tiles")]
    public Tilemap myTilemap;
    public Grid gridComponent; // Reference to the Grid component for tile size adjustment
    public TileBase _wall;
    public TileBase _door;
    public TileBase _chest;
    public TileBase _enemy;
    public TileBase _none;
    public TileBase _win;

    [Header("Tile String Characters")]
    public string wall;
    public string door;
    public string chest;
    public string enemy;
    public string none;
    public string win;

    [Header("Map Dimensions")]
    public int mapWidth;
    public int mapHeight;
    
    [Header("Player Spawn")]
    public Vector3Int playerSpawnPosition;
    
    // Data-driven settings loaded from JSON
    private GameSettings gameSettings;
    private string currentMapName; // Track current map to detect changes

    void Start()
    {
        // Load JSON data
        LoadJsonData();
        
        // Store the current map name
        currentMapName = gameSettings.mapSettings.defaultMap;
        
        // Check for generate a random map or load a premade one
        if (gameSettings.mapSettings.defaultMap.ToLower() == "random")
        {
            Debug.Log("Generating random map...");
            GenerateRandomMap();
        }
        else
        {
            Debug.Log("Loading premade map...");
            LoadPremadeMap();
        }
        
        JsonDataLoader.OnDataReloaded += OnDataReloaded;
    }
    
    void OnDestroy()
    {
        JsonDataLoader.OnDataReloaded -= OnDataReloaded;
    }
    
    void OnDataReloaded()
    {
        Debug.Log("LoadMap: Data reloaded, updating settings...");
        LoadJsonData();
        
        string newMapName = gameSettings.mapSettings.defaultMap;
        if (newMapName != currentMapName)
        {
            Debug.Log($"Map changed from '{currentMapName}' to '{newMapName}'. Reloading map...");
            currentMapName = newMapName;
            
            // Reload or regenerate the map based on the new setting
            if (newMapName.ToLower() == "random")
            {
                Debug.Log("Generating new random map...");
                GenerateRandomMap();
            }
            else
            {
                Debug.Log("Loading new premade map...");
                LoadPremadeMap();
            }
        }
        else
        {
            Debug.Log("Map name unchanged, updating existing enemies...");
            UpdateExistingEnemies();
        }
    }
    
    void UpdateExistingEnemies()
    {
        // Update all existing enemies with new stats
        EnemyController[] enemies = FindObjectsOfType<EnemyController>();
        foreach (EnemyController enemy in enemies)
        {
            if (enemy != null && enemy.healthSystemref != null)
            {
                // Update enemy stats from JSON
                enemy.maxHealth = gameSettings.enemySettings.maxHealth;
                enemy.enemyDamage = gameSettings.enemySettings.enemyDamage;
                
                // Update health system stats
                enemy.healthSystemref.maxHealth = gameSettings.enemySettings.maxHealth;
                enemy.healthSystemref.currentHealth = Mathf.Min(enemy.healthSystemref.currentHealth, gameSettings.enemySettings.maxHealth);
                
                Debug.Log($"Updated enemy stats: HP={enemy.maxHealth}, Damage={enemy.enemyDamage}");
            }
        }
        
        // Update player health if it exists
        if (playerHealthSystemref != null)
        {
            int oldMaxHealth = playerHealthSystemref.maxHealth;
            playerHealthSystemref.maxHealth = gameSettings.playerSettings.maxHealth;
            
            if (gameSettings.playerSettings.maxHealth > oldMaxHealth)
            {
                float healthRatio = (float)playerHealthSystemref.currentHealth / oldMaxHealth;
                playerHealthSystemref.currentHealth = Mathf.RoundToInt(gameSettings.playerSettings.maxHealth * healthRatio);
            }
            else
            {
                // If new max health is lower, cap current health
                playerHealthSystemref.currentHealth = Mathf.Min(playerHealthSystemref.currentHealth, gameSettings.playerSettings.maxHealth);
            }
            
            // Update player damage
            playerHealthSystemref.playerDamage = gameSettings.playerSettings.playerDamage;
            
            playerHealthSystemref.UpdateHealthUI();
            Debug.Log($"Updated player stats: HP={playerHealthSystemref.currentHealth}/{playerHealthSystemref.maxHealth}, Damage={playerHealthSystemref.playerDamage}");
        }
    }
    
    // ---------- LOAD JSON DATA ---------- //
    void LoadJsonData()
    {
        gameSettings = JsonDataLoader.GameSettings;
        
        // Update tile characters from JSON
        wall = gameSettings.tileSettings.wallCharacter;
        door = gameSettings.tileSettings.doorCharacter;
        chest = gameSettings.tileSettings.chestCharacter;
        enemy = gameSettings.tileSettings.enemyCharacter;
        none = gameSettings.tileSettings.emptyCharacter;
        win = gameSettings.tileSettings.winCharacter;
        
        // Apply tile size from JSON settings
        ApplyTileSize();
        
        Debug.Log("JSON data loaded successfully");
        Debug.Log($"Tile characters: Wall='{wall}', Door='{door}', Enemy='{enemy}', Player='{gameSettings.tileSettings.playerCharacter}'");
    }
    
    void ApplyTileSize()
    {
        // Apply tile size from JSON if Grid component is assigned
        if (gridComponent != null)
        {
            float tileSize = gameSettings.mapSettings.tileSize;
            gridComponent.cellSize = new Vector3(tileSize, tileSize, tileSize);
            Debug.Log($"Grid cell size set to: {tileSize}");
        }
        else
        {
            // Try to find the Grid component automatically if not assigned
            if (myTilemap != null)
            {
                gridComponent = myTilemap.GetComponentInParent<Grid>();
                if (gridComponent != null)
                {
                    float tileSize = gameSettings.mapSettings.tileSize;
                    gridComponent.cellSize = new Vector3(tileSize, tileSize, tileSize);
                    Debug.Log($"Grid component auto-detected. Cell size set to: {tileSize}");
                }
                else
                {
                    Debug.LogWarning("Grid component not found. Tile size setting will not be applied.");
                }
            }
        }
    }

    // ---------- LOAD MAP FROM JSON SETTINGS ---------- //
    public void LoadPremadeMap()
    {
        string folderPath = Path.Combine(Application.streamingAssetsPath, "2DMapStrings");
        
        // Check if the directory exists
        if (!Directory.Exists(folderPath))
        {
            Debug.LogError($"Directory not found: {folderPath}");
            Debug.LogError("Please create the following folder structure in your project: Assets/StreamingAssets/2DMapStrings/");
            Debug.LogError("Then add your .txt map files to that folder.");
            return;
        }
        
        // Get the default map name from JSON settings
        string mapName = gameSettings.mapSettings.defaultMap;
        
        // If the map name doesn't include .txt extension, add it
        if (!mapName.EndsWith(".txt"))
        {
            mapName += ".txt";
        }
        
        string selectedFile = Path.Combine(folderPath, mapName);
        
        // Check if the specified map file exists
        if (!File.Exists(selectedFile))
        {
            Debug.LogError($"Map file not found: {selectedFile}");
            Debug.LogError($"Please create a map file named '{mapName}' in: Assets/StreamingAssets/2DMapStrings/");
            Debug.LogError("Or change the 'defaultMap' value in game_settings.json");
            return;
        }
        
        Debug.Log($"Loading map from JSON settings: {Path.GetFileName(selectedFile)}");
        string[] myLines = File.ReadAllLines(selectedFile); 
 
        mapHeight = myLines.Length;
        mapWidth = myLines[0].Length;

        myTilemap.ClearAllTiles();

        // Use JSON data for map center position
        Vector3 mapCenterPos = new Vector3(
            gameSettings.mapSettings.mapCenterX,
            gameSettings.mapSettings.mapCenterY,
            gameSettings.mapSettings.mapCenterZ
        );
        
        // converts the mapCenter position to integer tilemap coordinates
        Vector3Int mapOrigin = new Vector3Int(
            Mathf.RoundToInt(mapCenterPos.x) - mapWidth / 2,
            Mathf.RoundToInt(mapCenterPos.y) - mapHeight / 2,
            0
        );
        
        Debug.Log($"Map dimensions: {mapWidth}x{mapHeight}");
        Debug.Log($"MapCenter position: {mapCenter.position}");
        Debug.Log($"MapOrigin: {mapOrigin}");

        // Destroy all existing enemies before loading new map
        DestroyAllEnemies();

        // placing tiles
        for (int y = 0; y < myLines.Length; y++) 
        {
            string myLine = myLines[y]; 
            int tileY = y; 
            
            myLine = myLine.Replace('\t', ' ');
            Debug.Log($"Reading Line {y}: '{myLine}' -> tileY={tileY}");

            for (int x = 0; x < myLine.Length; x++)
            {   // on x axis, so accross the line to idv. char, read & assign each one
                string myChar = myLine[x].ToString();
                Vector3Int position = new Vector3Int(x, tileY, 0) + mapOrigin;
                    
                // Use dynamic tile character values from JSON settings
                if (myChar == wall)
                {
                    myTilemap.SetTile(position, _wall);
                    Debug.Log($"Wall at ({x}, {tileY})");
                }
                else if (myChar == door)
                {
                    myTilemap.SetTile(position, _door);
                }
                else if (myChar == chest)
                {
                    myTilemap.SetTile(position, _chest);
                }
                else if (myChar == enemy)
                {
                    CreateEnemy(position);
                }
                else if (myChar == gameSettings.tileSettings.playerCharacter)
                {
                    // Player spawn position - store it and place player tile
                    playerSpawnPosition = position;
                    myTilemap.SetTile(position, movePlayerref.playerTile);
                    Debug.Log($"Player spawn found at position ({x}, {tileY}) -> world position {position}");
                }
                else if (myChar == none)
                {
                    myTilemap.SetTile(position, null);
                }
                else if (myChar == win)
                {
                    myTilemap.SetTile(position, _win);
                }
            }
        }
    }
    
    // ---------- GENERATE RANDOM MAP ---------- //
    public void GenerateRandomMap()
    {
        // Use dimensions from JSON settings
        mapWidth = gameSettings.mapSettings.defaultMapWidth;
        mapHeight = gameSettings.mapSettings.defaultMapHeight;
        
        Debug.Log($"Generating random map with dimensions: {mapWidth}x{mapHeight}");
        
        myTilemap.ClearAllTiles();
        
        // Use JSON data for map center position
        Vector3 mapCenterPos = new Vector3(
            gameSettings.mapSettings.mapCenterX,
            gameSettings.mapSettings.mapCenterY,
            gameSettings.mapSettings.mapCenterZ
        );
        
        // converts the mapCenter position to integer tilemap coordinates
        Vector3Int mapOrigin = new Vector3Int(
            Mathf.RoundToInt(mapCenterPos.x) - mapWidth / 2,
            Mathf.RoundToInt(mapCenterPos.y) - mapHeight / 2,
            0
        );
        
        Debug.Log($"MapCenter position: {mapCenter.position}");
        Debug.Log($"MapOrigin: {mapOrigin}");
        
        // Destroy all existing enemies before generating new map
        DestroyAllEnemies();
        
        // Generate the map
        for (int y = 0; y < mapHeight; y++)
        {
            for (int x = 0; x < mapWidth; x++)
            {
                Vector3Int position = new Vector3Int(x, y, 0) + mapOrigin;
                
                // Place walls along the edges
                if (x == 0 || x == mapWidth - 1 || y == 0 || y == mapHeight - 1)
                {
                    myTilemap.SetTile(position, _wall);
                }
                else
                {
                    // Interior: randomly place elements
                    float randomValue = Random.value;
                    
                    if (randomValue < 0.1f) // 10% chance for walls
                    {
                        myTilemap.SetTile(position, _wall);
                    }
                    else if (randomValue < 0.15f) // 5% chance for enemies
                    {
                        CreateEnemy(position);
                    }
                    else if (randomValue < 0.18f) // 3% chance for chests
                    {
                        myTilemap.SetTile(position, _chest);
                    }
                    else if (randomValue < 0.20f) // 2% chance for doors
                    {
                        myTilemap.SetTile(position, _door);
                    }
                    else
                    {
                        // Empty space
                        myTilemap.SetTile(position, null);
                    }
                }
            }
        }
        
        playerSpawnPosition = new Vector3Int(2, 2, 0) + mapOrigin;
        myTilemap.SetTile(playerSpawnPosition, movePlayerref.playerTile);
        Debug.Log($"Player spawn set at position: {playerSpawnPosition}");
        
        // Place win tile near top-right corner
        Vector3Int winPosition = new Vector3Int(mapWidth - 3, mapHeight - 3, 0) + mapOrigin;
        myTilemap.SetTile(winPosition, _win);
        Debug.Log($"Win tile placed at position: {winPosition}");
    }
    
    // ---------- HELPER METHODS ---------- //
    private void DestroyAllEnemies()
    {
        EnemyController[] existingEnemies = FindObjectsOfType<EnemyController>();
        Debug.Log($"Found {existingEnemies.Length} existing enemies to destroy");
        
        foreach (EnemyController enemy in existingEnemies)
        {
            if (enemy != null)
            {
                Debug.Log($"Destroying enemy at position: {enemy.transform.position}");
                // Clear the enemy tile from the tilemap
                Vector3Int enemyTilePos = myTilemap.WorldToCell(enemy.transform.position);
                myTilemap.SetTile(enemyTilePos, null);
                Destroy(enemy.gameObject);
            }
        }
    }
    
    private void CreateEnemy(Vector3Int position)
    {
        Debug.Log($"Creating enemy at world position {position}");
        GameObject enemyObject = new GameObject("Enemy");
        EnemyController enemyController = enemyObject.AddComponent<EnemyController>();
        HealthSystem enemyHealthSystem = enemyObject.AddComponent<HealthSystem>();
        
        // assign enemycontroller variables to avoid null reference exception when instantiated
        enemyController.loadMap = this; 
        enemyController.enemyTile = _enemy;
        enemyController.playerTile = movePlayerref.playerTile; 
        enemyController.enemyPosition = position;
        
        // Set enemy stats from JSON data
        enemyController.maxHealth = gameSettings.enemySettings.maxHealth;
        enemyController.currentHealth = gameSettings.enemySettings.maxHealth;
        enemyController.enemyDamage = gameSettings.enemySettings.enemyDamage;
        
        // assign HealthSystem and its references
        enemyHealthSystem.loadMap = this;
        enemyHealthSystem.tilePosition = position;
        enemyHealthSystem.maxHealth = gameSettings.enemySettings.maxHealth;
        enemyHealthSystem.currentHealth = gameSettings.enemySettings.maxHealth;
        enemyController.healthSystemref = enemyHealthSystem;
        enemyHealthSystem.enemyController = enemyController;
        
        // assigning Combat and Player references 
        enemyController.loadMap.combatref = combatref;
        enemyController.loadMap.movePlayerref = movePlayerref;
        // assigning UI elements
        enemyHealthSystem.enemyhealthText = healthSystemref.enemyhealthText;
        enemyHealthSystem.healthText = healthSystemref.healthText;
        enemyHealthSystem.gameOverUI = healthSystemref.gameOverUI;

        // set enemy position on the Tilemap
        myTilemap.SetTile(position, _enemy);  // place enemy tile on map
        enemyObject.transform.position = myTilemap.CellToWorld(position); // match enemy to world position
    }
}