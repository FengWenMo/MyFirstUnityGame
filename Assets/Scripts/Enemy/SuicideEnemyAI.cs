using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SuicideEnemyAI : MonoBehaviour
{
    public float moveSpeed = 3f;           // 敌人移动速度
    public float acceleration = 5f;        // 加速度
    public float maxSpeed = 4f;            // 最大速度
    private Transform playerTarget;        // 玩家位置
    private Rigidbody2D rb;
    
    [Header("自爆设置")]
    public int explosionDamageToPlayer = 30;  // 对玩家的爆炸伤害
    public int explosionDamageToWall = 50;    // 对墙体的爆炸伤害
    public float explosionRange = 3f;         // 爆炸范围（可调节）
    public float explosionDelay = 1f;         // 触发自爆后的延迟时间
    public float minExplosionDistance = 0.5f; // 最小爆炸距离（尽量贴近目标再炸）
    public float maxExplosionDistance = 2f;   // 最大爆炸距离（超过这个距离不会自爆）
    public LayerMask explosionLayer;          // 爆炸影响的层级
    
    [Header("墙体特攻设置")]
    public int contactDamageToWall = 10;      // 接触墙体的伤害
    public float wallDetectionRange = 8f;     // 墙体检测范围
    public float wallPriorityMultiplier = 3f;  // 墙体优先级倍数
    public float wallAggroRange = 5f;         // 墙体吸引范围
    public float wallStickiness = 2f;         // 对墙体的"黏着度"，越高越不容易切换目标
    
    [Header("自爆计时器")]
    public float forcedExplosionTime = 8f;    // 强制自爆时间（避免无限追击）
    public float proximityExplosionThreshold = 1.5f; // 接近爆炸阈值
    
    [Header("视觉反馈")]
    public GameObject explosionEffect;        // 爆炸特效
    public GameObject warningEffect;          // 警告特效
    public Color warningColor = Color.red;    // 警告颜色
    public float warningFlashRate = 0.1f;    // 警告闪烁频率
    public float effectDuration = 0.5f;      // 特效持续时间
    
    [Header("状态")]
    public bool isExploding = false;          // 是否正在自爆
    public float health = 50f;               // 敌人生命值
    
    // 私有变量
    private Transform currentTarget;          // 当前目标
    private Vector2 lastMovementDirection;   // 最后移动方向
    private float wallCheckInterval = 0.2f;  // 墙体检测间隔
    private float lastWallCheckTime = 0f;    // 上次检测时间
    private SpriteRenderer spriteRenderer;    // 用于闪烁效果
    private Color originalColor;             // 原始颜色
    private Coroutine warningCoroutine;      // 警告协程
    private float spawnTime;                 // 生成时间
    private float timeSinceLastTargetChange = 0f; // 距离上次切换目标的时间
    private float forcedExplosionTimer = 0f; // 强制自爆计时器
    private float proximityTimer = 0f;       // 接近目标计时器
    private Vector2 currentVelocity;         // 当前速度（用于平滑移动）
    
    // 属性
    public float TimeSinceSpawn { get { return Time.time - spawnTime; } }
    public float TimeToExplosion { get { return forcedExplosionTime - forcedExplosionTimer; } }
    
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        spawnTime = Time.time;
        
        if (spriteRenderer != null)
        {
            originalColor = spriteRenderer.color;
        }
        
        // 寻找玩家
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            playerTarget = player.transform;
            currentTarget = playerTarget; // 默认目标为玩家
        }
        
        // 设置爆炸影响的层级
        if (explosionLayer.value == 0)
        {
            explosionLayer = LayerMask.GetMask("Player", "Enemy", "Default");
        }
        
        // 初始化计时器
        forcedExplosionTimer = 0f;
        proximityTimer = 0f;
    }
    
    void Update()
    {
        if (isExploding) return;
        
        // 强制自爆计时
        forcedExplosionTimer += Time.deltaTime;
        if (forcedExplosionTimer >= forcedExplosionTime)
        {
            StartExplosion();
            return;
        }
        
        // 定期检测附近的墙体
        if (Time.time - lastWallCheckTime >= wallCheckInterval)
        {
            UpdateTarget();
            lastWallCheckTime = Time.time;
        }
        
        // 检查是否应该自爆
        CheckForExplosion();
    }
    
    void FixedUpdate()
    {
        if (isExploding || currentTarget == null) return;
        
        // 计算移动方向
        Vector2 targetDirection = (currentTarget.position - transform.position).normalized;
        lastMovementDirection = targetDirection;
        
        // 计算与目标的距离
        float distance = Vector2.Distance(transform.position, currentTarget.position);
        
        // 根据距离调整移动行为
        if (distance > 0.1f) // 避免在很近时抖动
        {
            // 计算期望速度
            Vector2 desiredVelocity = targetDirection * moveSpeed;
            
            // 根据距离调整速度：离得越近，速度越慢（为了更精确地贴近目标）
            float speedMultiplier = 1f;
            if (distance < 1.5f)
            {
                speedMultiplier = Mathf.Lerp(0.3f, 1f, distance / 1.5f);
            }
            
            desiredVelocity *= speedMultiplier;
            
            // 平滑加速度
            currentVelocity = Vector2.Lerp(currentVelocity, desiredVelocity, acceleration * Time.fixedDeltaTime);
            
            // 限制最大速度
            if (currentVelocity.magnitude > maxSpeed)
            {
                currentVelocity = currentVelocity.normalized * maxSpeed;
            }
            
            // 应用速度
            rb.MovePosition(rb.position + currentVelocity * Time.fixedDeltaTime);
        }
    }
    
    // 更新目标（优先墙体，其次玩家）
    private void UpdateTarget()
    {
        if (isExploding) return;
        
        // 如果当前目标还在，检查是否应该更换目标
        float timeSinceChange = Time.time - timeSinceLastTargetChange;
        if (timeSinceChange < 0.5f) return; // 防止频繁切换目标
        
        // 优先寻找墙体
        Transform bestWallTarget = FindBestWallTarget();
        if (bestWallTarget != null)
        {
            // 如果当前目标不是墙体，或者新墙体更近，则切换目标
            if (currentTarget == null || 
                !currentTarget.CompareTag("Wall") || 
                (bestWallTarget != currentTarget && 
                 Vector2.Distance(transform.position, bestWallTarget.position) < 
                 Vector2.Distance(transform.position, currentTarget.position) * 0.7f)) // 新目标比当前目标近30%才切换
            {
                SetTarget(bestWallTarget);
                return;
            }
        }
        
        // 如果没有找到墙体或者玩家目标丢失，以玩家为目标
        if (currentTarget == null || (currentTarget.CompareTag("Wall") && playerTarget != null))
        {
            // 检查玩家是否在视野内
            if (playerTarget != null)
            {
                SetTarget(playerTarget);
            }
        }
    }
    
    // 寻找最佳墙体目标
    private Transform FindBestWallTarget()
    {
        Collider2D[] walls = Physics2D.OverlapCircleAll(
            transform.position, 
            wallDetectionRange, 
            LayerMask.GetMask("Default", "Wall")
        );
        
        Transform bestTarget = null;
        float bestScore = float.MinValue;
        
        foreach (Collider2D wall in walls)
        {
            if (wall.CompareTag("Wall"))
            {
                float distance = Vector2.Distance(transform.position, wall.transform.position);
                
                // 计算综合评分：距离越近分数越高，但也要考虑墙体的"吸引力"
                float distanceScore = 1f - Mathf.Clamp01(distance / wallDetectionRange);
                float priorityScore = wallPriorityMultiplier;
                float wallHealthScore = 1f;
                
                // 检查墙体生命值，优先攻击低生命值墙体
                WallHealth wallHealth = wall.GetComponent<WallHealth>();
                if (wallHealth != null)
                {
                    wallHealthScore = 1f - wallHealth.GetHealthPercent(); // 生命值越低，分数越高
                }
                
                float totalScore = distanceScore * priorityScore * wallHealthScore;
                
                if (totalScore > bestScore)
                {
                    bestScore = totalScore;
                    bestTarget = wall.transform;
                }
            }
        }
        
        return bestTarget;
    }
    
    // 检查是否应该自爆
    private void CheckForExplosion()
    {
        if (isExploding || currentTarget == null) return;
        
        float distance = Vector2.Distance(transform.position, currentTarget.position);
        
        // 如果目标在爆炸范围内
        if (distance <= explosionRange)
        {
            // 计算理想爆炸条件
            bool shouldExplode = false;
            string reason = "";
            
            // 条件1：距离非常近（保证一定能炸到）
            if (distance <= minExplosionDistance)
            {
                shouldExplode = true;
                reason = "距离非常近";
                proximityTimer = 0f; // 重置接近计时器
            }
            // 条件2：距离较近，并且已经接近了一段时间
            else if (distance <= proximityExplosionThreshold)
            {
                proximityTimer += Time.deltaTime;
                if (proximityTimer >= 0.3f) // 在接近范围内停留0.3秒
                {
                    shouldExplode = true;
                    reason = "持续接近目标";
                }
            }
            // 条件3：强制自爆时间快到了
            else if (forcedExplosionTimer >= forcedExplosionTime * 0.8f) // 最后20%时间
            {
                shouldExplode = true;
                reason = "强制自爆时间到";
            }
            // 条件4：目标是墙体且距离合适
            else if (currentTarget.CompareTag("Wall") && distance <= maxExplosionDistance)
            {
                shouldExplode = true;
                reason = "墙体在有效爆炸距离内";
            }
            else
            {
                // 重置接近计时器
                proximityTimer = 0f;
            }
            
            if (shouldExplode)
            {
                Debug.Log($"自爆触发: {reason} (距离: {distance:F2})");
                StartExplosion();
            }
        }
        else
        {
            // 不在爆炸范围内，重置接近计时器
            proximityTimer = 0f;
        }
    }
    
    // 开始自爆序列
    private void StartExplosion()
    {
        if (isExploding) return;
        
        isExploding = true;
        rb.velocity = Vector2.zero; // 停止移动
        currentVelocity = Vector2.zero;
        
        // 开始警告效果
        if (warningCoroutine == null)
        {
            warningCoroutine = StartCoroutine(WarningEffect());
        }
        
        // 延迟后爆炸
        StartCoroutine(ExplodeAfterDelay());
    }
    
    // 延迟后爆炸
    private IEnumerator ExplodeAfterDelay()
    {
        yield return new WaitForSeconds(explosionDelay);
        Explode();
    }
    
    // 执行爆炸
    private void Explode()
    {
        // 停止警告效果
        if (warningCoroutine != null)
        {
            StopCoroutine(warningCoroutine);
            warningCoroutine = null;
        }
        
        // 播放爆炸特效
        if (explosionEffect != null)
        {
            GameObject effect = Instantiate(
                explosionEffect, 
                transform.position, 
                Quaternion.identity
            );
            Destroy(effect, effectDuration);
        }
        
        // 应用爆炸伤害
        ApplyExplosionDamage();
        
        // 销毁自己
        Destroy(gameObject);
    }
    
    // 应用360度爆炸伤害
    private void ApplyExplosionDamage()
    {
        Collider2D[] colliders = Physics2D.OverlapCircleAll(
            transform.position, 
            explosionRange, 
            explosionLayer
        );
        
        foreach (Collider2D collider in colliders)
        {
            Vector2 directionToTarget = (collider.transform.position - transform.position);
            float distance = directionToTarget.magnitude;
            
            // 计算伤害衰减（距离越远伤害越低）
            float damageMultiplier = Mathf.Clamp01(1f - (distance / explosionRange));
            
            if (collider.CompareTag("Player"))
            {
                PlayerHealth playerHealth = collider.GetComponent<PlayerHealth>();
                if (playerHealth != null)
                {
                    int finalDamage = Mathf.RoundToInt(explosionDamageToPlayer * damageMultiplier);
                    playerHealth.TakeDamage(finalDamage);
                    Debug.Log($"爆炸对玩家造成 {finalDamage} 点伤害 (距离: {distance:F2})");
                }
            }
            else if (collider.CompareTag("Wall"))
            {
                WallHealth wallHealth = collider.GetComponent<WallHealth>();
                if (wallHealth != null)
                {
                    int finalDamage = Mathf.RoundToInt(explosionDamageToWall * damageMultiplier);
                    wallHealth.TakeDamage(finalDamage);
                    Debug.Log($"爆炸对墙体造成 {finalDamage} 点伤害 (距离: {distance:F2})");
                }
            }
        }
    }
    
    // 警告效果协程
    private IEnumerator WarningEffect()
    {
        if (spriteRenderer == null) yield break;
        
        float timer = 0f;
        while (timer < explosionDelay)
        {
            // 闪烁效果，越接近爆炸闪烁越快
            float flashSpeed = Mathf.Lerp(1f, 3f, timer / explosionDelay);
            float flashValue = Mathf.PingPong(timer * flashSpeed, 1f);
            spriteRenderer.color = Color.Lerp(originalColor, warningColor, flashValue);
            
            timer += Time.deltaTime;
            yield return null;
        }
        
        spriteRenderer.color = originalColor;
    }
    
    // 当接触墙体时造成接触伤害
    void OnCollisionEnter2D(Collision2D collision)
    {
        if (isExploding) return;
        
        if (collision.gameObject.CompareTag("Wall"))
        {
            // 对墙体造成接触伤害
            WallHealth wallHealth = collision.gameObject.GetComponent<WallHealth>();
            if (wallHealth != null)
            {
                wallHealth.TakeDamage(contactDamageToWall);
            }
            
            // 如果是墙体目标，立即自爆
            if (currentTarget != null && currentTarget == collision.transform)
            {
                explosionDelay = 0.2f; // 接触时更快爆炸
                StartExplosion();
            }
        }
        else if (collision.gameObject.CompareTag("Player"))
        {
            // 接触到玩家，立即自爆
            explosionDelay = 0.1f; // 接触玩家时几乎立即爆炸
            StartExplosion();
        }
    }
    
    // 当进入触发器（可选，用于检测玩家进入攻击范围）
    void OnTriggerEnter2D(Collider2D other)
    {
        if (isExploding) return;
        
        // 如果玩家进入近距离范围，加快接近速度
        if (other.CompareTag("Player"))
        {
            moveSpeed *= 1.5f; // 加速追击
        }
    }
    
    // 设置目标
    private void SetTarget(Transform newTarget)
    {
        if (newTarget == null) return;
        
        currentTarget = newTarget;
        timeSinceLastTargetChange = Time.time;
        
        Debug.Log($"切换目标到: {newTarget.tag} - {newTarget.name}");
    }
    
    // 当受到伤害时
    public void TakeDamage(float damage)
    {
        if (isExploding) return;
        
        health -= damage;
        
        // 如果生命值过低，立即自爆
        if (health <= 0)
        {
            explosionDelay = 0.2f; // 濒死时更快爆炸
            StartExplosion();
        }
    }
    
    // 绘制检测范围Gizmos（调试用）
    void OnDrawGizmosSelected()
    {
        // 绘制爆炸范围
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, explosionRange);
        
        // 绘制最佳爆炸距离范围
        Gizmos.color = new Color(1f, 0.5f, 0f, 0.5f); // 橙色
        Gizmos.DrawWireSphere(transform.position, minExplosionDistance);
        
        // 绘制最大爆炸距离
        Gizmos.color = new Color(1f, 0f, 0f, 0.3f); // 淡红色
        Gizmos.DrawWireSphere(transform.position, maxExplosionDistance);
        
        // 绘制墙体检测范围
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, wallDetectionRange);
        
        // 绘制墙体吸引范围
        Gizmos.color = new Color(1f, 1f, 0f, 0.3f); // 淡黄色
        Gizmos.DrawWireSphere(transform.position, wallAggroRange);
        
        // 绘制当前目标方向
        if (currentTarget != null)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawLine(transform.position, currentTarget.position);
            
            // 绘制到目标的距离
            float distance = Vector2.Distance(transform.position, currentTarget.position);
            Vector3 labelPos = transform.position + (currentTarget.position - transform.position) / 2f;
            #if UNITY_EDITOR
            UnityEditor.Handles.Label(labelPos, $"距离: {distance:F2}");
            #endif
        }
    }
    
    // 外部触发自爆
    public void TriggerExplosion(float delay = 0.5f)
    {
        if (!isExploding)
        {
            explosionDelay = delay;
            StartExplosion();
        }
    }
}
