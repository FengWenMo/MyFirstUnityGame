using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class DayDisplayUI : MonoBehaviour
{
    [Header("UI组件")]
    public TextMeshProUGUI dayText;  // 将自身的TextMeshPro组件拖到这里
    
    [Header("设置")]
    public string prefix = "DAY:";     // 前缀，如"第"
    public string suffix = "";     // 后缀，如"天"
    
    private int currentDay = 1;      // 当前天数
    private GameTimeManager timeManager; // 引用时间管理器
    
    void Start()
    {
        // 如果dayText没有手动设置，尝试自动获取
        if (dayText == null)
        {
            dayText = GetComponent<TextMeshProUGUI>();
        }
        
        // 查找并获取时间管理器
        timeManager = FindObjectOfType<GameTimeManager>();
        if (timeManager == null)
        {
            Debug.LogError("未找到GameTimeManager！请确保场景中有一个GameTimeManager组件。");
            return;
        }
        
        // 正确的事件订阅语法：使用 +=
        GameTimeManager.OnDayChanged += OnDayChanged;
        
        // 初始显示
        currentDay = timeManager.CurrentDay;
        UpdateDayDisplay();
        
        Debug.Log("DayDisplayUI初始化完成");
    }
    
    // 当天数变化时调用
    private void OnDayChanged(int newDay)
    {
        currentDay = newDay;
        UpdateDayDisplay();
        Debug.Log($"DayDisplayUI收到天数变化: {newDay}");
    }
    
    // 更新天数显示
    public void UpdateDayDisplay()
    {
        if (dayText != null)
        {
            //dayText.text = $"{prefix}{currentDay}{suffix}";
            dayText.text= $"Day {currentDay}";
            Debug.Log($"更新天数显示: {dayText.text}");
        }
        else
        {
            Debug.LogError("DayText引用为空！请检查UI连接。");
        }
    }
    
    // 增加天数（可以从其他脚本调用）
    public void IncreaseDay()
    {
        currentDay++;
        UpdateDayDisplay();
        Debug.Log($"天数增加到: {currentDay}");
    }
    
    // 直接设置天数
    public void SetDay(int newDay)
    {
        if (newDay > 0)
        {
            currentDay = newDay;
            UpdateDayDisplay();
        }
    }
    
    // 获取当前天数
    public int GetCurrentDay()
    {
        return currentDay;
    }
    
    // 清理事件订阅
    private void OnDestroy()
    {
        // 正确的事件取消订阅语法：使用 -=
        GameTimeManager.OnDayChanged -= OnDayChanged;
    }
}