using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridSystem : MonoBehaviour
{
    [Header("网格设置")]
    public float gridSize = 1f; // 每个网格的大小（与墙体大小匹配）
    public LayerMask groundLayer; // 地面层级，用于射线检测
    
    [Header("网格显示设置")]
    public float lineWidth = 0.03f; // 网格线宽度
    public Color gridColor = new Color(0.8f, 0.8f, 0.8f, 1f); // 网格颜色（灰色，40%透明度）
    
    private GameObject gridVisual; // 网格可视化对象
    private LineRenderer lineRenderer; // 用于绘制网格线
    
    void Start()
    {
        CreateGridVisual();
        Debug.Log("网格系统初始化完成");
    }
    
    // 创建网格可视化
    private void CreateGridVisual()
    {
        // 如果已经存在网格对象，先销毁
        if (gridVisual != null)
        {
            Destroy(gridVisual);
        }
        
        // 创建新的网格对象
        gridVisual = new GameObject("GridVisual");
        gridVisual.transform.SetParent(transform);
        
        // 创建并配置LineRenderer
        lineRenderer = gridVisual.AddComponent<LineRenderer>();
        ConfigureLineRenderer();
        
        // 初始状态为隐藏
        gridVisual.SetActive(true);
        
        Debug.Log("网格可视化对象已创建");
    }
    
    // 配置LineRenderer的所有属性
    private void ConfigureLineRenderer()
    {
        // 1. 设置材质
        Material lineMaterial = new Material(Shader.Find("Sprites/Default"));
        lineMaterial.color = gridColor;
        lineRenderer.material = lineMaterial;
        
        // 2. 设置宽度曲线
        AnimationCurve widthCurve = new AnimationCurve();
        widthCurve.AddKey(0.0f, lineWidth);
        widthCurve.AddKey(1.0f, lineWidth);
        lineRenderer.widthCurve = widthCurve;
        
        // 3. 设置颜色渐变
        Gradient gradient = new Gradient();
        gradient.SetKeys(
            new GradientColorKey[] 
            { 
                new GradientColorKey(gridColor, 0.0f), 
                new GradientColorKey(gridColor, 1.0f) 
            },
            new GradientAlphaKey[] 
            { 
                new GradientAlphaKey(gridColor.a, 0.0f), 
                new GradientAlphaKey(gridColor.a, 1.0f) 
            }
        );
        lineRenderer.colorGradient = gradient;
        
        // 4. 设置其他属性
        lineRenderer.useWorldSpace = true;
        lineRenderer.loop = false;
        lineRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        lineRenderer.receiveShadows = false;
        lineRenderer.alignment = LineAlignment.View;
        lineRenderer.textureMode = LineTextureMode.Stretch;
        lineRenderer.numCornerVertices = 0;
        lineRenderer.numCapVertices = 0;
        
        // 5. 立即绘制一个测试网格
        DrawTestGrid();
    }
    
    // 绘制一个测试网格（调试用）
    private void DrawTestGrid()
    {
        // 创建一个简单的4x4网格
        List<Vector3> positions = new List<Vector3>();
        int gridCells = 4; // 4x4网格
        
        // 绘制垂直线
        for (int x = -gridCells; x <= gridCells; x++)
        {
            float xPos = x * gridSize;
            positions.Add(new Vector3(xPos, -gridCells * gridSize, 0));
            positions.Add(new Vector3(xPos, gridCells * gridSize, 0));
        }
        
        // 绘制水平线
        for (int y = -gridCells; y <= gridCells; y++)
        {
            float yPos = y * gridSize;
            positions.Add(new Vector3(-gridCells * gridSize, yPos, 0));
            positions.Add(new Vector3(gridCells * gridSize, yPos, 0));
        }
        
        lineRenderer.positionCount = positions.Count;
        lineRenderer.SetPositions(positions.ToArray());
    }
    
    // 显示/隐藏网格
    public void SetGridVisible(bool visible)
    {
        if (gridVisual != null)
        {
            gridVisual.SetActive(visible);
            
            if (visible)
            {
                // 重新绘制网格以确保设置正确
                ConfigureLineRenderer();
                Debug.Log("网格已显示");
            }
        }
    }
    
    // 获取鼠标位置的网格对齐坐标
    public Vector3 GetGridPosition(Vector3 worldPosition)
    {
        float x = Mathf.Round(worldPosition.x / gridSize) * gridSize;
        float y = Mathf.Round(worldPosition.y / gridSize) * gridSize;
        return new Vector3(x, y, 0);
    }
    
    // 检查位置是否在网格上（在地面上）
    public bool IsPositionOnGrid(Vector3 worldPosition)
    {
        // 向下发射射线检测地面
        RaycastHit2D hit = Physics2D.Raycast(
            worldPosition + Vector3.up * 10f, 
            Vector2.down, 
            20f, 
            groundLayer
        );
        
        return hit.collider != null;
    }
    
    // 在指定区域绘制网格
    public void DrawGridInArea(Vector3 center, int radius)
    {
        if (lineRenderer == null) return;
        
        List<Vector3> positions = new List<Vector3>();
        
        // 计算区域边界（网格单位）
        int minX = Mathf.FloorToInt((center.x - radius * gridSize) / gridSize);
        int maxX = Mathf.CeilToInt((center.x + radius * gridSize) / gridSize);
        int minY = Mathf.FloorToInt((center.y - radius * gridSize) / gridSize);
        int maxY = Mathf.CeilToInt((center.y + radius * gridSize) / gridSize);
        
        // 绘制垂直线
        for (int x = minX; x <= maxX; x++)
        {
            float xPos = x * gridSize;
            positions.Add(new Vector3(xPos, minY * gridSize, 0));
            positions.Add(new Vector3(xPos, maxY * gridSize, 0));
        }
        
        // 绘制水平线
        for (int y = minY; y <= maxY; y++)
        {
            float yPos = y * gridSize;
            positions.Add(new Vector3(minX * gridSize, yPos, 0));
            positions.Add(new Vector3(maxX * gridSize, yPos, 0));
        }
        
        lineRenderer.positionCount = positions.Count;
        lineRenderer.SetPositions(positions.ToArray());
        
        Debug.Log($"在区域({center}, 半径:{radius})绘制网格，共{positions.Count}个点");
    }
}