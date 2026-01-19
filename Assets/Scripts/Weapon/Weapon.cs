using System.Collections;
using System.Collections.Generic;
// Weapon.cs
using UnityEngine;

public abstract class Weapon : MonoBehaviour
{
    [Header("通用武器设置")]
    public string weaponName = "武器";
    public Sprite weaponIcon;
    public KeyCode switchKey = KeyCode.T;
    
    [Header("攻击设置")]
    public float attackCooldown = 0.5f;
    protected float lastAttackTime = 0f;
    public int damage = 1;
    
    [Header("玩家引用")]
    protected PlayerWeaponManager playerManager;
    protected Transform firePoint;
    
    public virtual void Initialize(PlayerWeaponManager manager, Transform point)
    {
        playerManager = manager;
        firePoint = point;
    }
    
    public abstract void Attack();
    
    public virtual bool CanAttack()
    {
        return Time.time - lastAttackTime >= attackCooldown;
    }
    
    public virtual void OnEquip() { }
    public virtual void OnUnequip() { }
    
    public virtual void Update() { }
}