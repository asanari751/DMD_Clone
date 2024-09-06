using UnityEngine;
using UnityEngine.InputSystem;
using DG.Tweening;

public class PlayerStates : MonoBehaviour
{
    [SerializeField] private float dashDistance = 50f;
    [SerializeField] private float dashDuration = 0.2f;
    [SerializeField] private int maxDashGauge = 2;
    [SerializeField] private float dashGaugeRecoveryTime = 4f;
    [SerializeField] private Ease dashEase = Ease.OutQuad;

    private PlayerState currentState;
    private int currentDashGauge;
    private float lastDashTime;

    private PlayerController playerController;

    public int CurrentDashGauge => currentDashGauge;
    public int MaxDashGauge => maxDashGauge;

    public bool IsDashing => currentState is DashState;

    private void Start()
    {
        currentState = new NormalState(this);
        currentDashGauge = maxDashGauge;
        playerController = GetComponent<PlayerController>();
    }

    private void Update()
    {
        currentState.Update();
        RecoverDashGauge();
    }

    public void OnDash(InputValue value)
    {
        if (value.isPressed)
        {
            currentState.OnDash();
        }
    }

    public void ChangeState(PlayerState newState)
    {
        currentState = newState;
    }

    public void Dash()
    {
        if (currentDashGauge > 0)
        {
            PerformDash();
            currentDashGauge--;
            lastDashTime = Time.time;
        }
    }

    private void PerformDash()
    {
        Vector2 dashDirection = playerController.InputVector.normalized;
        Vector2 dashTarget = (Vector2)transform.position + dashDirection * dashDistance;

        transform.DOMove(dashTarget, dashDuration)
            .SetEase(dashEase)
            .OnComplete(() => ChangeState(new NormalState(this)));

        ChangeState(new DashState(this));
    }

    private void RecoverDashGauge()
    {
        if (currentDashGauge < maxDashGauge && Time.time - lastDashTime >= dashGaugeRecoveryTime)
        {
            currentDashGauge++;
            lastDashTime = Time.time;
        }
    }
}

public abstract class PlayerState
{
    protected PlayerStates playerStates;

    public PlayerState(PlayerStates playerStates)
    {
        this.playerStates = playerStates;
    }

    public abstract void Update();
    public abstract void OnDash();
}

public class NormalState : PlayerState
{
    public NormalState(PlayerStates playerStates) : base(playerStates) { }

    public override void Update() { }

    public override void OnDash()
    {
        playerStates.Dash();
    }
}

public class DashState : PlayerState
{
    public DashState(PlayerStates playerStates) : base(playerStates) { }

    public override void Update() { }

    public override void OnDash() { }
}