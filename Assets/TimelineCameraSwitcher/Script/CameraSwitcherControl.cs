
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
[ExecuteAlways]
public class CameraSwitcherControl : MonoBehaviour
{
    
    
    [SerializeField] public CameraSwitcherSettings cameraSwitcherSettings;

    [SerializeField] public CameraSwitcherOutputTarget outputTarget = CameraSwitcherOutputTarget.RenderTexture;
    [SerializeField] public RenderTexture outPutRenderTarget;
    [SerializeField] public RawImage outputRawImage;

    // [SerializeField] private Texture tex;
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

    private void Update()
    {
        Blit();
    }

    public void Blit()
    {
      
       
       
            if (outputRawImage != null)
            {
                outputRawImage.material = material;
                outputRawImage.texture = null;
            }
            
            
            
            if (outPutRenderTarget)
            {
                Graphics.Blit(Texture2D.whiteTexture,outPutRenderTarget,cameraSwitcherSettings.material);
            }


    }
}
