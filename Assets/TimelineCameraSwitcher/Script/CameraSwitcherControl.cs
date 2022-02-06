
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
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
    [SerializeField] public DofControlProps baseDofValues;
   
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

    public void ChangeDofMode()
    {
#if USE_URP
        dof.mode.value = baseDofValues.depthOfFieldMode;
#endif
    }
    public void SetBaseDofValues()
    {
        if(volume == null) return;
        volume.TryGet<DepthOfField>(out dof);

#if USE_URP

        baseDofValues.depthOfFieldMode = dof.mode.value;
        baseDofValues.focusDistance = dof.focusDistance.value;
        baseDofValues.focalLength = dof.focalLength.value;
        baseDofValues.aperture = dof.aperture.value;
        baseDofValues.bladeCount = dof.bladeCount.value;
        baseDofValues.bladeRotation = dof.bladeRotation.value;
        baseDofValues.start = dof.gaussianStart.value;
        baseDofValues.end = dof.gaussianEnd.value;
        baseDofValues.maxRadius = dof.gaussianMaxRadius.value;
        baseDofValues.highQualitySampling = dof.highQualitySampling.value;
#elif USE_HDRP
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
