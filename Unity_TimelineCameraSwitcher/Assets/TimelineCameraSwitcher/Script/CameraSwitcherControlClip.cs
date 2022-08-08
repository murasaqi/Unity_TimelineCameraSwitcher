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
    
    [SerializeField] ExposedReference<Camera> camera;
    [SerializeField] CameraSwitcherControlBehaviour template = new CameraSwitcherControlBehaviour ();
    // [SerializeField] public bool lookAt = false;
    [HideInInspector] public Transform target;

    [HideInInspector] public Volume volume;

    [HideInInspector] public Camera targetCamera;

    [HideInInspector] public LookAtConstraint lookAtConstraint;
    // public CameraSwitcherControlBehaviour clone;
    public DepthOfFieldMode mode
    {
        set
        {
            template.mode = value;
        }
    }

    public ClipCaps clipCaps
    {
        get { return ClipCaps.Blending; }
    }

    public override Playable CreatePlayable (PlayableGraph graph, GameObject owner)
    {
        var playable = ScriptPlayable<CameraSwitcherControlBehaviour>.Create (graph, template); 
        var clone = playable.GetBehaviour ();
        clone.camera= camera.Resolve (graph.GetResolver ());
        targetCamera = clone.camera;
        target = clone.lookAtProps.target.Resolve(graph.GetResolver());
        if (clone.camera != null)
        {
            volume = clone.camera.GetComponent<Volume>();
            if (volume == null) volume = clone.camera.gameObject.AddComponent<Volume>();

            lookAtConstraint = clone.camera.GetComponent<LookAtConstraint>();
            if(lookAtConstraint == null) lookAtConstraint = clone.camera.gameObject.AddComponent<LookAtConstraint>();
            lookAtConstraint.enabled = false;
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
        
    }
}
