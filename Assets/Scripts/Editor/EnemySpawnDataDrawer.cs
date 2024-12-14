using UnityEngine;
using UnityEditor;

[CustomPropertyDrawer(typeof(EnemySpawner.EnemySpawnData))]
public class EnemySpawnDataDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        // EnemySpawnData의 부모 리스트를 가져옵니다.
        SerializedProperty enemySpawnDataList = property.serializedObject.FindProperty("enemySpawnDataList");
        int elementCount = enemySpawnDataList.arraySize;

        // 최대 spawnProbability를 계산합니다.
        float maxProbability = Mathf.Round(100f / elementCount);

        // 각 프로퍼티를 그립니다.
        EditorGUI.BeginProperty(position, label, property);
        SerializedProperty enemyPrefabProp = property.FindPropertyRelative("enemyPrefab");
        SerializedProperty spawnProbabilityProp = property.FindPropertyRelative("spawnProbability");
        SerializedProperty spawnTime0Prop = property.FindPropertyRelative("spawnTime0");
        SerializedProperty spawnTime1Prop = property.FindPropertyRelative("spawnTime1");

        float singleLineHeight = EditorGUIUtility.singleLineHeight;
        Rect rect = new Rect(position.x, position.y, position.width, singleLineHeight);

        EditorGUI.PropertyField(rect, enemyPrefabProp);
        rect.y += singleLineHeight + EditorGUIUtility.standardVerticalSpacing;

        // spawnProbability에 동적 Range 적용
        spawnProbabilityProp.floatValue = EditorGUI.Slider(rect, "Spawn Probability", spawnProbabilityProp.floatValue, 0f, maxProbability);
        rect.y += singleLineHeight + EditorGUIUtility.standardVerticalSpacing;

        EditorGUI.PropertyField(rect, spawnTime0Prop);
        rect.y += singleLineHeight + EditorGUIUtility.standardVerticalSpacing;

        EditorGUI.PropertyField(rect, spawnTime1Prop);

        EditorGUI.EndProperty();
    }

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        // 필드 4개에 대한 높이를 계산합니다.
        return (EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing) * 4;
    }
}
