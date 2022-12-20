using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Rendering;

namespace CameraLiveSwitcher
{

    public enum DepthStencilFormat
    {
        NONE = 0,
        D16_UNORM = 16,
        D24_UNORM = 32,
        D24_UNORM_S8_UINT = 24,
        D32_SFLOAT = 32,
        D32_SFLOAT_S8_UINT = 32,
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

        public RenderTexture outputTarget;


        public List<Camera> cameraList = new List<Camera>();

        // Start is called before the first frame update
        void Start()
        {

        }
        
        public void InitRenderTextures()
        {
            if (renderTexture1 != null)
            {
                renderTexture1.Release();
            }
            if (renderTexture2 != null)
            {
                renderTexture2.Release();
            }
            renderTexture1 = new RenderTexture(width, height, (int)depthStencilFormat, format);
            // renderTexture1.Create();
            renderTexture2 = new RenderTexture(width, height, (int)depthStencilFormat, format);
            // renderTexture2.Create();
        }

        [ContextMenu("Initialize")]
        public void Initialize()
        {
            if(material)DestroyImmediate(material);
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


        public void Render()
        {
            if(renderTexture1 == null || renderTexture2 == null || material == null)
            {
                Initialize();
            }
            if (cam1 != null) cam1.Render();
            if (cam2 != null) cam2.Render();
            
            material.SetFloat("_CrossFade", fader);
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
            if (cam1) cam1.targetTexture = renderTexture1;
            cam2 = camera2;
            if (cam2) cam2.targetTexture = renderTexture2;
            // Debug.Log(blend);
            fader = blend;
        }

        // Update is called once per frame
        void Update()
        {
            if (!useTimeline)
            {
                cameraList.Distinct();
                DisableCameras();
                Render();
            }
            // Debug.Log(fader);
            // DisableCameras();
            // Render();
        }
    }

}