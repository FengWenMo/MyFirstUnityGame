using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    // --- 移动部分 ---
    public float moveSpeed = 5f;
    private Rigidbody2D rb;
    private Vector2 movement;

    // --- 射击部分 ---
    public GameObject bulletPrefab; // 将用于存放子弹预制体
    public Transform firePoint;     // 子弹生成位置（我们将指定为枪口）
    public float bulletForce = 20f; // 子弹发射速度

    // 添加公共属性用于数据收集
    public bool isMoving { get; private set; }
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        // 如果没指定开火点，就默认用角色自身位置
        if (firePoint == null)
            firePoint = transform;
    }

    void Update()
    {
        // 在移动逻辑中更新状态
        float moveX = Input.GetAxisRaw("Horizontal");
        float moveY = Input.GetAxisRaw("Vertical");
        
        // 更新移动状态
        isMoving = Mathf.Abs(moveX) > 0.1f || Mathf.Abs(moveY) > 0.1f;
        // 移动输入（保持不变）
        movement.x = Input.GetAxisRaw("Horizontal");
        movement.y = Input.GetAxisRaw("Vertical");
        LookAtMouse();

        // --- 新增：射击输入检测 ---
        // 当按下鼠标左键时，调用Shoot方法
        if (Input.GetButtonDown("Fire1")) // "Fire1" 默认为鼠标左键或Ctrl键
        {
            Shoot();
        }
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

    // --- 新增：射击方法 ---
    void Shoot()
    {
        // 1. 从预制体实例化一颗子弹
        GameObject bullet = Instantiate(bulletPrefab, firePoint.position, firePoint.rotation);
        
        // 2. 获取子弹的刚体组件
        Rigidbody2D rb = bullet.GetComponent<Rigidbody2D>();
        
        // 3. 给子弹一个向前（即枪口朝向）的力
        rb.AddForce(firePoint.up * bulletForce, ForceMode2D.Impulse);
        
        // 可选：添加代码让子弹在几秒后自动销毁，防止堆积
        Destroy(bullet, 3f);
    }
}