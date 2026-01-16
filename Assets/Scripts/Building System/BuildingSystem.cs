using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class BuildingSystem : MonoBehaviour
{
    [Header("建造模式设置")]
    public KeyCode buildModeKey = KeyCode.B; // 建造模式按键
    public bool requireShift = true; // 是否需要按住Shift
    
    [Header("建造参数")]
    public GameObject wallPrefab; // 墙体预制体
    public float maxBuildDistance = 3f; // 最大建造距离（网格单位）
    public int wallBuildCost = 10; // 墙体建造花费
    
    [Header("组件引用")]
    public GridSystem gridSystem;
    public CoinManager coinManager;
    public PlayerMovement playerMovement;
    public GameObject buildPreview; // 建造预览对象
    
    [Header("UI引用")]
    public GameObject buildModeUI; // 建造模式UI面板
    public TextMeshProUGUI buildModeText;
    public Image buildRangeIndicator; // 建造范围指示器
    
    // 状态变量
    private bool isBuildMode = false;
    private Vector3 currentGridPosition;
    private bool canBuildAtPosition = false;
    
    // 预览相关
    private SpriteRenderer previewRenderer;
    private Color canBuildColor = new Color(0, 1, 0, 0.5f); // 可建造：半透绿
    private Color cannotBuildColor = new Color(1, 0, 0, 0.5f); // 不可建造：半透红
    
    void Start()
    {
        // 初始化预览对象
        if (buildPreview != null)
        {
            buildPreview.SetActive(false);
            previewRenderer = buildPreview.GetComponent<SpriteRenderer>();
            if (previewRenderer == null)
            {
                previewRenderer = buildPreview.AddComponent<SpriteRenderer>();
            }
        }
        
        // 隐藏UI
        if (buildModeUI != null) buildModeUI.SetActive(false);
        if (buildRangeIndicator != null) buildRangeIndicator.gameObject.SetActive(false);
        
        Debug.Log("建造系统初始化完成");
    }
    
    void Update()
    {
        // 检测建造模式切换
        CheckBuildModeToggle();
        
        // 建造模式下的逻辑
        if (isBuildMode)
        {
            HandleBuildMode();
        }
    }
    
    // 检测是否切换建造模式
    private void CheckBuildModeToggle()
    {
        bool keyPressed = requireShift ? 
            (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift)) && Input.GetKeyDown(buildModeKey) :
            Input.GetKeyDown(buildModeKey);
        
        if (keyPressed)
        {
            ToggleBuildMode();
        }
        
        // 按ESC退出建造模式
        if (isBuildMode && Input.GetKeyDown(KeyCode.Escape))
        {
            SetBuildMode(false);
        }
    }
    
    // 切换建造模式
    private void ToggleBuildMode()
    {
        SetBuildMode(!isBuildMode);
    }
    
    // 设置建造模式状态
    private void SetBuildMode(bool enable)
    {
        isBuildMode = enable;
        
        // 更新UI
        if (buildModeUI != null) buildModeUI.SetActive(enable);
        if (buildModeText != null) buildModeText.text = enable ? "建造模式" : "";
        
        // 显示/隐藏网格
        if (gridSystem != null) gridSystem.SetGridVisible(enable);
        
        // 显示/隐藏预览
        if (buildPreview != null) buildPreview.SetActive(enable);
        
        // 显示/隐藏范围指示器
        if (buildRangeIndicator != null)
        {
            buildRangeIndicator.gameObject.SetActive(enable);
            if (enable)
            {
                // 设置范围指示器大小
                float diameter = maxBuildDistance * 2f * gridSystem.gridSize;
                buildRangeIndicator.rectTransform.sizeDelta = new Vector2(diameter, diameter);
            }
        }
        
        // 禁用/启用玩家射击（如果有射击脚本）
        // 注意：你需要根据实际的射击脚本进行调整
        // if (playerMovement != null && playerMovement.GetComponent<PlayerShooting>() != null)
        // {
        //     playerMovement.GetComponent<PlayerShooting>().enabled = !enable;
        // }
        
        Debug.Log(enable ? "进入建造模式" : "退出建造模式");
    }
    
    // 处理建造模式下的逻辑
    private void HandleBuildMode()
    {
        if (gridSystem == null || buildPreview == null) return;
        
        // 获取鼠标世界位置
        Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mouseWorldPos.z = 0;
        
        // 对齐到网格
        currentGridPosition = gridSystem.GetGridPosition(mouseWorldPos);
        
        // 检查是否可以建造
        canBuildAtPosition = CheckCanBuild(currentGridPosition);
        
        // 更新预览位置和颜色
        buildPreview.transform.position = currentGridPosition;
        if (previewRenderer != null)
        {
            previewRenderer.color = canBuildAtPosition ? canBuildColor : cannotBuildColor;
        }
        
        // 绘制玩家周围的网格
        if (playerMovement != null)
        {
            gridSystem.DrawGridInArea(playerMovement.transform.position, Mathf.FloorToInt(maxBuildDistance));
        }
        
        // 更新范围指示器位置
        if (buildRangeIndicator != null && playerMovement != null)
        {
            buildRangeIndicator.transform.position = playerMovement.transform.position;
        }
        
        // 鼠标左键建造
        if (Input.GetMouseButtonDown(0) && canBuildAtPosition)
        {
            AttemptToBuild();
        }
        
        // 鼠标右键取消/快速退出
        if (Input.GetMouseButtonDown(1))
        {
            SetBuildMode(false);
        }
    }
    
    // 检查指定位置是否可以建造
    private bool CheckCanBuild(Vector3 buildPosition)
    {
        if (playerMovement == null) return false;
        
        // 1. 检查距离
        float distance = Vector3.Distance(playerMovement.transform.position, buildPosition);
        float maxDistance = maxBuildDistance * gridSystem.gridSize;
        if (distance > maxDistance)
        {
            return false;
        }
        
        // 2. 检查是否在地面上
        if (gridSystem != null && !gridSystem.IsPositionOnGrid(buildPosition))
        {
            return false;
        }
        
        // 3. 检查是否有足够金币
        if (coinManager != null && !coinManager.HasEnoughCoins(wallBuildCost))
        {
            return false;
        }
        
        // 4. 检查位置是否被占用
        Collider2D overlap = Physics2D.OverlapBox(buildPosition, 
            new Vector2(gridSystem.gridSize * 0.9f, gridSystem.gridSize * 0.9f), 0f);
        if (overlap != null && overlap.CompareTag("Wall"))
        {
            return false; // 位置已有墙体
        }
        
        return true;
    }
    
    // 尝试建造
    private void AttemptToBuild()
    {
        if (wallPrefab == null)
        {
            Debug.LogError("墙体预制体未设置！");
            return;
        }
        
        // 检查金币
        if (coinManager != null && coinManager.SpendCoins(wallBuildCost))
        {
            // 创建墙体
            GameObject newWall = Instantiate(wallPrefab, currentGridPosition, Quaternion.identity);
            
            // 确保墙体有正确标签
            newWall.tag = "Wall";
            
            // 播放建造音效（可选）
            // AudioManager.Instance.PlaySound("build");
            
            Debug.Log($"成功建造墙体于 {currentGridPosition}, 花费 {wallBuildCost} 金币");
            
            // 短暂隐藏预览，避免重叠显示
            if (buildPreview != null)
            {
                StartCoroutine(HidePreviewBriefly());
            }
        }
        else
        {
            Debug.Log($"金币不足！需要 {wallBuildCost} 金币");
            // 可以在这里添加强烈的视觉/音频反馈
        }
    }
    
    // 短暂隐藏预览
    private System.Collections.IEnumerator HidePreviewBriefly()
    {
        if (buildPreview != null)
        {
            buildPreview.SetActive(false);
            yield return new WaitForSeconds(0.2f);
            if (isBuildMode) // 如果还在建造模式，重新显示
            {
                buildPreview.SetActive(true);
            }
        }
    }
    
    // 获取当前是否在建造模式
    public bool IsInBuildMode()
    {
        return isBuildMode;
    }
}
