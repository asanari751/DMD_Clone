using UnityEngine;
using UnityEngine.UI;

public class DashGaugeDisplay : MonoBehaviour
{
    [SerializeField] private Image[] dashGaugeImages;
    [SerializeField] private PlayerStates playerStates;
    [SerializeField] private Color activeColor = Color.white;
    [SerializeField] private Color inactiveColor = Color.gray;

    private void Start()
    {
        // 시작 시 대시 게이지 이미지 개수 확인
        if (dashGaugeImages.Length != playerStates.MaxDashGauge)
        {
            Debug.LogWarning("대시 게이지 이미지 개수가 최대 대시 게이지 수와 일치하지 않습니다.");
        }
    }

    private void Update()
    {
        UpdateDashGauge();
    }

    private void UpdateDashGauge()
    {
        int currentGauge = playerStates.CurrentDashGauge;
        
        for (int i = 0; i < dashGaugeImages.Length; i++)
        {
            if (i < currentGauge)
            {
                dashGaugeImages[i].color = activeColor;
            }
            else
            {
                dashGaugeImages[i].color = inactiveColor;
            }
        }
    }
}