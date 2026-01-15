using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI; // 用于UI显示
using UnityEngine.SceneManagement; // 用于重新加载场景

public class PlayerHealth : MonoBehaviour
{
    [Header("生命值设置")]
    public int maxHealth = 100;      // 最大生命值
    public int currentHealth;        // 当前生命值
    
    [Header("受伤无敌时间")]
    public float invincibilityTime = 1.5f;  // 受伤后无敌时间（秒）
    private float invincibilityTimer = 0f;  // 无敌时间计时器
    
    [Header("视觉反馈")]
    public SpriteRenderer playerSprite;  // 玩家的Sprite渲染器
    public Color damageColor = Color.red; // 受伤时闪红的颜色
    public float flashDuration = 0.1f;    // 闪烁持续时间
    
    [Header("UI显示")]
    public Slider healthSlider;      // 血条Slider（可选）
    public Text healthText;          // 生命值文本（可选）
    
    private Color originalColor;     // 保存原始颜色
    private bool isInvincible = false; // 是否处于无敌状态

    private UIController uiController;
    void Start()
    {
        // 初始化生命值
        currentHealth = maxHealth;
        
        // 获取玩家的Sprite渲染器
        if (playerSprite == null)
        {
            playerSprite = GetComponent<SpriteRenderer>();
        }
        
        if (playerSprite != null)
        {
            originalColor = playerSprite.color;
        }
        // 获取UIController
        uiController = FindObjectOfType<UIController>();
        // 更新UI
        UpdateHealthUI();
    }
    
    void Update()
    {
        // 更新无敌时间计时器
        if (isInvincible)
        {
            invincibilityTimer -= Time.deltaTime;
            if (invincibilityTimer <= 0f)
            {
                isInvincible = false;
                if (playerSprite != null)
                {
                    playerSprite.color = originalColor;
                }
            }
        }
    }
    
    // 玩家受到伤害的方法
    public void TakeDamage(int damageAmount)
    {
        // 如果处于无敌状态，则不受伤害
        if (isInvincible) return;
        
        // 减少生命值
        currentHealth -= damageAmount;
        
        // 确保生命值不会低于0
        if (currentHealth < 0) currentHealth = 0;
        
        // 受伤视觉效果
        if (playerSprite != null)
        {
            StartCoroutine(DamageFlash());
        }
        
        // 开始无敌时间
        isInvincible = true;
        invincibilityTimer = invincibilityTime;
        
        // 更新UI
        UpdateHealthUI();
        
        Debug.Log($"玩家受到 {damageAmount} 点伤害，剩余生命值: {currentHealth}/{maxHealth}");
        
        // 检查是否死亡
        if (currentHealth <= 0)
        {
            Die();
        }
    }
    
    // 受伤闪烁效果
    private System.Collections.IEnumerator DamageFlash()
    {
        if (playerSprite != null)
        {
            // 变为受伤颜色
            playerSprite.color = damageColor;
            
            // 等待一段时间
            yield return new WaitForSeconds(flashDuration);
            
            // 如果还在无敌状态，继续闪烁
            if (isInvincible)
            {
                // 闪烁效果：在无敌期间持续闪烁
                float flashTimer = 0f;
                while (flashTimer < invincibilityTime - flashDuration)
                {
                    flashTimer += Time.deltaTime;
                    // 每0.1秒切换一次透明度
                    float alpha = Mathf.PingPong(flashTimer * 10f, 1f) * 0.7f + 0.3f;
                    playerSprite.color = new Color(damageColor.r, damageColor.g, damageColor.b, alpha);
                    yield return null;
                }
            }
            
            // 恢复原始颜色
            if (!isInvincible && playerSprite != null)
            {
                playerSprite.color = originalColor;
            }
        }
    }
    
    // 更新UI显示
    private void UpdateHealthUI()
    {
        // 如果有血条Slider，更新其值
        if (healthSlider != null)
        {
            healthSlider.maxValue = maxHealth;
            healthSlider.value = currentHealth;
        }
        
        // 如果有生命值文本，更新文本
        if (healthText != null)
        {
            healthText.text = $"{currentHealth}/{maxHealth}";
        }
    }
    
    // 玩家死亡
    private void Die()
    {
        Debug.Log("玩家死亡！游戏结束。");
        
        // 禁用玩家控制
        PlayerMovement playerMovement = GetComponent<PlayerMovement>();
        if (playerMovement != null)
        {
            playerMovement.enabled = false;
        }
        
        // 禁用碰撞体
        Collider2D collider = GetComponent<Collider2D>();
        if (collider != null)
        {
            collider.enabled = false;
        }
        
        // 玩家死亡效果：可以变成半透明或播放死亡动画
        if (playerSprite != null)
        {
            playerSprite.color = new Color(1f, 1f, 1f, 0.5f);
        }
        
        // 等待2秒后重新加载场景
        Invoke("RestartGame", 2f);
    }
    
    // 重新开始游戏
    private void RestartGame()
    {
        // 重新加载当前场景
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}