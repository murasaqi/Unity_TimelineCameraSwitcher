
using System;
using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Rendering;
using UnityEngine.UI;



// public enum CameraSwitcherOutputTarget
// {
//     RawImage,
//     RenderTexture
// }
[ExecuteAlways]
public class CameraSwitcherControl : MonoBehaviour
{
    
    
    [SerializeField] public CameraSwitcherSettings cameraSwitcherSettings;
    [SerializeField] public VolumeProfile volume;
    // [SerializeField] public CameraSwitcherOutputTarget outputTarget = CameraSwitcherOutputTarget.RenderTexture;
    [SerializeField] public RawImage outputRawImage;
    [SerializeField] public RenderTexture outPutRenderTarget;
    
    [SerializeField] private Vector2 m_resolution = new Vector2(1920,1080);
    [SerializeField] public int m_prerenderingFrameCount = 3;
    [SerializeField] private RenderTextureFormat m_renderTextureFormat;
    // [SerializeField] private CameraSwitcherSettings m_cameraSwitcherSettings;
    [SerializeField]public DepthList m_depth;
    
    [SerializeField] public bool dofControl = false;
    [SerializeField] public DofControlProps baseDofValues;
    

    
        
    // public int width => (int)m_resolution.x;
    // public int height => (int)m_resolution.y;
    public RenderTextureFormat renderTextureFormat => m_renderTextureFormat;

    // public CameraSwitcherSettings cameraSwitcherSettings => m_cameraSwitcherSettings;
    public DepthList depthList => m_depth;
    
    
    // [SerializeField] private Texture tex;
    private float m_fader;
    public RenderTexture renderTextureA => cameraSwitcherSettings.renderTextureA;
    public RenderTexture renderTextureB => cameraSwitcherSettings.renderTextureB;
    
    public int width => (int)m_resolution.x;
    public int height =>(int) m_resolution.y;



    public Camera cameraA;
    public Camera cameraB;
    
    public Material material => cameraSwitcherSettings.material;
    
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
        obj.AddComponent<CameraSwitcherControl>();
        //　作成したゲームオブジェクトを選択状態にする
        Selection.activeObject = obj;
    }

    #endif
    
    
    public int preRenderingFrameCount
    {
        get
        {
            return cameraSwitcherSettings.preRenderingFrameCount;
        }
        set
        {
            cameraSwitcherSettings.preRenderingFrameCount = value;
        }
    }

    private void Update()
    {

        if (cameraSwitcherSettings.resolution != m_resolution)
        {
            cameraSwitcherSettings.resolution = m_resolution;
        }

        if (cameraSwitcherSettings.preRenderingFrameCount != m_prerenderingFrameCount)
        {
            cameraSwitcherSettings.preRenderingFrameCount = m_prerenderingFrameCount;
        }
        
        // if(cameraSwitcherSettings.de)
        if (outputRawImage != null)
        {
            outputRawImage.material = material;
            outputRawImage.texture = null;
        } 
        
        Blit();
        
       
    }
    
    public void ReleaseRenderTarget()
    {
        if (cameraA) cameraA.targetTexture = null;
        if(cameraB) cameraB.targetTexture = null;
        
        renderTextureA.Release();
        renderTextureB.Release();
        // cameraA = null;
        // cameraB = null;
    }

    public void Blit()
    {
        if(material != null && outPutRenderTarget != null)Graphics.Blit(Texture2D.blackTexture, outPutRenderTarget, material, material.FindPass("Universal Forward"));
    }
}
