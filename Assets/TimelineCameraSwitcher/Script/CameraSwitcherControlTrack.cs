using System;
using System.Linq;
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
    [HideInInspector]public bool findMissingCameraInHierarchy = false;
    [HideInInspector]public bool fixMissingPrefabByCameraName = false;
    [SerializeField] private Vector2 m_resolution = new Vector2(1920,1080);
    [SerializeField] public int m_prerenderingFrameCount = 3;
    [SerializeField] private RenderTextureFormat m_renderTextureFormat;
    // [SerializeField] private CameraSwitcherSettings m_cameraSwitcherSettings;
    [SerializeField]public DepthList m_depth;
   
    private CameraSwitcherControlMixerBehaviour _cameraSwitcherControlMixerBehaviour;
    public int width => (int)m_resolution.x;
    public int height => (int)m_resolution.y;
    public RenderTextureFormat renderTextureFormat => m_renderTextureFormat;

    // public CameraSwitcherSettings cameraSwitcherSettings => m_cameraSwitcherSettings;
    public DepthList depthList => m_depth;

    public override Playable CreateTrackMixer(PlayableGraph graph, GameObject go, int inputCount)
    {
        
        var playableDirector = go.GetComponent<PlayableDirector>();
     
        var playable = ScriptPlayable<CameraSwitcherControlMixerBehaviour>.Create (graph, inputCount);
        var playableBehaviour = playable.GetBehaviour();
        _cameraSwitcherControlMixerBehaviour = playableBehaviour;
        
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
