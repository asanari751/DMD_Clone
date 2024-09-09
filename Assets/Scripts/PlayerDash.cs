using UnityEngine;
using UnityEngine.InputSystem;
using DG.Tweening;

public class PlayerDash : MonoBehaviour
{
    [SerializeField] private float dashDistance = 50f;
    [SerializeField] private float dashDuration = 0.2f;
    [SerializeField] private int dashGaugeCount = 3;
    [SerializeField] private float dashGaugeRecoveryRate = 0.25f;

    private float[] dashGauges;
    private PlayerController playerController;
    private bool isDashing;

    public event System.Action OnDashStateChanged;

    private void Awake()
    {
        InitializeDashGauges();
    }

    private void Start()
    {
        playerController = GetComponent<PlayerController>();
    }

    private void Update()
    {
        RecoverDashGauges();
    }

    public void OnDash(InputValue value)
    {
        if (value.isPressed && !isDashing && CanDash())
        {
            PerformDash();
        }
    }

    private void InitializeDashGauges()
    {
        dashGauges = new float[dashGaugeCount];
        for (int i = 0; i < dashGaugeCount; i++)
        {
            dashGauges[i] = 1f;
        }
    }

    private bool CanDash()
    {
        for (int i = dashGaugeCount - 1; i >= 0; i--)
        {
            if (dashGauges[i] >= 1f)
            {
                return true;
            }
        }
        return false;
    }

    private void PerformDash()
    {
        isDashing = true;
        Vector2 dashDirection = playerController.InputVector.normalized;
        Vector2 dashTarget = (Vector2)transform.position + dashDirection * dashDistance;

        transform.DOMove(dashTarget, dashDuration)
            .SetEase(Ease.OutQuad)
            .OnComplete(CompleteDash);

        ConsumeDashGauge();
        OnDashStateChanged?.Invoke();
    }

    private void CompleteDash()
    {
        isDashing = false;
        OnDashStateChanged?.Invoke();
    }

    private void ConsumeDashGauge()
    {
        for (int i = dashGaugeCount - 1; i >= 0; i--)
        {
            if (dashGauges[i] >= 1f)
            {
                dashGauges[i] = 0f;
                OnDashStateChanged?.Invoke();
                break;  // 하나의 게이지만 소비하고 루프를 종료
            }
        }
    }

    private void RecoverDashGauges()
    {
        bool gaugeChanged = false;
        for (int i = 0; i < dashGaugeCount; i++)
        {
            if (dashGauges[i] < 1f)
            {
                dashGauges[i] = Mathf.Min(dashGauges[i] + dashGaugeRecoveryRate * Time.deltaTime, 1f);
                gaugeChanged = true;
            }
        }
        if (gaugeChanged)
        {
            OnDashStateChanged?.Invoke();
        }
    }

    public float[] GetDashGauges()
    {
        return dashGauges;
    }

    public bool IsDashing()
    {
        return isDashing;
    }
}