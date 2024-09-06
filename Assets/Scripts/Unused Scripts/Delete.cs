using UnityEngine;
using UnityEngine.InputSystem;

public class DirectionIndicator : MonoBehaviour
{
    [SerializeField] private PlayerController playerController;
    [SerializeField] private float arrowLength = 2f;
    [SerializeField] private float arrowHeadSize = 0.25f;
    [SerializeField] private Color arrowColor = Color.red;

    private LineRenderer lineRenderer;

    private void Awake()
    {
        lineRenderer = gameObject.AddComponent<LineRenderer>();
        SetupLineRenderer();

        if (playerController == null)
        {
            Debug.LogError("PlayerController is not assigned!");
        }
    }

    private void SetupLineRenderer()
    {
        lineRenderer.startWidth = 0.1f;
        lineRenderer.endWidth = 0.1f;
        lineRenderer.positionCount = 5; // 화살표 몸통과 두 개의 화살표 머리
        lineRenderer.material = new Material(Shader.Find("Sprites/Default"));
        lineRenderer.startColor = arrowColor;
        lineRenderer.endColor = arrowColor;
        lineRenderer.useWorldSpace = false; // 로컬 좌표계 사용
    }

    private void Update()
    {
        UpdateArrow();
    }

    private void UpdateArrow()
    {
        Vector2 moveDirection = playerController.InputVector.normalized;

        if (moveDirection.magnitude < 0.1f)
        {
            lineRenderer.enabled = false;
            return;
        }

        lineRenderer.enabled = true;

        Vector3 start = Vector3.zero; // 플레이어의 중심
        Vector3 end = (Vector3)(moveDirection * arrowLength);

        lineRenderer.SetPosition(0, start);
        lineRenderer.SetPosition(1, end);

        // 화살표 머리 그리기
        Vector3 right = Quaternion.Euler(0, 0, 45) * -moveDirection * arrowHeadSize;
        Vector3 left = Quaternion.Euler(0, 0, -45) * -moveDirection * arrowHeadSize;

        lineRenderer.SetPosition(2, end);
        lineRenderer.SetPosition(3, end + right);
        lineRenderer.SetPosition(4, end + left);
    }
}