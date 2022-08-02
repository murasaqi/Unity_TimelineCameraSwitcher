using System;
using System;
using System.Linq;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;
using UnityEngine.UI;

[Serializable]
[TrackColor(0.8700427f, 0.4803044f, 0.790566f)]
[TrackClipType(typeof(CameraSwitcherControlClip))]
[TrackBindingType(typeof(CameraSwitcherControl))]
public class CameraSwitcherControlTrack : TrackAsset
{
    
  
    public ExposedReference<TextMeshProUGUI> cameraNamePreviewGUI; 
    public bool disableWiggler = false;
    private CameraSwitcherControlMixerBehaviour _cameraSwitcherControlMixerBehaviour;
  
    public override Playable CreateTrackMixer(PlayableGraph graph, GameObject go, int inputCount)
    {
        
        var playableDirector = go.GetComponent<PlayableDirector>();
     
        var playable = ScriptPlayable<CameraSwitcherControlMixerBehaviour>.Create (graph, inputCount);
        var playableBehaviour = playable.GetBehaviour();
        _cameraSwitcherControlMixerBehaviour = playableBehaviour;
        
        // InitTexturePools();

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

    public void InitTexturePools()
    {
       KillRenderTexturePool();
    }

    public void KillRenderTexturePool()
    {
        // cameraSwitcherSettings.material.SetTexture("_TextureA",cameraSwitcherSettings.renderTextureA);
        // cameraSwitcherSettings.material.SetTexture("_TextureB",cameraSwitcherSettings.renderTextureB);
        // if(_cameraSwitcherControlMixerBehaviour !=null) _cameraSwitcherControlMixerBehaviour.ResetCameraTarget();
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
