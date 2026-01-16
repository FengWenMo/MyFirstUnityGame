using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class WallHealth : MonoBehaviour
{
    [Header("墙体属性")]
    public int maxHealth = 50;              // 最大生命值
    public int currentHealth;               // 当前生命值
    public int buildCost = 10;              // 建造消耗金币
    
    [Header("视觉反馈")]
    public SpriteRenderer wallSprite;       // 墙体Sprite
    public Color[] healthColors;            // 根据生命值变化的颜色
    public GameObject damageEffect;         // 受伤特效
    public GameObject destroyEffect;        // 摧毁特效
    
    [Header("UI显示")]
    public Slider healthSlider;             // 可选：生命条
    public GameObject healthBarCanvas;      // 生命条画布
    
    private Color originalColor;            // 原始颜色
    private bool isDead = false;            // 是否已被摧毁
    
    void Start()
    {
        // 初始化生命值
        currentHealth = maxHealth;
        
        // 获取Sprite渲染器
        if (wallSprite == null)
        {
            wallSprite = GetComponent<SpriteRenderer>();
        }
        
        if (wallSprite != null)
        {
            originalColor = wallSprite.color;
        }
        
        // 初始化颜色数组（绿->黄->红）
        if (healthColors == null || healthColors.Length == 0)
        {
            healthColors = new Color[]
            {
                Color.green,    // 健康 (>75%)
                Color.yellow,   // 受损 (75-25%)
                Color.red       // 危险 (<25%)
            };
        }
        
        // 隐藏生命条（可选）
        if (healthBarCanvas != null)
        {
            healthBarCanvas.SetActive(false);
        }
        
        UpdateAppearance();
    }
    
    // 墙体受到伤害
    public void TakeDamage(int damage)
    {
        if (isDead) return;
        
        // 减少生命值
        currentHealth -= damage;
        
        // 显示生命条（如果有）
        if (healthBarCanvas != null)
        {
            StartCoroutine(ShowHealthBarTemporarily());
        }
        
        // 受伤特效
        if (damageEffect != null)
        {
            Instantiate(damageEffect, transform.position, Quaternion.identity);
        }
        
        // 更新外观
        UpdateAppearance();
        
        // 屏幕震动（可选）
        // CameraShake.Instance.Shake(0.1f, 0.1f);
        
        Debug.Log($"墙体受到 {damage} 点伤害，剩余生命值: {currentHealth}/{maxHealth}");
        
        // 检查是否被摧毁
        if (currentHealth <= 0)
        {
            Die();
        }
    }
    
    // 更新墙体外观
    private void UpdateAppearance()
    {
        if (wallSprite == null) return;
        
        // 根据生命值百分比改变颜色
        float healthPercent = (float)currentHealth / maxHealth;
        
        if (healthPercent > 0.75f)
        {
            wallSprite.color = Color.Lerp(healthColors[1], healthColors[0], (healthPercent - 0.75f) * 4);
        }
        else if (healthPercent > 0.25f)
        {
            wallSprite.color = Color.Lerp(healthColors[2], healthColors[1], (healthPercent - 0.25f) * 2);
        }
        else
        {
            wallSprite.color = healthColors[2];
        }
        
        // 更新生命条（如果有）
        if (healthSlider != null)
        {
            healthSlider.value = healthPercent;
        }
    }
    
    // 临时显示生命条
    private System.Collections.IEnumerator ShowHealthBarTemporarily()
    {
        if (healthBarCanvas != null)
        {
            healthBarCanvas.SetActive(true);
            yield return new WaitForSeconds(2f);
            healthBarCanvas.SetActive(false);
        }
    }
    
    // 墙体被摧毁
    private void Die()
    {
        if (isDead) return;
        isDead = true;
        
        Debug.Log("墙体被摧毁！");
        
        // 播放摧毁特效
        if (destroyEffect != null)
        {
            Instantiate(destroyEffect, transform.position, Quaternion.identity);
        }
        
        // 播放音效（可选）
        // AudioManager.Instance.PlaySound("wallDestroy");
        
        // 销毁墙体
        Destroy(gameObject);
        
        // 可选：掉落少量资源
        // DropResources();
    }
    
    // 获取建造花费
    public int GetBuildCost()
    {
        return buildCost;
    }
    
    // 获取当前生命值百分比
    public float GetHealthPercent()
    {
        return (float)currentHealth / maxHealth;
    }
}