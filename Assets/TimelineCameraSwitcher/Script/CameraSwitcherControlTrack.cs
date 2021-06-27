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
    // [Serializable] private SizeBufferPoolSizeSorter _depthState;
    [SerializeField] private RenderTexture _referenceRenderTexture;
    // [SerializeField]private int _dephBufferSize = 16;

    [SerializeField] public ExposedReference<RawImage> m_rawImage;
    private RenderTexture m_textureA;
    private RenderTexture m_textureB;
    private List<RenderTexture> m_texturePool;

    public RenderTexture textureA => m_textureA;
    public RenderTexture textureB => m_textureB;

    // public RawImage rawImage;

    public List<RenderTexture> texturePool => m_texturePool;
    
    public override Playable CreateTrackMixer(PlayableGraph graph, GameObject go, int inputCount)
    {
        var playableDirector = go.GetComponent<PlayableDirector>();
        var playable = ScriptPlayable<CameraSwitcherControlMixerBehaviour>.Create (graph, inputCount);
        var playableBehaviour = playable.GetBehaviour();
        playableBehaviour.rawImage = m_rawImage.Resolve(graph.GetResolver());
        playableBehaviour.compositeMaterial = m_rawImage.Resolve(graph.GetResolver()).material;

       InitTexturePools();

       m_textureA = new RenderTexture(_referenceRenderTexture);
       m_textureB = new RenderTexture(_referenceRenderTexture);

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

        if(m_texturePool == null) m_texturePool = new List<RenderTexture>();
        if (textureA != null && textureB != null)
        {
           
            m_texturePool.Clear();

            m_texturePool.Add(m_textureA);
            m_texturePool.Add(m_textureB);
        }
        else
        {
            Debug.LogWarning("referenceRenderTextureSetting is null.");
        }
    }

    public void KillRenderTexturePool()
    {
        if(m_textureA != null) DestroyImmediate(m_textureA);
        if(m_textureB != null) DestroyImmediate(m_textureB);
    }

    private void OnDestroy()
    {
        KillRenderTexturePool();
    }
}
