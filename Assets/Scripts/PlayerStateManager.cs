using UnityEngine;
using DG.Tweening;

public class PlayerStateManager : MonoBehaviour
{
    [SerializeField] private float detectionRadius = 5f;
    [SerializeField] private LayerMask targetLayers;
    [SerializeField] private bool showDetectionRadius = true;
    [SerializeField] private Color detectionRadiusColor = new Color(1f, 1f, 0f, 0.2f);
    [SerializeField] private float detectionInterval = 0.5f;

    private GameObject currentTarget;
    private LineRenderer circleRenderer;
    private Tween detectionTween;

    private void Awake()
    {
        if (showDetectionRadius)
        {
            SetupCircleRenderer();
        }
    }

    private void Start()
    {
        StartDetectionLoop();
    }

    private void OnDisable()
    {
        StopDetectionLoop();
    }

    private void SetupCircleRenderer()
    {
        circleRenderer = gameObject.AddComponent<LineRenderer>();
        circleRenderer.useWorldSpace = true;
        circleRenderer.loop = true;
        circleRenderer.startWidth = 0.1f;
        circleRenderer.endWidth = 0.1f;
        circleRenderer.positionCount = 51;
        circleRenderer.material = new Material(Shader.Find("Sprites/Default"));
        circleRenderer.startColor = detectionRadiusColor;
        circleRenderer.endColor = detectionRadiusColor;

        DrawDetectionRadius();
    }

    private void DrawDetectionRadius()
    {
        float deltaTheta = (2f * Mathf.PI) / 50;
        float theta = 0f;

        for (int i = 0; i < 51; i++)
        {
            float x = detectionRadius * Mathf.Cos(theta);
            float y = detectionRadius * Mathf.Sin(theta);
            Vector3 pos = transform.position + new Vector3(x, y, 0);
            circleRenderer.SetPosition(i, pos);
            theta += deltaTheta;
        }
    }

    private void StartDetectionLoop()
    {
        detectionTween = DOVirtual.DelayedCall(detectionInterval, () =>
        {
            DetectClosestTarget();
            StartDetectionLoop();
        }, false);
    }

    private void StopDetectionLoop()
    {
        detectionTween?.Kill();
    }

    private void DetectClosestTarget()
    {
        Collider2D[] colliders = Physics2D.OverlapCircleAll(transform.position, detectionRadius, targetLayers);

        float closestDistance = float.MaxValue;
        GameObject closestTarget = null;

        foreach (Collider2D collider in colliders)
        {
            float distance = Vector2.Distance(transform.position, collider.transform.position);
            if (distance < closestDistance)
            {
                closestDistance = distance;
                closestTarget = collider.gameObject;
            }
        }

        currentTarget = closestTarget;

        // if (currentTarget != null)
        // {
            // Debug.Log($"Closest Target: {currentTarget.name}, Distance: {closestDistance}");
        // }
        // else
        // {
            // Debug.Log("No target detected");
        // }
    }

    public GameObject GetClosestTarget()
    {
        return currentTarget;
    }

    private void OnValidate()
    {
        if (showDetectionRadius && circleRenderer != null)
        {
            DrawDetectionRadius();
            circleRenderer.startColor = detectionRadiusColor;
            circleRenderer.endColor = detectionRadiusColor;
        }
    }

    private void Update()
    {
        if (showDetectionRadius)
        {
            DrawDetectionRadius();
        }
    }
}