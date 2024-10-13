using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PlayerSkills : MonoBehaviour
{
    public static PlayerSkills Instance { get; private set; }

    private Dictionary<string, int> skillLevels = new Dictionary<string, int>();

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void AddOrUpgradeSkill(string skillName)
    {
        if (skillLevels.ContainsKey(skillName))
        {
            skillLevels[skillName]++;
        }
        else
        {
            skillLevels[skillName] = 2;
        }
    }

    public int GetSkillLevel(string skillName)
    {
        return skillLevels.TryGetValue(skillName, out int level) ? level : 0;
    }
}
