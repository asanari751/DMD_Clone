using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(SkillData))]
public class SkillDataEditor : Editor
{
    public override void OnInspectorGUI()
    {
        SkillData skillData = (SkillData)target;
        serializedObject.Update();

        // 기본 속성들만 표시 (타입별 속성 제외)
        DrawPropertiesExcluding(serializedObject, new string[] { 
            "radius", "range", "angle", "maxTargets", "knockbackForce" 
        });

        // 패시브가 아닌 경우에만 넉백 값 표시
        if (!skillData.isPassive)
        {
            EditorGUILayout.Space();
            SerializedProperty knockbackProp = serializedObject.FindProperty("knockbackForce");
            EditorGUILayout.PropertyField(knockbackProp);
        }

        // 선택된 스킬 타입에 따라 추가 속성 표시
        EditorGUILayout.Space();
        EditorGUILayout.LabelField("타입별 속성", EditorStyles.boldLabel);

        switch (skillData.skillRangeType)
        {
            case SkillData.SkillRangeType.Circle:
                SerializedProperty radiusProp = serializedObject.FindProperty("radius");
                EditorGUILayout.PropertyField(radiusProp);
                break;

            case SkillData.SkillRangeType.Line:
                SerializedProperty rangeProp = serializedObject.FindProperty("range");
                EditorGUILayout.PropertyField(rangeProp);
                break;

            case SkillData.SkillRangeType.Cone:
                SerializedProperty angleProp = serializedObject.FindProperty("angle");
                SerializedProperty rangeProp2 = serializedObject.FindProperty("range");
                EditorGUILayout.PropertyField(angleProp);
                EditorGUILayout.PropertyField(rangeProp2);
                break;

            case SkillData.SkillRangeType.Single:
                SerializedProperty maxTargetsProp = serializedObject.FindProperty("maxTargets");
                EditorGUILayout.PropertyField(maxTargetsProp);
                break;
        }

        serializedObject.ApplyModifiedProperties();
    }
}
