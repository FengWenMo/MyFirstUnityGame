using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class BloodMoonOverlayController : MonoBehaviour
{
    public Image bloodMoonOverlay; // 将UI面板的Image组件拖到这里
    public float fadeDuration = 2f; // 淡入淡出的持续时间

    // 预设不同阶段的颜色
    public Color dayColor = new Color(0f, 0f, 0f, 0f); // 完全透明
    public Color nightColor = new Color(0.1f, 0.1f, 0.3f,0.3f); // 深蓝色，30%透明度
    public Color bloodMoonColor = new Color(0.8f, 0.1f, 0.1f,0.4f); // 深红色，40%透明度

    private void Start()
    {
        // 游戏开始时，立即将滤镜设为完全透明
        if (bloodMoonOverlay != null)
        {
            bloodMoonOverlay.color = dayColor;
        }
        else
        {
            Debug.LogError("血月滤镜未连接！");
        }
    }

    private void OnEnable()
    {
        // 订阅事件
        GameTimeManager.OnBloodMoonStart += OnBloodMoonStart;
        GameTimeManager.OnPhaseChanged += OnPhaseChanged;
    }

    private void OnDisable()
    {
        // 取消订阅事件（重要！避免内存泄漏）
        GameTimeManager.OnBloodMoonStart -= OnBloodMoonStart;
        GameTimeManager.OnPhaseChanged -= OnPhaseChanged;
    }

    private void OnBloodMoonStart()
    {
        // 血月开始，过渡到红色滤镜
        StopAllCoroutines(); // 停止所有正在进行的颜色过渡
        StartCoroutine(TransitionColor(bloodMoonOverlay.color, bloodMoonColor, fadeDuration));
    }

    private void OnPhaseChanged(GameTimeManager.GamePhase newPhase)
    {
        if (bloodMoonOverlay == null) return;
        
        StopAllCoroutines(); // 停止所有正在进行的颜色过渡
        
        switch (newPhase)
        {
            case GameTimeManager.GamePhase.Day:
                // 白天：无滤镜
                StartCoroutine(TransitionColor(bloodMoonOverlay.color, dayColor, fadeDuration));
                break;
                
            case GameTimeManager.GamePhase.Night:
                // 普通夜晚：深蓝色滤镜
                StartCoroutine(TransitionColor(bloodMoonOverlay.color, nightColor, fadeDuration));
                break;
                
            case GameTimeManager.GamePhase.BloodMoon:
                // 血月：深红色滤镜（已在OnBloodMoonStart中处理）
                // 这里可以再次调用，确保从任何状态都能正确过渡到血月颜色
                StartCoroutine(TransitionColor(bloodMoonOverlay.color, bloodMoonColor, fadeDuration));
                break;
        }
    }

    // 过渡颜色的协程方法
    private IEnumerator TransitionColor(Color startColor, Color targetColor, float duration)
    {
        float timer = 0f;
        
        while (timer < duration)
        {
            timer += Time.deltaTime;
            float t = timer / duration;
            bloodMoonOverlay.color = Color.Lerp(startColor, targetColor, t);
            yield return null; // 等待下一帧
        }
        
        // 确保最终值准确
        bloodMoonOverlay.color = targetColor;
    }
}