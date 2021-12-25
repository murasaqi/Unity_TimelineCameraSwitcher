using System;
using System.ComponentModel;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

[Serializable]

public class CameraSwitcherControlClip : PlayableAsset, ITimelineClipAsset
{
    
    [SerializeField] ExposedReference<Camera> camera;
    [SerializeField] CameraSwitcherControlBehaviour template = new CameraSwitcherControlBehaviour ();
    [SerializeField] public bool lookAt = false;
    [SerializeField] ExposedReference<Transform> target;
    private CameraSwitcherControlBehaviour clone;
   
    public ClipCaps clipCaps
    {
        get { return ClipCaps.Blending; }
    }

    public override Playable CreatePlayable (PlayableGraph graph, GameObject owner)
    {
        var playable = ScriptPlayable<CameraSwitcherControlBehaviour>.Create (graph, template);
        clone = playable.GetBehaviour ();
        clone.camera= camera.Resolve (graph.GetResolver ());
        clone.target = target.Resolve(graph.GetResolver());
        return playable;
        
    }


    private void OnDestroy()
    {
        
    }
}
