using UnityEngine;

public class MonsterHealthUI : HealthUI
{
    [SerializeField] private Transform monsterTransform;
    
    private void Update()
    {
        if (monsterTransform != null)
        {
            transform.position = Camera.main.WorldToScreenPoint(monsterTransform.position + Vector3.up * 1.5f);
        }
    }
}