using System;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;
using UnityEngine.UI;

[Serializable]
[TrackColor(0.8700427f, 0.4803044f, 0.790566f)]
[TrackClipType(typeof(TimelineCameraSwticherClip))]
[TrackBindingType(typeof(CameraSwitcherBrain))]
public class TimelineCameraSwticherTrack : TrackAsset
{
    [HideInInspector]public bool findMissingCameraInHierarchy = false;
    [HideInInspector]public bool fixMissingPrefabByCameraName = false;
    private TimelineCameraSwticherBehaviour _timelineCameraSwticherBehaviour;
  
    public override Playable CreateTrackMixer(PlayableGraph graph, GameObject go, int inputCount)
    {
        
        var playableDirector = go.GetComponent<PlayableDirector>();
     
        var playable = ScriptPlayable<TimelineCameraSwticherBehaviour>.Create (graph, inputCount);
        var playableBehaviour = playable.GetBehaviour();
        _timelineCameraSwticherBehaviour = playableBehaviour;
        
        // InitTexturePools();

        if (playableDirector != null)
        {
            playableBehaviour.director = playableDirector;
            playableBehaviour.fps = timelineAsset.editorSettings.fps;
            playableBehaviour.clips = GetClips().ToList();
            playableBehaviour.track = this;
        }

        return playable;    
    }
    

    private void OnDestroy()
    {
        
        // KillRenderTexturePool();
        
    }
}

