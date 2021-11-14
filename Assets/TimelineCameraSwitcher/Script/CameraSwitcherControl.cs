
using System;
using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.UI;

#if UNITY_EDITOR


#endif

public enum CameraSwitcherOutputTarget
{
    RawImage,
    RenderTexture
}
[ExecuteInEditMode]
public class CameraSwitcherControl : MonoBehaviour
{
    
    
    [SerializeField] public CameraSwitcherSettings cameraSwitcherSettings;

    [SerializeField] public CameraSwitcherOutputTarget outputTarget = CameraSwitcherOutputTarget.RenderTexture;
    [SerializeField] public RenderTexture outPutRenderTarget;
    [SerializeField] public RawImage outputRawImage;
    private float m_fader;
    public RenderTexture renderTextureA => cameraSwitcherSettings.renderTextureA;
    public RenderTexture renderTextureB => cameraSwitcherSettings.renderTextureB;
    
    public int widht => (int)cameraSwitcherSettings.resolution.x;
    public int height =>(int) cameraSwitcherSettings.resolution.y;

    public Material material => cameraSwitcherSettings.material;

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
    
    public void Blit()
    {
        if (outPutRenderTarget)
        {
            Debug.Log("update");
            Graphics.Blit(null,outPutRenderTarget,cameraSwitcherSettings.material);
            if (outputRawImage != null)
            {
                outputRawImage.texture = outPutRenderTarget;
                outputRawImage.material = null;
            }
        }
        else
        {
            Debug.Log("nul");
            if (outputRawImage != null)
            {
                outputRawImage.material = material;
                outputRawImage.texture = null;
            }
        }

        
        
    }
}
