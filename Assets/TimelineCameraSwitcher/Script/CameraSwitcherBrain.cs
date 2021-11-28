using System;
using System.Collections.Generic;
using System.Linq;
using PlasticGui;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;


[Serializable]
public enum DepthList
{
    None = 0,
    AtLeast16 = 16,
    AtLeast24_WidthStencil = 24,
}

// [Serializable]
// public struct CameraSwitcherRenderQueValues
// {
//     public Camera camera;
//     public RenderTexture renderTexture;
// }
// [Serializable]
// public class CameraSwitcherRenderQue
// {
//     [SerializeField]
//     private List<CameraSwitcherRenderQueValues> _renderQues = new List<CameraSwitcherRenderQueValues>();
//     
//     public void AddQue(Camera camera, RenderTexture renderTexture)
//     {
//         var q = new CameraSwitcherRenderQueValues();
//         q.camera = camera;
//         q.renderTexture = renderTexture;
//         _renderQues.Add(q);
//     }
//
//     public void Render()
//     {
//         foreach (var renderQue in _renderQues)
//         {
//             if (renderQue.camera != null && renderQue.renderTexture != null)
//             {
//                 renderQue.camera.targetTexture = renderQue.renderTexture;
//                 renderQue.camera.enabled = true;
//                 // if(renderQue.camera.enabled) renderQue.camera.Render();
//                 // renderQue.camera.targetTexture = null;
//             }
//            
//         }
//     }
// }
[ExecuteAlways]
public class CameraSwitcherBrain : MonoBehaviour
{
    [SerializeField] public CameraSwitcherSettings cameraSwitcherSettings;
    [SerializeField] public RawImage outputRawImage;
    [SerializeField] public RenderTexture outPutRenderTarget;
    private float _fader;
    [SerializeField] private Vector2Int resolution = new Vector2Int(1920, 1080);
    [SerializeField] private int prerenderingFrameCount = 3;
    [SerializeField] private RenderTextureFormat renderTextureFormat;
    [SerializeField] private DepthList depth = DepthList.AtLeast16;
    public Material material => cameraSwitcherSettings.material;
    public RenderTexture renderTextureA => cameraSwitcherSettings.renderTextureA;
    public RenderTexture renderTextureB => cameraSwitcherSettings.renderTextureB;


#if UNITY_EDITOR
    [MenuItem("GameObject/Timeline Camera Switcher/Camera Switcher Brain", false, 10)]
    static void CreateCameraSwitcherControl(MenuCommand command)
    {
        //　空のゲームオブジェクト作成
        GameObject obj = new GameObject("Camera Switcher Brain");
        //　ゲームオブジェクトの親の設定
        GameObjectUtility.SetParentAndAlign(obj, command.context as GameObject);
        //　Undo操作を加える(Ctrl+Zキーの操作に加える）
        Undo.RegisterCreatedObjectUndo(obj, "Create " + obj.name);
        //　初期位置を設定
        obj.AddComponent<CameraSwitcherBrain>();
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
        if (cameraSwitcherSettings == null) return;

        if (resolution != cameraSwitcherSettings.resolution)
        {
            cameraSwitcherSettings.resolution = resolution;
            Init();
        }

        if (depth != cameraSwitcherSettings.depth)
        {
            cameraSwitcherSettings.depth = depth;
            Init();
        }

        if (prerenderingFrameCount != cameraSwitcherSettings.preRenderingFrameCount)
        {
            cameraSwitcherSettings.preRenderingFrameCount = prerenderingFrameCount;
            Init();
        }

        if (renderTextureFormat != cameraSwitcherSettings.renderTextureFormat)
        {
            Init();
        }

        if (outPutRenderTarget != null)
        {
            if (
                outPutRenderTarget.depth != (int) cameraSwitcherSettings.depth || outPutRenderTarget.format != cameraSwitcherSettings.renderTextureFormat || outPutRenderTarget.width != cameraSwitcherSettings.resolution.x || outPutRenderTarget.height != cameraSwitcherSettings.resolution.y)
            {
                Init();
            }
        }
        if (outputRawImage != null)
        {
            outputRawImage.material = cameraSwitcherSettings.material;
            outputRawImage.texture = null;
        }


        if (outPutRenderTarget && cameraSwitcherSettings.material)
        {
            Graphics.Blit(Texture2D.whiteTexture, outPutRenderTarget, cameraSwitcherSettings.material);
        }
    }

    public void Init()
    {
        cameraSwitcherSettings.resolution = resolution;
        cameraSwitcherSettings.depth = depth;
        cameraSwitcherSettings.preRenderingFrameCount = prerenderingFrameCount;
        cameraSwitcherSettings.renderTextureFormat = renderTextureFormat;

        cameraSwitcherSettings.renderTextureA.Release();
        cameraSwitcherSettings.renderTextureA.format = cameraSwitcherSettings.renderTextureFormat;
        cameraSwitcherSettings.renderTextureA.depth = (int) cameraSwitcherSettings.depth;
        cameraSwitcherSettings.renderTextureA.width = cameraSwitcherSettings.resolution.x;
        cameraSwitcherSettings.renderTextureA.height = cameraSwitcherSettings.resolution.y;

        cameraSwitcherSettings.renderTextureB.Release();
        cameraSwitcherSettings.renderTextureB.format = cameraSwitcherSettings.renderTextureFormat;
        cameraSwitcherSettings.renderTextureB.depth = (int) cameraSwitcherSettings.depth; ;
        cameraSwitcherSettings.renderTextureB.width = cameraSwitcherSettings.resolution.x;
        cameraSwitcherSettings.renderTextureB.height = cameraSwitcherSettings.resolution.y;

        outPutRenderTarget.Release();
        outPutRenderTarget.format = cameraSwitcherSettings.renderTextureFormat;
        outPutRenderTarget.depth = (int) cameraSwitcherSettings.depth;
        outPutRenderTarget.width = cameraSwitcherSettings.resolution.x;
        outPutRenderTarget.height = cameraSwitcherSettings.resolution.y;
    }
}
