using System;
using System.ComponentModel;
using UnityEngine;
using UnityEngine.Playables;

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

#if USE_URP
    

    public DepthOfFieldMode mode
    {
        set
        {
            template.mode = value;
        }
    }
#elif USE_HDRP
#endif 
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
        return playable;
        
    }


    private void OnDestroy()
    {
        
    }
}
