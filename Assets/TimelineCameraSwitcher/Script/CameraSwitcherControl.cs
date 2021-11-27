using System;
using System.Collections.Generic;
using System.Linq;
using PlasticGui;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

[Serializable]
public struct CameraSwitcherRenderQueValues
{
    public Camera camera;
    public RenderTexture renderTexture;
}
[Serializable]
public class CameraSwitcherRenderQue
{
    [SerializeField]
    private List<CameraSwitcherRenderQueValues> _renderQues = new List<CameraSwitcherRenderQueValues>();
    
    public void AddQue(Camera camera, RenderTexture renderTexture)
    {
        var q = new CameraSwitcherRenderQueValues();
        q.camera = camera;
        q.renderTexture = renderTexture;
        _renderQues.Add(q);
    }

    public void Render()
    {
        foreach (var renderQue in _renderQues)
        {
            renderQue.camera.targetTexture = renderQue.renderTexture;
            if(!renderQue.camera.enabled) renderQue.camera.Render();
        }
    }
}
[ExecuteAlways]
public class CameraSwitcherControl : MonoBehaviour
{
    [SerializeField] public CameraSwitcherSettings cameraSwitcherSettings;
    [SerializeField] public RawImage outputRawImage;
    [SerializeField] public RenderTexture outPutRenderTarget;
    private float _fader;
    public RenderTexture renderTextureA => cameraSwitcherSettings.renderTextureA;
    public RenderTexture renderTextureB => cameraSwitcherSettings.renderTextureB;

    [SerializeField] private Vector2Int resolution = new Vector2Int(1920,1080);
    [SerializeField] private int prerenderingFrameCount = 3;
    [SerializeField] private RenderTextureFormat renderTextureFormat;
    [SerializeField] private DepthList depth = DepthList.AtLeast16;
    public Material material => cameraSwitcherSettings.material;

    private CameraSwitcherRenderQue _cameraSwitcherRenderQue;
    
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

    public void AddRenderQue(CameraSwitcherRenderQue cameraSwitcherRenderQue)
    {
        _cameraSwitcherRenderQue = cameraSwitcherRenderQue;
    }

    private void Update()
    {

        if (resolution != cameraSwitcherSettings.resolution)
        {
            cameraSwitcherSettings.resolution = resolution;
            InitRenderTexture();
        }

        if (depth != cameraSwitcherSettings.depth)
        {
            cameraSwitcherSettings.depth = depth;
            InitRenderTexture();
        }

        if (prerenderingFrameCount != cameraSwitcherSettings.preRenderingFrameCount)
        {
            cameraSwitcherSettings.preRenderingFrameCount = prerenderingFrameCount;
            InitRenderTexture();
        }

        if (renderTextureFormat != cameraSwitcherSettings.renderTextureFormat)
        {
            InitRenderTexture();
        }

        Render();
        if (outputRawImage != null)
        {
            outputRawImage.material = material;
            outputRawImage.texture = null;
        }
    }

    public void Render()
    {
        if(_cameraSwitcherRenderQue != null)_cameraSwitcherRenderQue.Render();
    }
    
    public void InitRenderTexture()
    {
        
        var depth = 0;
        if (cameraSwitcherSettings.depth == DepthList.AtLeast16) depth = 16;
        if (cameraSwitcherSettings.depth == DepthList.AtLeast24_WidthStencil) depth = 24;

       
            renderTextureA.Release();
            renderTextureA.format= cameraSwitcherSettings.renderTextureFormat;
            renderTextureA.depth = depth;
            renderTextureA.width = cameraSwitcherSettings.resolution.x;
            renderTextureA.height = cameraSwitcherSettings.resolution.y;
       
           renderTextureB.Release();
           renderTextureB.format= cameraSwitcherSettings.renderTextureFormat;
           renderTextureB.depth = depth;
           renderTextureB.width = cameraSwitcherSettings.resolution.x;
           renderTextureB.height = cameraSwitcherSettings.resolution.y;
           
           outPutRenderTarget.Release();
           outPutRenderTarget.format= cameraSwitcherSettings.renderTextureFormat;
           outPutRenderTarget.depth = depth;
           outPutRenderTarget.width = cameraSwitcherSettings.resolution.x;
           outPutRenderTarget.height =cameraSwitcherSettings.resolution.y;
    
    }

    private void LateUpdate()
    {
        if (outPutRenderTarget)
        {
            Graphics.Blit(Texture2D.whiteTexture,outPutRenderTarget,cameraSwitcherSettings.material);
        }
        
    }

    public void Blit()
    {
        
    }
}
