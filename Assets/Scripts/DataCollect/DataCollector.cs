using System.Collections;
using System.Collections.Generic;
using System;
using System.IO;
using UnityEngine;
public class DataCollector : MonoBehaviour
{
    [Header("采集设置")]
    public float collectionInterval = 0.1f;  // 采集间隔（秒），0.1=每秒10次
    public bool collectPositions = true;     // 是否采集位置信息
    public bool collectStates = true;        // 是否采集状态信息
    public bool collectEvents = true;        // 是否采集事件信息
    
    [Header("输出设置")]
    public bool logToConsole = true;         // 输出到控制台
    public bool saveToFile = true;           // 保存到文件
    public string fileName = "game_data";    // 文件名（不含扩展名）
    
    [Header("调试")]
    public bool showCollectionInfo = true;   // 显示采集信息
    
    // 数据存储
    private List<GameFrameData> frameData = new List<GameFrameData>();
    private float collectionTimer = 0f;
    private int frameCount = 0;
    private StreamWriter fileWriter;
    
    // 单例模式
    public static DataCollector Instance { get; private set; }
    
    void Awake()
    {
        // 设置单例
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    void Start()
    {
        InitializeDataCollection();
    }
    
    void Update()
    {
        // 定时采集数据
        collectionTimer += Time.deltaTime;
        if (collectionTimer >= collectionInterval)
        {
            collectionTimer = 0f;
            CollectFrameData();
        }
    }
    
    void OnDestroy()
    {
        // 保存数据
        if (saveToFile && fileWriter != null)
        {
            SaveDataToJson();
            fileWriter.Close();
        }
    }
    
    void InitializeDataCollection()
{
    if (saveToFile)
    {
        // 创建唯一文件名（带时间戳）
        string timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
        string fullFileName = $"{fileName}_{timestamp}.json";
        
        // 自定义保存路径
        string customPath = @"D:\GameData";  // 使用 @ 避免转义字符
        
        // 确保目录存在
        if (!Directory.Exists(customPath))
        {
            try
            {
                Directory.CreateDirectory(customPath);
                Debug.Log($"创建自定义目录: {customPath}");
            }
            catch (System.Exception e)
            {
                Debug.LogError($"创建目录失败: {e.Message}");
                Debug.LogWarning($"将使用默认路径: {Application.persistentDataPath}");
                customPath = Application.persistentDataPath; // 回退到默认路径
            }
        }
        
        // 创建完整文件路径
        string filePath = Path.Combine(customPath, fullFileName);
        
        try
        {
            // 创建文件写入器
            fileWriter = new StreamWriter(filePath, true);
            Debug.Log($"数据将保存到: {filePath}");
            
            // 记录文件创建成功
            fileWriter.WriteLine("{\"info\": \"数据收集开始\", \"timestamp\": \"" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "\", \"game\": \"末日僵尸生存\"},");
            fileWriter.Flush(); // 立即写入
            
            Debug.Log($"文件创建成功: {filePath}");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"创建文件失败: {e.Message}");
            Debug.LogError($"尝试路径: {filePath}");
            
            // 尝试备用路径
            //TryAlternativePath(fullFileName, customPath);
        }
    }
    
    Debug.Log($"数据采集器初始化完成");
    Debug.Log($"采集间隔: {collectionInterval}秒");
    Debug.Log($"每秒采集次数: {1f/collectionInterval}");
}
    
    void CollectFrameData()
    {
        frameCount++;
        GameFrameData frame = new GameFrameData();
        
        // 设置基本信息
        frame.frameNumber = frameCount;
        frame.timestamp = Time.time;
        frame.gameTime = Time.timeSinceLevelLoad;
        
        // 收集玩家数据
        if (collectPositions)
        {
            frame.playerData = CollectPlayerData();
        }
        
        // 收集敌人数据
        if (collectPositions)
        {
            frame.enemiesData = CollectEnemiesData();
        }
        
        // 收集墙体数据
        if (collectPositions)
        {
            frame.wallsData = CollectWallsData();
        }
        
        // 收集游戏状态
        if (collectStates)
        {
            frame.gameState = CollectGameState();
        }
        
        // 添加到列表
        frameData.Add(frame);
        
        // 输出到控制台
        if (logToConsole && showCollectionInfo)
        {
            LogFrameData(frame);
        }
        
        // 写入文件
        if (saveToFile && fileWriter != null)
        {
            string json = JsonUtility.ToJson(frame, true);
            fileWriter.WriteLine(json + ",");
        }
    }
    
    PlayerData CollectPlayerData()
    {
        PlayerData data = new PlayerData();
        
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            data.position = new Vector2Data(player.transform.position);
            data.rotation = player.transform.eulerAngles.z;
            
            // 收集玩家状态
            PlayerHealth playerHealth = player.GetComponent<PlayerHealth>();
            if (playerHealth != null)
            {
                data.health = playerHealth.currentHealth;
                data.maxHealth = playerHealth.maxHealth;
            }
            
            // 收集移动状态
            PlayerMovement playerMovement = player.GetComponent<PlayerMovement>();
            if (playerMovement != null)
            {
                data.isMoving = playerMovement.isMoving; // 需要修改PlayerMovement暴露此状态
            }
        }
        
        return data;
    }
    
    List<EnemyData> CollectEnemiesData()
    {
        List<EnemyData> enemies = new List<EnemyData>();
        
        // 查找所有敌人
        GameObject[] enemyObjects = GameObject.FindGameObjectsWithTag("Enemy");
        foreach (GameObject enemy in enemyObjects)
        {
            EnemyData data = new EnemyData();
            data.position = new Vector2Data(enemy.transform.position);
            data.rotation = enemy.transform.eulerAngles.z;
            
            // 收集敌人状态
            EnemyHealth enemyHealth = enemy.GetComponent<EnemyHealth>();
            if (enemyHealth != null)
            {
                data.health = enemyHealth.currentHealth;
                data.maxHealth = enemyHealth.maxHealth;
            }
            
            // 收集敌人AI状态
            EnemyAI enemyAI = enemy.GetComponent<EnemyAI>();
            if (enemyAI != null)
            {
                data.isAttacking = enemyAI.isAttacking; // 需要EnemyAI暴露此状态
            }
            
            enemies.Add(data);
        }
        
        return enemies;
    }
    
    List<WallData> CollectWallsData()
    {
        List<WallData> walls = new List<WallData>();
        
        // 查找所有墙体
        GameObject[] wallObjects = GameObject.FindGameObjectsWithTag("Wall");
        foreach (GameObject wall in wallObjects)
        {
            WallData data = new WallData();
            data.position = new Vector2Data(wall.transform.position);
            data.rotation = wall.transform.eulerAngles.z;
            
            // 收集墙体状态
            WallHealth wallHealth = wall.GetComponent<WallHealth>();
            if (wallHealth != null)
            {
                data.health = wallHealth.currentHealth;
                data.maxHealth = wallHealth.maxHealth;
                data.healthPercent = wallHealth.GetHealthPercent();
            }
            
            walls.Add(data);
        }
        
        return walls;
    }
    
    GameStateData CollectGameState()
    {
        GameStateData state = new GameStateData();
        
        // 收集金币和分数
        if (CoinManager.Instance != null)
        {
            state.coins = CoinManager.Instance.GetCurrentCoins();
        }
        
        if (ScoreManager.Instance != null)
        {
            state.score = ScoreManager.Instance.GetCurrentScore();
        }
        
        // 收集游戏时间状态
        GameTimeManager timeManager = FindObjectOfType<GameTimeManager>();
        if (timeManager != null)
        {
            state.currentDay = timeManager.CurrentDay;
            state.currentPhase = timeManager.CurrentPhase.ToString();
        }
        
        return state;
    }
    
    void LogFrameData(GameFrameData frame)
    {
        if (frameCount % 10 == 0) // 每10帧输出一次概要
        {
            Debug.Log($"帧: {frame.frameNumber}, 时间: {frame.timestamp:F2}");
            Debug.Log($"玩家: {frame.playerData.position.x:F1}, {frame.playerData.position.y:F1}");
            Debug.Log($"敌人数量: {frame.enemiesData.Count}");
            Debug.Log($"墙体数量: {frame.wallsData.Count}");
        }
    }
    
    void SaveDataToJson()
    {
        if (frameData.Count == 0) return;
        
        // 创建完整的数据结构
        GameDataCollection completeData = new GameDataCollection
        {
            metadata = new CollectionMetadata
            {
                gameName = "末日僵尸生存",
                version = "1.0",
                collectionStartTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                totalFrames = frameCount,
                collectionInterval = collectionInterval
            },
            frames = frameData.ToArray()
        };
        
        // 保存为JSON
        string json = JsonUtility.ToJson(completeData, true);
        string filePath = Path.Combine(Application.persistentDataPath, $"{fileName}_complete_{DateTime.Now:yyyyMMdd_HHmmss}.json");
        File.WriteAllText(filePath, json);
        
        Debug.Log($"完整数据已保存到: {filePath}");
        Debug.Log($"总帧数: {frameCount}");
        Debug.Log($"文件大小: {json.Length / 1024} KB");
    }
    
    // 公开方法：手动触发数据采集
    public void CollectNow()
    {
        CollectFrameData();
    }
    
    // 公开方法：获取当前数据快照
    public GameFrameData GetCurrentSnapshot()
    {
        if (frameData.Count > 0)
            return frameData[frameData.Count - 1];
        return null;
    }
}