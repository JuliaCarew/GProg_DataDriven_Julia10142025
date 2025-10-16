using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class GameSettings
{
    public MapSettings mapSettings;
    public PlayerSettings playerSettings;
    public EnemySettings enemySettings;
    public CombatSettings combatSettings;
    public UISettings uiSettings;
    public TileSettings tileSettings;
}

[Serializable]
public class MapSettings
{
    public string defaultMap;
    public int defaultMapWidth;
    public int defaultMapHeight;
    public float tileSize;
    public float mapCenterX;
    public float mapCenterY;
    public float mapCenterZ;
}

[Serializable]
public class PlayerSettings
{
    public int maxHealth;
    public int playerDamage;
    public bool isPlayer;
}

[Serializable]
public class EnemySettings
{
    public int maxHealth;
    public int enemyDamage;
    public float moveSpeed;
    public bool isPlayer;
}

[Serializable]
public class CombatSettings
{
    public float turnDelay;
    public bool allowDiagonalAttacks;
    public bool wallCollisionEnabled;
}

[Serializable]
public class UISettings
{
    public string healthTextPrefix;
    public string enemyHealthTextPrefix;
    public string gameOverText;
    public string levelCompleteText;
}

[Serializable]
public class TileSettings
{
    public string wallCharacter;
    public string doorCharacter;
    public string chestCharacter;
    public string enemyCharacter;
    public string playerCharacter;
    public string emptyCharacter;
    public string winCharacter;
}

[Serializable]
public class EnemyData
{
    public List<EnemyInfo> enemies;
}

[Serializable]
public class EnemyInfo
{
    public string enemyId;
    public string name;
    public int maxHealth;
    public int damage;
    public float moveSpeed;
    public string description;
}
