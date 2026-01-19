using System.Collections;
using System.Collections.Generic;
// MeleeWeapon.cs
using UnityEngine;

public class MeleeWeapon : Weapon
{
    [Header("近战武器设置")]
    public float attackRange = 1.5f;
    public float attackAngle = 90f;
    public LayerMask hitLayers;
    
    [Header("视觉反馈")]
    public GameObject attackEffectPrefab;
    public float effectDuration = 0.2f;
    
    public override void Attack()
    {
        if (!CanAttack()) return;
        
        // 1. 计算攻击区域
        Vector2 attackDirection = firePoint.up;
        Vector2 attackPosition = firePoint.position;
        
        // 2. 检测攻击区域内的敌人
        Collider2D[] hits = Physics2D.OverlapCircleAll(attackPosition, attackRange, hitLayers);
        
        foreach (Collider2D hit in hits)
        {
            // 检查是否在攻击角度内
            Vector2 toTarget = (hit.transform.position - firePoint.position).normalized;
            float angle = Vector2.Angle(attackDirection, toTarget);
            
            if (angle <= attackAngle / 2)
            {
                // 对敌人造成伤害
                EnemyHealth enemyHealth = hit.GetComponent<EnemyHealth>();
                if (enemyHealth != null)
                {
                    enemyHealth.TakeDamage(damage);
                }
                
                // 对墙体造成伤害
                WallHealth wallHealth = hit.GetComponent<WallHealth>();
                if (wallHealth != null)
                {
                    wallHealth.TakeDamage(damage);
                }
            }
        }
        
        // 3. 显示攻击效果
        if (attackEffectPrefab != null)
        {
            GameObject effect = Instantiate(attackEffectPrefab, firePoint.position, firePoint.rotation);
            Destroy(effect, effectDuration);
        }
        
        // 4. 更新攻击时间
        lastAttackTime = Time.time;
        
        // 可以在这里添加音效
    }
}
