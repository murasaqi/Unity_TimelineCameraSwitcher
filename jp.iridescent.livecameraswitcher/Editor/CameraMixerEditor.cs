using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UIElements;

namespace CameraLiveSwitcher
{
    [CustomEditor(typeof(CameraMixer))]
    [CanEditMultipleObjects]
    public class CameraMixerEditor : Editor
    {
        private CameraMixer cameraMixer;
        private Image camera1Image;
        private Image camera2Image;
        private Image outputImage;
        private float previewWidth = -1;
        private VisualElement root;
        public override VisualElement CreateInspectorGUI()
        {
            
            cameraMixer = serializedObject.targetObject as CameraMixer;
            root = Resources.Load<VisualTreeAsset>("CameraSwitcherResources/CameraSwitcherEditorGUI")
                .CloneTree("CameraMixer");
            
            var camera1Preview = root.Q<VisualElement>("Camera1Preview");
            camera1Image = new Image();
            camera1Preview.Add(camera1Image);
            camera1Image.image = cameraMixer.renderTexture1;
            
            var camera2Preview = root.Q<VisualElement>("Camera2Preview");
            camera2Image = new Image();
            camera2Preview.Add(camera2Image);
            camera2Image.image = cameraMixer.renderTexture2;

            outputImage = new Image();
            var outputPreview = root.Q<VisualElement>("OutputPreview");
            outputPreview.Add(outputImage);
            if(cameraMixer.outputTarget)outputImage.image = cameraMixer.outputTarget;
            
            
            var resolutionField = root.Q<Vector2IntField>("ResolutionField");
            resolutionField.value = new Vector2Int(cameraMixer.width, cameraMixer.height);
            resolutionField.RegisterValueChangedCallback((v) =>
            {
                cameraMixer.width = v.newValue.x;
                cameraMixer.height = v.newValue.y;
                cameraMixer.InitRenderTextures();
                Resize();
            });
            var xInput = resolutionField.Q<VisualElement>("unity-x-input");
            var yInput = resolutionField.Q<VisualElement>("unity-y-input");
            xInput.Q<Label>().text = "W";
            yInput.Q<Label>().text = "H";
            
            var antiAliasingField = root.Q<EnumField>("RTAntiAliasingField");
            antiAliasingField.RegisterValueChangedCallback((v) =>
            {
                cameraMixer.InitRenderTextures();
                Resize();
            });

            resolutionField.RegisterValueChangedCallback((v) =>
            {
                cameraMixer.InitRenderTextures();
                Resize();
            });
            
            root.Q<ObjectField>("Cam1Field").objectType = typeof(Camera);
            root.Q<ObjectField>("Cam2Field").objectType = typeof(Camera);
            
            var popUpField1 = root.Q<DropdownField>("CameraList1");
            var cameraList = new List<string>();
            // convert cameraMixer.cameraList to camera name list
            if (cameraMixer.cameraList != null)
            {
                foreach (var camera in cameraMixer.cameraList)
                {
                    if(camera != null)cameraList.Add(camera.name);
                }
    
            }
            
            popUpField1.choices = cameraList;
            popUpField1.index = cameraMixer.cam1 == null ? -1 : cameraMixer.cameraList.IndexOf(cameraMixer.cam1);
            popUpField1.RegisterValueChangedCallback((v) =>
            {
                cameraMixer.cam1 = cameraMixer.cameraList[popUpField1.index];
            });
            var popUpField2 = root.Q<DropdownField>("CameraList2");
            popUpField2.choices = cameraList;
            popUpField2.index = cameraMixer.cam2 == null ? -1 : cameraMixer.cameraList.IndexOf(cameraMixer.cam2);
            popUpField2.RegisterValueChangedCallback((v) =>
            {
                cameraMixer.cam2 = cameraMixer.cameraList[popUpField2.index];
            });

            
            var listField = root.Q<ListView>("CameraListView");
            listField.Bind(serializedObject);
            listField.bindingPath = "cameraList";
            listField.itemsSourceChanged += () =>
            {
                Resize();
            };
            Resize();
            return root;
        }

        public void Resize()
        {
            if(root == null) return;
            
            var camera1 = root.Q<VisualElement>("Camera1");
            var camera2 = root.Q<VisualElement>("Camera2");
            
            var aspectRatio = (float)cameraMixer.height / (float)cameraMixer.width;
            
            if (float.IsNaN(camera1.layout.width) || float.IsNaN(camera2.layout.width)) return;
            if(previewWidth == camera1.layout.width) return;
            // Debug.Log($"{camera1.layout.width}, {camera2.layout.width}");
            camera1Image.image = cameraMixer.renderTexture1 != null ? cameraMixer.renderTexture1 : Texture2D.grayTexture;
            camera2Image.image = cameraMixer.renderTexture2 != null ? cameraMixer.renderTexture2 : Texture2D.grayTexture;
            camera1Image.style.height =  camera1.layout.width * aspectRatio;
            camera2Image.style.height =  camera2.layout.width * aspectRatio;

            outputImage.image = cameraMixer.outputTarget == null ? Texture2D.grayTexture : cameraMixer.outputTarget;
            
            outputImage.style.height = camera1.layout.width * aspectRatio;
            
            previewWidth = camera1.layout.width;
        }

        private void OnSceneGUI()
        {
            Resize();
        }

        private void OnEnable()
        {
            Resize();
        }
        
        // private void OnDisable()
        // {
        //     Resize();
        // }

        private void OnValidate()
        {
            Resize();
        }
    }
}