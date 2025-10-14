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
    public TileBase _wall;
    public TileBase _door;
    public TileBase _chest;
    public TileBase _enemy;
    public TileBase _none;
    public TileBase _win;

    [Header("Tile String Characters")]
    public string wall = "#";
    public string door = "O";
    public string chest = "*";
    public string enemy = "@";
    public string none = " ";
    public string win = "%";

    [Header("Map Dimensions")]
    public int mapWidth;
    public int mapHeight;

    void Start()
    {
        //Debug.Log("Loading premade map...");
        LoadPremadeMap();  
    }

    // ---------- LOAD RANDOM MAP ---------- //
    public void LoadPremadeMap()
    {
        //Debug.Log("reading text file");
        string folderPath = Path.Combine(Application.streamingAssetsPath, "2DMapStrings");
        string[] mapFiles = Directory.GetFiles(folderPath, "*.txt"); // Get all text files
        
        // get random text file
        int randomIndex = Random.Range(0, mapFiles.Length);
        string selectedFile = mapFiles[randomIndex];
        string[] myLines = File.ReadAllLines(selectedFile); // create string from all idv. lines read
 
        mapHeight = myLines.Length;
        mapWidth = myLines[0].Length;

        myTilemap.ClearAllTiles();

        // converts the mapCenter position to integer tilemap coordinates
        Vector3Int mapOrigin = new Vector3Int(
            Mathf.RoundToInt(mapCenter.position.x) - mapWidth / 2,
            Mathf.RoundToInt((mapCenter.position.y) - mapHeight / 2) + 6, // + y 0.5
            0
        );

        GameObject existingEnemy = GameObject.Find("Enemy");
        if (existingEnemy != null)
        {
            Destroy(existingEnemy);
        }

        // placing tiles
        for (int y = myLines.Length - 1; y>= 0; y--) 
        {
            string myLine = myLines[y]; // so each line gets read in proper order one-by-one
            //Debug.Log($"Reading Line: {myLine} at {-y}");

            for (int x = 0; x < myLine.Length; x++)
            {   // on x axis, so accross the line to idv. char, read & assign each one
                char myChar = myLine[x];
                //Debug.Log($"Reading Char: {myChar} at {x}");
                Vector3Int position = new Vector3Int(x, -y, 0) + mapOrigin;
                    
                switch (myChar)
                {
                    case '#':
                        myTilemap.SetTile(position, _wall);
                        break;
                    case 'O':
                        myTilemap.SetTile(position, _door);
                        break;
                    case '*':
                        myTilemap.SetTile(position, _chest);
                        break;
                    case '@':
                        // Instantiate Enemy GameObject
                        GameObject enemyObject = new GameObject("Enemy");
                        EnemyController enemyController = enemyObject.AddComponent<EnemyController>();
                        HealthSystem enemyHealthSystem = enemyObject.AddComponent<HealthSystem>();
                        
                        // assign enemycontroller variables to avoid null reference exception when instantiated
                        enemyController.loadMap = this; 
                        enemyController.enemyTile = _enemy;
                        enemyController.playerTile = movePlayerref.playerTile; 
                        enemyController.enemyPosition = position;
                        // assign HealthSystem and its references
                        enemyHealthSystem.loadMap = this;
                        enemyHealthSystem.tilePosition = position;
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

                        break;
                    case ' ':
                        myTilemap.SetTile(position, null);
                        break;
                    case '%':
                        myTilemap.SetTile(position, _win);
                        break;
                }
            }
        }
    }
}