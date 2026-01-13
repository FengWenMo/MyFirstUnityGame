using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OffScreenEnemySpawner : MonoBehaviour
{
    public static OffScreenEnemySpawner Instance; // 单例模式，方便WaveManager调用
    
    public GameObject enemyPrefab; // 敌人预制体
    public float spawnInterval = 3f; // 生成间隔
    public float minSpawnDistance = 12f; // 最小生成距离（应大于相机视野半径）
    public float maxSpawnDistance = 20f; // 最大生成距离

    private Transform playerTransform;
    private Camera mainCamera;
    private Plane[] cameraFrustumPlanes; // 相机视锥体平面数组
    private float timer;

    void Awake()
    {
        // 设置单例
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        // 查找玩家和主相机
        playerTransform = GameObject.FindGameObjectWithTag("Player").transform;
        mainCamera = Camera.main; // 确保你的主相机标签是"MainCamera"

        if (playerTransform == null || mainCamera == null)
        {
            Debug.LogError("未找到玩家或主相机！");
            return;
        }
    }

    void Update()
    {
        timer += Time.deltaTime;
        if (timer >= spawnInterval)
        {
            timer = 0f;
            TrySpawnEnemy();
        }

        // 每帧更新相机视锥体平面（相机移动后平面会变化）
        if (mainCamera != null)
        {
            cameraFrustumPlanes = GeometryUtility.CalculateFrustumPlanes(mainCamera);
        }
    }

    // 这个公共方法供WaveManager调用
    public void SpawnEnemy(GameObject enemyPrefabToSpawn)
    {
        // 这里复用你之前写好的在屏幕外随机生成位置的逻辑
        Vector3 spawnPos = GetRandomSpawnPositionOutsideCamera();
        if (spawnPos != Vector3.zero)
        {
            Instantiate(enemyPrefabToSpawn, spawnPos, Quaternion.identity);
            Debug.Log("WaveManager生成敌人在: " + spawnPos);
        }
        else
        {
            Debug.LogWarning("WaveManager未找到有效的屏幕外生成位置，本次跳过。");
        }
    }

    void TrySpawnEnemy()
    {
        Vector3 spawnPosition = GetRandomSpawnPositionOutsideCamera();
        
        // 验证位置是否有效且在屏幕外
        if (spawnPosition != Vector3.zero)
        {
            Instantiate(enemyPrefab, spawnPosition, Quaternion.identity);
            Debug.Log("定时敌人在屏幕外生成于: " + spawnPosition);
        }
        else
        {
            Debug.LogWarning("未找到有效的屏幕外生成位置，本次跳过。");
        }
    }

    Vector3 GetRandomSpawnPositionOutsideCamera()
    {
        // 如果玩家或相机未初始化，尝试初始化
        if (playerTransform == null || mainCamera == null)
        {
            InitializeReferences();
        }
        
        if (playerTransform == null || mainCamera == null)
        {
            Debug.LogError("玩家或相机未找到，无法生成敌人！");
            return Vector3.zero;
        }

        // 方法：在玩家周围的一个"环形"区域随机生成点，直到找到一个屏幕外的位置
        int maxAttempts = 30; // 防止无限循环
        int attempts = 0;

        while (attempts < maxAttempts)
        {
            attempts++;

            // 1. 随机方向
            Vector2 randomDirection = Random.insideUnitCircle.normalized;
            // 2. 随机距离
            float randomDistance = Random.Range(minSpawnDistance, maxSpawnDistance);
            // 3. 计算世界坐标下的生成点
            Vector3 candidatePosition = playerTransform.position + new Vector3(randomDirection.x, randomDirection.y, 0) * randomDistance;

            // 4. 检查该点是否在屏幕外
            if (IsOutsideCameraView(candidatePosition))
            {
                return candidatePosition; // 找到有效的屏幕外点
            }
        }
        return Vector3.zero; // 多次尝试后未找到有效位置
    }

    bool IsOutsideCameraView(Vector3 worldPosition)
    {
        // 确保相机平面已更新
        if (cameraFrustumPlanes == null || cameraFrustumPlanes.Length == 0)
        {
            if (mainCamera != null)
            {
                cameraFrustumPlanes = GeometryUtility.CalculateFrustumPlanes(mainCamera);
            }
            else
            {
                return false;
            }
        }
        
        // 关键：使用GeometryUtility.TestPlanesAABB进行快速检测
        // 创建一个极小的"边界框"来代表这个世界点
        Bounds pointBounds = new Bounds(worldPosition, Vector3.one * 0.1f);

        // 如果这个点位于所有视锥体平面的"外侧"，则返回true（在屏幕外）
        // GeometryUtility.TestPlanesAABB 返回false表示点在视锥体外
        return !GeometryUtility.TestPlanesAABB(cameraFrustumPlanes, pointBounds);
    }
    
    // 用于重新初始化引用（如果玩家或相机被销毁/重新加载）
    public void InitializeReferences()
    {
        playerTransform = GameObject.FindGameObjectWithTag("Player")?.transform;
        mainCamera = Camera.main;
    }
}