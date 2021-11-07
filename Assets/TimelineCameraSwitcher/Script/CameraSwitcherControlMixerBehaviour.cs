// using UnityEngin
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;
using UnityEngine.UI;

public class CameraSwitcherControlMixerBehaviour : PlayableBehaviour
{
    
    private List<TimelineClip> m_Clips;
    private List<Camera> _cameras = new List<Camera>();
    private PlayableDirector m_Director;
    private Material m_CompositeMaterial;
    private CameraSwitcherControlTrack m_track;
    private bool m_FirstFrameHappened;
    private CameraSwitcherSettings m_CameraSwitcherSettings = null;

    
    
    internal PlayableDirector director
    {
        get { return m_Director; }
        set { m_Director = value; }
    }
    
    internal CameraSwitcherSettings cameraSwitcherSettings
    {
        get { return m_CameraSwitcherSettings; }
        set { m_CameraSwitcherSettings = value; }
    }

    
    internal CameraSwitcherControlTrack track
    {
        get { return m_track; }
        set { m_track = value; }
    }

    internal List<TimelineClip> clips
    {
        get { return m_Clips; }
        set { m_Clips = value; }
    }
    
    internal Material compositeMaterial
    {
        get { return m_CompositeMaterial; }
        set { m_CompositeMaterial = value; }
    }

    private RawImage m_TrackBinding;
    
    public void ResetCameraTarget()
    {
        foreach (var camera in _cameras)
        {
            Debug.Log(camera.name);
            camera.targetTexture = null;
        }
    }
    
    private void InitRenderTexture(bool isA)
    {
        var depth = 0;
        if (m_track.depthList == DepthList.AtLeast16) depth = 16;
        if (m_track.depthList == DepthList.AtLeast24_WidthStencil) depth = 24;
        
        if (isA)
        {
            m_CameraSwitcherSettings.renderTextureA.Release();
            m_CameraSwitcherSettings.renderTextureA.format= m_track.renderTextureFormat;
            m_CameraSwitcherSettings.renderTextureA.depth = depth;
            m_CameraSwitcherSettings.renderTextureA.width = m_track.width;
            m_CameraSwitcherSettings.renderTextureA.height = m_track.height;
        }else
        {
            m_CameraSwitcherSettings.renderTextureB.Release();
            m_CameraSwitcherSettings.renderTextureB.format= m_track.renderTextureFormat;
            m_CameraSwitcherSettings.renderTextureB.depth = depth;
            m_CameraSwitcherSettings.renderTextureB.width = m_track.width;
            m_CameraSwitcherSettings.renderTextureB.height = m_track.height;
        }
    }
    public override void ProcessFrame(Playable playable, FrameData info, object playerData)
    {
        m_TrackBinding = playerData as RawImage;
        if (m_TrackBinding == null)
            return;
        if (!m_FirstFrameHappened)
        {
            InitRenderTexture(true);
            InitRenderTexture(false);
            m_FirstFrameHappened = true;
        }
        if (m_CompositeMaterial != null)
        {
            m_TrackBinding.material = m_CompositeMaterial;
            m_TrackBinding.SetMaterialDirty();
        }
        if(m_CameraSwitcherSettings.renderTextureA.format != m_track.renderTextureFormat) InitRenderTexture(true);
        if(m_CameraSwitcherSettings.renderTextureB.format != m_track.renderTextureFormat) InitRenderTexture(false);
        if(m_CameraSwitcherSettings.renderTextureA.width != m_track.width || m_CameraSwitcherSettings.renderTextureA.height != m_track.height) InitRenderTexture(true);
        if(m_CameraSwitcherSettings.renderTextureB.width != m_track.width || m_CameraSwitcherSettings.renderTextureB.height != m_track.height) InitRenderTexture(false);
        
        m_CompositeMaterial.SetFloat("_PlayableDirectorTime",(float)director.time);
        _cameras.Clear();

        int i = 0;
        foreach (TimelineClip clip in m_Clips)
        {
            var scriptPlayable =  (ScriptPlayable<CameraSwitcherControlBehaviour>)playable.GetInput(i);
            var playableBehaviour = scriptPlayable.GetBehaviour();
            if (playableBehaviour.camera != null)
            {
                _cameras.Add(playableBehaviour.camera);
                playableBehaviour.camera.enabled = false;
            }
            i++;
        }

        i = 0;

#if UNITY_EDITOR
        // var missingCameraCount = 0;
        if (m_track.findMissingCameraInHierarchy)
        {
           
            
            var timelineAsset = director.playableAsset as TimelineAsset;
            var fps = timelineAsset != null ? timelineAsset.editorSettings.fps : 60;
            foreach (var clip in m_Clips)
            {
                var scriptPlayable =  (ScriptPlayable<CameraSwitcherControlBehaviour>)playable.GetInput(i);
                var playableBehaviour = scriptPlayable.GetBehaviour();

                var missing = false;
                if (playableBehaviour.camera == null)
                {
                    Debug.Log($"<color=#DA70D6>[frame: {Mathf.CeilToInt((float)clip.start*fps )} clip: {clip.displayName}] Camera reference is null.</color>");
                    missing = true;
                }
                else
                {
                    if (clip.displayName != playableBehaviour.camera.name)
                    {
                        Debug.Log($"<color=#9370DB>[frame: {Mathf.CeilToInt((float)clip.start*fps )} clip: {clip.displayName}] Name does not match the camera</color>");
                        missing = true;
                    }
                }

                if (missing && m_track.fixMissingPrefabByCameraName)
                {
                    
                    var cam = GameObject.Find(clip.displayName);
                    if (cam!=null)
                    {
                        var serializedObject = new SerializedObject(clip.asset, director);
                        var serializedProperty = serializedObject.FindProperty("camera");
                        // if(cam.GetComponent<Camera>())Debug.Log(cam);
                        Debug.Log(cam.name);
                        // playableBehaviour.camera = cam.GetComponent<Camera>();
                        if(cam.GetComponent<Camera>())serializedProperty.exposedReferenceValue = cam.GetComponent<Camera>();
                        serializedObject.ApplyModifiedProperties();
                        Debug.Log($"<color=#00BFFF>Fix {clip.displayName}</color>");
                    }
                }

                i++;
            }
           
        }

#endif
        
       
        int inputPort = 0;
        
        foreach (TimelineClip clip in m_Clips)
        {
            
            var inputWeight = playable.GetInputWeight(inputPort);
           
            var scriptPlayable =  (ScriptPlayable<CameraSwitcherControlBehaviour>)playable.GetInput(inputPort);
            var playableBehaviour = scriptPlayable.GetBehaviour();

          

            if (clip.start <= m_Director.time && m_Director.time <= clip.start + clip.duration )
            {
               
                if (playableBehaviour.camera != null)
                {
                    playableBehaviour.camera.enabled = true;
                    playableBehaviour.camera.targetTexture = m_CameraSwitcherSettings.renderTextureA;
                    
                }

                if (inputPort + 1 < m_Clips.Count())
                {
                    var nextClip = m_Clips.ToList()[inputPort + 1];
                    var _scriptPlayable =  (ScriptPlayable<CameraSwitcherControlBehaviour>)playable.GetInput(inputPort+1);
                    var _playableBehaviour = _scriptPlayable.GetBehaviour();
                    
                    
                    if (nextClip.start <= m_Director.time && m_Director.time <= nextClip.start + clip.duration )
                    {
                        _playableBehaviour.camera.enabled = true;
                        _playableBehaviour.camera.targetTexture = m_CameraSwitcherSettings.renderTextureB;
                        m_CompositeMaterial.SetTexture("_TextureA",m_CameraSwitcherSettings.renderTextureA);
                        var offsetPositionA = new Vector2(
                            playableBehaviour.offsetPosition.x / m_TrackBinding.rectTransform.rect.width,
                            playableBehaviour.offsetPosition.y / m_TrackBinding.rectTransform.rect.height);
                        m_CompositeMaterial.SetFloat("_WigglePowerA",playableBehaviour.wiggle ? 1f:0f);
                        m_CompositeMaterial.SetVector("_NoiseSeedA",playableBehaviour.noiseSeed);
                        m_CompositeMaterial.SetVector("_NoiseScaleA",playableBehaviour.noiseScale);
                        m_CompositeMaterial.SetFloat("_TimeScaleA",playableBehaviour.roughness);
                        m_CompositeMaterial.SetVector("_RangeA",playableBehaviour.wiggleRange/100f);
                        m_CompositeMaterial.SetVector("_OffsetPositionA",offsetPositionA);

                        
                        var offsetPositionB = new Vector2(
                            _playableBehaviour.offsetPosition.x / m_TrackBinding.rectTransform.rect.width,
                            _playableBehaviour.offsetPosition.y / m_TrackBinding.rectTransform.rect.height);
                        m_CompositeMaterial.SetTexture("_TextureB",m_CameraSwitcherSettings.renderTextureB);
                        m_CompositeMaterial.SetFloat("_WigglePowerB",_playableBehaviour.wiggle ? 1f:0f);
                        m_CompositeMaterial.SetVector("_NoiseSeedB",_playableBehaviour.noiseSeed);
                        m_CompositeMaterial.SetVector("_NoiseScaleB",_playableBehaviour.noiseScale);
                        m_CompositeMaterial.SetFloat("_TimeScaleB",_playableBehaviour.roughness);
                        m_CompositeMaterial.SetVector("_RangeB",_playableBehaviour.wiggleRange/100f);
                        m_CompositeMaterial.SetFloat("_CrossFade", 1f-inputWeight);
                        m_CompositeMaterial.SetVector("_OffsetPositionB",offsetPositionB);
                    }
                    else
                    {
                        m_CompositeMaterial.SetTexture("_TextureA",m_CameraSwitcherSettings.renderTextureA);
                        m_CompositeMaterial.SetFloat("_WigglePowerA",playableBehaviour.wiggle ? 1f:0f);
                        m_CompositeMaterial.SetTexture("_TextureB",m_CameraSwitcherSettings.renderTextureA);
                        m_CompositeMaterial.SetFloat("_WigglePowerB",playableBehaviour.wiggle ? 1f:0f);
                        m_CompositeMaterial.SetFloat("_CrossFade", inputWeight);
                        
                        var offsetPositionA = new Vector2(
                            playableBehaviour.offsetPosition.x / m_TrackBinding.rectTransform.rect.width,
                            playableBehaviour.offsetPosition.y / m_TrackBinding.rectTransform.rect.height);
                        
                        m_CompositeMaterial.SetFloat("_WigglePowerA",playableBehaviour.wiggle ? 1f:0f);
                        m_CompositeMaterial.SetVector("_NoiseSeedA",playableBehaviour.noiseSeed);
                        m_CompositeMaterial.SetVector("_NoiseScaleA",playableBehaviour.noiseScale);
                        m_CompositeMaterial.SetFloat("_TimeScaleA",playableBehaviour.roughness);
                        m_CompositeMaterial.SetVector("_RangeA",playableBehaviour.wiggleRange/100f);
                        m_CompositeMaterial.SetVector("_OffsetPositionA",offsetPositionA);
                        
                        m_CompositeMaterial.SetFloat("_WigglePowerB",playableBehaviour.wiggle ? 1f:0f);
                        m_CompositeMaterial.SetVector("_NoiseSeedB",playableBehaviour.noiseSeed);
                        m_CompositeMaterial.SetVector("_NoiseScaleB",playableBehaviour.noiseScale);
                        m_CompositeMaterial.SetFloat("_TimeScaleB",playableBehaviour.roughness);
                        m_CompositeMaterial.SetVector("_RangeB",playableBehaviour.wiggleRange/100f);
                        m_CompositeMaterial.SetVector("_OffsetPositionB",offsetPositionA);
                    }
                   
                }
                else
                {
                    m_CompositeMaterial.SetTexture("_TextureA",m_CameraSwitcherSettings.renderTextureA);
                    m_CompositeMaterial.SetFloat("_WigglePowerA",playableBehaviour.wiggle ? 1f:0f);
                    m_CompositeMaterial.SetTexture("_TextureB",m_CameraSwitcherSettings.renderTextureA);
                    m_CompositeMaterial.SetFloat("_WigglePowerB",playableBehaviour.wiggle ? 1f:0f);
                    m_CompositeMaterial.SetFloat("_CrossFade", inputWeight);
                        
                    var offsetPositionA = new Vector2(
                        playableBehaviour.offsetPosition.x / m_TrackBinding.rectTransform.rect.width,
                        playableBehaviour.offsetPosition.y / m_TrackBinding.rectTransform.rect.height);
                        
                    m_CompositeMaterial.SetFloat("_WigglePowerA",playableBehaviour.wiggle ? 1f:0f);
                    m_CompositeMaterial.SetVector("_NoiseSeedA",playableBehaviour.noiseSeed);
                    m_CompositeMaterial.SetVector("_NoiseScaleA",playableBehaviour.noiseScale);
                    m_CompositeMaterial.SetFloat("_TimeScaleA",playableBehaviour.roughness);
                    m_CompositeMaterial.SetVector("_RangeA",playableBehaviour.wiggleRange/100f);
                    m_CompositeMaterial.SetVector("_OffsetPositionA",offsetPositionA);
                        
                    m_CompositeMaterial.SetFloat("_WigglePowerB",playableBehaviour.wiggle ? 1f:0f);
                    m_CompositeMaterial.SetVector("_NoiseSeedB",playableBehaviour.noiseSeed);
                    m_CompositeMaterial.SetVector("_NoiseScaleB",playableBehaviour.noiseScale);
                    m_CompositeMaterial.SetFloat("_TimeScaleB",playableBehaviour.roughness);
                    m_CompositeMaterial.SetVector("_RangeB",playableBehaviour.wiggleRange/100f);
                    m_CompositeMaterial.SetVector("_OffsetPositionB",offsetPositionA);
                }

                break;
                
            }
            inputPort++;
        }

       
        
    }


    public override void OnPlayableDestroy(Playable playable)
    {
        base.OnPlayableDestroy(playable);
        if (m_CompositeMaterial != null) m_CompositeMaterial.SetFloat("_PlayableDirectorTime", 0f);
    }
    
}
