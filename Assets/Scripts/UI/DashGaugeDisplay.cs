using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Linq;

public class DashGaugeDisplay : MonoBehaviour
{
    [SerializeField] private Image dashGaugeImage;
    [SerializeField] private TextMeshProUGUI dashCountText;
    [SerializeField] private PlayerDash playerDash;
    [SerializeField] private Color activeColor = Color.white;
    [SerializeField] private Color inactiveColor = Color.gray;

    private void Start()
    {
        if (dashGaugeImage == null || playerDash == null || dashCountText == null)
        {
            Debug.LogError("Missing references in DashGaugeDisplay");
            return;
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

        int availableDashes = gauges.Count(g => g >= 1f);
        int totalDashes = gauges.Length;

        // Update the dash count text
        dashCountText.text = $"{availableDashes}";

        // Update the gauge image fill amount
        float fillAmount = (float)availableDashes / totalDashes;
        dashGaugeImage.fillAmount = fillAmount;

        // Update the color based on availability
        dashGaugeImage.color = availableDashes > 0 ? activeColor : inactiveColor;
    }
}