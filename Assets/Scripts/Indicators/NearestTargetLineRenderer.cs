using UnityEngine;
using TMPro;
using UnityEngine.InputSystem;

public class TargetLiner : MonoBehaviour
{
    [SerializeField] private PlayerStateManager autoAttackQ;
    [SerializeField] private Transform playerTransform;
    [SerializeField] private Color targetLineColor = Color.red;
    [SerializeField] private float targetLineWidth = 0.1f;
    [SerializeField] private TextMeshProUGUI debugText;
    [SerializeField] private bool showDebugInfo = true;

    private LineRenderer targetLineRenderer;
    private bool isActive = false;

    private void Awake()
    {
        SetupTargetLineRenderer();
        SetDebugText("Activate Liner : Q");
    }

    private void SetupTargetLineRenderer()
    {
        targetLineRenderer = gameObject.AddComponent<LineRenderer>();
        targetLineRenderer.useWorldSpace = true;
        targetLineRenderer.startWidth = targetLineWidth;
        targetLineRenderer.endWidth = targetLineWidth;
        targetLineRenderer.positionCount = 2;
        targetLineRenderer.material = new Material(Shader.Find("Sprites/Default"));
        targetLineRenderer.startColor = targetLineColor;
        targetLineRenderer.endColor = targetLineColor;
        targetLineRenderer.enabled = false;
    }

    private void Update()
    {
        if (Keyboard.current.qKey.wasPressedThisFrame)
        {
            ToggleTargetLiner();
        }

        if (isActive)
        {
            UpdateTargetLine();
        }
    }

    private void ToggleTargetLiner()
    {
        isActive = !isActive;
        if (isActive)
        {
            SetDebugText("Target liner active");
        }
        else
        {
            ClearTargetLine();
            SetDebugText("Target liner inactive");
        }
    }

    private void UpdateTargetLine()
    {
        if (playerTransform == null)
        {
            Debug.LogError("Player Transform is not assigned to TargetLiner!");
            return;
        }

        GameObject closestTarget = autoAttackQ.GetClosestTarget();

        if (closestTarget != null)
        {
            targetLineRenderer.enabled = true;
            targetLineRenderer.SetPosition(0, playerTransform.position);
            targetLineRenderer.SetPosition(1, closestTarget.transform.position);
            SetDebugText($"Auto Target : {closestTarget.name}");
        }
        else
        {
            targetLineRenderer.enabled = false;
            SetDebugText("No target found");
        }
    }

    private void ClearTargetLine()
    {
        targetLineRenderer.enabled = false;
    }

    private void OnValidate()
    {
        if (targetLineRenderer != null)
        {
            targetLineRenderer.startColor = targetLineColor;
            targetLineRenderer.endColor = targetLineColor;
            targetLineRenderer.startWidth = targetLineWidth;
            targetLineRenderer.endWidth = targetLineWidth;
        }
    }

    private void SetDebugText(string text)
    {
        if (showDebugInfo && debugText != null)
        {
            debugText.text = text;
        }
        else if (debugText != null)
        {
            debugText.text = "";
        }
    }
}