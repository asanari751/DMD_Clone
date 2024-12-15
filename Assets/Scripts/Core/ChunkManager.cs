using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class ChunkManager : MonoBehaviour
{
    [SerializeField] private bool showChunkGizmos = true;
    [SerializeField] private Transform chunksContainer;
    public static ChunkManager Instance;
    public Transform player;
    public int chunkSize = 16;
    public int viewDistance = 3;
    private int removeDistance => viewDistance * 2;
    public MultiSizeRandomTileGenerator.TileData[] availableTiles;
    public Tilemap worldLayerTilemap;
    private Dictionary<Vector2Int, Chunk> chunks = new Dictionary<Vector2Int, Chunk>();
    private List<Vector2Int> chunksToRemove = new List<Vector2Int>();
    private Vector2Int currentChunk;

    private void Awake()
    {
        Instance = this;
    }

    private void Update()
    {
        Vector2Int newChunk = GetChunkCoordFromPosition(player.position);

        if (newChunk != currentChunk)
        {
            UpdateChunks(newChunk);
            currentChunk = newChunk;
        }
    }

    private Vector2Int GetChunkCoordFromPosition(Vector3 position)
    {
        return new Vector2Int(
            Mathf.FloorToInt(position.x / chunkSize),
            Mathf.FloorToInt(position.y / chunkSize)
        );
    }

    private void UpdateChunks(Vector2Int centerChunk)
    {
        chunksToRemove.Clear();

        foreach (var chunk in chunks)
        {
            if (Mathf.Abs(chunk.Key.x - centerChunk.x) > removeDistance ||
                Mathf.Abs(chunk.Key.y - centerChunk.y) > removeDistance)
            {
                RemoveChunkTiles(chunk.Key);
                chunksToRemove.Add(chunk.Key);
            }
        }

        foreach (var coord in chunksToRemove)
        {
            chunks[coord].gameObject.SetActive(false);
            chunks.Remove(coord);
        }

        for (int x = -viewDistance; x <= viewDistance; x++)
        {
            for (int y = -viewDistance; y <= viewDistance; y++)
            {
                Vector2Int coord = new Vector2Int(centerChunk.x + x, centerChunk.y + y);
                if (!chunks.ContainsKey(coord))
                {
                    CreateChunk(coord);
                }
            }
        }
    }

    private void RemoveChunkTiles(Vector2Int chunkCoord)
    {
        Vector3Int startPos = new Vector3Int(
            chunkCoord.x * chunkSize,
            chunkCoord.y * chunkSize,
            0
        );

        for (int x = 0; x < chunkSize; x++)
        {
            for (int y = 0; y < chunkSize; y++)
            {
                Vector3Int tilePos = startPos + new Vector3Int(x, y, 0);
                worldLayerTilemap.SetTile(tilePos, null);
            }
        }
    }

    private void CreateChunk(Vector2Int coord)
    {
        GameObject chunkObj = new GameObject($"Chunk_{coord.x}_{coord.y}");
        chunkObj.transform.SetParent(chunksContainer);
        Vector3 worldPosition = new Vector3(
            coord.x * chunkSize * 1.0f,
            coord.y * chunkSize * 1.0f,
            0
        );
        chunkObj.transform.position = worldPosition;

        Chunk chunk = chunkObj.AddComponent<Chunk>();
        chunk.Initialize(chunkSize, coord);

        chunks.Add(coord, chunk);
    }

    private void OnDrawGizmos()
    {
        if (!Application.isPlaying || !showChunkGizmos) return;

        // 현재 플레이어가 있는 청크와 주변 청크들을 표시
        Gizmos.color = new Color(0.5f, 1f, 0f, 0.5f);

        foreach (var chunk in chunks)
        {
            Vector3 chunkCenter = new Vector3(
                chunk.Key.x * chunkSize + chunkSize / 2f,
                chunk.Key.y * chunkSize + chunkSize / 2f,
                0
            );

            // 청크 영역 표시
            Gizmos.DrawWireCube(chunkCenter, new Vector3(chunkSize, chunkSize, 0));

            // 청크 영역 채우기
            Gizmos.color = new Color(0.2f, 0.8f, 1f, 0.1f);
            Gizmos.DrawCube(chunkCenter, new Vector3(chunkSize, chunkSize, 0));
            Gizmos.color = new Color(0.2f, 0.8f, 1f, 0.3f);
        }

        // 현재 플레이어가 있는 청크 강조 표시
        if (player != null)
        {
            Vector2Int currentChunkCoord = GetChunkCoordFromPosition(player.position);
            Vector3 currentChunkCenter = new Vector3(
                currentChunkCoord.x * chunkSize + chunkSize / 2f,
                currentChunkCoord.y * chunkSize + chunkSize / 2f,
                0
            );

            Gizmos.color = new Color(0.5f, 1f, 0f, 0.5f); // 연두색 반투명
            Gizmos.DrawWireCube(currentChunkCenter, new Vector3(chunkSize, chunkSize, 0));
        }
    }
}
