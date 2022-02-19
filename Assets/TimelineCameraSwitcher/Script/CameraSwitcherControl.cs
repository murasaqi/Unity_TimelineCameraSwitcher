
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
using System;
#if USE_URP
using UnityEngine.Rendering.Universal;
#elif USE_HDRP
using UnityEngine.Rendering.HighDefinition;
#endif
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
    [SerializeField] public RawImage outputRawImage;
    [SerializeField] public RenderTexture outPutRenderTarget;
    [SerializeField] public Vector2Int resolution = new Vector2Int(1920,1080);
    [SerializeField, Range(0,10)] public int prerenderingFrameCount = 3;
    [SerializeField] public RenderTextureFormat renderTextureFormat = RenderTextureFormat.DefaultHDR;
    [SerializeField] public DepthList depth;
    [HideInInspector] public Material material;
    [SerializeField] public bool dofControl = false;
    [SerializeField] public DepthOfFieldMode depthOfFieldMode;
#if USE_URP
    [SerializeField] public BokehProp bokehBaseValues= new BokehProp();
    [SerializeField] public GaussianProp gaussianBaseValues= new GaussianProp();
#elif USE_HDRP
    [SerializeField] public PhysicalCameraProps physicalCameraBaseValues= new PhysicalCameraProps();
    [SerializeField] public ManualRangeProps manualRangeBaseValues= new ManualRangeProps();
#endif
  
    
   
    public DepthList depthList => depth;
    
    
    // [SerializeField] private Texture tex;
    private float m_fader;
    public RenderTexture renderTextureA;
    public RenderTexture renderTextureB;
    
    public int width => (int)resolution.x;
    public int height =>(int) resolution.y;



    public Camera cameraA;
    public Camera cameraB;
    
    // public Material material => material;
    
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
    
    
 
    private void Update()
    {

        if (outputRawImage != null)
        {
            outputRawImage.material = material;
            outputRawImage.texture = null;
        } 
        
        Blit();
        
       
    }

    private DepthOfField dof;

    public void ApplyProfileSettings()
    {
        if (cameraSwitcherSettings == null) return;

        renderTextureA = cameraSwitcherSettings.renderTextureA;
        renderTextureB = cameraSwitcherSettings.renderTextureB;
        resolution = cameraSwitcherSettings.resolution;
        volume = cameraSwitcherSettings.volume;
    }

    public void SaveProfile()
    {
        if(cameraSwitcherSettings == null) return;

        cameraSwitcherSettings.resolution = resolution;
        cameraSwitcherSettings.volume = volume;
        
    }
    public void ChangeDofMode()
    {


        if (dof == null)
        {
            if (volume != null) volume.TryGet<DepthOfField>(out dof);
        }
        
        if(dof == null) return;
    #if USE_HDRP
        dof.focusMode.value = depthOfFieldMode;
#endif
        // dof.quality.value = 0;
    }
    public void SetBaseDofValues()
    {
        if(volume == null) return;
        cameraSwitcherSettings.volume = volume;
        volume.TryGet<DepthOfField>(out dof);
#if USE_URP
        
        // bokehBaseValues.depthOfFieldMode = dof.mode.value;
        bokehBaseValues.focusDistance = dof.focusDistance.value;
        bokehBaseValues.focalLength = dof.focalLength.value;
        bokehBaseValues.aperture = dof.aperture.value;
        bokehBaseValues.bladeCount = dof.bladeCount.value;
        bokehBaseValues.bladeRotation = dof.bladeRotation.value;
        gaussianBaseValues.start = dof.gaussianStart.value;
        gaussianBaseValues.end = dof.gaussianEnd.value;
        gaussianBaseValues.maxRadius = dof.gaussianMaxRadius.value;
        gaussianBaseValues.highQualitySampling = dof.highQualitySampling.value;
#elif USE_HDRP
        depthOfFieldMode = dof.focusMode.value;
        physicalCameraBaseValues.focusDistanceMode = dof.focusDistanceMode.value;
        physicalCameraBaseValues.focusDistance = dof.focusDistance.value;
        physicalCameraBaseValues.nearBluer.maxRadius = dof.nearMaxBlur;
        physicalCameraBaseValues.nearBluer.sampleCount = dof.nearSampleCount;
        physicalCameraBaseValues.farBluer.maxRadius = dof.farMaxBlur;
        physicalCameraBaseValues.farBluer.sampleCount = dof.farSampleCount;

        manualRangeBaseValues.nearRange.start = dof.nearFocusStart.value;
        manualRangeBaseValues.nearRange.end = dof.nearFocusEnd.value;
        manualRangeBaseValues.farRange.start = dof.farFocusStart.value;
        manualRangeBaseValues.farRange.end = dof.farFocusEnd.value;
        manualRangeBaseValues.quality = (QualitySetting)Enum.ToObject(typeof(QualitySetting), dof.quality.value);
        manualRangeBaseValues.nearBluer.maxRadius = dof.nearMaxBlur;
        manualRangeBaseValues.nearBluer.sampleCount = dof.nearSampleCount;
        manualRangeBaseValues.farBluer.maxRadius = dof.farMaxBlur;
        manualRangeBaseValues.farBluer.sampleCount = dof.farSampleCount;
        
#endif
    }
    
     public void ApplyBaseDofValuesToVolumeProfile()
    {
        Debug.Log("Update dof value");
        if(volume == null) return;
        volume.TryGet<DepthOfField>(out dof);
#if USE_URP
        
        // // bokehBaseValues.depthOfFieldMode = dof.mode.value;
        // bokehBaseValues.focusDistance = dof.focusDistance.value;
        // bokehBaseValues.focalLength = dof.focalLength.value;
        // bokehBaseValues.aperture = dof.aperture.value;
        // bokehBaseValues.bladeCount = dof.bladeCount.value;
        // bokehBaseValues.bladeRotation = dof.bladeRotation.value;
        // gaussianBaseValues.start = dof.gaussianStart.value;
        // gaussianBaseValues.end = dof.gaussianEnd.value;
        // gaussianBaseValues.maxRadius = dof.gaussianMaxRadius.value;
        // gaussianBaseValues.highQualitySampling = dof.highQualitySampling.value;
#elif USE_HDRP
        
        
        dof.quality.value = (int) physicalCameraBaseValues.quality;
        
        
        if (dof.focusMode == DepthOfFieldMode.UsePhysicalCamera)
        {
            dof.focusMode.value = depthOfFieldMode;
            dof.focusDistanceMode.value = physicalCameraBaseValues.focusDistanceMode;
            dof.focusDistance.value = physicalCameraBaseValues.focusDistance;
            dof.nearMaxBlur = physicalCameraBaseValues.nearBluer.maxRadius;
            dof.nearSampleCount = physicalCameraBaseValues.nearBluer.sampleCount;
            dof.farMaxBlur = physicalCameraBaseValues.farBluer.maxRadius;
            dof.farSampleCount = physicalCameraBaseValues.farBluer.sampleCount;    
        }

        if (dof.focusMode == DepthOfFieldMode.Manual)
        {
            dof.nearFocusStart.value = manualRangeBaseValues.nearRange.start;
            dof.nearFocusEnd.value = manualRangeBaseValues.nearRange.end;
            dof.farFocusStart.value = manualRangeBaseValues.farRange.start;
            dof.farFocusEnd.value = manualRangeBaseValues.farRange.end;
            dof.nearMaxBlur = manualRangeBaseValues.nearBluer.maxRadius;
            dof.nearSampleCount = manualRangeBaseValues.nearBluer.sampleCount;
            dof.farMaxBlur = manualRangeBaseValues.farBluer.maxRadius;
            dof.farSampleCount = manualRangeBaseValues.farBluer.sampleCount;
        }

#endif
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
