using System;
using System.ComponentModel;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

[Serializable]

public class TimelineCameraSwticherClip : PlayableAsset, ITimelineClipAsset
{
    [SerializeField] TimelineCameraSwitcherBehaviour template = new TimelineCameraSwitcherBehaviour ();
    [SerializeField] ExposedReference<Camera> camera;
    private TimelineCameraSwitcherBehaviour clone;
   
    public ClipCaps clipCaps
    {
        get { return ClipCaps.Blending; }
    }

    public override Playable CreatePlayable (PlayableGraph graph, GameObject owner)
    {
        var playable = ScriptPlayable<TimelineCameraSwitcherBehaviour>.Create (graph, template);
        clone = playable.GetBehaviour ();
        clone.camera= camera.Resolve (graph.GetResolver ());
        return playable;
        
    }


    private void OnDestroy()
    {
        
    }
}
