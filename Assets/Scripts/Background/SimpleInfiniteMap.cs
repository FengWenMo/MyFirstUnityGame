using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SimpleInfiniteMap : MonoBehaviour
{
    public GameObject tilePrefab;      // 瓦片预制体
    public Transform player;           // 玩家引用
    public int viewDistance = 5;       // 可见距离
    public int tileSize = 10;          // 瓦片大小
    
    // 用于追踪已加载的瓦片
    private Dictionary<Vector2Int, GameObject> loadedTiles = new Dictionary<Vector2Int, GameObject>();
    
    void Update()
    {
        if (player == null) return;
        
        // 计算玩家所在的瓦片坐标
        Vector2Int playerTile = new Vector2Int(
            Mathf.RoundToInt(player.position.x / tileSize),
            Mathf.RoundToInt(player.position.y / tileSize)
        );
        
        // 创建一个列表来记录应该存在的瓦片位置
        HashSet<Vector2Int> shouldExistTiles = new HashSet<Vector2Int>();
        
        // 计算玩家周围应该存在的瓦片
        for (int x = -viewDistance; x <= viewDistance; x++)
        {
            for (int y = -viewDistance; y <= viewDistance; y++)
            {
                Vector2Int tilePos = new Vector2Int(
                    playerTile.x + x, 
                    playerTile.y + y
                );
                
                shouldExistTiles.Add(tilePos);
                
                // 如果这个位置的瓦片不存在，就创建它
                if (!loadedTiles.ContainsKey(tilePos))
                {
                    // 创建瓦片位置
                    Vector3 spawnPosition = new Vector3(
                        tilePos.x * tileSize,
                        tilePos.y * tileSize,
                        0
                    );
                    
                    // 生成瓦片
                    GameObject newTile = Instantiate(tilePrefab, spawnPosition, Quaternion.identity);
                    loadedTiles[tilePos] = newTile;
                }
            }
        }
        
        // 删除距离玩家太远的瓦片
        RemoveDistantTiles(shouldExistTiles);
    }
    
    // 删除距离玩家太远的瓦片
    private void RemoveDistantTiles(HashSet<Vector2Int> shouldExistTiles)
    {
        // 创建一个列表来记录需要删除的瓦片
        List<Vector2Int> tilesToRemove = new List<Vector2Int>();
        
        // 遍历所有已加载的瓦片
        foreach (var tileEntry in loadedTiles)
        {
            Vector2Int tilePos = tileEntry.Key;
            
            // 如果这个瓦片不在"应该存在"的列表中，就标记为需要删除
            if (!shouldExistTiles.Contains(tilePos))
            {
                tilesToRemove.Add(tilePos);
            }
        }
        
        // 删除标记的瓦片
        foreach (Vector2Int tilePos in tilesToRemove)
        {
            if (loadedTiles.TryGetValue(tilePos, out GameObject tile))
            {
                // 销毁GameObject
                Destroy(tile);
            }
            // 从字典中移除
            loadedTiles.Remove(tilePos);
        }
    }
    
    // 可选的：在游戏结束时清理所有瓦片
    private void OnDestroy()
    {
        foreach (var tile in loadedTiles.Values)
        {
            if (tile != null)
            {
                Destroy(tile);
            }
        }
        loadedTiles.Clear();
    }
}