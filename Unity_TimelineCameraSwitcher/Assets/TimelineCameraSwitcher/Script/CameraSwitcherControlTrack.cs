using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;
using UnityEngine.UI;

[Serializable]
[TrackColor(0.6f, 0.4f, 1)]
[TrackClipType(typeof(CameraSwitcherControlClip))]
[TrackBindingType(typeof(CameraSwitcherControl))]
public class CameraSwitcherControlTrack : TrackAsset
{
    
  
    public ExposedReference<TextMeshProUGUI> cameraNamePreviewGUI; 
    public bool disableWiggler = false;
    public bool drawThumbnail = true;
    public Color thumbnailColor = Color.white;
    public DrawTimeMode drawTimeMode = DrawTimeMode.Frame;
    public Material thumbnailMaterial;
    private CameraSwitcherControlMixerBehaviour cameraSwitcherControlMixerBehaviour;
    public override Playable CreateTrackMixer(PlayableGraph graph, GameObject go, int inputCount)
    {
        
        var playableDirector = go.GetComponent<PlayableDirector>();
     
        var playable = ScriptPlayable<CameraSwitcherControlMixerBehaviour>.Create (graph, inputCount);
        var playableBehaviour = playable.GetBehaviour();
        cameraSwitcherControlMixerBehaviour = playableBehaviour;


        if (playableDirector != null)
        {
            playableBehaviour.cameraNamePreviewGUI = cameraNamePreviewGUI.Resolve(graph.GetResolver());
            playableBehaviour.director = playableDirector;
            playableBehaviour.fps = timelineAsset.editorSettings.fps;
            playableBehaviour.clips = GetClips().ToList();
            playableBehaviour.disableWiggler = disableWiggler;
            playableBehaviour.track = this;
        }

        return playable;    
    }


    private void OnEnable()
    {
    }

    private void OnDestroy()
    {
        
        // KillRenderTexturePool();
        
    }
}

public enum DepthList
{
    None =0,
    AtLeast16 = 16,
    AtLeast24_WidthStencil = 24,


}

public enum DrawTimeMode
{
    None,
    Frame,
    Duration,
    TimeCode
}
