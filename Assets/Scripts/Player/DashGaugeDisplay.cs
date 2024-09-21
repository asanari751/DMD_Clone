using UnityEngine;
using UnityEngine.UI;

public class DashGaugeDisplay : MonoBehaviour
{
    [SerializeField] private Image[] dashGaugeImages;
    [SerializeField] private PlayerDash playerDash;
    [SerializeField] private Color activeColor = Color.white;
    [SerializeField] private Color inactiveColor = Color.gray;

    private void Start()
    {
        if (dashGaugeImages == null || dashGaugeImages.Length == 0 || playerDash == null)
        {
            return;
        }

        for (int i = 0; i < dashGaugeImages.Length; i++)
        {
            dashGaugeImages[i].type = Image.Type.Filled;
            dashGaugeImages[i].fillMethod = Image.FillMethod.Horizontal;
            dashGaugeImages[i].fillOrigin = (int)Image.OriginHorizontal.Left;
        }

        UpdateDashGauge();
        playerDash.OnDashStateChanged += UpdateDashGauge;
    }

    private void OnDestroy()
    {
        if (playerDash != null)
        {
            playerDash.OnDashStateChanged -= UpdateDashGauge;
        }
    }

    private void UpdateDashGauge()
    {
        if (playerDash == null)
        {
            return;
        }

        float[] gauges = playerDash.GetDashGauges();
        if (gauges == null)
        {
            return;
        }
        
        for (int i = 0; i < dashGaugeImages.Length; i++)
        {
            if (dashGaugeImages[i] == null)
            {
                continue;
            }

            if (i < gauges.Length)
            {
                float targetFillAmount = gauges[i];
                dashGaugeImages[i].fillAmount = targetFillAmount;
                dashGaugeImages[i].color = targetFillAmount >= 1f ? activeColor : inactiveColor;
            }
            else
            {
                dashGaugeImages[i].fillAmount = 0f;
                dashGaugeImages[i].color = inactiveColor;
            }
        }
    }
}