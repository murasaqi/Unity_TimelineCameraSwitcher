#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;
using System;
using System.Collections.Generic;
using UnityEngine.Rendering;
#if USE_URP
using UnityEngine.Rendering.Universal;
#elif USE_HDRP
using UnityEngine.Rendering.HighDefinition;
#endif
using UnityEngine.UI;


[Serializable]
public class CameraVolumeProfileSetting
{
    public Camera camera;
    public VolumeProfile volumeProfile;
}

[ExecuteInEditMode]
public class CameraSwitcherControl : MonoBehaviour
{
    
    
    [SerializeField] public CameraSwitcherSettings cameraSwitcherSettings = null;
    // [SerializeField] public VolumeProfile volume;
    [SerializeField] public RawImage outputRawImage;
    [SerializeField] public RenderTexture outPutRenderTarget;
    [SerializeField] public Vector2Int resolution = new Vector2Int(1920,1080);
    [SerializeField, Range(0,10)] public int prerenderingFrameCount = 3;
    [SerializeField] public RenderTextureFormat renderTextureFormat = RenderTextureFormat.DefaultHDR;
    [SerializeField] public DepthList depth = DepthList.AtLeast24_WidthStencil;
    [HideInInspector] public Material material;


    [SerializeField] public Volume volumeA;
    [SerializeField] public Volume volumeB;
    
    [SerializeField] public LayerMask cameraALayer= new LayerMask();
    [SerializeField] public LayerMask cameraBLayer= new LayerMask();

  
    
   
    public DepthList depthList => depth;
    
    
    // [SerializeField] private Texture tex;
    private float m_fader;
    public RenderTexture renderTextureA;
    public RenderTexture renderTextureB;

    public VolumeProfile volumeProfileA;
    public VolumeProfile volumeProfileB;
    
    public int width => (int)resolution.x;
    public int height =>(int) resolution.y;

    public Camera cameraA;
    public Camera cameraB;
    
    public List<CameraVolumeProfileSetting> cameraVolumeProfileSettings = new List<CameraVolumeProfileSetting>();

    private DepthOfField dof;
    
    #if UNITY_EDITOR
    [MenuItem("GameObject/Camera Switcher Control/Camera Switcher Control", false, 10)]
    static void CreateCameraSwitcherControl(MenuCommand command) {
        //　空のゲームオブジェクト作成
        GameObject obj = new GameObject ("Camera Switcher Control");
        //　ゲームオブジェクトの親の設定
        GameObjectUtility.SetParentAndAlign (obj, command.context as GameObject);
        //　Undo操作を加える(Ctrl+Zキーの操作に加える）
        Undo.RegisterCreatedObjectUndo (obj, "Create " + obj.name);
        //　初期位置を設定
        var cameraSwitcherControl =  obj.AddComponent<CameraSwitcherControl>();
        //　作成したゲームオブジェクトを選択状態にする
        Selection.activeObject = obj;



        var volumeA = new GameObject().AddComponent<Volume>();
        volumeA.transform.SetParent(obj.transform);
        var volumeB = new GameObject().AddComponent<Volume>();
        volumeB.transform.SetParent(obj.transform);
        cameraSwitcherControl.volumeA = volumeA;
        cameraSwitcherControl.volumeB = volumeB;

        volumeA.gameObject.layer = cameraSwitcherControl.cameraALayer;
        volumeB.gameObject.layer = cameraSwitcherControl.cameraBLayer;


    }

    #endif


    public int depthValue
    {
        get
        {
            if (depthList == DepthList.AtLeast16)
            {
                return 16;
            }else if (depthList == DepthList.AtLeast24_WidthStencil)
            {
                return 24;
            }else
            {
                return 8;
            }
        }
    }    
 
    private void Update()
    {




        if(cameraSwitcherSettings == null) return;
        if (renderTextureA == null && cameraSwitcherSettings.renderTextureA != null)
            renderTextureA = cameraSwitcherSettings.renderTextureA;
        
        if (renderTextureB == null && cameraSwitcherSettings.renderTextureB != null)
            renderTextureB = cameraSwitcherSettings.renderTextureB;
        if (outputRawImage != null)
        {
            outputRawImage.material = material;
            outputRawImage.texture = null;
        }

        // CheckTextureFormat();
        
        Blit();
    }

    
    public void CheckTextureFormat()
    {
        if(renderTextureA != null)CheckTextureFormat(renderTextureA);
        if(renderTextureB != null)CheckTextureFormat(renderTextureB);
    }

    private void CheckTextureFormat(RenderTexture renderTexture)
    {
        var depth = renderTexture.depth;
        if (renderTexture.format != renderTextureFormat ||
            renderTexture.format != renderTextureFormat ||
            renderTexture.width != width ||
            renderTexture.height != height
           )
        {
            InitRenderTexture(renderTexture);
        }
       
    }

    public VolumeProfile CloneProfile(VolumeProfile volumeProfile)
    {
        return Instantiate(volumeProfile);
    }

    
    public void ApplyProfileSettings()
    {
        if (cameraSwitcherSettings == null) return;

        renderTextureA = cameraSwitcherSettings.renderTextureA;
        renderTextureB = cameraSwitcherSettings.renderTextureB;
        resolution = cameraSwitcherSettings.resolution;
#if UNITY_EDITOR
        EditorUtility.SetDirty(cameraSwitcherSettings);
        AssetDatabase.SaveAssets();
    
#endif
        // volume = cameraSwitcherSettings.volume;
    }
    
    public void SaveProfile()
    {
        if(cameraSwitcherSettings == null) return;

        cameraSwitcherSettings.resolution = resolution;
#if UNITY_EDITOR
        EditorUtility.SetDirty(cameraSwitcherSettings);
        AssetDatabase.SaveAssets();
#endif
        // cameraSwitcherSettings.volume = volume;
        
    }

    
     private void InitRenderTexture(RenderTexture renderTexture)
     {
        
         renderTexture.Release();
         renderTexture.format = renderTextureFormat;
         renderTexture.depth = depthValue;
         renderTexture.width = width;
         renderTexture.height = height;    
     }

     public void InitRenderTextures()
     {
         if(renderTextureA != null)InitRenderTexture(renderTextureA);
         if(renderTextureB != null)InitRenderTexture(renderTextureB);
     }

    public void ReleaseRenderTarget()
    {
        if (cameraA) cameraA.targetTexture = null;
        if(cameraB) cameraB.targetTexture = null;
        
        renderTextureA.Release();
        renderTextureB.Release();
    }

    private Material thumbnailsMaterial;
    public void BlitThumbnail(RenderTexture target, Color color)
    {
        if(thumbnailsMaterial == null)
        {
            thumbnailsMaterial = Resources.Load<Material>("CameraSwitcherControlResources/ThumbnailMat");
        }
        thumbnailsMaterial.SetColor("_Color",color);
        Graphics.Blit(renderTextureA, target, thumbnailsMaterial, thumbnailsMaterial.FindPass("Universal Forward"));

    }

    public void Blit()
    {
        #if USE_URP
        if(material != null && outPutRenderTarget != null)Graphics.Blit(Texture2D.blackTexture, outPutRenderTarget, material, material.FindPass("Universal Forward"));
#elif USE_HDRP
        #endif
    }
}
