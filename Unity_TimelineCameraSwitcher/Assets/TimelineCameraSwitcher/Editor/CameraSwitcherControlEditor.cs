#if  UNITY_EDITOR

using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEditor.VersionControl;
using UnityEngine;
using UnityEngine.Rendering;
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
    private VisualTreeAsset cameraVolumeProfileSettingElement;
    private VisualElement cameraVolumeProfileSettingContainer;
    private CameraSwitcherControl cameraSwitcherControl;
    private Button removeCameraVolumeProfileButton;
    public override VisualElement CreateInspectorGUI(){
        
        
        cameraSwitcherControl = target as CameraSwitcherControl;
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
            }
            cameraSwitcherPropsElement.SetEnabled(profileField.value != null);
            createButton.SetEnabled(profileField.value == null);

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

        container.Q<VisualElement>("VolumeProfileSettingElement").Clear();
        cameraVolumeProfileSettingElement = Resources.Load<VisualTreeAsset>("CameraSwitcherControlResources/CameraVolumeProfile");
        cameraVolumeProfileSettingContainer = container.Q<ScrollView>("CameraVolumeProfileContainer");
        
        var volumeAObjectField =  container.Q<ObjectField>("VolumeAObjectField");
        volumeAObjectField.objectType = typeof(Volume);
        volumeAObjectField.value = cameraSwitcherControl.volumeA;
        volumeAObjectField.RegisterValueChangedCallback((v) =>
        {
            cameraSwitcherControl.volumeA = v.newValue as Volume;
        });
        var volumeBObjectField = container.Q<ObjectField>("VolumeBObjectField");
        volumeBObjectField.objectType = typeof(Volume);
        volumeBObjectField.value = cameraSwitcherControl.volumeB;
        volumeBObjectField.RegisterValueChangedCallback((v) =>
        {
            cameraSwitcherControl.volumeB = v.newValue as Volume;
        });

        if (cameraSwitcherControl.cameraVolumeProfileSettings.Count == 0)
        {
            var line = AddCameraVolumeProfileField(cameraVolumeProfileSettingElement.Instantiate(), null, null);
            cameraVolumeProfileSettingContainer.Add(line);
        }
        else
        {
            foreach (var pair in cameraSwitcherControl.cameraVolumeProfileSettings)
            {
                var line = AddCameraVolumeProfileField(cameraVolumeProfileSettingElement.Instantiate(), pair.camera, pair.volumeProfile);
                cameraVolumeProfileSettingContainer.Add(line);
            }    
        }
        
        
        
        var addVolumeSettingButton = container.Q<Button>("AddVolumeSettingButton");
        addVolumeSettingButton.clicked += () =>
        {
            var line = AddCameraVolumeProfileField(cameraVolumeProfileSettingElement.Instantiate(), null, null);
            cameraVolumeProfileSettingContainer.Add(line);
            CheckCameraVolumeSettingRemovable();
        };
        
        removeCameraVolumeProfileButton = container.Q<Button>("RemoveCameraVolumeSettingButton");
        removeCameraVolumeProfileButton.clicked += () =>
        {
            var removeTarget = new List<VisualElement>();
            foreach (var child in cameraVolumeProfileSettingContainer.Children())
            {
                var toggle = child.Q<Toggle>();
                if (toggle != null && toggle.value)
                {
                    removeTarget.Add(child);
                }
            }

            foreach (var target in removeTarget)
            {
                cameraVolumeProfileSettingContainer.Remove(target);
            }
            CheckCameraVolumeSettingRemovable();
        };
        
        
        
        CheckCameraVolumeSettingRemovable();
        // container.Remove();
        //
        // container.Q<VisualElement>("VolumeProfileSettingElement").visible = false;
        // var dofParameterElement = container.Q<VisualElement>("DoFParameterElement");
        // var dofParameters = container.Q<VisualElement>("DoFParameterElement");
        // var dofMode = container.Q<EnumField>("DepthOfFieldMode");
        // CheckDofMode(cameraSwitcherControl);
        // dofMode.RegisterValueChangedCallback((evt) =>
        // {
        //     CheckDofMode(cameraSwitcherControl);
        //     
        // });
        //
        // var bokeh = dofParameterElement.Q<Foldout>("Bokeh");
        // var gaussian = dofParameterElement.Q<Foldout>("Gaussian");
        // var physical = dofParameterElement.Q<PropertyField>("PhysicalCameraProps");
        // var manual = dofParameterElement.Q<PropertyField>("ManualRangeProps");
        //
        // var applyProfileButton = container.Q<Button>("ApplyProfileButton");
        // applyProfileButton.clicked += () =>
        // {
        //     cameraSwitcherControl.ApplyBaseDofValuesToVolumeProfile();
        // };
        //
        // physical.RegisterValueChangeCallback((evt) =>
        // {
        //     cameraSwitcherControl.ApplyBaseDofValuesToVolumeProfile();
        // });
        //
        // manual.RegisterValueChangeCallback((evt) =>
        // {
        //     cameraSwitcherControl.ApplyBaseDofValuesToVolumeProfile();
        // });
        return container;
    

    }

    public void UpdateCameraVolumeProfileData()
    {
        cameraSwitcherControl.cameraVolumeProfileSettings.Clear();
        foreach (var child in cameraVolumeProfileSettingContainer.Children())
        {
            var camera = child.Q<ObjectField>("CameraField").value as Camera;
            var profile  = child.Q<ObjectField>("VolumeProfileField").value as VolumeProfile;

            
            if(camera != null && profile != null)cameraSwitcherControl.cameraVolumeProfileSettings.Add(new CameraVolumeProfileSetting()
            {
                camera =  camera,
                volumeProfile = profile
            });
            
            
        }
        foreach (var pair in cameraSwitcherControl.cameraVolumeProfileSettings)
        {
            Debug.Log($"{pair.camera}, {pair.volumeProfile}");
        }

        Save();
    }

    private void CheckCameraVolumeSettingRemovable()
    {
        var isRemove = false;
        foreach (var child in cameraVolumeProfileSettingContainer.Children())
        {
            var toggle = child.Q<Toggle>();
            if(toggle!= null && toggle.value) isRemove = true;
        }
        
        removeCameraVolumeProfileButton.SetEnabled(isRemove);
        
        Save();
    }

    private VisualElement AddCameraVolumeProfileField(VisualElement cameraVolumeProfileSettingElement, Camera camera, VolumeProfile volumeProfile)
    {
        // var cameraVolumeProfileSetting = cameraVolumeProfileSettingElement.Instantiate();
        var cameraField = cameraVolumeProfileSettingElement.Q<ObjectField>("CameraField");
        var volumeProfileField = cameraVolumeProfileSettingElement.Q<ObjectField>("VolumeProfileField");
        cameraField.objectType = typeof(Camera);
        cameraField.value = camera;
        volumeProfileField.objectType = typeof(VolumeProfile);
        volumeProfileField.value = volumeProfile;
        var toggle = cameraVolumeProfileSettingElement.Q<Toggle>();

        toggle.RegisterValueChangedCallback(evt =>
        {
            CheckCameraVolumeSettingRemovable();
        });

        
        cameraField.RegisterValueChangedCallback((evt) =>
        {
            UpdateCameraVolumeProfileData();
        });
        
        volumeProfileField.RegisterValueChangedCallback((evt) =>
        {
            UpdateCameraVolumeProfileData();
        });
        Save();
        return cameraVolumeProfileSettingElement;
        
    
    }


    internal void Save()
    {
        EditorUtility.SetDirty(target);
        AssetDatabase.SaveAssets();
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
                EditorUtility.SetDirty(cameraSwitcherControl.cameraSwitcherSettings);
                AssetDatabase.SaveAssets();
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
                
                EditorUtility.SetDirty(setting);
                AssetDatabase.SaveAssets();

            // }
    }
    private void CreateMaterialAsset()
    {
        
    }

}  

#endif

