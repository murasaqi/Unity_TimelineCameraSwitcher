using System;
using System.ComponentModel;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Rendering.HighDefinition;

#if USE_URP
using UnityEngine.Rendering.Universal;
#elif USE_HDRP
#endif

using UnityEngine.Timeline;

[Serializable]

public class CameraSwitcherControlClip : PlayableAsset, ITimelineClipAsset
{
    
    [SerializeField] ExposedReference<Camera> camera;
    [SerializeField] CameraSwitcherControlBehaviour template = new CameraSwitcherControlBehaviour ();
    // [SerializeField] public bool lookAt = false;
    [HideInInspector] public Transform target;
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
        target = clone.lookAtProps.target.Resolve(graph.GetResolver());
#if USE_HDRP
        // clone.camera.focusDi
        template.hdAdditionalCameraData = clone.camera.GetComponent<HDAdditionalCameraData>();
#endif
        return playable;
        
    }


    private void OnDestroy()
    {
        
    }
}
