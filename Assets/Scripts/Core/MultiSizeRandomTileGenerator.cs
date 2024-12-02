using System.Linq;
using UnityEngine;
using UnityEngine.Tilemaps;

public class MultiSizeRandomTileGenerator : MonoBehaviour
{
    [System.Serializable]
    public class TileData
    {
        public TileBase tile;
        public Vector2Int size = Vector2Int.one; // 타일의 크기 (1x1, 2x2, 3x3 등)
    }

    public Tilemap tilemap;
    public TileData[] tiles;
    public Vector2Int mapSize = new Vector2Int(10, 10);
    private bool[,] occupiedSpaces;
    private int blockSize; // 블록 크기 (청크 사이즈의 1/4)
    private Vector2Int[,] blockTileTypes; // 각 블록별 타일 타입 저장

    public void GenerateRandomTiles()
    {
        if (tilemap == null || tiles == null || tiles.Length == 0)
            return;

        blockSize = mapSize.x / 4; // 청크를 4x4 블록으로 분할
        blockTileTypes = new Vector2Int[4, 4];
        occupiedSpaces = new bool[mapSize.x, mapSize.y];

        // 각 블록별로 랜덤 타일 타입 지정
        for (int bx = 0; bx < 4; bx++)
        {
            for (int by = 0; by < 4; by++)
            {
                int randomTileIndex = Random.Range(0, tiles.Length);
                blockTileTypes[bx, by] = new Vector2Int(randomTileIndex, 0);
            }
        }

        // 블록별로 타일 배치
        for (int bx = 0; bx < 4; bx++)
        {
            for (int by = 0; by < 4; by++)
            {
                FillBlock(bx, by);
            }
        }
    }

    private void FillBlock(int blockX, int blockY)
    {
        int startX = blockX * blockSize;
        int startY = blockY * blockSize;
        int tileIndex = blockTileTypes[blockX, blockY].x;
        TileData selectedTile = tiles[tileIndex];

        for (int x = 0; x < blockSize; x += selectedTile.size.x)
        {
            for (int y = 0; y < blockSize; y += selectedTile.size.y)
            {
                if (CanPlaceTile(startX + x, startY + y, selectedTile.size))
                {
                    PlaceTile(startX + x, startY + y, selectedTile);
                }
            }
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
