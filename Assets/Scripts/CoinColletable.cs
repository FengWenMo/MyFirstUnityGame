using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CoinCollectable : MonoBehaviour
{
    [Header("金币设置")]
    public int coinValue = 1;  // 金币价值
    public AudioClip collectSound;  // 收集音效（可选）
    
    [Header("视觉效果")]
    public float rotationSpeed = 180f;  // 旋转速度
    public float floatAmplitude = 0.2f; // 上下浮动幅度
    public float floatFrequency = 1f;   // 浮动频率
    
    private Vector3 startPosition;
    private float floatTimer = 0f;
    
    void Start()
    {
        startPosition = transform.position;
    }
    
    void Update()
    {
        // 旋转效果
        transform.Rotate(0, 0, rotationSpeed * Time.deltaTime);
        
        // 上下浮动效果
        floatTimer += Time.deltaTime;
        float newY = startPosition.y + Mathf.Sin(floatTimer * floatFrequency) * floatAmplitude;
        transform.position = new Vector3(transform.position.x, newY, transform.position.z);
    }
    
    // 当被玩家触碰时
    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            CollectCoin();
        }
    }
    
    private void CollectCoin()
    {
        // 通知金币管理器
        if (CoinManager.Instance != null)
        {
            CoinManager.Instance.AddCoins(coinValue);
        }
        
        // 播放音效（可选）
        if (collectSound != null)
        {
            AudioSource.PlayClipAtPoint(collectSound, transform.position);
        }
        
        // 播放收集特效（可选）
        // 可以在这里实例化一个粒子效果
        
        // 销毁金币
        Destroy(gameObject);
        
        Debug.Log($"收集到金币！价值: {coinValue}");
    }
}