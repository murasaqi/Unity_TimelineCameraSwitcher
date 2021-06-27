using System;
using System.Collections.Generic;
using System.Linq;
using Codice.Utils.Buffers;
using PlasticGui;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Rendering;
using UnityEngine.Timeline;
using UnityEngine.UI;

[Serializable]
[TrackColor(0.8700427f, 0.4803044f, 0.790566f)]
[TrackClipType(typeof(CameraSwitcherControlClip))]


public class CameraSwitcherControlTrack : TrackAsset
{
    [HideInInspector]public bool findMissingCameraInHierarchy = false;
    [HideInInspector]public bool fixMissingPrefabByCameraName = false;
    // [SerializeField] private RenderTexture _referenceRenderTexture; 
    private Material _material;

    [SerializeField] public ExposedReference<RawImage> m_rawImage;
    [SerializeField] private RenderTexture m_textureA;
    [SerializeField] private RenderTexture m_textureB;

    private CameraSwitcherControlMixerBehaviour _cameraSwitcherControlMixerBehaviour;
    // private List<RenderTexture> m_texturePool;

    public RenderTexture textureA => m_textureA;
    public RenderTexture textureB => m_textureB;

    
    public override Playable CreateTrackMixer(PlayableGraph graph, GameObject go, int inputCount)
    {
        var playableDirector = go.GetComponent<PlayableDirector>();
        var playable = ScriptPlayable<CameraSwitcherControlMixerBehaviour>.Create (graph, inputCount);
        var playableBehaviour = playable.GetBehaviour();
        playableBehaviour.rawImage = m_rawImage.Resolve(graph.GetResolver());
        _material = m_rawImage.Resolve(graph.GetResolver()).material;
        playableBehaviour.compositeMaterial = _material;
        _cameraSwitcherControlMixerBehaviour = playableBehaviour;
       InitTexturePools();

        if (playableDirector != null)
        {
            playableBehaviour.director = playableDirector;
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
        if(_cameraSwitcherControlMixerBehaviour !=null) _cameraSwitcherControlMixerBehaviour.ResetCameraTarget();
    }

    private void OnDestroy()
    {
        if (_material != null) _material.SetFloat("_PlayableDirectorTime", 0f);
        // KillRenderTexturePool();
        
    }
}
