using System.Collections;
using System.Collections.Generic;
// PlayerMovement.cs (修改后)
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    // --- 移动部分 ---
    public float moveSpeed = 5f;
    private Rigidbody2D rb;
    private Vector2 movement;
    
    // --- 武器管理 ---
    public PlayerWeaponManager weaponManager;
    
    // --- 原有属性 ---
    public bool isMoving { get; private set; }
    
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        
        // 自动获取武器管理器
        if (weaponManager == null)
            weaponManager = GetComponent<PlayerWeaponManager>();
    }
    
    void Update()
    {
        // 移动输入
        float moveX = Input.GetAxisRaw("Horizontal");
        float moveY = Input.GetAxisRaw("Vertical");
        
        // 更新移动状态
        isMoving = Mathf.Abs(moveX) > 0.1f || Mathf.Abs(moveY) > 0.1f;
        movement.x = moveX;
        movement.y = moveY;
        
        // 朝向鼠标
        LookAtMouse();
        
        // 注意：攻击逻辑已移到PlayerWeaponManager中
    }
    
    void FixedUpdate()
    {
        rb.MovePosition(rb.position + movement.normalized * moveSpeed * Time.fixedDeltaTime);
    }
    
    void LookAtMouse()
    {
        Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Vector2 direction = new Vector2(mousePos.x - transform.position.x, mousePos.y - transform.position.y);
        transform.up = direction;
    }
}