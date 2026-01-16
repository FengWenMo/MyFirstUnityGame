using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
[Serializable]
public class GameFrameData
{
    public int frameNumber;              // 帧编号
    public float timestamp;              // 时间戳
    public float gameTime;               // 游戏时间
    public PlayerData playerData;        // 玩家数据
    public List<EnemyData> enemiesData;  // 敌人数据列表
    public List<WallData> wallsData;     // 墙体数据列表
    public GameStateData gameState;      // 游戏状态
}

[Serializable]
public class Vector2Data
{
    public float x;
    public float y;
    
    public Vector2Data(Vector3 vector)
    {
        x = vector.x;
        y = vector.y;
    }
}

[Serializable]
public class PlayerData
{
    public Vector2Data position;         // 位置
    public float rotation;               // 旋转角度
    public int health;                   // 当前生命值
    public int maxHealth;                // 最大生命值
    public bool isMoving;                // 是否在移动
    public float moveSpeed;              // 移动速度
}

[Serializable]
public class EnemyData
{
    public Vector2Data position;         // 位置
    public float rotation;               // 旋转角度
    public int health;                   // 当前生命值
    public int maxHealth;                // 最大生命值
    public bool isAttacking;             // 是否在攻击
    public string enemyType;             // 敌人类型
}

[Serializable]
public class WallData
{
    public Vector2Data position;         // 位置
    public float rotation;               // 旋转角度
    public int health;                   // 当前生命值
    public int maxHealth;                // 最大生命值
    public float healthPercent;          // 生命值百分比
}

[Serializable]
public class GameStateData
{
    public int coins;                    // 当前金币
    public int score;                    // 当前分数
    public int currentDay;               // 当前天数
    public string currentPhase;          // 当前阶段（白天/夜晚/血月）
    public int totalEnemies;             // 总敌人数
    public int totalWalls;               // 总墙数
}

[Serializable]
public class GameDataCollection
{
    public CollectionMetadata metadata;  // 元数据
    public GameFrameData[] frames;       // 所有帧数据
}

[Serializable]
public class CollectionMetadata
{
    public string gameName;              // 游戏名称
    public string version;               // 游戏版本
    public string collectionStartTime;   // 采集开始时间
    public int totalFrames;              // 总帧数
    public float collectionInterval;     // 采集间隔
}
