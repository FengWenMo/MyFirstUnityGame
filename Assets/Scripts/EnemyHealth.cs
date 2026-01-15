using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyHealth : MonoBehaviour
{
    [Header("生命值设置")]
    public int maxHealth = 3;      // 最大生命值
    public int currentHealth;      // 当前生命值
    
    [Header("视觉效果")]
    public SpriteRenderer enemySprite;  // 敌人的Sprite渲染器
    public Color damageColor = Color.white;  // 受伤时闪白的颜色
    public float flashDuration = 0.1f;       // 闪烁持续时间
    
    [Header("死亡效果")]
    public GameObject deathEffectPrefab;  // 死亡特效预制体（可选）
    
    private Color originalColor;  // 保存原始颜色
    private bool isFlashing = false;  // 防止重复闪烁
    
    void Start()
    {
        // 初始化生命值
        currentHealth = maxHealth;
        
        // 获取敌人的Sprite渲染器
        if (enemySprite == null)
        {
            enemySprite = GetComponent<SpriteRenderer>();
        }
        
        if (enemySprite != null)
        {
            originalColor = enemySprite.color;
        }
    }
    
    // 敌人受到伤害的方法
    public void TakeDamage(int damageAmount)
    {
        if (currentHealth <= 0) return;  // 如果已经死了，不再处理
        
        // 减少生命值
        currentHealth -= damageAmount;
        
        // 受伤视觉效果
        if (enemySprite != null && !isFlashing)
        {
            StartCoroutine(DamageFlash());
        }
        
        // 检查是否死亡
        if (currentHealth <= 0)
        {
            Die();
        }
        else
        {
            Debug.Log($"敌人受到 {damageAmount} 点伤害，剩余生命值: {currentHealth}/{maxHealth}");
        }
    }
    
    // 受伤闪烁效果
    private IEnumerator DamageFlash()
    {
        isFlashing = true;
        
        if (enemySprite != null)
        {
            // 变为受伤颜色
            enemySprite.color = damageColor;
            
            // 等待一段时间
            yield return new WaitForSeconds(flashDuration);
            
            // 恢复原始颜色
            enemySprite.color = originalColor;
        }
        
        isFlashing = false;
    }
    
    // 敌人死亡
    private void Die()
    {
        Debug.Log("敌人死亡！");
        // 增加击杀分数
        if (ScoreManager.Instance != null)
        {
        ScoreManager.Instance.AddKillScore();
        }
        // 播放死亡特效（如果有）
        if (deathEffectPrefab != null)
        {
            Instantiate(deathEffectPrefab, transform.position, Quaternion.identity);
        }
        
        // 销毁敌人对象
        Destroy(gameObject);
    }
}
