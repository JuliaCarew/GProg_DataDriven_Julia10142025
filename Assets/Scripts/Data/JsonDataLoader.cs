using System;
using System.IO;
using UnityEngine;
// using System.Text.Json; // Removed because Unity uses JsonUtility

public class JsonDataLoader : MonoBehaviour
{
    private static GameSettings _gameSettings;
    private static EnemyData _enemyData;
    
    // Event for when data is reloaded
    public static System.Action OnDataReloaded;
    
    public static GameSettings GameSettings
    {
        get
        {
            if (_gameSettings == null)
            {
                LoadGameSettings();
            }
            return _gameSettings;
        }
    }
    
    public static EnemyData EnemyData
    {
        get
        {
            if (_enemyData == null)
            {
                LoadEnemyData();
            }
            return _enemyData;
        }
    }
    
    public static void LoadGameSettings()
    {
        try
        {
            string filePath = Path.Combine(Application.streamingAssetsPath, "game_settings.json");
            
            if (File.Exists(filePath))
            {
                string jsonContent = File.ReadAllText(filePath);
                var wrapper = JsonUtility.FromJson<GameSettingsWrapper>(jsonContent);
                _gameSettings = wrapper.gameSettings;
                Debug.Log("Game settings loaded successfully from JSON");
            }
            else
            {
                Debug.LogError($"Game settings file not found at: {filePath}");
                _gameSettings = GetDefaultGameSettings();
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"Error loading game settings: {e.Message}");
            _gameSettings = GetDefaultGameSettings();
        }
    }
    
    public static void LoadEnemyData()
    {
        try
        {
            string filePath = Path.Combine(Application.streamingAssetsPath, "enemy_data.json");
            
            if (File.Exists(filePath))
            {
                string jsonContent = File.ReadAllText(filePath);
                _enemyData = JsonUtility.FromJson<EnemyData>(jsonContent);
                Debug.Log("Enemy data loaded successfully from JSON");
            }
            else
            {
                Debug.LogError($"Enemy data file not found at: {filePath}");
                _enemyData = GetDefaultEnemyData();
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"Error loading enemy data: {e.Message}");
            _enemyData = GetDefaultEnemyData();
        }
    }
    
    private static GameSettings GetDefaultGameSettings()
    {
        return new GameSettings
        {
            mapSettings = new MapSettings
            {
                defaultMap = "map1",
                defaultMapWidth = 17,
                defaultMapHeight = 8,
                tileSize = 0.08f,
                mapCenterX = 0.0f,
                mapCenterY = 0.0f,
                mapCenterZ = 0.0f
            },
            playerSettings = new PlayerSettings
            {
                maxHealth = 50,
                playerDamage = 10,
                isPlayer = true
            },
            enemySettings = new EnemySettings
            {
                maxHealth = 30,
                enemyDamage = 5,
                moveSpeed = 0.8f,
                isPlayer = false
            },
            combatSettings = new CombatSettings
            {
                turnDelay = 0.2f,
                allowDiagonalAttacks = true,
                wallCollisionEnabled = true
            },
            uiSettings = new UISettings
            {
                healthTextPrefix = "Player HP: ",
                enemyHealthTextPrefix = "Enemy HP: ",
                gameOverText = "Game Over!",
                levelCompleteText = "Level Complete!"
            },
            tileSettings = new TileSettings
            {
                wallCharacter = "#",
                doorCharacter = "O",
                chestCharacter = "*",
                enemyCharacter = "@",
                playerCharacter = "$",
                emptyCharacter = " ",
                winCharacter = "%"
            }
        };
    }
    
    private static EnemyData GetDefaultEnemyData()
    {
        return new EnemyData
        {
            enemies = new System.Collections.Generic.List<EnemyInfo>
            {
                new EnemyInfo
                {
                    enemyId = "basic_enemy",
                    name = "Basic Enemy",
                    maxHealth = 30,
                    damage = 5,
                    moveSpeed = 0.8f,
                    description = "A basic enemy with standard stats"
                }
            }
        };
    }
    
    // Method to reload data at runtime (useful for testing)
    public static void ReloadAllData()
    {
        _gameSettings = null;
        _enemyData = null;
        LoadGameSettings();
        LoadEnemyData();
        Debug.Log("All data reloaded from JSON files");
        
        // Notify all listeners that data has been reloaded
        OnDataReloaded?.Invoke();
    }
}

// Wrapper class for the JSON structure
[Serializable]
public class GameSettingsWrapper
{
    public GameSettings gameSettings;
}
