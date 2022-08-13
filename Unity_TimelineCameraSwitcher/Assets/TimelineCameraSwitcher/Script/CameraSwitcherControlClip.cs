using System;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Playables;
using UnityEngine.Rendering;


#if USE_URP
using UnityEngine.Rendering.Universal;
#elif USE_HDRP
using UnityEngine.Rendering.HighDefinition;
#endif

using UnityEngine.Timeline;

[Serializable]

public class CameraSwitcherControlClip : PlayableAsset, ITimelineClipAsset
{
    
    [SerializeField] public ExposedReference<Camera> camera;
    [HideInInspector] public CameraSwitcherControlBehaviour template = new CameraSwitcherControlBehaviour ();

    [HideInInspector] public Volume defaultVolume;
    // [HideInInspector] public Volume targetCameraVolumeB;
    
    [SerializeField] public bool wiggle = false;
    [SerializeField] public WigglerProps wigglerProps = new WigglerProps();
    
   
    [SerializeField] public bool lookAt = false;
    [SerializeField] public ExposedReference<Transform> lookAtTarget;
    [SerializeField] public LookAtProps lookAtProps = new LookAtProps();
 
    
    [SerializeField] public bool colorBlend;
    [SerializeField] public ColorBlendProps colorBlendProps;
    
    [SerializeField] public bool volumeOverride = false;
    [HideInInspector]public VolumeProfile volumeProfile;

    private CameraSwitcherControlBehaviour clone;
    private PlayableGraph playableGraph;
    
    [HideInInspector]public RenderTexture thumbnailRenderTexture;
    [HideInInspector] public DrawTimeMode drawTimeMode = DrawTimeMode.Frame;

    [HideInInspector]public bool isUpdateThumbnail = false;
    public ClipCaps clipCaps
    {
        get { return ClipCaps.Blending; }
    }

    public void Init()
    {
        clone.camera= camera.Resolve (playableGraph.GetResolver ());
    }

    public void InitThumbnail(RenderTexture sorce = null)
    {
        if (sorce == null)
        {
            thumbnailRenderTexture = new RenderTexture (256, 256, 0, RenderTextureFormat.ARGB32);
        }
        else
        {
            thumbnailRenderTexture = new RenderTexture(256, 256, sorce.depth, sorce.format);
        }
    
    }
    
    
    
    

    public override Playable CreatePlayable (PlayableGraph graph, GameObject owner)
    {
        playableGraph = graph;
        var playable = ScriptPlayable<CameraSwitcherControlBehaviour>.Create (graph, template); 
        clone = playable.GetBehaviour ();
        clone.camera= camera.Resolve (graph.GetResolver ());
        clone.lookAtTarget = lookAtTarget.Resolve(graph.GetResolver());
        if (clone.camera != null)
        {
            
            defaultVolume = clone.camera.GetComponent<Volume>();
            if (defaultVolume == null)
            {
                defaultVolume = clone.camera.gameObject.AddComponent<Volume>();
            }

            clone.lookAtConstraint = clone.camera.GetComponent<LookAtConstraint>();
            if(clone.lookAtConstraint == null) clone.lookAtConstraint = clone.camera.gameObject.AddComponent<LookAtConstraint>();
            clone.lookAtConstraint.enabled = false;
        }
        

        
        
#if USE_HDRP
        // clone.physicalCameraProps.focusLength = clone.camera.focalLength;
        // clone.camera.focusDi
        if(clone.camera != null)template.hdAdditionalCameraData = clone.camera.GetComponent<HDAdditionalCameraData>();
#elif USE_URP
        if(clone.camera != null)template.universalAdditionalCameraData = clone.camera.GetComponent<UniversalAdditionalCameraData>();
#endif
        return playable;
        
    }
    


    private void OnDestroy()
    {
        DestroyImmediate(thumbnailRenderTexture);
    }
}
