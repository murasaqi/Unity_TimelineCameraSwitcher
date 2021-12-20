
using System;
using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.Experimental.Rendering;
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

    // [SerializeField] public CameraSwitcherOutputTarget outputTarget = CameraSwitcherOutputTarget.RenderTexture;
    [SerializeField] public RawImage outputRawImage;
    [SerializeField] public RenderTexture outPutRenderTarget;

    // [SerializeField] private Texture tex;
    private float m_fader;
    public RenderTexture renderTextureA => cameraSwitcherSettings.renderTextureA;
    public RenderTexture renderTextureB => cameraSwitcherSettings.renderTextureB;
    
    public int widht => (int)cameraSwitcherSettings.resolution.x;
    public int height =>(int) cameraSwitcherSettings.resolution.y;

    
    
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
        if (outputRawImage != null)
        {
            outputRawImage.material = material;
            outputRawImage.texture = null;
        }
        
        if (outPutRenderTarget)
        {
            Graphics.Blit(Texture2D.whiteTexture,outPutRenderTarget,cameraSwitcherSettings.material,cameraSwitcherSettings.material.FindPass("Universal Forward"));
        }

    }

    private void LateUpdate()
    {
       


    }

    public void Blit()
    {
      
       
       
            // if (outputRawImage != null)
            // {
            //     outputRawImage.material = material;
            //     outputRawImage.texture = null;
            // }
            
            
            
        
    }
}
