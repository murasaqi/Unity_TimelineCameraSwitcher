using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Rendering;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UIElements;
    

[CustomEditor(typeof(TestVolume))]
public class TestVolumeEditor : Editor
{
    SerializedProperty m_Profile;
    GUIContent m_ProfileLabel;
    GUIContent m_NewLabel;
    VolumeComponentListEditor m_ComponentList;
    TestVolume m_Target;
    GUIContent m_CloneLabel;
    void OnEnable()
    {
        m_Target = target as TestVolume;
        m_NewLabel = new GUIContent("New", "Create a new profile.");
        m_ProfileLabel = new GUIContent("Profile", "A reference to a profile asset");
        m_CloneLabel = new GUIContent("Clone", "Create a new profile and copy the content of the currently assigned profile.");
        m_Profile = serializedObject.FindProperty("profile");
    }

    void OnDisable()
    {
        if (m_ComponentList != null)
            m_ComponentList.Clear();
    }
    
    void RefreshVolumeComponentEditor(VolumeProfile asset)
    {
        if (m_ComponentList == null)
            m_ComponentList = new VolumeComponentListEditor(this);
        m_ComponentList.Clear();
        if (asset != null)
            m_ComponentList.Init(asset, new SerializedObject(asset));
    }


    public override void OnInspectorGUI()
    {
        EditorGUI.BeginChangeCheck();
        DrawProfileInspectorGUI();
        if (EditorGUI.EndChangeCheck())
        {
            m_Target.InvalidateCachedProfile();
            serializedObject.ApplyModifiedProperties();
        }
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
                var scene = m_Target.gameObject.scene;
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
                if (assetHasChanged)
                    RefreshVolumeComponentEditor((VolumeProfile)m_Profile.objectReferenceValue);
                if (m_ComponentList != null)
                    m_ComponentList.OnGUI();
            }
        }

        // Copied from UnityEditor.Rendering.PostProcessing.ProfileFactory.CreateVolumeProfile() because it's internal
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
public class TestVolume : MonoBehaviour
{

    public void InvalidateCachedProfile()
    {
        // var list = GetAllExtraStates<VcamExtraState>();
        // for (int i = 0; i < list.Count; ++i)
        //     list[i].DestroyProfileCopy();
    }
    public VolumeProfile profile;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
