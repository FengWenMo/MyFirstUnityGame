using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bullet : MonoBehaviour
{
    public int damage = 1; // 子弹伤害值

    // 当子弹作为触发器进入其他碰撞体时调用
    void OnTriggerEnter2D(Collider2D hitInfo)
    {
           // 添加这行，看函数到底有没有被调用
    Debug.Log("！！！子弹碰撞检测函数被调用，碰到了：" + hitInfo.gameObject.name);
    // ... 你原来的代码 ...
        // 这里可以检查击中了什么，例如：
    // 检查是否击中了带有 EnemyAI 脚本的对象
    EnemyAI enemy = hitInfo.GetComponent<EnemyAI>();
    if (enemy != null)
    {
        // 如果击中敌人，销毁敌人（后续可替换为扣血逻辑）
        EnemyHealth enemyHealth = hitInfo.GetComponent<EnemyHealth>();
        if (enemyHealth != null)
        {
            // 调用敌人的受伤方法
            enemyHealth.TakeDamage(damage);
        }
    }
     // 2. 检查是否击中了墙体
    WallHealth wallHealth = hitInfo.GetComponent<WallHealth>();
    if (wallHealth != null)
    {
        wallHealth.TakeDamage(1);  // 每发子弹对墙体造成1点伤害
        Destroy(gameObject);
        return;
    }
    // 无论击中什么（除了玩家），都销毁子弹
    // 注意：确保玩家和子弹在不同物理层（Layer），且不相互碰撞
    Destroy(gameObject);
}
        // Enemy enemy = hitInfo.GetComponent<Enemy>();
        // if (enemy != null)
        // {
        //     enemy.TakeDamage(damage);
        // }
        
        // 暂时先简单处理：无论击中什么（除了玩家和子弹自己），都销毁子弹
        // 注意：需要为玩家和子弹自己设置好不同的Layer（图层）以避免误伤
    }