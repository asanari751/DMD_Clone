using UnityEngine;
using UnityEditor;

[CustomPropertyDrawer(typeof(GameSettings.Resolution))]
public class ResolutionDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        EditorGUI.BeginProperty(position, label, property);

        var labelProp = property.FindPropertyRelative("label");
        var widthProp = property.FindPropertyRelative("width");
        var heightProp = property.FindPropertyRelative("height");
        var refreshProp = property.FindPropertyRelative("refreshRate");

        // 레이블 표시
        position.height = EditorGUIUtility.singleLineHeight;
        EditorGUI.PropertyField(position, labelProp, new GUIContent("Label"));

        position.y += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
        EditorGUI.PropertyField(position, widthProp);

        position.y += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
        EditorGUI.PropertyField(position, heightProp);

        position.y += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
        EditorGUI.PropertyField(position, refreshProp);

        EditorGUI.EndProperty();
    }

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        return (EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing) * 4;
    }
}
