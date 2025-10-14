using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using TMPro;

public class HealthSystem : MonoBehaviour
{
    [Header("References")]
    public LoadMap loadMap;
    public EnemyController enemyController;
    public Vector3Int tilePosition;

    [Header("Health Settings")]
    public int maxHealth = 50;
    public SpriteRenderer spriteRenderer;
    public int currentHealth;
    public int playerDamage = 10;
    private Color originalColor;

    [Header("UI Reference")]
    public TextMeshProUGUI healthText;
    public TextMeshProUGUI enemyhealthText;
    public GameObject gameOverUI;

    [Header("Player bool")]
    public bool isPlayer = false;

    void Awake()
    {
        currentHealth = maxHealth;

        if (enemyController != null)
        {
            enemyController.currentHealth = maxHealth;
        }
        else
        {
            Debug.LogWarning("EnemyController reference is missing!");
        }
    }
    void Start()
    {
        gameOverUI.SetActive(false);
        //Debug.Log($"Initial health set to {currentHealth}.");
        UpdateHealthUI();

        if (spriteRenderer != null)
        {
            originalColor = spriteRenderer.color;
        }
        if (enemyController != null)
        {
            enemyController.currentHealth = enemyController.maxHealth;
        }
        else
        {
            Debug.LogWarning("EnemyController reference is missing in HealthSystem!");
        }
    }     
    
    // ---------- DAMAGE ---------- //
    public void TakeDamage(int damage)
    {
        Debug.Log($"Taking damage: {damage}. Health before damage: {currentHealth}");
        currentHealth = Mathf.Clamp(currentHealth - damage, 0, maxHealth);
        Debug.Log($"Health after damage: {currentHealth}");
        UpdateHealthUI();

        if (currentHealth <= 0)
        {
            currentHealth = 0;
            Vector3Int position = loadMap.myTilemap.WorldToCell(transform.position);
            Die(position);
        }
        else
        {
            ChangePlayerTileColor(Color.red);
        }
    }

    // ---------- DIE ---------- //
    public void Die(Vector3Int position)
    {
        Debug.Log(gameObject.name + " has died!");

        if (loadMap.myTilemap != null)
        {
            loadMap.myTilemap.SetTile(position, null);
            Debug.Log($"Tile at {position} set to null.");
        }
        if (isPlayer) // game over for player
        {
            Debug.Log("Game over!");
            //loadMap.myTilemap.SetTile(position, null);
            ShowGameOverScreen();
        }
    }

    // ---------- UI ---------- //
    public void UpdateHealthUI()
    {
        if (healthText != null)
        {
            healthText.text = "Player HP: " + currentHealth;
        }

        // update the enemy health text 
        if (enemyhealthText != null && enemyController != null)
        {
            enemyhealthText.text = "Enemy HP: " + enemyController.currentHealth;
        }
    }

    // ---------- GAME OVER ---------- //
    private void ShowGameOverScreen() 
    {       
        //Debug.Log("Game over screen SHOW");
        gameOverUI.SetActive(true);
        Time.timeScale = 0f;  
    }

    // ---------- DMG RED INDICATOR ---------- // 
    public void ChangePlayerTileColor(Color color)
    {
        Vector3Int playerTilePosition = loadMap.myTilemap.WorldToCell(transform.position);
        if (loadMap.movePlayerref.playerTile is ColoredTile coloredTile)
        {
            coloredTile.SetColor(color);
            loadMap.myTilemap.RefreshTile(playerTilePosition);
        }
    }

    public void ResetHealth()
    {
        currentHealth = maxHealth;
        enemyController.currentHealth = enemyController.maxHealth;
        UpdateHealthUI();
    }
}

// ---------- CLASS TO CHANGE TILE COLOR RED ON DMG ---------- //
public class ColoredTile : Tile
{
    public Color tileColor = Color.white;

    // override the GetTileColor method to modify the tile's color
    public override void RefreshTile(Vector3Int position, ITilemap tilemap)
    {
        base.RefreshTile(position, tilemap);
    }

    public Color GetColor()
    {
        return tileColor;
    }

    public void SetColor(Color color)
    {
        tileColor = color;
    }
}