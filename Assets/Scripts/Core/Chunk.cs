using UnityEngine;
using UnityEngine.Tilemaps;

public class Chunk : MonoBehaviour
{
    private Tilemap tilemap;
    private MultiSizeRandomTileGenerator tileGenerator;
    private Vector2Int coordinates;
    public void Initialize(int size, Vector2Int coord)
    {
        coordinates = coord;

        tilemap = ChunkManager.Instance.worldLayerTilemap;

        tileGenerator = gameObject.AddComponent<MultiSizeRandomTileGenerator>();
        tileGenerator.tilemap = tilemap;
        tileGenerator.tiles = ChunkManager.Instance.availableTiles;
        tileGenerator.mapSize = new Vector2Int(size, size);

        int seed = coordinates.x * 100000 + coordinates.y;
        Random.InitState(seed);

        tileGenerator.GenerateRandomTiles();
    }

    public void Deactivate()
    {
        gameObject.SetActive(false);
    }

    public void Activate()
    {
        gameObject.SetActive(true);
    }
}
