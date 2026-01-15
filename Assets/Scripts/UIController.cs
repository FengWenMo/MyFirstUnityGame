using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro; 

public class UIController : MonoBehaviour
{
    [Header("血条UI")]
    public Slider healthSlider;
    public TextMeshProUGUI healthText; // 改为TextMeshProUGUI
    
    [Header("游戏时间UI")]
    public TextMeshProUGUI dayText;    // 改为TextMeshProUGUI
    public TextMeshProUGUI timeText;   // 改为TextMeshProUGUI
    
    [Header("游戏结束UI")]
    public GameObject gameOverPanel;
    public TextMeshProUGUI survivalText; // 改为TextMeshProUGUI
    public Button restartButton;
    
    [Header("游戏结束分数显示")]
    public TextMeshProUGUI finalScoreText;
    private PlayerHealth playerHealth;
    private GameTimeManager gameTimeManager;
    
    void Start()
    {
        // 获取引用
        playerHealth = FindObjectOfType<PlayerHealth>();
        gameTimeManager = FindObjectOfType<GameTimeManager>();
        
        // 确保游戏结束面板初始隐藏
        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(false);
        }
        
        // 设置重新开始按钮的点击事件
        if (restartButton != null)
        {
            restartButton.onClick.AddListener(RestartGame);
        }
        
        // 更新初始UI
        UpdateHealthUI();
        UpdateTimeUI();
    }
    
    void Update()
    {
        UpdateHealthUI();
        UpdateTimeUI();
    }
    
    public void UpdateHealthUI()
    {
        if (playerHealth != null)
        {
            if (healthSlider != null)
            {
                healthSlider.maxValue = playerHealth.maxHealth;
                healthSlider.value = playerHealth.currentHealth;
            }
            
            if (healthText != null)
            {
                healthText.text = $"{playerHealth.currentHealth}/{playerHealth.maxHealth}";
            }
        }
    }
    
    public void UpdateTimeUI()
    {
        if (gameTimeManager != null && dayText != null && timeText != null)
        {
            // 显示当前天数（需要GameTimeManager暴露currentDay）
            dayText.text = $"第{gameTimeManager.CurrentDay}天";
            
            // 显示当前阶段
            switch (gameTimeManager.CurrentPhase)
            {
                case GameTimeManager.GamePhase.Day:
                    timeText.text = "白天";
                    timeText.color = Color.yellow;
                    break;
                case GameTimeManager.GamePhase.Night:
                    timeText.text = "夜晚";
                    timeText.color = Color.blue;
                    break;
                case GameTimeManager.GamePhase.BloodMoon:
                    timeText.text = "血月！";
                    timeText.color = Color.red;
                    break;
            }
        }
    }
    
    public void ShowGameOver(int survivalDays)
    {
        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(true);
            
            if (survivalText != null)
            {
                survivalText.text = $"你生存了：{survivalDays}天";
            }
            
            // 暂停游戏
            Time.timeScale = 0f;
        }
    }
    
    public void RestartGame()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
    
    public void OnPlayerDie(int survivalDays)
    {
        ShowGameOver(survivalDays);
    }
}