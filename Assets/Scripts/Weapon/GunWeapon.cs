using System.Collections;
using System.Collections.Generic;
// GunWeapon.cs
using UnityEngine;

public class GunWeapon : Weapon
{
    [Header("射击设置")]
    public GameObject bulletPrefab;  // 子弹预制体
    public float bulletForce = 20f;  // 子弹速度
    public float bulletLifetime = 3f; // 子弹存在时间
    
    [Header("视觉效果")]
    public GameObject muzzleFlash;   // 枪口火焰效果
    public float flashDuration = 0.1f;
    
    [Header("音效")]
    public AudioClip shootSound;
    private AudioSource audioSource;
    
    public override void Initialize(PlayerWeaponManager manager, Transform point)
    {
        base.Initialize(manager, point);
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
    }
    
    // 这里是攻击的具体实现！
    public override void Attack()
    {
        // 检查是否可以攻击
        if (!CanAttack()) return;
        
        // 检查必要的组件
        if (bulletPrefab == null)
        {
            Debug.LogError("GunWeapon: 子弹预制体未分配！");
            return;
        }
        
        if (firePoint == null)
        {
            Debug.LogError("GunWeapon: FirePoint 为空！");
            return;
        }
        
        // 1. 创建子弹
        GameObject bullet = Instantiate(
            bulletPrefab, 
            firePoint.position, 
            firePoint.rotation
        );
        
        // 2. 确保子弹朝向正确
        bullet.transform.up = firePoint.up;
        
        // 3. 给子弹施加力
        Rigidbody2D bulletRb = bullet.GetComponent<Rigidbody2D>();
        if (bulletRb != null)
        {
            bulletRb.AddForce(firePoint.up * bulletForce, ForceMode2D.Impulse);
        }
        else
        {
            Debug.LogWarning("子弹没有 Rigidbody2D 组件，无法移动");
        }
        
        // 4. 设置子弹伤害
        Bullet bulletScript = bullet.GetComponent<Bullet>();
        if (bulletScript != null)
        {
            bulletScript.damage = damage;
        }
        
        // 5. 自动销毁子弹
        Destroy(bullet, bulletLifetime);
        
        // 6. 播放枪口火焰效果
        if (muzzleFlash != null)
        {
            GameObject flash = Instantiate(muzzleFlash, firePoint.position, firePoint.rotation);
            flash.transform.parent = firePoint;
            Destroy(flash, flashDuration);
        }
        
        // 7. 播放射击音效
        if (shootSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(shootSound);
        }
        
        // 8. 更新冷却时间
        lastAttackTime = Time.time;
        
        Debug.Log($"{weaponName} 发射子弹！位置: {firePoint.position}");
    }
}
