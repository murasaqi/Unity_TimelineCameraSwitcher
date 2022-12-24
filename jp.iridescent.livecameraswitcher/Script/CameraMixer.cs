using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Rendering;
using UnityEngine.UI;

namespace CameraLiveSwitcher
{

    public enum DepthStencilFormat
    {
        NONE = 1,
        D16_UNORM = 16,
        D24_UNORM = 32,
        D24_UNORM_S8_UINT = 24,
        D32_SFLOAT = 32,
        D32_SFLOAT_S8_UINT = 32,
    }


    public enum AntiAliasing
    {
        NONE = 0,
        x2 = 2,
        x4 = 4,
        x8 = 8,
    }

        [ExecuteAlways]
    public class CameraMixer : MonoBehaviour
    {
        public bool useTimeline = false;
        public Camera cam1;
        public Camera cam2;
        public int width = 1920;
        public int height = 1080;
        public RenderTextureFormat format = RenderTextureFormat.ARGB32;
        public DepthStencilFormat depthStencilFormat = DepthStencilFormat.D32_SFLOAT_S8_UINT;
        public RenderTexture renderTexture1;
        public RenderTexture renderTexture2;
        [Range(0, 1)] public float fader = 0f;
        public Shader shader;
        [SerializeField] private Material material;
        public AntiAliasing antiAliasing = AntiAliasing.NONE;
        public RenderTexture outputTarget;
        public RawImage outputImage;


        public List<Camera> cameraList = new List<Camera>();

        // Start is called before the first frame update
        void Start()
        {

        }
        
        public void InitRenderTextures()
        {
            RemoveCameraTargetTexture();
            if (renderTexture1 != null)
            {
                renderTexture1.Release();
                DestroyImmediate(renderTexture1);
            }
            if (renderTexture2 != null)
            {
                renderTexture2.Release();
                DestroyImmediate(renderTexture2);
            }
            renderTexture1 = new RenderTexture(width, height, (int)depthStencilFormat, format);
            renderTexture1.antiAliasing = (int)antiAliasing;
            
            renderTexture2 = new RenderTexture(width, height, (int)depthStencilFormat, format);
            renderTexture2.antiAliasing = (int)antiAliasing;
            if(material != null) material.SetTexture("_TextureA", renderTexture1);
            if(material != null) material.SetTexture("_TextureB", renderTexture2);
            
        }

        public void RemoveCameraTargetTexture()
        {

            foreach (var camera in cameraList)
            {
                if(camera)camera.targetTexture = null;
            }
            if (cam1 != null)
            {
                cam1.targetTexture = null;
            }
            if (cam2 != null)
            {
                cam2.targetTexture = null;
            }
        }

        [ContextMenu("Initialize")]
        public void Initialize()
        {
            if(material)DestroyImmediate(material);
            shader = Resources.Load<Shader>("CameraSwitcherResources/Shader/CameraSwitcherFader");
            material = new Material(shader);
            InitRenderTextures();
            if(cam1 != null)cam1.targetTexture = renderTexture1;
            if(cam2 != null)cam2.targetTexture = renderTexture2;
            material.SetTexture("_TextureA", renderTexture1);
            material.SetTexture("_TextureB", renderTexture2);
        }

        private void OnDestroy()
        {
            DestroyImmediate(material);
            DestroyImmediate(renderTexture1);
            DestroyImmediate(renderTexture2);
            cam1.targetTexture = null;
            cam2.targetTexture = null;
        }


        // public void Render()
        // {
            // if(renderTexture1 == null || renderTexture2 == null || material == null)
            // {
            //     Initialize();
            // }
            
            // if (cam1 != null) cam1.Render();
            // if (cam2 != null) cam2.Render();
            
            // material.SetFloat("_CrossFade", fader);
            
        // }

        public void BlitOutputTarget()
        {
            Graphics.Blit(Texture2D.blackTexture, outputTarget, material);
        }

        public void DisableCameras()
        {
            foreach (var camera in cameraList)
            {
                camera.enabled = false;
            }
        }

        public void SetCamera(Camera camera1, Camera camera2 = null, float blend = 0f)
        {
            // Debug.Log(blend);
            if (camera1 == null && camera2 == null) return;

            cam1 = camera1;
            if (cam1)
            {
                cam1.targetTexture = renderTexture1;
                cam1.enabled = true;
            }
            cam2 = camera2;
            if (cam2)
            {
                cam2.targetTexture = renderTexture2;
                cam2.enabled = true;
            }
            
            // Debug.Log(blend);
            fader = blend;
        }

        // Update is called once per frame
        void Update()
        {
            
            // cameraList.Distinct();
            // Render();
            
            if(renderTexture1 == null || renderTexture2 == null || material == null)
            {
                Initialize();
            }
            
            material.SetFloat("_CrossFade", fader);
            
            if (outputTarget != null)
            {
                BlitOutputTarget();    
            }
            
            if(outputImage!= null)
            {
                outputImage.material = material;
            }
            // Debug.Log(fader);
            // DisableCameras();
            // Render();
        }
    }

}