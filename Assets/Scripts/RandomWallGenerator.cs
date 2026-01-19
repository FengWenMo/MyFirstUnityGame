using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class GridWallGenerator : MonoBehaviour
{
    [Header("基础设置")]
    public GameObject wallPrefab;
    [Range(5, 100)] public int wallCount = 15;
    public float safeRadius = 5f;
    
    [Header("网格设置")]
    [Range(5, 50)] public int gridSize = 20; // 网格大小，表示从中心向四周扩展的格子数
    public float gridSpacing = 1.0f; // 网格间距
    public bool forceGridAlignment = true; // 强制网格对齐
    
    [Header("随机性")]
    [Range(0f, 1f)] public float groupingFactor = 0.3f;
    [Range(1, 50)] public int maxPlacementAttempts = 20;
    
    [Header("墙体属性")]
    public bool randomizeHealth = true;
    [Range(20, 50)] public int minHealth = 25;
    [Range(50, 100)] public int maxHealth = 60;
    
    [Header("性能保护")]
    public float maxGenerationTimePerFrame = 0.016f; // 每帧最大生成时间（秒）
    public int maxWallsPerFrame = 5; // 每帧最多生成的墙体数量
    
    [Header("调试")]
    public bool debugLogs = true;
    public bool drawGridGizmos = false;
    public Color gridColor = Color.gray;
    
    // 内部状态
    private GridCell[,] gridCells;
    private List<Vector2> gridCenters = new List<Vector2>();
    private List<GameObject> spawnedWalls = new List<GameObject>();
    private List<Vector2Int> availableGridPositions = new List<Vector2Int>();
    private List<Vector2Int> occupiedGridPositions = new List<Vector2Int>();
    
    private Transform player;
    private bool isGenerating = false;
    private bool hasGenerated = false;
    private Vector2 generationCenter = Vector2.zero;
    
    // 网格单元结构
    public struct GridCell
    {
        public Vector2 worldPosition;
        public Vector2Int gridPosition;
        public bool isAvailable;
        public float distanceToPlayer;
        public float distanceToCenter;
        
        public GridCell(Vector2 worldPos, Vector2Int gridPos, bool available, float distToPlayer, float distToCenter)
        {
            worldPosition = worldPos;
            gridPosition = gridPos;
            isAvailable = available;
            distanceToPlayer = distToPlayer;
            distanceToCenter = distToCenter;
        }
    }
    
    void Start()
    {
        CheckForDuplicateGenerators();
        StartCoroutine(Initialize());
    }
    
    void CheckForDuplicateGenerators()
    {
        GridWallGenerator[] generators = FindObjectsOfType<GridWallGenerator>();
        if (generators.Length > 1)
        {
            if (debugLogs)
                Debug.LogWarning($"发现 {generators.Length} 个网格墙体生成器实例，本实例将被销毁");
            
            if (this != generators[0])
            {
                Destroy(gameObject);
            }
        }
    }
    
    IEnumerator Initialize()
    {
        yield return null;
        
        if (hasGenerated || isGenerating)
        {
            if (debugLogs)
                Debug.LogWarning("墙体生成器已经在运行或被重复调用，跳过");
            yield break;
        }
        
        isGenerating = true;
        
        player = GameObject.FindGameObjectWithTag("Player")?.transform;
        
        if (wallPrefab == null)
        {
            Debug.LogError("墙体预制体未分配！");
            isGenerating = false;
            yield break;
        }
        
        if (gridSize <= 0)
        {
            Debug.LogError("网格大小必须大于0！");
            gridSize = 20;
        }
        
        yield return StartCoroutine(SafeGenerateWalls());
        
        isGenerating = false;
        hasGenerated = true;
    }
    
    IEnumerator SafeGenerateWalls()
    {
        if (debugLogs)
            Debug.Log("开始安全生成墙体...");
        
        // 清空现有墙体
        yield return StartCoroutine(SafeClearWalls());
        
        // 初始化网格
        InitializeGrid();
        
        // 生成墙体
        yield return StartCoroutine(GenerateWallsOnGrid());
        
        if (debugLogs)
            Debug.Log($"✅ 网格化生成完成：生成 {spawnedWalls.Count}/{wallCount} 个墙体");
    }
    
    void InitializeGrid()
    {
        if (player != null)
        {
            generationCenter = (Vector2)player.position;
        }
        
        // 计算网格大小
        int totalGridCells = gridSize * 2 + 1; // 包含中心
        gridCells = new GridCell[totalGridCells, totalGridCells];
        gridCenters.Clear();
        availableGridPositions.Clear();
        occupiedGridPositions.Clear();
        
        if (debugLogs)
            Debug.Log($"初始化网格: {totalGridCells}x{totalGridCells} 个单元格");
        
        // 填充网格
        for (int x = -gridSize; x <= gridSize; x++)
        {
            for (int y = -gridSize; y <= gridSize; y++)
            {
                int gridX = x + gridSize;
                int gridY = y + gridSize;
                
                // 计算世界位置（确保中心在x.5, y.5）
                Vector2 worldPos = new Vector2(
                    generationCenter.x + x + 0.5f,
                    generationCenter.y + y + 0.5f
                );
                
                float distanceToCenter = Vector2.Distance(worldPos, generationCenter);
                float distanceToPlayer = player != null ? 
                    Vector2.Distance(worldPos, (Vector2)player.position) : 
                    float.MaxValue;
                
                // 检查单元格是否可用
                bool isAvailable = CheckCellAvailability(worldPos, distanceToCenter, distanceToPlayer);
                
                // 创建网格单元格
                gridCells[gridX, gridY] = new GridCell(
                    worldPos,
                    new Vector2Int(x, y),
                    isAvailable,
                    distanceToPlayer,
                    distanceToCenter
                );
                
                // 如果是可用的，添加到列表
                if (isAvailable)
                {
                    availableGridPositions.Add(new Vector2Int(x, y));
                    gridCenters.Add(worldPos);
                }
            }
        }
        
        if (debugLogs)
            Debug.Log($"网格初始化完成: {availableGridPositions.Count}/{totalGridCells * totalGridCells} 个单元格可用");
    }
    
    bool CheckCellAvailability(Vector2 worldPos, float distanceToCenter, float distanceToPlayer)
    {
        // 安全检查：不在安全区域内
        if (player != null && distanceToPlayer < safeRadius)
            return false;
        
        // 安全检查：不超出最大生成半径
        float maxGenerationRadius = gridSize * gridSpacing;
        if (distanceToCenter > maxGenerationRadius)
            return false;
        
        return true;
    }
    
    IEnumerator GenerateWallsOnGrid()
    {
        if (availableGridPositions.Count == 0)
        {
            Debug.LogWarning("没有可用的网格位置！");
            yield break;
        }
        
        int wallsPlaced = 0;
        int attempts = 0;
        int maxAttempts = wallCount * maxPlacementAttempts;
        
        // 随机化可用位置列表
        List<Vector2Int> shuffledPositions = new List<Vector2Int>(availableGridPositions);
        ShuffleList(shuffledPositions);
        
        // 分帧生成，避免卡顿
        while (wallsPlaced < wallCount && shuffledPositions.Count > 0 && attempts < maxAttempts)
        {
            float startTime = Time.realtimeSinceStartup;
            
            for (int i = 0; i < Mathf.Min(maxWallsPerFrame, wallCount - wallsPlaced); i++)
            {
                if (shuffledPositions.Count == 0)
                    break;
                
                // 获取下一个网格位置
                Vector2Int gridPos = shuffledPositions[0];
                shuffledPositions.RemoveAt(0);
                
                Vector2 worldPos = GridToWorld(gridPos);
                
                // 检查集群生成概率
                if (placedPositions.Count > 0 && Random.value < groupingFactor)
                {
                    // 尝试在已放置的墙体附近生成
                    Vector2 clusterPos = GetClusterPosition(worldPos);
                    if (IsValidGridPosition(WorldToGrid(clusterPos)))
                    {
                        worldPos = clusterPos;
                    }
                }
                
                // 验证位置
                if (ValidateWallPosition(worldPos))
                {
                    CreateWallAtPosition(worldPos, wallsPlaced);
                    wallsPlaced++;
                    
                    // 标记相邻网格为不可用，避免墙体太近
                    MarkAdjacentCellsUnavailable(gridPos);
                }
                
                attempts++;
                
                // 检查是否超时
                if (Time.realtimeSinceStartup - startTime > maxGenerationTimePerFrame)
                {
                    yield return null; // 让出一帧
                    startTime = Time.realtimeSinceStartup;
                }
            }
            
            if (shuffledPositions.Count > 0)
                yield return null; // 让出一帧，避免卡顿
        }
        
        if (debugLogs)
        {
            if (wallsPlaced < wallCount)
                Debug.LogWarning($"⚠️ 只生成了 {wallsPlaced}/{wallCount} 个墙体，网格位置不足");
        }
    }
    
    Vector2 GridToWorld(Vector2Int gridPos)
    {
        return new Vector2(
            generationCenter.x + gridPos.x + 0.5f,
            generationCenter.y + gridPos.y + 0.5f
        );
    }
    
    Vector2Int WorldToGrid(Vector2 worldPos)
    {
        return new Vector2Int(
            Mathf.RoundToInt(worldPos.x - generationCenter.x - 0.5f),
            Mathf.RoundToInt(worldPos.y - generationCenter.y - 0.5f)
        );
    }
    
    bool IsValidGridPosition(Vector2Int gridPos)
    {
        int gridX = gridPos.x + gridSize;
        int gridY = gridPos.y + gridSize;
        
        if (gridX < 0 || gridX >= gridCells.GetLength(0) || 
            gridY < 0 || gridY >= gridCells.GetLength(1))
            return false;
        
        return gridCells[gridX, gridY].isAvailable;
    }
    
    Vector2 GetClusterPosition(Vector2 basePosition)
    {
        if (placedPositions.Count == 0)
            return basePosition;
        
        Vector2 clusterCenter = placedPositions[Random.Range(0, placedPositions.Count)];
        Vector2 direction = (basePosition - clusterCenter).normalized;
        
        if (direction.magnitude < 0.001f)
            direction = Random.insideUnitCircle.normalized;
        
        // 在集群中心附近1-3个网格内生成
        int distance = Random.Range(1, 4);
        Vector2 clusterPos = clusterCenter + direction * distance;
        
        // 确保位置对齐到网格
        if (forceGridAlignment)
        {
            Vector2Int gridPos = WorldToGrid(clusterPos);
            clusterPos = GridToWorld(gridPos);
        }
        
        return clusterPos;
    }
    
    bool ValidateWallPosition(Vector2 worldPos)
    {
        // 检查网格位置是否有效
        Vector2Int gridPos = WorldToGrid(worldPos);
        if (!IsValidGridPosition(gridPos))
            return false;
        
        // 检查是否与已有墙体太近
        foreach (Vector2 placedPos in placedPositions)
        {
            if (Vector2.Distance(worldPos, placedPos) < gridSpacing)
                return false;
        }
        
        return true;
    }
    
    void MarkAdjacentCellsUnavailable(Vector2Int gridPos)
    {
        // 将相邻的网格标记为不可用，避免墙体太近
        for (int x = -1; x <= 1; x++)
        {
            for (int y = -1; y <= 1; y++)
            {
                if (x == 0 && y == 0) continue; // 跳过中心单元格
                
                Vector2Int adjacentPos = new Vector2Int(gridPos.x + x, gridPos.y + y);
                int gridX = adjacentPos.x + gridSize;
                int gridY = adjacentPos.y + gridSize;
                
                if (gridX >= 0 && gridX < gridCells.GetLength(0) &&
                    gridY >= 0 && gridY < gridCells.GetLength(1))
                {
                    // 标记为不可用
                    var cell = gridCells[gridX, gridY];
                    cell.isAvailable = false;
                    gridCells[gridX, gridY] = cell;
                    
                    // 从可用列表中移除
                    availableGridPositions.Remove(adjacentPos);
                }
            }
        }
    }
    
    List<Vector2> placedPositions = new List<Vector2>();
    
    void CreateWallAtPosition(Vector2 position, int index)
    {
        try
        {
            GameObject wall = Instantiate(
                wallPrefab,
                new Vector3(position.x, position.y, 0),
                Quaternion.identity,
                transform
            );
            
            wall.name = $"GridWall_{index:000}_({position.x:F1},{position.y:F1})";
            spawnedWalls.Add(wall);
            placedPositions.Add(position);
            
            if (randomizeHealth)
            {
                WallHealth health = wall.GetComponent<WallHealth>();
                if (health != null)
                {
                    int healthValue = Random.Range(minHealth, maxHealth + 1);
                    health.maxHealth = healthValue;
                    health.currentHealth = healthValue;
                }
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"创建墙体时出错: {e.Message}");
        }
    }
    
    IEnumerator SafeClearWalls()
    {
        if (spawnedWalls.Count == 0)
            yield break;
        
        if (debugLogs)
            Debug.Log($"开始安全清理 {spawnedWalls.Count} 个墙体...");
        
        int clearedPerFrame = 0;
        for (int i = spawnedWalls.Count - 1; i >= 0; i--)
        {
            if (spawnedWalls[i] != null)
            {
                Destroy(spawnedWalls[i]);
                clearedPerFrame++;
                
                if (clearedPerFrame >= 10) // 每帧最多清理10个
                {
                    clearedPerFrame = 0;
                    yield return null;
                }
            }
        }
        
        spawnedWalls.Clear();
        placedPositions.Clear();
        
        // 清理子物体
        clearedPerFrame = 0;
        for (int i = transform.childCount - 1; i >= 0; i--)
        {
            Destroy(transform.GetChild(i).gameObject);
            clearedPerFrame++;
            
            if (clearedPerFrame >= 10)
            {
                clearedPerFrame = 0;
                yield return null;
            }
        }
    }
    
    void ShuffleList<T>(List<T> list)
    {
        for (int i = 0; i < list.Count; i++)
        {
            T temp = list[i];
            int randomIndex = Random.Range(i, list.Count);
            list[i] = list[randomIndex];
            list[randomIndex] = temp;
        }
    }
    
    public void Regenerate()
    {
        if (!Application.isPlaying || isGenerating)
        {
            if (debugLogs)
                Debug.LogWarning("墙体生成器正在运行，请等待完成");
            return;
        }
        
        StartCoroutine(SafeRegenerate());
    }
    
    IEnumerator SafeRegenerate()
    {
        isGenerating = true;
        yield return StartCoroutine(SafeGenerateWalls());
        isGenerating = false;
    }
    
    void OnDestroy()
    {
        StopAllCoroutines();
        StartCoroutine(SafeClearWalls());
    }
    
    void OnDrawGizmosSelected()
    {
        if (!Application.isPlaying || !drawGridGizmos)
            return;
        
        // 绘制网格
        Gizmos.color = gridColor;
        float gridWorldSize = gridSize * gridSpacing;
        
        for (int x = -gridSize; x <= gridSize; x++)
        {
            for (int y = -gridSize; y <= gridSize; y++)
            {
                Vector2 worldPos = new Vector2(
                    generationCenter.x + x + 0.5f,
                    generationCenter.y + y + 0.5f
                );
                
                // 只绘制在生成半径内的网格
                if (Vector2.Distance(worldPos, generationCenter) <= gridWorldSize)
                {
                    Gizmos.DrawWireCube(
                        new Vector3(worldPos.x, worldPos.y, 0),
                        new Vector3(0.9f, 0.9f, 0.1f)
                    );
                }
            }
        }
        
        // 绘制安全区域
        Gizmos.color = new Color(1, 0, 0, 0.3f);
        Gizmos.DrawWireSphere(generationCenter, safeRadius);
        
        // 绘制生成区域
        Gizmos.color = new Color(0, 1, 0, 0.2f);
        Gizmos.DrawWireSphere(generationCenter, gridWorldSize);
        
        // 绘制已生成的墙体位置
        Gizmos.color = Color.blue;
        foreach (Vector2 pos in placedPositions)
        {
            Gizmos.DrawCube(new Vector3(pos.x, pos.y, 0), new Vector3(0.5f, 0.5f, 0.1f));
        }
    }
}