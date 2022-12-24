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

        private RenderTexture outputThumbnail;
        private Vector2 aspectRatio = Vector2.one;
        public DropdownField popUpField1;
        public DropdownField popUpField2;
        public override VisualElement CreateInspectorGUI()
        {

            DestroyInstantiateObjects();
            
            cameraMixer = serializedObject.targetObject as CameraMixer;
            
            outputThumbnail = new RenderTexture((int)(cameraMixer.width*0.1f), (int)(cameraMixer.height*0.1), 0,RenderTextureFormat.DefaultHDR);
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
            
            popUpField1 = root.Q<DropdownField>("CameraList1");
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
            popUpField1.index = cameraMixer.camera1Queue == null ? -1 : cameraMixer.cameraList.IndexOf(cameraMixer.camera1Queue);
            popUpField1.RegisterValueChangedCallback((v) =>
            {
                if(cameraMixer.cameraList.IndexOf(cameraMixer.camera1Queue) == popUpField1.index) return;
                var index = popUpField1.index;
                cameraMixer.camera1Queue = index >= 0 && index < cameraMixer.cameraList.Count ? cameraMixer.cameraList[index] : null;
            });
            popUpField2 = root.Q<DropdownField>("CameraList2");
            popUpField2.choices = cameraList;
            popUpField2.index = cameraMixer.camera2Queue == null ? -1 : cameraMixer.cameraList.IndexOf(cameraMixer.camera2Queue);
            popUpField2.RegisterValueChangedCallback((v) =>
            {
                if(cameraMixer.cameraList.IndexOf(cameraMixer.camera2Queue) == popUpField2.index) return;
                var index = popUpField2.index;
                cameraMixer.camera2Queue = index >= 0 && index < cameraMixer.cameraList.Count ? cameraMixer.cameraList[index] : null;
            });

            Resize();
            
            EditorApplication.update -= Repaint; //増殖を防ぐ
            EditorApplication.update += Repaint;
            
            
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

            if (cameraMixer.outputTarget != null)
            {
                outputImage.image = cameraMixer.outputTarget;
            }
            else if (cameraMixer.outputImage != null)
            {
                outputImage.image = outputThumbnail;

            }
            
            outputImage.style.height = camera1.layout.width * aspectRatio;
            
            previewWidth = camera1.layout.width;
            
        }

        private void OnDestroy()
        {
            DestroyInstantiateObjects();
        }

        private void OnDisable()
        {
            DestroyInstantiateObjects();
        }

        private void DestroyInstantiateObjects()
        {
            if (outputThumbnail != null)
            {
                outputThumbnail.Release();
                DestroyImmediate(outputThumbnail);
                
            }
        }
        
        private void Repaint()
        {
            Resize();
            
            if(cameraMixer == null) return;
            if (popUpField1 != null && popUpField1.index != cameraMixer.cameraList.IndexOf(cameraMixer.camera1Queue))
            {
                popUpField1.index = cameraMixer.camera1Queue == null ? -1 : cameraMixer.cameraList.IndexOf(cameraMixer.camera1Queue);
                serializedObject.ApplyModifiedProperties();
            }
            if (popUpField2 != null && popUpField2.index != cameraMixer.cameraList.IndexOf(cameraMixer.camera2Queue))
            {
                popUpField2.index = cameraMixer.camera2Queue == null ? -1 : cameraMixer.cameraList.IndexOf(cameraMixer.camera2Queue);
                serializedObject.ApplyModifiedProperties();
            }

            if (cameraMixer.outputImage != null && cameraMixer.outputTarget == null)
            {
                cameraMixer.BlitOutputTarget(outputThumbnail);
            }
        }
        private void OnEnable()
        {
            Resize();
        }
        
    }
}