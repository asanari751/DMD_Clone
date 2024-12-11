using UnityEngine;

public class S1 : MonoBehaviour // 원귀
{
    private SkillData skillData;
    private int skillLevel;

    public void Initialize(SkillData data, int level)
    {
        skillData = data;
        skillLevel = level;
        // 원귀 스킬의 구체적인 구현
    }
}