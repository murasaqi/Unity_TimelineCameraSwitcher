using System.IO;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.UI;
using UnityEngine.UIElements;
using Button = UnityEngine.UIElements.Button;
using Toggle = UnityEngine.UIElements.Toggle;

[CustomEditor(typeof(CameraSwitcherControl))]//拡張するクラスを指定
public class CameraSwitcherControlEditor : Editor {
    
    public override VisualElement CreateInspectorGUI(){
        
        
        CameraSwitcherControl cameraSwitcherControl = target as CameraSwitcherControl;
        var treeAsset = Resources.Load<VisualTreeAsset>("CameraSwitcherControlResources/TimelineCameraSwitcherUI");
        var container = treeAsset.Instantiate();
        var createButton = container.Q<Button>("CreateSettingsButton");

        if (cameraSwitcherControl.cameraSwitcherSettings != null) createButton.SetEnabled(false);
        createButton.clicked += () =>
        {
            
            CreateAssets(cameraSwitcherControl);
            
        };

        var settingsFoldout = container.Q<Foldout>("Settings");

        var rowImageField = container.Q<ObjectField>("RawImageField");
        rowImageField.objectType = typeof(RawImage);
        
        var outPutRenderTargetField = container.Q<ObjectField>("RenderTextureField");
        outPutRenderTargetField.objectType = typeof(RenderTexture);


        var dofControlField = container.Q<Toggle>("DofControlField");
        var volumeFiled = container.Q<PropertyField>("VolumeField");
        var value = volumeFiled.binding as Volume;
        var dofParameterElement = container.Q<VisualElement>("DoFParameterElement");
        dofControlField.RegisterValueChangedCallback((evt => dofParameterElement.SetEnabled(evt.newValue)));
        
        var dofParameters = container.Q<VisualElement>("DofParametersField");
        var dofMode = dofParameters.Q<PropertyField>("DepthOfFieldMode");
        var bokeh = dofParameters.Q<Foldout>("Bokeh");
        var gaussian = dofParameters.Q<Foldout>("Gaussian");


        // var depthOfFieldMode = dofMode.binding as EnumField;
        
        dofMode.RegisterValueChangeCallback((evt) =>
        {
            if(cameraSwitcherControl.volume != null) cameraSwitcherControl.ChangeDofMode();
            if(cameraSwitcherControl.volume != null) cameraSwitcherControl.SetBaseDofValues();
            CheckDofMode(cameraSwitcherControl, bokeh, gaussian);
        });
        volumeFiled.RegisterValueChangeCallback((evt) =>
        {
            if(cameraSwitcherControl.volume != null) cameraSwitcherControl.SetBaseDofValues();
            dofParameters.SetEnabled(cameraSwitcherControl.volume != null);
            CheckDofMode(cameraSwitcherControl, bokeh, gaussian);
        });
        return container;
        //targetを変換して対象を取得


        //PrivateMethodを実行する用のボタン


    }

    private void CheckDofMode(CameraSwitcherControl cameraSwitcherControl,Foldout bokeh, Foldout gaussian)
    {
        bokeh.value = cameraSwitcherControl.baseDofValues.depthOfFieldMode == DepthOfFieldMode.Bokeh;
        bokeh.SetEnabled(cameraSwitcherControl.baseDofValues.depthOfFieldMode == DepthOfFieldMode.Bokeh);
        gaussian.value = cameraSwitcherControl.baseDofValues.depthOfFieldMode == DepthOfFieldMode.Gaussian;
        gaussian.SetEnabled(cameraSwitcherControl.baseDofValues.depthOfFieldMode == DepthOfFieldMode.Gaussian);

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


