#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
public static class PropertyDrawerUtility
{
    public static Rect DrawDefaultGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        property = property.serializedObject.FindProperty(property.propertyPath);
        var fieldRect = position;
        fieldRect.height = EditorGUIUtility.singleLineHeight;

        using ( new EditorGUI.PropertyScope(fieldRect, label, property)) 
        {
            if (property.hasChildren) {
                // 子要素があれば折り畳み表示
                property.isExpanded = EditorGUI.Foldout (fieldRect, property.isExpanded, label);
            }
            else {
                // 子要素が無ければラベルだけ表示
                EditorGUI.LabelField(fieldRect, label);
                return fieldRect;
            }
            fieldRect.y += EditorGUIUtility.singleLineHeight;
            fieldRect.y += EditorGUIUtility.standardVerticalSpacing;

            if (property.isExpanded) {

                using (new EditorGUI.IndentLevelScope()) 
                {
                    // 最初の要素を描画
                    property.NextVisible(true);
                    var depth = property.depth;
                    EditorGUI.PropertyField(fieldRect, property, true);
                    fieldRect.y += EditorGUI.GetPropertyHeight(property, true);
                    fieldRect.y += EditorGUIUtility.standardVerticalSpacing;

                    // それ以降の要素を描画
                    while(property.NextVisible(false)) {
                        
                        // depthが最初の要素と同じもののみ処理
                        if (property.depth != depth) {
                            break;
                        }

                        property.isExpanded = true;
                        EditorGUI.PropertyField(fieldRect, property, true);
                        fieldRect.y += EditorGUI.GetPropertyHeight(property, true);
                        fieldRect.y += EditorGUIUtility.standardVerticalSpacing;
                    }
                }
            }
        }

        return fieldRect;
    }

    public static float GetDefaultPropertyHeight(SerializedProperty property, GUIContent label)
    {
        property = property.serializedObject.FindProperty(property.propertyPath);
        var height = 0.0f;
        
        // プロパティ名
        height += EditorGUIUtility.singleLineHeight;
        height += EditorGUIUtility.standardVerticalSpacing;

        if (!property.hasChildren) {
            // 子要素が無ければラベルだけ表示
            return height;
        }
        
        if (property.isExpanded) {
        
            // 最初の要素
            property.NextVisible(true);
            var depth = property.depth;
            height += EditorGUI.GetPropertyHeight(property, true);
            height += EditorGUIUtility.standardVerticalSpacing;
            
            // それ以降の要素
            while(property.NextVisible(false))
            {
                // depthが最初の要素と同じもののみ処理
                if (property.depth != depth) {
                    break;
                }
                height += EditorGUI.GetPropertyHeight(property, true);
                height += EditorGUIUtility.standardVerticalSpacing;
            }
            // 最後はスペース不要なので削除
            height -= EditorGUIUtility.standardVerticalSpacing;
        }

        return height;
    }
}

#endif
