using System.Collections;
using System.Collections.Generic;
// GridDisplay.cs
// GridDisplay.cs
using UnityEngine;

public class GridDisplay : MonoBehaviour
{
    [Header("玩家引用")]
    [SerializeField] private Transform playerTransform; // 玩家Transform
    
    [Header("网格设置")]
    [SerializeField] private Color gridColor = new Color(1f, 1f, 1f, 0.3f); // 网格线颜色
    [SerializeField] private Color highlightColor = new Color(0f, 0.8f, 1f, 0.5f); // 高亮单元格颜色
    
    [Header("建造范围（可调节）")]
    [Tooltip("以玩家为中心的网格大小（单位：格）。例如：8表示8x8的建造区域")]
    [SerializeField] private int gridSize = 8; // 可调节的网格大小
    
    [Tooltip("是否显示中心线")]
    [SerializeField] private bool showCenterLines = true;
    [SerializeField] private Color centerLineColor = new Color(1f, 0f, 0f, 0.5f); // 中心线颜色
    
    private Material lineMaterial;
    private Vector2Int currentGridCell = Vector2Int.one * -1;
    private bool isBuildMode = false;
    private int currentBuildRange = 0; // 当前使用的建造范围

    void Start()
    {
        // 创建用于绘制线条的材质
        CreateLineMaterial();
        
        // 自动查找玩家（如果未手动赋值）
        if (playerTransform == null)
        {
            GameObject player = GameObject.Find("Player");
            if (player != null) playerTransform = player.transform;
        }
    }

    void CreateLineMaterial()
    {
        if (!lineMaterial)
        {
            var shader = Shader.Find("Hidden/Internal-Colored");
            lineMaterial = new Material(shader);
            lineMaterial.hideFlags = HideFlags.HideAndDontSave;
            lineMaterial.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
            lineMaterial.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
            lineMaterial.SetInt("_Cull", (int)UnityEngine.Rendering.CullMode.Off);
            lineMaterial.SetInt("_ZWrite", 0);
        }
    }

    public void SetBuildMode(bool active, int buildRange = 0)
    {
        isBuildMode = active;
        if (buildRange > 0)
        {
            currentBuildRange = buildRange;
        }
        else
        {
            currentBuildRange = gridSize; // 使用Inspector中设置的值
        }
        
        if (!active) currentGridCell = Vector2Int.one * -1;
    }

    public void SetCurrentGridCell(Vector2Int cell)
    {
        currentGridCell = cell;
    }

    void OnRenderObject()
    {
        if (!isBuildMode || playerTransform == null) return;

        // 设置绘制材质
        lineMaterial.SetPass(0);
        GL.PushMatrix();
        GL.MultMatrix(Matrix4x4.identity);
        
        // 计算网格边界（玩家为中心的建造区域）
        Vector3 playerPos = playerTransform.position;
        Vector3 gridCenter = new Vector3(
            Mathf.Floor(playerPos.x) + 0.5f,
            Mathf.Floor(playerPos.y) + 0.5f,
            0
        );
        
        float displayGridSize = currentBuildRange;
        float halfSize = displayGridSize / 2f;
        
        Vector3 bottomLeft = new Vector3(
            gridCenter.x - halfSize,
            gridCenter.y - halfSize,
            gridCenter.z
        );

        // 绘制网格线
        GL.Begin(GL.LINES);
        
        // 绘制普通网格线
        GL.Color(gridColor);
        
        // 绘制垂直线
        for (int i = 0; i <= displayGridSize; i++)
        {
            float x = bottomLeft.x + i;
            GL.Vertex3(x, bottomLeft.y, 0);
            GL.Vertex3(x, bottomLeft.y + displayGridSize, 0);
        }

        // 绘制水平线
        for (int i = 0; i <= displayGridSize; i++)
        {
            float y = bottomLeft.y + i;
            GL.Vertex3(bottomLeft.x, y, 0);
            GL.Vertex3(bottomLeft.x + displayGridSize, y, 0);
        }
        
        // 绘制中心线
        if (showCenterLines)
        {
            GL.Color(centerLineColor);
            
            // 垂直中心线
            float centerX = bottomLeft.x + halfSize;
            GL.Vertex3(centerX, bottomLeft.y, 0);
            GL.Vertex3(centerX, bottomLeft.y + displayGridSize, 0);
            
            // 水平中心线
            float centerY = bottomLeft.y + halfSize;
            GL.Vertex3(bottomLeft.x, centerY, 0);
            GL.Vertex3(bottomLeft.x + displayGridSize, centerY, 0);
        }

        // 绘制高亮单元格
        if (currentGridCell.x >= 0 && currentGridCell.x < displayGridSize && 
            currentGridCell.y >= 0 && currentGridCell.y < displayGridSize)
        {
            GL.Color(highlightColor);
            
            float cellX = bottomLeft.x + currentGridCell.x;
            float cellY = bottomLeft.y + currentGridCell.y;
            
            // 绘制单元格边框
            GL.Vertex3(cellX, cellY, 0);
            GL.Vertex3(cellX + 1, cellY, 0);
            
            GL.Vertex3(cellX + 1, cellY, 0);
            GL.Vertex3(cellX + 1, cellY + 1, 0);
            
            GL.Vertex3(cellX + 1, cellY + 1, 0);
            GL.Vertex3(cellX, cellY + 1, 0);
            
            GL.Vertex3(cellX, cellY + 1, 0);
            GL.Vertex3(cellX, cellY, 0);
            
            // 填充单元格（绘制一个四边形）
            GL.End();
            GL.Begin(GL.QUADS);
            GL.Vertex3(cellX, cellY, 0);
            GL.Vertex3(cellX + 1, cellY, 0);
            GL.Vertex3(cellX + 1, cellY + 1, 0);
            GL.Vertex3(cellX, cellY + 1, 0);
        }

        GL.End();
        GL.PopMatrix();
    }
}