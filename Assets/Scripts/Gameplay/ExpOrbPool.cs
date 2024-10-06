using UnityEngine;
using System.Collections.Generic;

public class ExpOrbPool : MonoBehaviour
{
    [SerializeField] private GameObject expOrbPrefab;
    [SerializeField] private int poolSize = 50;

    private List<GameObject> expOrbPool;

    private void Awake()
    {
        InitializePool();
    }

    private void InitializePool()
    {
        expOrbPool = new List<GameObject>();
        for (int i = 0; i < poolSize; i++)
        {
            GameObject expOrb = Instantiate(expOrbPrefab, transform);
            expOrb.SetActive(false);
            expOrbPool.Add(expOrb);
        }
    }

    public GameObject GetExpOrb()
    {
        foreach (GameObject expOrb in expOrbPool)
        {
            if (!expOrb.activeInHierarchy)
            {
                return expOrb;
            }
        }

        GameObject newExpOrb = Instantiate(expOrbPrefab, transform);
        expOrbPool.Add(newExpOrb);
        return newExpOrb;
    }
}
