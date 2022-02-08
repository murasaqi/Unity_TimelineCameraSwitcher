using PlasticGui.Configuration.CloudEdition.Welcome;
using UnityEngine;
using UnityEditor;

[CustomPropertyDrawer(typeof(CameraSwitcherControlBehaviour))]
public class CameraSwitcherControlBehaviourDrawer :  PropertyDrawer
{
    
    
    // GUIContent m_TweenPositionContent = new GUIContent("Tween Position", "This should be true if the transformToMove to change position.  This causes recalulations each frame which are more CPU intensive.");
    // GUIContent m_wigglerPropsContent = new GUIContent("Tween Rotation", "This should be true if the transformToMove to change rotation.");
    // private GUIContent m_bokehPropsContent = new GUIContent("Tween Type");
    public override float GetPropertyHeight (SerializedProperty property, GUIContent label)
    {
        int fieldCount = 54;
        return fieldCount * (EditorGUIUtility.singleLineHeight);
    }
    
    public override void OnGUI (Rect position, SerializedProperty property, GUIContent label)
    {
        SerializedProperty wiggleProp = property.FindPropertyRelative ("wiggle");
        SerializedProperty wigglePropsProp = property.FindPropertyRelative("wigglerProps");
        var dofModeProp = property.FindPropertyRelative ("mode");
        SerializedProperty dofProp = property.FindPropertyRelative ("dofOverride");
#if USE_URP
        SerializedProperty bokehProp = property.FindPropertyRelative ("bokehProps");
        SerializedProperty gaussianProp = property.FindPropertyRelative ("gaussianProps");
#elif USE_HDRP
        
        SerializedProperty physicalCameraProp = property.FindPropertyRelative ("physicalCameraProps");
        SerializedProperty manualRangeProp = property.FindPropertyRelative ("manualRangeProps");
#endif
        SerializedProperty lookAtProp = property.FindPropertyRelative ("lookAt");
        SerializedProperty lookAtPropsProp = property.FindPropertyRelative ("lookAtProps");
        
        SerializedProperty colorBlendProp = property.FindPropertyRelative ("colorBlend");
        SerializedProperty colorBlendPropsProp = property.FindPropertyRelative ("colorBlendProps");
        
        Rect singleFieldRect = new Rect(position.x, position.y, position.width, EditorGUIUtility.singleLineHeight);
        EditorGUI.PropertyField (singleFieldRect, wiggleProp);
        position.y += EditorGUIUtility.singleLineHeight;

        EditorGUI.BeginDisabledGroup(!wiggleProp.boolValue);
        // wigglePropsProp.isExpanded = wiggleProp.boolValue;
        PropertyDrawerUtility.DrawDefaultGUI(position, wigglePropsProp, new GUIContent("Wiggler values"));
        position.y += wigglePropsProp.isExpanded ? EditorGUIUtility.singleLineHeight * 6 : EditorGUIUtility.singleLineHeight;
        EditorGUI.EndDisabledGroup();
        
        
        // EditorGUILayout.bar
        singleFieldRect = new Rect(position.x, position.y, position.width, EditorGUIUtility.singleLineHeight);
        EditorGUI.PropertyField (singleFieldRect, dofProp);
        position.y += EditorGUIUtility.singleLineHeight;
#if USE_URP        
        EditorGUI.BeginDisabledGroup(!dofProp.boolValue);
            EditorGUI.BeginDisabledGroup(dofModeProp.enumValueIndex != 2);

            PropertyDrawerUtility.DrawDefaultGUI(position, bokehProp, new GUIContent("Bokeh"));
            position.y += bokehProp.isExpanded ? EditorGUIUtility.singleLineHeight * 8 : EditorGUIUtility.singleLineHeight;

            EditorGUI.EndDisabledGroup();
        
            EditorGUI.BeginDisabledGroup(dofModeProp.enumValueIndex != 1);
            PropertyDrawerUtility.DrawDefaultGUI(position, gaussianProp, new GUIContent("Gaussian"));   
            position.y += gaussianProp.isExpanded ? EditorGUIUtility.singleLineHeight * 6 : EditorGUIUtility.singleLineHeight;
            position.y += bokehProp.isExpanded ? EditorGUIUtility.singleLineHeight * 8 : EditorGUIUtility.singleLineHeight;
        


            EditorGUI.EndDisabledGroup();  
        EditorGUI.EndDisabledGroup();

#elif USE_HDRP
        
        EditorGUI.BeginDisabledGroup(!dofProp.boolValue);
            // Debug.Log(dofModeProp.enumValueIndex);
            EditorGUI.BeginDisabledGroup(dofModeProp.enumValueIndex == 2);

            PropertyDrawerUtility.DrawDefaultGUI(position, physicalCameraProp, new GUIContent("Physical Camera"));
            position.y += physicalCameraProp.isExpanded
                ? EditorGUIUtility.singleLineHeight * 12
                : EditorGUIUtility.singleLineHeight;

            EditorGUI.EndDisabledGroup();
            
            EditorGUI.BeginDisabledGroup(dofModeProp.enumValueIndex == 1);
            PropertyDrawerUtility.DrawDefaultGUI(position, manualRangeProp, new GUIContent("Manual Range"));   
            position.y += manualRangeProp.isExpanded ? EditorGUIUtility.singleLineHeight * 16 : EditorGUIUtility.singleLineHeight;
            position.y += physicalCameraProp.isExpanded
                ? EditorGUIUtility.singleLineHeight *  1
                : 0;
            EditorGUI.EndDisabledGroup();  
        EditorGUI.EndDisabledGroup();
        
#endif
        
        
        singleFieldRect = new Rect(position.x, position.y, position.width, EditorGUIUtility.singleLineHeight);
        EditorGUI.PropertyField (singleFieldRect, lookAtProp);

        // lookAtPropsProp.isExpanded = lookAtProp.boolValue;
        position.y += EditorGUIUtility.singleLineHeight;

        // lookAtPropsProp. = lookAtProp.boolValue;
        EditorGUI.BeginDisabledGroup(!lookAtProp.boolValue);
        PropertyDrawerUtility.DrawDefaultGUI(position, lookAtPropsProp, new GUIContent("LookAt values"));
        position.y += lookAtPropsProp.isExpanded ? EditorGUIUtility.singleLineHeight * 10 :EditorGUIUtility.singleLineHeight;
       EditorGUI.EndDisabledGroup();
       
       
       singleFieldRect = new Rect(position.x, position.y, position.width, EditorGUIUtility.singleLineHeight);
       EditorGUI.PropertyField (singleFieldRect, colorBlendProp);
       position.y += EditorGUIUtility.singleLineHeight;
       EditorGUI.BeginDisabledGroup(!colorBlendProp.boolValue);
       PropertyDrawerUtility.DrawDefaultGUI(position, colorBlendPropsProp, new GUIContent("Blend color values"));   
       EditorGUI.EndDisabledGroup();
    }
    
    
    // public override Login.ValidateData
    
    
   
    

    // public override VisualElement CreatePropertyGUI(SerializedProperty property)
    // {
    //     // Create property container element.
    //     var container = new VisualElement();
    //
    //     // Create property fields.
    //     var amountField = new PropertyField(property.FindPropertyRelative("camera"));
    //     var unitField = new PropertyField(property.FindPropertyRelative("wiggle"));
    //     // var nameField = new PropertyField(property.FindPropertyRelative("name"), "Fancy Name");
    //
    //     // Add fields to the container.
    //     container.Add(amountField);
    //     container.Add(unitField);
    //     // container.Add(nameField);
    //
    //     return container;
    // }
}
