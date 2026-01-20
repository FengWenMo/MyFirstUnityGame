using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TilePoolManager : MonoBehaviour
{
    [System.Serializable]
    public class TilePool
    {
        public GameObject prefab;
        public int initialPoolSize = 50;
        [HideInInspector] public Queue<GameObject> availableTiles = new Queue<GameObject>();
        [HideInInspector] public List<GameObject> allTiles = new List<GameObject>();
    }

    [Header("池配置")]
    public TilePool groundPool;
    public TilePool obstaclePool;
    
    [Header("瓦片设置")]
    public Material tileMaterial;
    public Sprite[] tileSprites;
    
    private static TilePoolManager _instance;
    public static TilePoolManager Instance => _instance;
    
    void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }
        _instance = this;
        
        InitializePool(groundPool);
        InitializePool(obstaclePool);
    }
    
    void InitializePool(TilePool pool)
    {
        for (int i = 0; i < pool.initialPoolSize; i++)
        {
            GameObject tile = Instantiate(pool.prefab);
            tile.SetActive(false);
            tile.transform.SetParent(transform);
            pool.availableTiles.Enqueue(tile);
            pool.allTiles.Add(tile);
        }
    }
    
    public GameObject GetTile(TilePool pool, Vector3 position)
    {
        if (pool.availableTiles.Count == 0)
        {
            ExpandPool(pool, 10);
        }
        
        GameObject tile = pool.availableTiles.Dequeue();
        tile.transform.position = position;
        tile.SetActive(true);
        
        return tile;
    }
    
    public void ReturnTile(GameObject tile, TilePool pool)
    {
        tile.SetActive(false);
        pool.availableTiles.Enqueue(tile);
    }
    
    void ExpandPool(TilePool pool, int amount)
    {
        for (int i = 0; i < amount; i++)
        {
            GameObject tile = Instantiate(pool.prefab);
            tile.SetActive(false);
            tile.transform.SetParent(transform);
            pool.availableTiles.Enqueue(tile);
            pool.allTiles.Add(tile);
        }
        
        Debug.Log($"扩展池 {pool.prefab.name}，新大小: {pool.allTiles.Count}");
    }
    
    // 统计信息
    public void PrintPoolInfo()
    {
        Debug.Log($"地面池: {groundPool.availableTiles.Count}/{groundPool.allTiles.Count} 可用");
        Debug.Log($"障碍池: {obstaclePool.availableTiles.Count}/{obstaclePool.allTiles.Count} 可用");
    }
}