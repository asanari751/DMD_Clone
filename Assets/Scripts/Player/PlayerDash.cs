using UnityEngine;
using UnityEngine.InputSystem;
using DG.Tweening;

public class PlayerDash : MonoBehaviour
{
    [SerializeField] private PlayerStats playerStats;
    [SerializeField] private AudioManager audioManager;
    [SerializeField] private PlayerHealthUI playerHealth;
    private float dashDistance;
    private float dashDuration = 0.2f;
    private int dashGaugeCount;
    private float dashGaugeRecoveryRate = 1f;

    private float[] dashGauges;
    private PlayerController playerController;
    private bool isDashing;
    public LayerMask collisionMask;

    public event System.Action OnDashStateChanged;
    
    private void Start()
    {
        playerStats = PlayerStats.Instance;
        playerController = GetComponent<PlayerController>();
        dashGaugeCount = (int)playerStats.dashCount;
        InitializeDashGauges();
    }

    private void Update()
    {
        RecoverDashGauges();
    }

    public void OnDash(InputValue value)
    {
        if (value.isPressed && !isDashing && CanDash() && !playerHealth.IsDead())
        {
            PerformDash();
            audioManager.PlaySFX("S1");
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
        if (playerController.InputVector.magnitude < 0.1f)
        {
            return false;
        }

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
        Vector2 dashTarget = (Vector2)transform.position + dashDirection * playerStats.dashRange;

        RaycastHit2D hit = Physics2D.Raycast(transform.position, dashDirection, playerStats.dashRange, collisionMask);
        if (hit.collider != null)
        {
            dashTarget = hit.point - (dashDirection * 0.1f);
        }

        float actualDashDistance = Vector2.Distance(transform.position, dashTarget);
        float adjustedDashDuration = dashDuration * (actualDashDistance / playerStats.dashRange);

        transform.DOMove(dashTarget, adjustedDashDuration)
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