using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyAI : MonoBehaviour
{
    public float moveSpeed = 3f;           // 敌人移动速度
    public float stoppingDistance = 0.5f;  // 距离玩家多近时停止
    private Transform playerTarget;        // 玩家位置
    private Rigidbody2D rb;

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