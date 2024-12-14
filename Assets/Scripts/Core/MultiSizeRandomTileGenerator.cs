using System.Linq;
using UnityEngine;
using UnityEngine.Tilemaps;

public class MultiSizeRandomTileGenerator : MonoBehaviour
{
    [System.Serializable]
    public class TileData
    {
        public TileBase tile;
        public Vector2Int size = Vector2Int.one;
        public float spawnWeight = 1f;
    }

    public Tilemap tilemap;
    public TileData[] tiles;
    public Vector2Int mapSize = new Vector2Int(10, 10);
    private bool[,] occupiedSpaces;

    private int GetRandomTileIndexByWeight()
    {
        float totalWeight = tiles.Sum(t => t.spawnWeight);
        float randomValue = Random.Range(0f, totalWeight);
        float currentWeight = 0f;

        for (int i = 0; i < tiles.Length; i++)
        {
            currentWeight += tiles[i].spawnWeight;
            if (randomValue <= currentWeight)
                return i;
        }

        return tiles.Length - 1;
    }

    public void GenerateRandomTiles()
    {
        if (tilemap == null || tiles == null || tiles.Length == 0)
            return;

        occupiedSpaces = new bool[mapSize.x, mapSize.y];

        for (int x = 0; x < mapSize.x;)
        {
            for (int y = 0; y < mapSize.y;)
            {
                int randomTileIndex = GetRandomTileIndexByWeight();
                TileData selectedTile = tiles[randomTileIndex];

                if (CanPlaceTile(x, y, selectedTile.size))
                {
                    PlaceTile(x, y, selectedTile);
                    y += selectedTile.size.y;
                }
                else
                {
                    y++;
                }
            }
            x++;
        }
    }

    private bool TryPlaceTile(TileData tile)
    {
        // 모든 가능한 위치에 배치 시도
        for (int x = 0; x < mapSize.x - tile.size.x + 1; x++)
        {
            for (int y = 0; y < mapSize.y - tile.size.y + 1; y++)
            {
                if (CanPlaceTile(x, y, tile.size))
                {
                    PlaceTile(x, y, tile);
                    return true;
                }
            }
        }
        return false;
    }

    private bool CanPlaceTile(int x, int y, Vector2Int size)
    {
        for (int i = 0; i < size.x; i++)
        {
            for (int j = 0; j < size.y; j++)
            {
                if (occupiedSpaces[x + i, y + j])
                    return false;
            }
        }
        return true;
    }

    private void PlaceTile(int x, int y, TileData tileData)
    {
        Vector3 chunkPosition = transform.position;

        for (int i = 0; i < tileData.size.x; i++)
        {
            for (int j = 0; j < tileData.size.y; j++)
            {
                Vector3Int pos = new Vector3Int(
                    Mathf.FloorToInt(chunkPosition.x) + x + i,
                    Mathf.FloorToInt(chunkPosition.y) + y + j,
                    0
                );
                tilemap.SetTile(pos, tileData.tile);
                occupiedSpaces[x + i, y + j] = true;
            }
        }
    }

    private bool IsFull()
    {
        for (int x = 0; x < mapSize.x; x++)
        {
            for (int y = 0; y < mapSize.y; y++)
            {
                if (!occupiedSpaces[x, y]) return false;
            }
        }
        return true;
    }
}
