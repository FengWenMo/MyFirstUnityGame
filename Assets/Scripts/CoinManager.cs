using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System;

public class CoinManager : MonoBehaviour
{
    [Header("金币设置")]
    [SerializeField] private int startingCoins = 0;  // 初始金币数
    
    [Header("UI引用")]
    [SerializeField] private TextMeshProUGUI coinText;  // 金币显示文本
    
    private int currentCoins = 0;  // 当前金币数量
    
    // 单例模式
    public static CoinManager Instance { get; private set; }
    
    // 金币变化事件
    public static event Action<int> OnCoinsChanged;
    
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
    }
    
    void Start()
    {
        // 初始化金币
        currentCoins = startingCoins;
        UpdateCoinUI();
        
        Debug.Log("金币系统初始化完成");
    }
    
    // 增加金币
    public void AddCoins(int amount)
    {
        if (amount <= 0) return;
        
        currentCoins += amount;
        UpdateCoinUI();
        
        // 触发金币变化事件
        OnCoinsChanged?.Invoke(currentCoins);
        
        Debug.Log($"获得金币: +{amount}, 当前金币: {currentCoins}");
    }
    
    // 花费金币（先不实现具体用途，但预留方法）
    public bool SpendCoins(int amount)
    {
        if (amount <= 0) return false;
        
        if (currentCoins >= amount)
        {
            currentCoins -= amount;
            UpdateCoinUI();
            
            OnCoinsChanged?.Invoke(currentCoins);
            
            Debug.Log($"花费金币: -{amount}, 剩余金币: {currentCoins}");
            return true;
        }
        else
        {
            Debug.Log($"金币不足！需要: {amount}, 当前: {currentCoins}");
            return false;
        }
    }
    
    // 获取当前金币数
    public int GetCurrentCoins()
    {
        return currentCoins;
    }
    
    // 设置金币数（用于特殊事件）
    public void SetCoins(int amount)
    {
        if (amount < 0) amount = 0;
        
        currentCoins = amount;
        UpdateCoinUI();
        
        OnCoinsChanged?.Invoke(currentCoins);
    }
    
    // 重置金币
    public void ResetCoins()
    {
        currentCoins = startingCoins;
        UpdateCoinUI();
        Debug.Log("金币已重置");
    }
    
    // 检查是否有足够金币
    public bool HasEnoughCoins(int amount)
    {
        return currentCoins >= amount;
    }
    
    // 更新UI显示
    private void UpdateCoinUI()
    {
        if (coinText != null)
        {
            coinText.text = $"Coins: {currentCoins}";
        }
    }
    
    // 保存金币数（未来可以保存到PlayerPrefs）
    public void SaveCoins()
    {
        PlayerPrefs.SetInt("PlayerCoins", currentCoins);
        PlayerPrefs.Save();
    }
    
    // 加载金币数
    public void LoadCoins()
    {
        if (PlayerPrefs.HasKey("PlayerCoins"))
        {
            currentCoins = PlayerPrefs.GetInt("PlayerCoins", startingCoins);
            UpdateCoinUI();
        }
    }
}