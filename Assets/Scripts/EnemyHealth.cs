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
    [Header("金币掉落设置")]
    public GameObject coinPrefab;           // 金币预制体
    public float coinDropChance = 0.3f;     // 30%掉落概率
    public int minCoins = 1;                // 最少掉落金币数
    public int maxCoins = 3;                // 最多掉落金币数
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
        // 有概率掉落金币
        DropCoins();
        // 播放死亡特效（如果有）
        if (deathEffectPrefab != null)
        {
            Instantiate(deathEffectPrefab, transform.position, Quaternion.identity);
        }
        
        // 销毁敌人对象
        Destroy(gameObject);
    }
    private void DropCoins()
    {
        if (coinPrefab == null) return;
        
        // 随机决定是否掉落
        if (Random.Range(0f, 1f) <= coinDropChance)
        {
            // 随机掉落数量
            int coinCount = Random.Range(minCoins, maxCoins + 1);
            
            for (int i = 0; i < coinCount; i++)
            {
                // 计算随机偏移，让金币分散一点
                Vector3 randomOffset = new Vector3(
                    Random.Range(-0.5f, 0.5f),
                    Random.Range(-0.5f, 0.5f),
                    0
                );
                
                // 实例化金币
                Instantiate(coinPrefab, transform.position + randomOffset, Quaternion.identity);
            }
            
            Debug.Log($"掉落 {coinCount} 枚金币");
        }
    }
}
