using System;
using UnityEngine;
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
        if (clone.camera != null)
        {
            volume = clone.camera.GetComponent<Volume>();
            if (volume == null) volume = clone.camera.gameObject.AddComponent<Volume>();
        }
        

        // if (volume.profile != null && template.volumeProfile == null)
        // {
        //     template.volumeProfile = volume.profile;
        // }
        //
        // if (volume.profile == null && template.volumeProfile != null)
        // {
        //     volume.profile = template.volumeProfile;
        // }

        
        
#if USE_HDRP
        // clone.physicalCameraProps.focusLength = clone.camera.focalLength;
        // clone.camera.focusDi
        if(clone.camera != null)template.hdAdditionalCameraData = clone.camera.GetComponent<HDAdditionalCameraData>();
#endif
        return playable;
        
    }


    private void OnDestroy()
    {
        
    }
}
