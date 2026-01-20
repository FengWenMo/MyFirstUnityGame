using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyAI : MonoBehaviour
{
    public float moveSpeed = 3f;           // 敌人移动速度
    public float stoppingDistance = 0.5f;  // 距离玩家多近时停止
    private Transform playerTarget;        // 玩家位置
    private Rigidbody2D rb;
    
    [Header("攻击设置")]
    public int damageToPlayer = 10;       // 每次接触造成的伤害
    public int damageToWall = 5;          // 对墙体的伤害
    public float attackCooldown = 1f;     // 攻击冷却时间
    public float attackRange = 1f;        // 攻击范围
    public LayerMask wallLayer;           // 墙体层级
    public float wallDetectionRange = 2f; // 墙体检测范围
    
    private float lastAttackTime = 0f;     // 上次攻击时间
    private Transform currentWallTarget;  // 当前攻击的墙体目标
    private bool isAttackingWall = false; // 是否正在攻击墙体
    private Vector2 lastMovementDirection; // 最后移动方向
    private float wallCheckInterval = 0.5f; // 墙体检测间隔
    private float lastWallCheckTime = 0f;  // 上次检测时间
    
    [Header("视觉反馈")]
    public GameObject attackEffect;       // 攻击特效
    public float effectDuration = 0.3f;   // 特效持续时间

    // 添加公共属性用于数据收集
    public bool isAttacking { get; private set; }
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        playerTarget = GameObject.FindGameObjectWithTag("Player").transform;
        
        // 如果wallLayer未设置，设置为Default
        if (wallLayer.value == 0)
        {
            wallLayer = LayerMask.GetMask("Default");
        }
    }
    
    void Update()
    {
        // 定期检测附近的墙体
        if (Time.time - lastWallCheckTime >= wallCheckInterval)
        {
            CheckForWalls();
            lastWallCheckTime = Time.time;
        }
    }
    
    void FixedUpdate()
    {
        if (playerTarget != null)
        {
            // 计算指向玩家的方向
            Vector2 direction = (playerTarget.position - transform.position).normalized;
            lastMovementDirection = direction;
            
            // 计算与玩家的距离
            float distance = Vector2.Distance(transform.position, playerTarget.position);
            
            // 如果正在攻击墙体，停止移动
            if (isAttackingWall && currentWallTarget != null)
            {
                // 朝向墙体攻击
                Vector2 wallDirection = (currentWallTarget.position - transform.position).normalized;
                rb.velocity = Vector2.zero; // 停止移动
                
                // 检查是否应该攻击墙体
                float wallDistance = Vector2.Distance(transform.position, currentWallTarget.position);
                if (wallDistance <= attackRange && Time.time - lastAttackTime >= attackCooldown)
                {
                    AttackWall();
                }
                return;
            }
            
            // 如果距离大于停止距离，就向玩家移动
            if (distance > stoppingDistance)
            {
                rb.MovePosition(rb.position + direction * moveSpeed * Time.fixedDeltaTime);
            }
            else
            {
                // 到达停止距离，停止移动
                rb.velocity = Vector2.zero;
            }
        }
    }
    
    // 检测附近的墙体
    private void CheckForWalls()
    {
        // 如果已经在攻击墙体，不需要检测新的
        if (isAttackingWall && currentWallTarget != null) return;
        
        // 在移动方向上检测墙体
        Vector2 checkDirection = lastMovementDirection;
        if (checkDirection == Vector2.zero)
        {
            checkDirection = Vector2.right; // 默认方向
        }
        
        // 发射射线检测墙体
        RaycastHit2D hit = Physics2D.Raycast(
            transform.position, 
            checkDirection, 
            wallDetectionRange, 
            wallLayer
        );
        
        if (hit.collider != null && hit.collider.CompareTag("Wall"))
        {
            // 找到墙体，设置为攻击目标
            currentWallTarget = hit.collider.transform;
            isAttackingWall = true;
            Debug.Log($"检测到墙体: {hit.collider.name}，开始攻击");
        }
    }
    
    // 攻击墙体
    private void AttackWall()
    {
        if (currentWallTarget == null)
        {
            isAttackingWall = false;
            return;
        }
        
        // 确保敌人在攻击范围内
        float distance = Vector2.Distance(transform.position, currentWallTarget.position);
        if (distance > attackRange)
        {
            isAttackingWall = false;
            currentWallTarget = null;
            return;
        }
        isAttacking = true;
        // 获取墙体生命组件
        WallHealth wallHealth = currentWallTarget.GetComponent<WallHealth>();
        if (wallHealth != null)
        {
            wallHealth.TakeDamage(damageToWall);
            lastAttackTime = Time.time;
            
            // 显示攻击特效
            if (attackEffect != null)
            {
                GameObject effect = Instantiate(
                    attackEffect, 
                    transform.position, 
                    Quaternion.identity
                );
                Destroy(effect, effectDuration);
            }
            isAttacking = true;
            Debug.Log($"敌人对墙体造成 {damageToWall} 点伤害");
            
            // 如果墙体被摧毁，重置目标
            if (wallHealth.GetHealthPercent() <= 0)
            {
                isAttackingWall = false;
                isAttacking = false;
                currentWallTarget = null;
                Debug.Log("墙体被摧毁，继续追击玩家");
            }
        }
        else
        {
            // 如果没有WallHealth组件，取消攻击
            isAttackingWall = false;
            currentWallTarget = null;
        }
    }
    
    // 当敌人与玩家碰撞时
    void OnCollisionStay2D(Collision2D collision)
    {
        // 如果撞到墙体，开始攻击
        if (collision.gameObject.CompareTag("Wall") && !isAttackingWall)
        {
            currentWallTarget = collision.transform;
            isAttackingWall = true;
        }
        
        // 如果碰撞到玩家，优先攻击玩家
        if (collision.gameObject.CompareTag("Player"))
        {
            isAttackingWall = false; // 切换到攻击玩家
            
            // 检查攻击冷却
            if (Time.time - lastAttackTime >= attackCooldown)
            {
                isAttacking = true;
                // 对玩家造成伤害
                PlayerHealth playerHealth = collision.gameObject.GetComponent<PlayerHealth>();
                if (playerHealth != null)
                {
                    playerHealth.TakeDamage(damageToPlayer);
                    lastAttackTime = Time.time;
                    Debug.Log($"敌人对玩家造成 {damageToPlayer} 点伤害");
                }
            }
        }
    }
    
    // 当离开墙体时
    void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Wall"))
        {
            // 如果离开当前正在攻击的墙体
            if (currentWallTarget == collision.transform)
            {
                isAttackingWall = false;
                // 攻击结束后重置状态
                isAttacking = false;
                currentWallTarget = null;
            }
        }
    }
    
    // 绘制检测范围Gizmos（调试用）
    void OnDrawGizmosSelected()
    {
        // 绘制攻击范围
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
        
        // 绘制墙体检测范围
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, wallDetectionRange);
        
        // 绘制移动方向
        Gizmos.color = Color.blue;
        Vector3 direction = new Vector3(lastMovementDirection.x, lastMovementDirection.y, 0);
        Gizmos.DrawRay(transform.position, direction * wallDetectionRange);
    }
    
    // 添加一个方法来强制切换目标（例如被其他敌人呼叫）
    public void SetWallTarget(Transform wallTransform)
    {
        if (wallTransform != null && wallTransform.CompareTag("Wall"))
        {
            currentWallTarget = wallTransform;
            isAttackingWall = true;
        }
    }
}