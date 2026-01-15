using UnityEngine;
using TMPro;
using System;

public class ScoreManager : MonoBehaviour
{
    [Header("分数设置")]
    [SerializeField] private int killScore = 5;      // 击杀一只僵尸的分数
    [SerializeField] private int surviveDayScore = 10; // 活过一天的分数
    
    [Header("UI引用")]
    [SerializeField] private TextMeshProUGUI scoreText; // 分数显示文本
    
    private int currentScore = 0; // 当前总分数
    
    // 单例模式，便于从其他脚本访问
    public static ScoreManager Instance { get; private set; }
    
    // 分数变化事件（可选，用于其他系统响应分数变化）
    public static event Action<int> OnScoreChanged;
    
    void Awake()
    {
        // 设置单例
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }
        
        // 初始化分数显示
        UpdateScoreUI();
        
        Debug.Log("分数系统初始化完成");
    }
    
    void Start()
    {
        // 订阅天数变化事件，当新的一天开始时增加生存分数
        GameTimeManager.OnDayChanged += AddSurvivalScore;
    }
    
    void OnDestroy()
    {
        // 取消订阅，避免内存泄漏
        GameTimeManager.OnDayChanged -= AddSurvivalScore;
    }
    
    // 增加击杀分数
    public void AddKillScore()
    {
        AddScore(killScore);
        Debug.Log($"击杀僵尸！+{killScore}分");
    }
    
    // 增加生存分数
    private void AddSurvivalScore(int day)
    {
        // 注意：第一天开始时不加分，从第二天开始才算"活过一天"
        if (day > 1) // 第2天开始时，表示活过了第1天
        {
            AddScore(surviveDayScore);
            Debug.Log($"成功活过第{day-1}天！+{surviveDayScore}分");
        }
    }
    
    // 增加指定分数
    public void AddScore(int amount)
    {
        if (amount <= 0) return;
        
        currentScore += amount;
        UpdateScoreUI();
        
        // 触发分数变化事件
        OnScoreChanged?.Invoke(currentScore);
        
        Debug.Log($"获得分数: +{amount}, 当前总分: {currentScore}");
    }
    
    // 减少分数
    public void SubtractScore(int amount)
    {
        if (amount <= 0) return;
        
        currentScore = Mathf.Max(0, currentScore - amount); // 确保不低于0
        UpdateScoreUI();
        
        OnScoreChanged?.Invoke(currentScore);
    }
    
    // 获取当前分数
    public int GetCurrentScore()
    {
        return currentScore;
    }
    
    // 重置分数
    public void ResetScore()
    {
        currentScore = 0;
        UpdateScoreUI();
        Debug.Log("分数已重置");
    }
    
    // 更新UI显示
    private void UpdateScoreUI()
    {
        if (scoreText != null)
        {
            scoreText.text = $"Score: {currentScore}";
        }
    }
    
    // // 保存分数（可选，未来可以保存到PlayerPrefs）
    // public void SaveScore()
    // {
    //     PlayerPrefs.SetInt("HighScore", Mathf.Max(currentScore, PlayerPrefs.GetInt("HighScore", 0)));
    //     PlayerPrefs.Save();
    // }
    
    // // 获取最高分
    // public int GetHighScore()
    // {
    //     return PlayerPrefs.GetInt("HighScore", 0);
    // }
}
