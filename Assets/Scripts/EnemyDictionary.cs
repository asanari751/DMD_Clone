using UnityEngine;
using System.Collections.Generic;

public class EnemyDictionary : MonoBehaviour
{
    // 적 정보를 저장할 딕셔너리
    private Dictionary<string, EnemyInfo> enemies = new Dictionary<string, EnemyInfo>();

    // 적 정보를 나타내는 구조체
    [System.Serializable]
    public struct EnemyInfo
    {
        public string enemyname;
        public int enemyHealth;
        public int enemyDamage;
        public int enemyArmor;
        public float enemyASpeed;
        public float enemySpeed;
        // 필요한 다른 속성들을 여기에 추가할 수 있습니다.
    }

    // 적 정보 추가 메서드
    public void AddEnemy(string key, EnemyInfo info)
    {
        if (!enemies.ContainsKey(key))
        {
            enemies.Add(key, info);
        }
        else
        {
            Debug.LogWarning($"적 '{key}'는 이미 존재합니다.");
        }
    }

    // 적 정보 가져오기 메서드
    public EnemyInfo GetEnemy(string key)
    {
        if (enemies.TryGetValue(key, out EnemyInfo info))
        {
            return info;
        }
        Debug.LogWarning($"적 '{key}'를 찾을 수 없습니다.");
        return default;
    }

    // 적 정보 업데이트 메서드
    public void UpdateEnemy(string key, EnemyInfo newInfo)
    {
        if (enemies.ContainsKey(key))
        {
            enemies[key] = newInfo;
        }
        else
        {
            Debug.LogWarning($"업데이트할 적 '{key}'를 찾을 수 없습니다.");
        }
    }

    // 적 정보 삭제 메서드
    public void RemoveEnemy(string key)
    {
        if (enemies.Remove(key))
        {
            Debug.Log($"적 '{key}'가 제거되었습니다.");
        }
        else
        {
            Debug.LogWarning($"제거할 적 '{key}'를 찾을 수 없습니다.");
        }
    }
}
