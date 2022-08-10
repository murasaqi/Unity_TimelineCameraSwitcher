using System.Collections.Generic;
using System.Linq;
using Cinemachine.Editor;
using UnityEditor;
using UnityEditor.Rendering;
using UnityEngine;
using UnityEngine.Rendering;

[CustomEditor(typeof(CameraSwitcherControlClip))]
public class CameraSwitcherControlClipEditor : BaseEditor<CameraSwitcherControlClip>
{
    List<string> mExcluded = new List<string>();
    SerializedProperty m_Profile;
    SerializedProperty m_volumeOverride;
    GUIContent m_ProfileLabel;
    GUIContent m_NewLabel;
    VolumeComponentListEditor m_ComponentList;
    CameraSwitcherControlClip m_Target;
    GUIContent m_CloneLabel;
    private void OnEnable()
    {
        m_Target = target as CameraSwitcherControlClip;
        m_NewLabel = new GUIContent("New", "Create a new profile.");
        m_ProfileLabel = new GUIContent("Profile", "A reference to a profile asset");
        m_CloneLabel = new GUIContent("Clone", "Create a new profile and copy the content of the currently assigned profile.");
        m_Profile = serializedObject.FindProperty("volumeProfile");
        m_volumeOverride = serializedObject.FindProperty("volumeOverride");
    }
    protected virtual void GetExcludedPropertiesInInspector(List<string> excluded)
    {
        excluded.Add("m_Script");
    }

    private void OnDisable()
    {
        
        DestroyComponentEditors();
    }

    private void OnDestroy()
    {
        DestroyComponentEditors();
    }
    
    public override void OnInspectorGUI()
    {
        BeginInspector();
        
        EditorGUI.BeginChangeCheck();
            DrawProperties(serializedObject, mExcluded.ToArray());
        if (EditorGUI.EndChangeCheck())
        {
            serializedObject.ApplyModifiedProperties();
            GUI.changed = false;
        }
        
        EditorGUI.BeginDisabledGroup(!m_volumeOverride.boolValue);
            EditorGUI.BeginChangeCheck();
                DrawProfileInspectorGUI();  
            EditorGUI.EndDisabledGroup();
        if (EditorGUI.EndChangeCheck())
        {
            serializedObject.ApplyModifiedProperties();
            GUI.changed = false;
        }

    }

    void DrawProperties(SerializedObject obj, params string[] propertyToExclude)
    {
        SerializedProperty iterator = obj.GetIterator();
        bool enterChildren = true;
        while (iterator.NextVisible(enterChildren))
        {
            
            
            enterChildren = false;
            if (!((IEnumerable<string>) propertyToExclude).Contains<string>(iterator.name))
            {
                if (iterator.name == "wigglerProps")
                {
                    EditorGUI.BeginDisabledGroup(!obj.FindProperty("wiggle").boolValue);
                    EditorGUILayout.PropertyField(iterator, true);
                    EditorGUI.EndDisabledGroup();
                }
                else
                if (iterator.name == "camera")
                {
                    
                    EditorGUI.BeginChangeCheck();
                    EditorGUILayout.PropertyField(iterator, true);
                    if (EditorGUI.EndChangeCheck())
                    {
                        serializedObject.ApplyModifiedProperties();
                        m_Target.Init();
                        GUI.changed = false;
                    }

                    EditorGUI.EndChangeCheck();

                }
                else
                if(iterator.name == "lookAtTarget" || iterator.name =="lookAtProps")
                {
                    EditorGUI.BeginDisabledGroup(!obj.FindProperty("lookAt").boolValue);
                    EditorGUILayout.PropertyField(iterator, true);
                    EditorGUI.EndDisabledGroup();
                }
                else
                if(iterator.name == "colorBlendProps")
                {
                    EditorGUI.BeginDisabledGroup(!obj.FindProperty("colorBlend").boolValue);
                    EditorGUILayout.PropertyField(iterator, true);
                    EditorGUI.EndDisabledGroup();
                }
                else
                {
                    EditorGUILayout.PropertyField(iterator, true);
                    
                }
               
                
            }
                
        }
    }
    void RefreshVolumeComponentEditor(VolumeProfile asset)
    {
        if (m_ComponentList == null)
            m_ComponentList = new VolumeComponentListEditor(this);
        m_ComponentList.Clear();
        if (asset != null)
            m_ComponentList.Init(asset, new SerializedObject(asset));
    }
    void DrawProfileInspectorGUI()
        {
            EditorGUILayout.Space();

            bool assetHasChanged = false;
            bool showCopy = m_Profile.objectReferenceValue != null;

            // The layout system sort of break alignement when mixing inspector fields with custom
            // layouted fields, do the layout manually instead
            int buttonWidth = showCopy ? 45 : 60;
            float indentOffset = EditorGUI.indentLevel * 15f;
            var lineRect = GUILayoutUtility.GetRect(1, EditorGUIUtility.singleLineHeight);
            var labelRect = new Rect(lineRect.x, lineRect.y, EditorGUIUtility.labelWidth - indentOffset, lineRect.height);
            var fieldRect = new Rect(labelRect.xMax, lineRect.y, lineRect.width - labelRect.width - buttonWidth * (showCopy ? 2 : 1), lineRect.height);
            var buttonNewRect = new Rect(fieldRect.xMax, lineRect.y, buttonWidth, lineRect.height);
            var buttonCopyRect = new Rect(buttonNewRect.xMax, lineRect.y, buttonWidth, lineRect.height);

            EditorGUI.PrefixLabel(labelRect, m_ProfileLabel);

            using (var scope = new EditorGUI.ChangeCheckScope())
            {
                m_Profile.objectReferenceValue
                    = (VolumeProfile)EditorGUI.ObjectField(
                        fieldRect, m_Profile.objectReferenceValue, typeof(VolumeProfile), false);
                assetHasChanged = scope.changed;
            }

            if (GUI.Button(buttonNewRect, m_NewLabel,
                showCopy ? EditorStyles.miniButtonLeft : EditorStyles.miniButton))
            {
                // By default, try to put assets in a folder next to the currently active
                // scene file. If the user isn't a scene, put them in root instead.
                var targetName = m_Target.name;
                var scene = m_Target.template.camera.scene;
                var asset = CreateVolumeProfile(scene, targetName);
                m_Profile.objectReferenceValue = asset;
                assetHasChanged = true;
            }

            if (showCopy && GUI.Button(buttonCopyRect, m_CloneLabel, EditorStyles.miniButtonRight))
            {
                // Duplicate the currently assigned profile and save it as a new profile
                var origin = (VolumeProfile)m_Profile.objectReferenceValue;
                var path = AssetDatabase.GetAssetPath(origin);
                path = AssetDatabase.GenerateUniqueAssetPath(path);

                var asset = Instantiate(origin);
                asset.components.Clear();
                AssetDatabase.CreateAsset(asset, path);

                foreach (var item in origin.components)
                {
                    var itemCopy = Instantiate(item);
                    itemCopy.hideFlags = HideFlags.HideInInspector | HideFlags.HideInHierarchy;
                    itemCopy.name = item.name;
                    asset.components.Add(itemCopy);
                    AssetDatabase.AddObjectToAsset(itemCopy, asset);
                }

                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();

                m_Profile.objectReferenceValue = asset;
                assetHasChanged = true;
            }

            if (m_Profile.objectReferenceValue == null)
            {
                if (assetHasChanged && m_ComponentList != null)
                    m_ComponentList.Clear(); // Asset wasn't null before, do some cleanup

                EditorGUILayout.HelpBox(
                    "Assign an existing Volume Profile by choosing an asset, or create a new one by clicking the \"New\" button.\n"
                    + "New assets are automatically put in a folder next to your scene file. If your scene hasn't "
                    + "been saved yet they will be created at the root of the Assets folder.",
                    MessageType.Info);
            }
            else
            {
                EditorGUILayout.Space();
                
                RefreshVolumeComponentEditor((VolumeProfile)m_Profile.objectReferenceValue);
                if (m_ComponentList != null)
                    m_ComponentList.OnGUI();
            }
        }
    void DestroyComponentEditors()
    {
        if (m_ComponentList != null) 
            m_ComponentList.Clear();
    }
    
    static VolumeProfile CreateVolumeProfile(UnityEngine.SceneManagement.Scene scene, string targetName)
    {
        var path = string.Empty;

        if (string.IsNullOrEmpty(scene.path))
        {
            path = "Assets/";
        }
        else
        {
            var scenePath = System.IO.Path.GetDirectoryName(scene.path);
            var extPath = scene.name + "_Profiles";
            var profilePath = scenePath + "/" + extPath;

            if (!AssetDatabase.IsValidFolder(profilePath))
                AssetDatabase.CreateFolder(scenePath, extPath);

            path = profilePath + "/";
        }

        path += targetName + " Profile.asset";
        path = AssetDatabase.GenerateUniqueAssetPath(path);

        var profile = ScriptableObject.CreateInstance<VolumeProfile>();
        AssetDatabase.CreateAsset(profile, path);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        return profile;
    }
    
}
