using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class CameraSwitcherManager : MonoBehaviour
{
    public Material compositorMat;

    public ReflectionProbeController probeController;

    // public CanvasImageWiggler canvasImageWiggler;
    // public RenderTexture IN01;
    // public RenderTexture IN02;
    // [Range(0, 1)] public float fader; 
    private float m_fader;

    public float fader
    {
        get => m_fader;
        set
        {
            m_fader = value;
            UpdateFader();
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // public void UpdateWiggler(float time)
    // {
    //     if (canvasImageWiggler != null)
    //     {
    //         canvasImageWiggler.UpdateWiggle(time);
    //         canvasImageWiggler.power = 1;
    //     }
    // }
    //
    // public void DisableWiggler(float time)
    // {
    //     if (canvasImageWiggler != null)
    //     {
    //         canvasImageWiggler.power = 0f;
    //         canvasImageWiggler.UpdateWiggle(time);
    //     }
    // }
    public void SetRenderTexture01(RenderTexture tex)
    {
        // IN01 = tex;
        compositorMat.SetTexture("_MainTexA",tex);
    }
    
    
    public void SetRenderTexture02(RenderTexture tex)
    {
        // IN02 = tex;
        compositorMat.SetTexture("_MainTexB",tex);
    }
    public void UpdateFader()

    {
        compositorMat.SetFloat("_Fader",fader);
    }
    // Update is called once per frame
    void Update()
    {
        // UpdateFader();
    }
}
