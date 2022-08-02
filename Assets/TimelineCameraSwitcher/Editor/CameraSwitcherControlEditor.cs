using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEditor.VersionControl;
using UnityEngine;
#if USE_URP

using UnityEngine.Rendering.Universal;

#elif USE_HDRP

using UnityEngine.Rendering.HighDefinition;
#endif
using UnityEngine.UI;
using UnityEngine.UIElements;
using Button = UnityEngine.UIElements.Button;
using Toggle = UnityEngine.UIElements.Toggle;

[CustomEditor(typeof(CameraSwitcherControl))]//拡張するクラスを指定
public class CameraSwitcherControlEditor : Editor
{


    

    private List<Toggle> toggles = new List<Toggle>();
    private TemplateContainer container;
    public override VisualElement CreateInspectorGUI(){
        
        
        CameraSwitcherControl cameraSwitcherControl = target as CameraSwitcherControl;
        var treeAsset = Resources.Load<VisualTreeAsset>("CameraSwitcherControlResources/TimelineCameraSwitcherUI");
        container = treeAsset.Instantiate();
        var createButton = container.Q<Button>("CreateSettingsButton");

        var profileField = container.Q<ObjectField>("ProfileField");
        var cameraSwitcherPropsElement = container.Q<VisualElement>("CameraSwitcherPropsElement");

        cameraSwitcherPropsElement.SetEnabled(cameraSwitcherControl.cameraSwitcherSettings != null);
        if(cameraSwitcherControl.cameraSwitcherSettings != null) InitResolutionListButton(cameraSwitcherControl);
        profileField.objectType = typeof(CameraSwitcherSettings);
        // profileField.value = cameraSwitcherControl.
        profileField.RegisterValueChangedCallback((evt) =>
        {
            if (evt.newValue != null)
            {
                InitResolutionListButton(cameraSwitcherControl);
                cameraSwitcherControl.ApplyProfileSettings();
                cameraSwitcherControl.ChangeDofMode();
                cameraSwitcherPropsElement.SetEnabled(profileField.value != null);
            }
           
          
            
        });
        createButton.SetEnabled(cameraSwitcherControl.cameraSwitcherSettings == null);

        createButton.RegisterValueChangedCallback((evt) =>
        {
            createButton.SetEnabled(cameraSwitcherControl.cameraSwitcherSettings == null);
        });
        createButton.clicked += () =>
        {
            
            CreateAssets(cameraSwitcherControl);
            InitResolutionListButton(cameraSwitcherControl);
        };

        var settingsFoldout = container.Q<Foldout>("Settings");


        var resolutionListButton = container.Q<VisualElement>("ResolutionListButton");
        var widthField = resolutionListButton.Q<IntegerField>("WidthField");
        var heightField = resolutionListButton.Q<IntegerField>("HeightField");

        widthField.value = cameraSwitcherControl.width;
        heightField.value = cameraSwitcherControl.height;

        widthField.RegisterValueChangedCallback((evt) =>
        {
            cameraSwitcherControl.resolution = new Vector2Int(
                widthField.value,
                heightField.value
            );
            cameraSwitcherControl.SaveProfile();
            
        });
        
        heightField.RegisterValueChangedCallback((evt) =>
        {
            cameraSwitcherControl.resolution = new Vector2Int(
                widthField.value,
                heightField.value
            );
            cameraSwitcherControl.SaveProfile();
        });
        
        
        var resolutionListField=  container.Q<ScrollView>("ResolutionListField");
        var removeButton = container.Q<Button>("RemoveButton");
        if (cameraSwitcherControl.cameraSwitcherSettings != null)
        {
            InitResolutionListButton(cameraSwitcherControl);
        }

        var customWidthField = resolutionListButton.Q<IntegerField>("CustomWidthField");
        var customHeightField = resolutionListButton.Q<IntegerField>("CustomHeightField");
        var addResolutionButton = container.Q<Button>("AddResolutionButton");
       
        addResolutionButton.clicked += () =>
        {
            if (cameraSwitcherControl.cameraSwitcherSettings != null)
            {
                if(customWidthField.value == 0 || customHeightField.value == 0) return;
                var newResolution = new Vector2Int(customWidthField.value, customHeightField.value);
                foreach (var res in cameraSwitcherControl.cameraSwitcherSettings.resolutionList)
                {
                    if (res == newResolution) return;
                }
                cameraSwitcherControl.cameraSwitcherSettings.resolutionList.Add(newResolution);
                EditorUtility.SetDirty(cameraSwitcherControl.cameraSwitcherSettings);
                InitResolutionListButton(cameraSwitcherControl);
            }
        };

        removeButton.SetEnabled(false);
        removeButton.clicked += () =>
        {
            var count = 0;
            foreach (var t in toggles)
            {
                if(t.value) cameraSwitcherControl.cameraSwitcherSettings.resolutionList.RemoveAt(count);
                count++;
            }
            
            if(cameraSwitcherControl.cameraSwitcherSettings.resolutionList.Count != toggles.Count) InitResolutionListButton(cameraSwitcherControl);
        };
        
        var rowImageField = container.Q<ObjectField>("RawImageField");
        rowImageField.objectType = typeof(RawImage);
        
        var outPutRenderTargetField = container.Q<ObjectField>("RenderTextureField");
        outPutRenderTargetField.objectType = typeof(RenderTexture);


        // var dofControlField = container.Q<Toggle>("DofControlField");
        // var volumeFiled = container.Q<ObjectField>("VolumeField");
        // volumeFiled.objectType = typeof(VolumeProfile);
        // var volume = volumeFiled.binding as VolumeProfile;
        var dofParameterElement = container.Q<VisualElement>("DoFParameterElement");
        // dofControlField.RegisterValueChangedCallback((evt => dofParameterElement.SetEnabled(evt.newValue)));
        
        var dofParameters = container.Q<VisualElement>("DoFParameterElement");
        var dofMode = container.Q<EnumField>("DepthOfFieldMode");


        // if (cameraSwitcherControl.volume != null)
        // { 
        //     volumeFiled.value = cameraSwitcherControl.volume;
        //     cameraSwitcherControl.ChangeDofMode();
        // }

        CheckDofMode(cameraSwitcherControl);
        
        dofMode.RegisterValueChangedCallback((evt) =>
        {
            CheckDofMode(cameraSwitcherControl);
            // if(cameraSwitcherControl.volume != null) cameraSwitcherControl.ChangeDofMode();
            // if(cameraSwitcherControl.volume != null) cameraSwitcherControl.SetBaseDofValues();
            
        });
        // volumeFiled.RegisterValueChangedCallback((evt) =>
        // {
        //     // cameraSwitcherControl.volume = evt.newValue as VolumeProfile;
        //     CheckDofMode(cameraSwitcherControl);
        //     // if(cameraSwitcherControl.volume != null) cameraSwitcherControl.SetBaseDofValues();
        //     // dofParameters.SetEnabled(cameraSwitcherControl.volume != null);
        //     
        // });
        
        
        
        var bokeh = dofParameterElement.Q<Foldout>("Bokeh");
        var gaussian = dofParameterElement.Q<Foldout>("Gaussian");
        var physical = dofParameterElement.Q<PropertyField>("PhysicalCameraProps");
        var manual = dofParameterElement.Q<PropertyField>("ManualRangeProps");

        var applyProfileButton = dofParameterElement.Q<Button>("ApplyProfileButton");
        applyProfileButton.clicked += () =>
        {
            cameraSwitcherControl.ApplyBaseDofValuesToVolumeProfile();
        };
        
        physical.RegisterValueChangeCallback((evt) =>
        {
            cameraSwitcherControl.ApplyBaseDofValuesToVolumeProfile();
        });
        
        manual.RegisterValueChangeCallback((evt) =>
        {
            cameraSwitcherControl.ApplyBaseDofValuesToVolumeProfile();
        });
        return container;
        //targetを変換して対象を取得


        //PrivateMethodを実行する用のボタン


    }


  
    private void CheckDofMode(CameraSwitcherControl cameraSwitcherControl)
    {
        var dofParameterElement = container.Q<VisualElement>("DoFParameterElement");
        var dofMode = dofParameterElement.Q<EnumField>("DepthOfFieldMode");
        var bokeh = dofParameterElement.Q<Foldout>("Bokeh");
        var gaussian = dofParameterElement.Q<Foldout>("Gaussian");
        var physical = dofParameterElement.Q<PropertyField>("PhysicalCameraProps");
        var manual = dofParameterElement.Q<PropertyField>("ManualRangeProps");

#if USE_URP
        // bokeh.value = cameraSwitcherControl.depthOfFieldMode == DepthOfFieldMode.Bokeh;
        // bokeh.SetEnabled(cameraSwitcherControl.depthOfFieldMode == DepthOfFieldMode.Bokeh);    
        // gaussian.value =cameraSwitcherControl.depthOfFieldMode == DepthOfFieldMode.Gaussian;
        // gaussian.SetEnabled(cameraSwitcherControl.depthOfFieldMode == DepthOfFieldMode.Gaussian);
        physical.visible = false;
        manual.visible = false;

#elif USE_HDRP
        // bokeh.value = false;
        // gaussian.value = false;
        // bokeh.visible = false;
        // gaussian.visible = false;
        // bokeh.SetEnabled(false);
        // gaussian.SetEnabled(false);
        if(bokeh != null)bokeh.RemoveFromHierarchy();
        if(gaussian != null)gaussian.RemoveFromHierarchy();
        physical.SetEnabled(cameraSwitcherControl.depthOfFieldMode == DepthOfFieldMode.UsePhysicalCamera);
        manual.SetEnabled(cameraSwitcherControl.depthOfFieldMode == DepthOfFieldMode.Manual);
#endif
    }

    private void InitResolutionListButton(CameraSwitcherControl cameraSwitcherControl)
    {

        // if (container == null) return;
        var resolutionListField=  container.Q<ScrollView>("ResolutionListField");
        var removeButton = container.Q<Button>("RemoveButton");
        var resolutionListButton = container.Q<VisualElement>("ResolutionListButton");
        var widthField = resolutionListButton.Q<IntegerField>("WidthField");
        var heightField = resolutionListButton.Q<IntegerField>("HeightField");
        toggles.Clear();
        var resolutionButton = Resources.Load<VisualTreeAsset>("CameraSwitcherControlResources/ResolutionButton");
        resolutionListField.Clear();
        foreach (var res in cameraSwitcherControl.cameraSwitcherSettings.resolutionList)
        {
            var buttonInstantiate = resolutionButton.Instantiate();
            var button = buttonInstantiate.Q<Button>("ResolutionButton");
            var toggle = buttonInstantiate.Q<Toggle>();
            toggles.Add(toggle);
            button.name = $"{res.x} x {res.y}";
            button.text = button.name;
            button.clicked += () =>
            {
                cameraSwitcherControl.resolution = new Vector2Int(res.x, res.y);
                widthField.value = res.x;
                heightField.value = res.y;
            };

            toggle.RegisterValueChangedCallback((evt) =>
            {
                foreach (var t in toggles)
                {
                    removeButton.SetEnabled(t.value);
                }
            });
            resolutionListField.Add(buttonInstantiate);
        }
    }
    private void CreateAssets(CameraSwitcherControl cameraSwitcherControl)
    {
          // if (GUILayout.Button("Create settings"))
          //   {
                
                var path = EditorUtility.SaveFilePanelInProject("Save Asset", "CameraSwitcherProfile", "asset", "Please enter a file name to save the texture to");
                // if (string.IsNullOrEmpty(path))
                //     return;
                string fileName = Path.GetFileName(path);
                string dir = Path.GetDirectoryName(path);
                Debug.Log($"dir: {dir}, file: {fileName}");
                var baseSettings = Resources.Load<CameraSwitcherSettings>("CameraSwitcherControlResources/CameraSwitcherSetting");
                var setting = CreateInstance<CameraSwitcherSettings>();
                
                var inputCameraA = new RenderTexture(1920, 1080,24);
                var inputCameraB = new RenderTexture(1920, 1080,24);
                var compoMat = new Material(baseSettings.material);
                var depth = 0;
                if (cameraSwitcherControl.depth == DepthList.AtLeast16) depth = 16;
                if (cameraSwitcherControl.depth == DepthList.AtLeast24_WidthStencil) depth = 24;

                inputCameraA.depth = depth;
                inputCameraA.width = cameraSwitcherControl.width;
                inputCameraA.height = cameraSwitcherControl.height;
                inputCameraA.format = cameraSwitcherControl.renderTextureFormat;
                
                inputCameraB.depth = depth;
                inputCameraB.width = cameraSwitcherControl.width;
                inputCameraB.height = cameraSwitcherControl.height;
                inputCameraB.format = cameraSwitcherControl.renderTextureFormat;
              
                var exportPath_rtA = dir+"/inputCameraA.asset";
                var exportPath_rtB = dir+"/inputCameraB.asset";
                var exportPath_compositeMat = dir+"/composite.mat";

                
        
               
                setting.name = fileName;
                AssetDatabase.CreateAsset(setting, path);
                AssetDatabase.Refresh();

                cameraSwitcherControl.cameraSwitcherSettings = setting;

               
                inputCameraA.name = "inputCameraA";
                AssetDatabase.CreateAsset(inputCameraA, exportPath_rtA);
                AssetDatabase.Refresh();

                cameraSwitcherControl.renderTextureA = AssetDatabase.LoadAssetAtPath<RenderTexture>(exportPath_rtA);

               
                inputCameraB.name = "inputCameraB";
                AssetDatabase.CreateAsset(inputCameraB, exportPath_rtB);
                AssetDatabase.Refresh();

                cameraSwitcherControl.renderTextureB = AssetDatabase.LoadAssetAtPath<RenderTexture>(exportPath_rtB);
                
                compoMat.name = "composite";
                AssetDatabase.CreateAsset(compoMat, exportPath_compositeMat);
                AssetDatabase.Refresh();

                cameraSwitcherControl.material = AssetDatabase.LoadAssetAtPath<Material>(exportPath_compositeMat);

                setting.material = compoMat;
                setting.renderTextureA = inputCameraA;
                setting.renderTextureB = inputCameraB;  
               

            // }
    }
    private void CreateMaterialAsset()
    {
        
    }

}  


