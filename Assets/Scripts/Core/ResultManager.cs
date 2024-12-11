using System.Collections.Generic;
using UnityEngine;

public class ResultManager : MonoBehaviour
{
    public static ResultManager Instance { get; private set; }

    private float totalDamageDealt;
    private int totalEnemiesKilled;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            InitializeStats();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void InitializeStats()
    {
        totalDamageDealt = 0f;
        totalEnemiesKilled = 0;
    }
    
    public void ResetStats()
    {
        InitializeStats();
    }

    public void AddKill() // 총 처치한 적
    {
        totalEnemiesKilled++;
    }

    public void AddDamage(float damage) // 총 입힌 데미지
    {
        totalDamageDealt += damage;
    }

    public int GetKillCount() => totalEnemiesKilled;
    public float GetTotalDamage() => totalDamageDealt;
}
