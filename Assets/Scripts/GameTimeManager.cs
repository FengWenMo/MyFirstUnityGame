using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.Events;

public class GameTimeManager : MonoBehaviour
{
    public enum GamePhase { Day, Night, BloodMoon }
    public GamePhase CurrentPhase { get; private set; }

    public float dayDurationInSeconds = 60f;  // 白天持续时间（秒）
    public float nightDurationInSeconds = 60f; // 夜晚持续时间（秒）
    public int daysToBloodMoon = 7; // 每几天一次血月
    private float timerInCurrentPhase;
    private int currentDay = 1;
    private bool isBloodMoonActive = false;

    // 定义重要事件，其他脚本通过监听这些事件来做出反应
    public static event Action<GamePhase> OnPhaseChanged;
    public static event Action<int> OnDayChanged;
    public static event Action OnBloodMoonStart; // 血月开始事件
 // 添加这行，让其他脚本可以读取当前天数
    public int CurrentDay { get { return currentDay; } }
    // 引用UI组件
    private DayDisplayUI dayDisplayUI;
    void Update()
    {
        timerInCurrentPhase += Time.deltaTime;

        switch (CurrentPhase)
        {
            case GamePhase.Day:
                if (timerInCurrentPhase >= dayDurationInSeconds)
                {
                    SwitchToPhase(GamePhase.Night);
                    CheckForBloodMoon();
                }
                break;
            case GamePhase.Night:
            case GamePhase.BloodMoon: // 血月之夜同样按照夜晚计时
                if (timerInCurrentPhase >= nightDurationInSeconds)
                {
                    currentDay++;
                    OnDayChanged?.Invoke(currentDay);
                    SwitchToPhase(GamePhase.Day);
                    isBloodMoonActive = false; // 新的一天开始，血月结束
                }
                break;
        }
    }
 // 添加Start方法
    void Start()
    {
        // 初始化为白天
        CurrentPhase = GamePhase.Day;
        timerInCurrentPhase = 0f;
        
        // 触发初始事件
        OnDayChanged?.Invoke(currentDay);
        OnPhaseChanged?.Invoke(CurrentPhase);
        
        Debug.Log($"游戏开始: 第{currentDay}天，{CurrentPhase}");
    }

    private void SwitchToPhase(GamePhase newPhase)
    {
        CurrentPhase = newPhase;
        timerInCurrentPhase = 0f;
        Debug.Log($"第{currentDay}天，切换到阶段: {newPhase}");
        OnPhaseChanged?.Invoke(newPhase);

        // 当切换到白天时，恢复环境光为白色
        if (newPhase == GamePhase.Day)
        {
            RenderSettings.ambientLight = Color.white; // 恢复白色环境光
            if (RenderSettings.sun != null)
            {
                RenderSettings.sun.color = Color.white;
            }
        }
    }

    private void CheckForBloodMoon()
    {
        // 检查是否是血月之夜（例如第7天、14天...）
        if (currentDay % daysToBloodMoon == 0)
        {
            isBloodMoonActive = true;
            CurrentPhase = GamePhase.BloodMoon; // 直接覆盖当前阶段为血月
            Debug.LogWarning($"！！！第{currentDay}天，血月当空！");
            OnBloodMoonStart?.Invoke(); // 触发血月事件

            // 改变环境光为深红色
            RenderSettings.ambientLight = new Color(0.3f, 0.1f, 0.1f);
            // 如果有全局2D光，也可以调整
            if (RenderSettings.sun != null)
            {
                RenderSettings.sun.color = new Color(0.8f, 0.3f, 0.3f);
            }
        }
    }
}