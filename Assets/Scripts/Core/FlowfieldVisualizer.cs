using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class FlowFieldVisualizer : MonoBehaviour
{
    public bool showGrid = true;
    public bool showCost = true;
    public bool showDirection = true;
    public bool showArrows = true;
    public Color gridColor = Color.yellow;
    public Color arrowColor = Color.red;
    public float arrowSize = 0.5f;
    public Color obstacleColor = Color.red;

    private Flowfield flowfield;

    private void Awake()
    {
        flowfield = GetComponent<Flowfield>();
    }

    private void OnDrawGizmos()
    {
        if (flowfield == null || flowfield.Grid == null) return;

        for (int x = 0; x < flowfield.GridSize.x; x++)
        {
            for (int y = 0; y < flowfield.GridSize.y; y++)
            {
                Vector3 worldPos = flowfield.GridToWorld(new Vector2Int(x, y));

                if (flowfield.Grid[x, y].isObstacle)
                {
                    DrawObstacle(worldPos);
                }
                else
                {
                    if (showGrid)
                    {
                        DrawGridCell(worldPos);
                    }

                    if (showCost || showDirection)
                    {
                        DrawCellInfo(worldPos, x, y);
                    }

                    if (showArrows && flowfield.Grid[x, y].bestDirection != Vector2.zero)
                    {
                        DrawDirectionArrow(worldPos, flowfield.Grid[x, y].bestDirection);
                    }
                }
            }
        }
    }

    private void DrawGridCell(Vector3 position)
    {
        Gizmos.color = gridColor;
        Gizmos.DrawWireCube(position, Vector3.one * flowfield.CellSize);
    }

    private void DrawCellInfo(Vector3 position, int x, int y)
    {
        string label = "";
        if (showCost)
        {
            label += $"{Mathf.RoundToInt(flowfield.Grid[x, y].cost),3}\n";
        }
        if (showDirection)
        {
            label += $"D:{flowfield.Grid[x, y].bestDirection}";
        }
#if UNITY_EDITOR
        UnityEditor.Handles.Label(position, label);
#endif
    }

    private void DrawDirectionArrow(Vector3 start, Vector2 direction)
    {
        Vector3 end = start + (Vector3)(direction * flowfield.CellSize * arrowSize);
        Gizmos.color = arrowColor;
        Gizmos.DrawLine(start, end);
        DrawArrowHead(start, end, 30f, flowfield.CellSize * 0.2f);
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
        float size = flowfield.CellSize * 0.8f;
        Vector3 topLeft = position + new Vector3(-size / 2, size / 2, 0);
        Vector3 topRight = position + new Vector3(size / 2, size / 2, 0);
        Vector3 bottomLeft = position + new Vector3(-size / 2, -size / 2, 0);
        Vector3 bottomRight = position + new Vector3(size / 2, -size / 2, 0);

        Gizmos.DrawLine(topLeft, bottomRight);
        Gizmos.DrawLine(topRight, bottomLeft);
    }
}