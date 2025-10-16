using UnityEngine;

public class SimpleDataTester : MonoBehaviour
{
    [Header("Testing Controls")]
    [SerializeField] private KeyCode reloadDataKey = KeyCode.R;
    [SerializeField] private KeyCode printSettingsKey = KeyCode.P;
    [SerializeField] private KeyCode reloadMapKey = KeyCode.M;
    
    [Header("References")]
    [SerializeField] private LoadMap loadMapScript;
    
    private string lastMapName = "";
    
    void Start()
    {
        // Try to find LoadMap script if not assigned
        if (loadMapScript == null)
        {
            loadMapScript = FindObjectOfType<LoadMap>();
            if (loadMapScript != null)
            {
                Debug.Log("SimpleDataTester: Found LoadMap script automatically");
            }
            else
            {
                Debug.LogWarning("SimpleDataTester: LoadMap script not found. Map reloading will not work.");
            }
        }
        
        // Store initial map name
        lastMapName = JsonDataLoader.GameSettings.mapSettings.defaultMap;
    }
    
    void Update()
    {
        // Reload JSON data at runtime
        if (Input.GetKeyDown(reloadDataKey))
        {
            Debug.Log("=== RELOADING JSON DATA ===");
            string oldMapName = JsonDataLoader.GameSettings.mapSettings.defaultMap;
            JsonDataLoader.ReloadAllData();
            string newMapName = JsonDataLoader.GameSettings.mapSettings.defaultMap;
            
            // Check if map name changed
            if (oldMapName != newMapName)
            {
                Debug.Log($"Map changed from '{oldMapName}' to '{newMapName}'");
                lastMapName = newMapName;
            }
            else
            {
                Debug.Log($"Map name unchanged: '{newMapName}'");
            }
            
            Debug.Log("=== JSON DATA RELOADED ===");
        }
        
        // Reload the map
        if (Input.GetKeyDown(reloadMapKey))
        {
            ReloadMap();
        }
        
        // Print current settings
        if (Input.GetKeyDown(printSettingsKey))
        {
            PrintCurrentSettings();
        }
    }
    
    void ReloadMap()
    {
        if (loadMapScript == null)
        {
            Debug.LogError("Cannot reload map: LoadMap script reference is missing!");
            return;
        }
        
        Debug.Log("=== RELOADING MAP ===");
        string mapName = JsonDataLoader.GameSettings.mapSettings.defaultMap;
        Debug.Log($"Loading map: '{mapName}.txt'");
        loadMapScript.LoadPremadeMap();
        Debug.Log("=== MAP RELOADED ===");
    }
    
    void PrintCurrentSettings()
    {
        try
        {
            var settings = JsonDataLoader.GameSettings;
            Debug.Log("=== CURRENT GAME SETTINGS ===");
            Debug.Log($"Map: '{settings.mapSettings.defaultMap}.txt', Size: {settings.mapSettings.defaultMapWidth}x{settings.mapSettings.defaultMapHeight}, TileSize: {settings.mapSettings.tileSize}");
            Debug.Log($"Player: HP={settings.playerSettings.maxHealth}, Damage={settings.playerSettings.playerDamage}");
            Debug.Log($"Enemy: HP={settings.enemySettings.maxHealth}, Damage={settings.enemySettings.enemyDamage}");
            Debug.Log($"Combat: TurnDelay={settings.combatSettings.turnDelay}, DiagonalAttacks={settings.combatSettings.allowDiagonalAttacks}");
            Debug.Log($"Tiles: Wall='{settings.tileSettings.wallCharacter}', Enemy='{settings.tileSettings.enemyCharacter}', Player='{settings.tileSettings.playerCharacter}'");
            Debug.Log("=== END SETTINGS ===");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Error printing settings: {e.Message}");
        }
    }
    
    void OnGUI()
    {
        GUILayout.BeginArea(new Rect(10, 100, 450, 150));
        GUILayout.Label("JSON Testing Controls:");
        GUILayout.Label($"Press {reloadDataKey} to reload JSON data (updates stats, map name, etc.)");
        GUILayout.Label($"Press {reloadMapKey} to reload the map (loads the current defaultMap)");
        GUILayout.Label($"Press {printSettingsKey} to print current settings");
        GUILayout.Label($"Current Map: '{JsonDataLoader.GameSettings.mapSettings.defaultMap}.txt'");
        GUILayout.EndArea();
    }
}
