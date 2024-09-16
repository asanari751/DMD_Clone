using UnityEngine;
using System.Collections.Generic;

public class Flowfield : MonoBehaviour
{
    public Vector2Int gridSize = new Vector2Int(20, 20);
    public float cellSize = 1f;
    private Cell[,] grid;
    private Vector2Int playerGridPosition;
    public Transform playerTransform; // 플레이어의 Transform 컴포넌트
    private Vector2 gridOrigin;
    private Vector2 lastUpdatePosition;

    [Header("Gizmo Settings")]
    public bool showGrid = true;
    public bool showCost = true;
    public bool showDirection = true;
    public bool showArrows = true;
    public Color gridColor = Color.yellow;
    public Color arrowColor = Color.red;
    public float arrowSize = 0.5f;

    public LayerMask obstacleLayer; // Inspector에서 장애물 레이어 설정
    public Color obstacleColor = Color.red;

    public float obstacleCheckInterval = 0.5f; // 장애물 검사 간격 (초)
    private float lastObstacleCheckTime;
    private Dictionary<Vector2Int, CellCache> cellCache;

    private void Start()
    {
        if (playerTransform == null)
        {
            Debug.LogError("Player Transform is not set!");
            return;
        }

        Vector2 playerPos = playerTransform.position;
        gridOrigin = new Vector2(
            playerPos.x - (gridSize.x / 2f) * cellSize,
            playerPos.y - (gridSize.y / 2f) * cellSize
        );

        cellCache = new Dictionary<Vector2Int, CellCache>();
        CreateGrid();
        UpdateFlowField(true);
    }

    private void Update()
    {
        Vector2 playerPosition = playerTransform.position;
        Vector2Int newPlayerGridPosition = WorldToGrid(playerPosition);

        // 플레이어가 새로운 셀로 이동했는지 확인
        if (newPlayerGridPosition != playerGridPosition || ShouldRepositionGrid(playerPosition))
        {
            if (ShouldRepositionGrid(playerPosition))
            {
                RepositionGrid(playerPosition);
            }
            UpdateFlowField(Time.time - lastObstacleCheckTime >= obstacleCheckInterval);
            playerGridPosition = newPlayerGridPosition;

            if (Time.time - lastObstacleCheckTime >= obstacleCheckInterval)
            {
                lastObstacleCheckTime = Time.time;
            }
        }
    }

    private bool ShouldRepositionGrid(Vector2 playerPosition)
    {
        return Mathf.Abs(playerPosition.x - lastUpdatePosition.x) >= cellSize ||
               Mathf.Abs(playerPosition.y - lastUpdatePosition.y) >= cellSize;
    }

    private void RepositionGrid(Vector2 playerPosition)
    {
        gridOrigin = new Vector2(
            Mathf.Floor(playerPosition.x / cellSize) * cellSize - (gridSize.x / 2f) * cellSize,
            Mathf.Floor(playerPosition.y / cellSize) * cellSize - (gridSize.y / 2f) * cellSize
        );
        lastUpdatePosition = playerPosition;

        // 그리드의 모든 셀에 대해 장애물을 다시 검사
        for (int x = 0; x < gridSize.x; x++)
        {
            for (int y = 0; y < gridSize.y; y++)
            {
                Vector3 worldPos = GridToWorld(new Vector2Int(x, y));
                grid[x, y].isObstacle = CheckForObstacle(worldPos);
            }
        }
    }

    private void UpdateFlowField(bool forceUpdate)
    {
        SetPlayerPosition();
        if (forceUpdate)
        {
            // 모든 셀의 장애물 상태를 다시 검사
            for (int x = 0; x < gridSize.x; x++)
            {
                for (int y = 0; y < gridSize.y; y++)
                {
                    Vector3 worldPos = GridToWorld(new Vector2Int(x, y));
                    grid[x, y].isObstacle = CheckForObstacle(worldPos);
                }
            }
            CalculateCostField();
            CalculateFlowField();
            UpdateCellCache();
        }
        else
        {
            UpdateLocalFlowField();
        }
    }

    private void SetPlayerPosition()
    {
        playerGridPosition = WorldToGrid(playerTransform.position);

        if (IsValidGridPosition(playerGridPosition))
        {
            grid[playerGridPosition.x, playerGridPosition.y].cost = 0;
        }
        else
        {
            Debug.LogWarning($"Player position {playerGridPosition} is outside the grid!");
            playerGridPosition = ClampToGrid(playerGridPosition);
        }
    }

    private bool IsValidGridPosition(Vector2Int position)
    {
        return position.x >= 0 && position.x < gridSize.x &&
               position.y >= 0 && position.y < gridSize.y;
    }

    private Vector2Int ClampToGrid(Vector2Int position)
    {
        return new Vector2Int(
            Mathf.Clamp(position.x, 0, gridSize.x - 1),
            Mathf.Clamp(position.y, 0, gridSize.y - 1)
        );
    }

    private Vector2Int WorldToGrid(Vector2 worldPosition)
    {
        Vector2Int gridPosition = new Vector2Int(
            Mathf.FloorToInt((worldPosition.x - gridOrigin.x) / cellSize),
            Mathf.FloorToInt((worldPosition.y - gridOrigin.y) / cellSize)
        );
        return gridPosition;
    }

    private void CalculateCostField()
    {
        foreach (Cell cell in grid)
        {
            cell.cost = cell.isObstacle ? 99 : 98;
        }

        Queue<Cell> cellsToCheck = new Queue<Cell>();
        Cell playerCell = grid[playerGridPosition.x, playerGridPosition.y];
        playerCell.cost = 0f;
        cellsToCheck.Enqueue(playerCell);

        while (cellsToCheck.Count > 0)
        {
            Cell current = cellsToCheck.Dequeue();
            List<Cell> neighbors = GetNeighbors(current.position);

            foreach (Cell neighbor in neighbors)
            {
                if (neighbor.isObstacle) continue;

                float moveCost = (neighbor.position - current.position).magnitude == 1 ? 1f : 2f;
                float newCost = current.cost + moveCost;

                if (newCost < neighbor.cost)
                {
                    neighbor.cost = newCost;
                    cellsToCheck.Enqueue(neighbor);
                }
            }
        }
    }

    private void CalculateFlowField()
    {
        foreach (Cell cell in grid)
        {
            if (cell.isObstacle || cell.position == playerGridPosition)
            {
                cell.bestDirection = Vector2.zero;
                continue;
            }

            List<Cell> neighbors = GetNeighbors(cell.position);
            Cell bestNeighbor = null;
            float bestCost = float.MaxValue;

            foreach (Cell neighbor in neighbors)
            {
                if (neighbor.cost < bestCost)
                {
                    bestNeighbor = neighbor;
                    bestCost = neighbor.cost;
                }
            }

            if (bestNeighbor != null)
            {
                Vector2 direction = (Vector2)(bestNeighbor.position - cell.position);
                cell.bestDirection = NormalizeToEightDirections(direction);
            }
            else
            {
                cell.bestDirection = Vector2.zero;
            }
        }
    }

    private Vector2 NormalizeToEightDirections(Vector2 direction)
    {
        Vector2 normalized = direction.normalized;
        float angle = Mathf.Atan2(normalized.y, normalized.x);
        float snapAngle = Mathf.Round(angle / (Mathf.PI / 4)) * (Mathf.PI / 4);
        return new Vector2(Mathf.Cos(snapAngle), Mathf.Sin(snapAngle));
    }

    private List<Cell> GetNeighbors(Vector2Int position)
    {
        List<Cell> neighbors = new List<Cell>();

        for (int x = -1; x <= 1; x++)
        {
            for (int y = -1; y <= 1; y++)
            {
                if (x == 0 && y == 0) continue;

                int checkX = position.x + x;
                int checkY = position.y + y;

                if (checkX >= 0 && checkX < gridSize.x && checkY >= 0 && checkY < gridSize.y)
                {
                    neighbors.Add(grid[checkX, checkY]);
                }
            }
        }

        return neighbors;
    }

    // 적 객체가 사용할 메서드
    public Vector2 GetFlowDirection(Vector3 worldPosition)
    {
        Vector2Int gridPosition = WorldToGrid(worldPosition);
        if (IsValidGridPosition(gridPosition))
        {
            return grid[gridPosition.x, gridPosition.y].bestDirection;
        }
        return Vector2.zero;
    }

    private void OnDrawGizmos()
    {
        if (grid == null) return;

        for (int x = 0; x < gridSize.x; x++)
        {
            for (int y = 0; y < gridSize.y; y++)
            {
                Vector3 worldPos = GridToWorld(new Vector2Int(x, y));

                if (grid[x, y].isObstacle)
                {
                    DrawObstacle(worldPos);
                }
                else
                {
                    // 장애물이 아닌 셀에만 박스를 그립니다
                    Gizmos.color = gridColor;
                    Gizmos.DrawWireCube(worldPos, Vector3.one * cellSize);

                    if (showCost || showDirection)
                    {
                        DrawCellInfo(worldPos, x, y);
                    }

                    if (showArrows && grid[x, y].bestDirection != Vector2.zero)
                    {
                        DrawDirectionArrow(worldPos, grid[x, y].bestDirection);
                    }
                }
            }
        }
    }

    private void DrawGridCell(Vector3 position)
    {
        Gizmos.color = gridColor;
        Gizmos.DrawWireCube(position, Vector3.one * cellSize);
    }

    private void DrawCellInfo(Vector3 position, int x, int y)
    {
        string label = "";
        if (showCost)
        {
            // Mathf.RoundToInt를 사용하여 반올림한 정수 값으로 표시
            label += $"{Mathf.RoundToInt(grid[x, y].cost)}\n";
        }
        if (showDirection)
        {
            label += $"D:{grid[x, y].bestDirection}";
        }
        UnityEditor.Handles.Label(position, label);
    }

    private void DrawDirectionArrow(Vector3 start, Vector2 direction)
    {
        Vector3 end = start + (Vector3)(direction * cellSize * arrowSize);
        Gizmos.color = arrowColor;
        Gizmos.DrawLine(start, end);
        DrawArrowHead(start, end, 30f, cellSize * 0.2f);
    }

    private void DrawArrowHead(Vector3 start, Vector3 end, float angle, float length)
    {
        Vector3 direction = (end - start).normalized;
        Vector3 right = Quaternion.Euler(0, 0, angle) * direction * length;
        Vector3 left = Quaternion.Euler(0, 0, -angle) * direction * length;

        Gizmos.DrawLine(end, end - right);
        Gizmos.DrawLine(end, end - left);
    }

    private void DrawObstacle(Vector3 position)
    {
        Gizmos.color = obstacleColor;
        float size = cellSize * 0.8f;
        Vector3 topLeft = position + new Vector3(-size / 2, size / 2, 0);
        Vector3 topRight = position + new Vector3(size / 2, size / 2, 0);
        Vector3 bottomLeft = position + new Vector3(-size / 2, -size / 2, 0);
        Vector3 bottomRight = position + new Vector3(size / 2, -size / 2, 0);

        Gizmos.DrawLine(topLeft, bottomRight);
        Gizmos.DrawLine(topRight, bottomLeft);
    }

    private Vector3 GridToWorld(Vector2Int gridPosition)
    {
        return new Vector3(
            gridPosition.x * cellSize + gridOrigin.x + cellSize / 2,
            gridPosition.y * cellSize + gridOrigin.y + cellSize / 2,
            0
        );
    }

    private void UpdateLocalCostField()
    {
        int range = 5; // 업데이트할 범위 설정
        Vector2Int min = new Vector2Int(
            Mathf.Max(0, playerGridPosition.x - range),
            Mathf.Max(0, playerGridPosition.y - range)
        );
        Vector2Int max = new Vector2Int(
            Mathf.Min(gridSize.x - 1, playerGridPosition.x + range),
            Mathf.Min(gridSize.y - 1, playerGridPosition.y + range)
        );

        for (int x = min.x; x <= max.x; x++)
        {
            for (int y = min.y; y <= max.y; y++)
            {
                // 여기서 각 셀의 비용을 업데이트합니다.
                // 예: grid[x, y].cost = CalculateCost(new Vector2Int(x, y));
            }
        }
    }

    private void UpdateLocalFlowField()
    {
        int range = 5; // 업데이트할 범위 설정
        Vector2Int min = new Vector2Int(
            Mathf.Max(0, playerGridPosition.x - range),
            Mathf.Max(0, playerGridPosition.y - range)
        );
        Vector2Int max = new Vector2Int(
            Mathf.Min(gridSize.x - 1, playerGridPosition.x + range),
            Mathf.Min(gridSize.y - 1, playerGridPosition.y + range)
        );

        for (int x = min.x; x <= max.x; x++)
        {
            for (int y = min.y; y <= max.y; y++)
            {
                Vector2Int pos = new Vector2Int(x, y);
                if (!cellCache.ContainsKey(pos) || HasCellChanged(pos))
                {
                    UpdateCell(pos);
                }
            }
        }
    }
    private bool HasCellChanged(Vector2Int pos)
    {
        Cell cell = grid[pos.x, pos.y];
        CellCache cache = cellCache[pos];
        return cell.isObstacle != cache.isObstacle ||
               !Mathf.Approximately(cell.cost, cache.cost) ||
               cell.bestDirection != cache.bestDirection;
    }

    private void UpdateCell(Vector2Int pos)
    {
        Cell cell = grid[pos.x, pos.y];
        cell.cost = CalculateCellCost(pos);
        cell.bestDirection = CalculateCellBestDirection(pos);

        cellCache[pos] = new CellCache(cell.isObstacle, cell.cost, cell.bestDirection);
    }

    private float CalculateCellCost(Vector2Int pos)
    {
        if (grid[pos.x, pos.y].isObstacle)
            return 99;

        float distanceToPlayer = Vector2Int.Distance(pos, playerGridPosition);
        return Mathf.Min(98, distanceToPlayer);
    }

    private Vector2 CalculateCellBestDirection(Vector2Int pos)
    {
        if (grid[pos.x, pos.y].isObstacle || pos == playerGridPosition)
            return Vector2.zero;

        List<Cell> neighbors = GetNeighbors(pos);
        Cell bestNeighbor = null;
        float bestCost = float.MaxValue;

        foreach (Cell neighbor in neighbors)
        {
            if (neighbor.cost < bestCost)
            {
                bestNeighbor = neighbor;
                bestCost = neighbor.cost;
            }
        }

        if (bestNeighbor != null)
        {
            Vector2 direction = (Vector2)(bestNeighbor.position - pos);
            return NormalizeToEightDirections(direction);
        }

        return Vector2.zero;
    }

    private void UpdateCellCache()
    {
        cellCache.Clear();
        for (int x = 0; x < gridSize.x; x++)
        {
            for (int y = 0; y < gridSize.y; y++)
            {
                Vector2Int pos = new Vector2Int(x, y);
                Cell cell = grid[x, y];
                cellCache[pos] = new CellCache(cell.isObstacle, cell.cost, cell.bestDirection);
            }
        }
    }

    private void CreateGrid()
    {
        grid = new Cell[gridSize.x, gridSize.y];
        for (int x = 0; x < gridSize.x; x++)
        {
            for (int y = 0; y < gridSize.y; y++)
            {
                Vector3 worldPos = GridToWorld(new Vector2Int(x, y));
                bool isObstacle = CheckForObstacle(worldPos);
                grid[x, y] = new Cell(new Vector2Int(x, y), isObstacle);
            }
        }
    }

    private bool CheckForObstacle(Vector3 worldPos)
    {
        // OverlapBox를 사용하여 장애물 검사
        Collider2D obstacle = Physics2D.OverlapBox(worldPos, Vector2.one * cellSize, 0f, obstacleLayer);
        return obstacle != null;
    }
}

public class Cell
{
    public Vector2Int position;
    public float cost = float.MaxValue;
    public Vector2 bestDirection;
    public bool isObstacle;

    public Cell(Vector2Int pos, bool obstacle = false)
    {
        position = pos;
        isObstacle = obstacle;
    }
}
public class CellCache
{
    public bool isObstacle;
    public float cost;
    public Vector2 bestDirection;

    public CellCache(bool isObstacle, float cost, Vector2 bestDirection)
    {
        this.isObstacle = isObstacle;
        this.cost = cost;
        this.bestDirection = bestDirection;
    }
}
