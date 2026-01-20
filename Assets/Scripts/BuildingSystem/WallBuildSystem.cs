using System.Collections;
using System.Collections.Generic;
// WallBuildSystem.cs
// WallBuildSystem.cs - 修复版
// WallBuildSystem.cs
using UnityEngine;

public class WallBuildSystem : MonoBehaviour
{
    [Header("对象引用")]
    [SerializeField] private GameObject wallPrefab; // 墙的预制体
    [SerializeField] private Transform playerTransform; // 玩家Transform
    [SerializeField] private GridDisplay gridDisplay; // 网格显示组件
    [SerializeField] private CoinManager coinManager; // 金币管理器

    [Header("建造设置")]
    [SerializeField] private int wallCost = 10; // 建造花费
    [SerializeField] private Color ghostValidColor = new Color(0f, 1f, 0f, 0.5f); // 可建造颜色
    [SerializeField] private Color ghostInvalidColor = new Color(1f, 0f, 0f, 0.5f); // 不可建造颜色
    
    [Header("建造范围设置（可调节）")]
    [Tooltip("以玩家为中心的建造区域大小（单位：格）。例如：8表示8x8的区域")]
    [SerializeField] private int buildRange = 8; // 可调节的建造范围
    
    [Tooltip("是否允许在玩家所在位置建造")]
    [SerializeField] private bool allowBuildOnPlayerCell = false;
    
    [Tooltip("建造范围预览颜色")]
    [SerializeField] private Color rangePreviewColor = new Color(0.5f, 0.5f, 1f, 0.2f);
    [SerializeField] private GameObject rangePreviewObject; // 可选：建造范围预览对象

    private bool isBuildMode = false;
    private GameObject ghostWall; // 虚影墙
    private SpriteRenderer ghostRenderer;
    private Vector2Int currentGridCell = Vector2Int.one * -1;
    private bool canBuildAtCurrentCell = true;
    private Material rangePreviewMaterial;

    void Start()
    {
        // 自动查找组件（如果未手动赋值）
        if (playerTransform == null)
        {
            GameObject player = GameObject.Find("Player");
            if (player != null) playerTransform = player.transform;
        }
        
        if (gridDisplay == null)
        {
            gridDisplay = FindObjectOfType<GridDisplay>();
        }
        
        if (coinManager == null && CoinManager.Instance != null)
        {
            coinManager = CoinManager.Instance;
        }

        // 创建虚影墙
        CreateGhostWall();
        
        // 创建建造范围预览
        CreateRangePreview();
    }

    void CreateGhostWall()
    {
        if (wallPrefab == null)
        {
            Debug.LogError("Wall prefab is not assigned!");
            return;
        }

        ghostWall = Instantiate(wallPrefab);
        ghostWall.name = "GhostWall";
        ghostRenderer = ghostWall.GetComponent<SpriteRenderer>();
        
        if (ghostRenderer == null)
        {
            ghostRenderer = ghostWall.AddComponent<SpriteRenderer>();
        }
        
        // 设置虚影材质和颜色
        var material = new Material(Shader.Find("Sprites/Default"));
        material.color = ghostValidColor;
        ghostRenderer.material = material;
        
        // 禁用碰撞体
        var collider = ghostWall.GetComponent<Collider2D>();
        if (collider != null) collider.enabled = false;
        
        ghostWall.SetActive(false);
    }
    
    void CreateRangePreview()
    {
        if (rangePreviewObject == null)
        {
            // 创建一个简单的范围预览对象
            rangePreviewObject = GameObject.CreatePrimitive(PrimitiveType.Quad);
            rangePreviewObject.name = "BuildRangePreview";
            rangePreviewObject.transform.parent = this.transform;
            
            // 移除碰撞体
            Destroy(rangePreviewObject.GetComponent<Collider>());
            
            // 创建半透明材质
            rangePreviewMaterial = new Material(Shader.Find("Sprites/Default"));
            rangePreviewMaterial.color = rangePreviewColor;
            
            var renderer = rangePreviewObject.GetComponent<Renderer>();
            renderer.material = rangePreviewMaterial;
            renderer.sortingOrder = -1; // 确保在墙后面
        }
        
        // 初始时隐藏
        rangePreviewObject.SetActive(false);
        
        // 根据建造范围设置大小
        rangePreviewObject.transform.localScale = new Vector3(buildRange, buildRange, 1f);
    }

    void Update()
    {
        // 切换建造模式：Shift+B
        if (Input.GetKey(KeyCode.LeftShift) && Input.GetKeyDown(KeyCode.B))
        {
            ToggleBuildMode();
        }

        if (!isBuildMode) return;

        // 在建造模式下
        UpdateGhostWall();
        UpdateRangePreview();
        
        // 检测鼠标点击建造
        if (Input.GetMouseButtonDown(0))
        {
            TryBuildWall();
        }
    }

    void ToggleBuildMode()
    {
        isBuildMode = !isBuildMode;
        
        // 通知网格显示系统
        if (gridDisplay != null)
        {
            gridDisplay.SetBuildMode(isBuildMode, buildRange);
        }
        
        // 显示/隐藏虚影墙
        ghostWall.SetActive(isBuildMode);
        
        // 显示/隐藏范围预览
        if (rangePreviewObject != null)
        {
            rangePreviewObject.SetActive(isBuildMode);
        }
        
        Debug.Log($"Build Mode: {isBuildMode}, Build Range: {buildRange}");
    }
    
    void UpdateRangePreview()
    {
        if (rangePreviewObject == null || playerTransform == null) return;
        
        // 更新位置跟随玩家
        Vector3 playerPos = playerTransform.position;
        Vector3 gridCenter = new Vector3(
            Mathf.Floor(playerPos.x) + 0.5f,
            Mathf.Floor(playerPos.y) + 0.5f,
            0
        );
        
        rangePreviewObject.transform.position = new Vector3(
            gridCenter.x,
            gridCenter.y,
            rangePreviewObject.transform.position.z
        );
    }

    void UpdateGhostWall()
    {
        if (playerTransform == null) return;

        // 获取鼠标世界坐标
        Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mouseWorldPos.z = 0;

        // 计算玩家为中心的建造网格位置
        Vector3 playerPos = playerTransform.position;
        Vector3 gridCenter = new Vector3(
            Mathf.Floor(playerPos.x) + 0.5f,
            Mathf.Floor(playerPos.y) + 0.5f,
            0
        );
        
        float halfSize = buildRange / 2f;
        
        Vector3 bottomLeft = new Vector3(
            gridCenter.x - halfSize,
            gridCenter.y - halfSize,
            gridCenter.z
        );

        // 计算鼠标所在的网格单元格
        int gridX = Mathf.FloorToInt(mouseWorldPos.x - bottomLeft.x);
        int gridY = Mathf.FloorToInt(mouseWorldPos.y - bottomLeft.y);
        
        currentGridCell = new Vector2Int(gridX, gridY);
        
        // 更新网格显示
        if (gridDisplay != null)
        {
            gridDisplay.SetCurrentGridCell(currentGridCell);
        }

        // 检查是否在网格范围内
        canBuildAtCurrentCell = gridX >= 0 && gridX < buildRange && 
                               gridY >= 0 && gridY < buildRange;
        
        // 检查是否在玩家所在单元格
        if (!allowBuildOnPlayerCell)
        {
            Vector3Int playerCell = new Vector3Int(
                Mathf.FloorToInt(playerPos.x - bottomLeft.x),
                Mathf.FloorToInt(playerPos.y - bottomLeft.y),
                0
            );
            
            if (gridX == playerCell.x && gridY == playerCell.y)
            {
                canBuildAtCurrentCell = false;
            }
        }

        if (canBuildAtCurrentCell)
        {
            // 计算墙体的中心位置（x.5, y.5）
            float wallX = bottomLeft.x + gridX + 0.5f;
            float wallY = bottomLeft.y + gridY + 0.5f;
            
            ghostWall.transform.position = new Vector3(wallX, wallY, 0);
            
            // 检查是否可以建造（金币是否足够）
            bool canAfford = coinManager != null && coinManager.HasEnoughCoins(wallCost);
            
            // 设置虚影颜色
            if (canAfford)
            {
                ghostRenderer.color = ghostValidColor;
            }
            else
            {
                ghostRenderer.color = ghostInvalidColor;
                canBuildAtCurrentCell = false;
            }
            
            // 检查该位置是否已经有墙
            Collider2D[] colliders = Physics2D.OverlapBoxAll(
                new Vector2(wallX, wallY), 
                new Vector2(0.8f, 0.8f), 
                0f
            );
            
            foreach (var collider in colliders)
            {
                if (collider.gameObject != ghostWall && 
                    collider.gameObject.CompareTag("Wall"))
                {
                    ghostRenderer.color = ghostInvalidColor;
                    canBuildAtCurrentCell = false;
                    break;
                }
            }
        }
        else
        {
            // 将虚影墙移到屏幕外
            ghostWall.transform.position = new Vector3(1000, 1000, 0);
            ghostRenderer.color = ghostInvalidColor;
        }
    }

    void TryBuildWall()
    {
        if (!canBuildAtCurrentCell || wallPrefab == null) return;

        // 检查金币
        if (coinManager != null && !coinManager.SpendCoins(wallCost))
        {
            Debug.Log("Not enough coins!");
            return;
        }

        // 获取建造位置
        Vector3 buildPosition = ghostWall.transform.position;
        
        // 实例化墙体
        GameObject newWall = Instantiate(wallPrefab, buildPosition, Quaternion.identity);
        newWall.name = "BuiltWall";
        
        // 可选：添加标签以便后续识别
        newWall.tag = "Wall";
        
        Debug.Log($"Wall built at ({buildPosition.x:F1}, {buildPosition.y:F1}), Cost: {wallCost}");
    }

    // 调试绘制建造范围
    void OnDrawGizmosSelected()
    {
        if (playerTransform == null) return;
        
        Gizmos.color = Color.yellow;
        Vector3 playerPos = Application.isPlaying ? playerTransform.position : Vector3.zero;
        
        Vector3 gridCenter = new Vector3(
            Mathf.Floor(playerPos.x) + 0.5f,
            Mathf.Floor(playerPos.y) + 0.5f,
            0
        );
        
        Gizmos.DrawWireCube(gridCenter, new Vector3(buildRange, buildRange, 0));
    }
    
    // 公开方法，可供其他脚本调用修改建造范围
    public void SetBuildRange(int newRange)
    {
        if (newRange < 1) 
        {
            Debug.LogWarning("Build range must be at least 1!");
            return;
        }
        
        buildRange = newRange;
        
        // 更新范围预览大小
        if (rangePreviewObject != null)
        {
            rangePreviewObject.transform.localScale = new Vector3(buildRange, buildRange, 1f);
        }
        
        Debug.Log($"Build range changed to: {buildRange}");
    }
    
    // 获取当前建造范围
    public int GetBuildRange()
    {
        return buildRange;
    }
}