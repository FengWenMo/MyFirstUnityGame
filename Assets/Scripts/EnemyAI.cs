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
    public int damageToPlayer = 10;  // 每次接触造成的伤害
    public float attackCooldown = 1f; // 攻击冷却时间
    private float lastAttackTime = 0f; // 上次攻击时间
    // 当敌人与玩家碰撞时
    void OnCollisionStay2D(Collision2D collision)
    {
        // 检查是否碰撞到玩家
        if (collision.gameObject.CompareTag("Player"))
        {
            // 检查攻击冷却
            if (Time.time - lastAttackTime >= attackCooldown)
            {
                // 对玩家造成伤害
                PlayerHealth playerHealth = collision.gameObject.GetComponent<PlayerHealth>();
                if (playerHealth != null)
                {
                    playerHealth.TakeDamage(damageToPlayer);
                    lastAttackTime = Time.time; // 记录本次攻击时间
                    Debug.Log($"敌人对玩家造成 {damageToPlayer} 点伤害");
                }
            }
        }
    }
    void Start()
    {
        // 获取自身的刚体
        rb = GetComponent<Rigidbody2D>();
        // 通过标签查找玩家（下一步设置）
        playerTarget = GameObject.FindGameObjectWithTag("Player").transform;
    }

    void FixedUpdate()
    {
        if (playerTarget != null)
        {
            // 计算指向玩家的方向
            Vector2 direction = (playerTarget.position - transform.position).normalized;
            // 计算与玩家的距离
            float distance = Vector2.Distance(transform.position, playerTarget.position);

            // 如果距离大于停止距离，就向玩家移动
            if (distance > stoppingDistance)
            {
                rb.MovePosition(rb.position + direction * moveSpeed * Time.fixedDeltaTime);
            }
        }

    }
}