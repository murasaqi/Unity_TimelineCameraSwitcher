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
    // private Material m_TrackBinding.cameraSwitcherSettings.material;
    private CameraSwitcherControlTrack m_track;
    private bool m_FirstFrameHappened;
    // private CameraSwitcherSettings m_TrackBinding.cameraSwitcherSettings = null;

    private float m_fps;
    
  

    internal float fps
    {
        get => m_fps;
        set { m_fps = value; }
    }
    
    internal PlayableDirector director
    {
        get { return m_Director; }
        set { m_Director = value; }
    }
    
    // internal CameraSwitcherSettings cameraSwitcherSettings
    // {
    //     get { return m_TrackBinding.cameraSwitcherSettings; }
    //     set { m_TrackBinding.cameraSwitcherSettings = value; }
    // }

    
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
    
    // internal Material compositeMaterial
    // {
    //     get { return m_TrackBinding.cameraSwitcherSettings.material; }
    //     set { m_TrackBinding.cameraSwitcherSettings.material = value; }
    // }

    private CameraSwitcherControl m_TrackBinding;
    
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
            m_TrackBinding.cameraSwitcherSettings.renderTextureA.Release();
            m_TrackBinding.cameraSwitcherSettings.renderTextureA.format= m_track.renderTextureFormat;
            m_TrackBinding.cameraSwitcherSettings.renderTextureA.depth = depth;
            m_TrackBinding.cameraSwitcherSettings.renderTextureA.width = m_track.width;
            m_TrackBinding.cameraSwitcherSettings.renderTextureA.height = m_track.height;
        }else
        {
            m_TrackBinding.cameraSwitcherSettings.renderTextureB.Release();
            m_TrackBinding.cameraSwitcherSettings.renderTextureB.format= m_track.renderTextureFormat;
            m_TrackBinding.cameraSwitcherSettings.renderTextureB.depth = depth;
            m_TrackBinding.cameraSwitcherSettings.renderTextureB.width = m_track.width;
            m_TrackBinding.cameraSwitcherSettings.renderTextureB.height = m_track.height;
        }

        if (m_TrackBinding.outPutTarget != null)
        {
            m_TrackBinding.outPutTarget.Release();
            m_TrackBinding.outPutTarget.format= m_track.renderTextureFormat;
            m_TrackBinding.outPutTarget.depth = depth;
            m_TrackBinding.outPutTarget.width = m_track.width;
            m_TrackBinding.outPutTarget.height = m_track.height;
        }
        
    }
    public override void ProcessFrame(Playable playable, FrameData info, object playerData)
    {
        // m_TrackBinding = playerData as RawImage;
        if (m_TrackBinding == null)
            return;
        if (!m_FirstFrameHappened)
        {
            InitRenderTexture(true);
            InitRenderTexture(false);
            m_FirstFrameHappened = true;
        }
        // if ( != null)
        // {
        //     m_TrackBinding.material = m_TrackBinding.cameraSwitcherSettings.material;
        //     m_TrackBinding.SetMaterialDirty();
        // }
        if(m_TrackBinding.cameraSwitcherSettings.renderTextureA.format != m_track.renderTextureFormat) InitRenderTexture(true);
        if(m_TrackBinding.cameraSwitcherSettings.renderTextureB.format != m_track.renderTextureFormat) InitRenderTexture(false);
        if(m_TrackBinding.cameraSwitcherSettings.renderTextureA.width != m_track.width || m_TrackBinding.cameraSwitcherSettings.renderTextureA.height != m_track.height) InitRenderTexture(true);
        if(m_TrackBinding.cameraSwitcherSettings.renderTextureB.width != m_track.width || m_TrackBinding.cameraSwitcherSettings.renderTextureB.height != m_track.height) InitRenderTexture(false);
        
        var timelineAsset = director.playableAsset as TimelineAsset;
       
        fps = timelineAsset != null ? timelineAsset.editorSettings.fps : 60;
        var offsetStartTime = (1f / fps) * m_track.m_prerenderingFrameCount;
        
        
        m_TrackBinding.cameraSwitcherSettings.material.SetFloat("_PlayableDirectorTime",(float)director.time);
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


        var currentDirectorTime = m_Director.time - offsetStartTime;
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
                    playableBehaviour.camera.targetTexture = m_TrackBinding.cameraSwitcherSettings.renderTextureA;
                    
                }

                if (inputPort + 1 < m_Clips.Count())
                {
                    var nextClip = m_Clips.ToList()[inputPort + 1];
                    var _scriptPlayable =  (ScriptPlayable<CameraSwitcherControlBehaviour>)playable.GetInput(inputPort+1);
                    var _playableBehaviour = _scriptPlayable.GetBehaviour();
                    
                    
                    if (nextClip.start-offsetStartTime <= m_Director.time && m_Director.time <= nextClip.start + clip.duration )
                    {
                        _playableBehaviour.camera.enabled = true;
                        _playableBehaviour.camera.targetTexture = m_TrackBinding.cameraSwitcherSettings.renderTextureB;
                        m_TrackBinding.cameraSwitcherSettings.material.SetTexture("_TextureA",m_TrackBinding.cameraSwitcherSettings.renderTextureA);
                        var offsetPositionA = new Vector2(
                            playableBehaviour.offsetPosition.x / m_track.width,
                            playableBehaviour.offsetPosition.y / m_track.height);
                        m_TrackBinding.cameraSwitcherSettings.material.SetFloat("_WigglePowerA",playableBehaviour.wiggle ? 1f:0f);
                        m_TrackBinding.cameraSwitcherSettings.material.SetVector("_NoiseSeedA",playableBehaviour.noiseSeed);
                        m_TrackBinding.cameraSwitcherSettings.material.SetVector("_NoiseScaleA",playableBehaviour.noiseScale);
                        m_TrackBinding.cameraSwitcherSettings.material.SetFloat("_TimeScaleA",playableBehaviour.roughness);
                        m_TrackBinding.cameraSwitcherSettings.material.SetVector("_RangeA",playableBehaviour.wiggleRange/100f);
                        m_TrackBinding.cameraSwitcherSettings.material.SetVector("_OffsetPositionA",offsetPositionA);

                        
                        var offsetPositionB = new Vector2(
                            _playableBehaviour.offsetPosition.x / m_track.width,
                            _playableBehaviour.offsetPosition.y / m_track.height);
                        m_TrackBinding.cameraSwitcherSettings.material.SetTexture("_TextureB",m_TrackBinding.cameraSwitcherSettings.renderTextureB);
                        m_TrackBinding.cameraSwitcherSettings.material.SetFloat("_WigglePowerB",_playableBehaviour.wiggle ? 1f:0f);
                        m_TrackBinding.cameraSwitcherSettings.material.SetVector("_NoiseSeedB",_playableBehaviour.noiseSeed);
                        m_TrackBinding.cameraSwitcherSettings.material.SetVector("_NoiseScaleB",_playableBehaviour.noiseScale);
                        m_TrackBinding.cameraSwitcherSettings.material.SetFloat("_TimeScaleB",_playableBehaviour.roughness);
                        m_TrackBinding.cameraSwitcherSettings.material.SetVector("_RangeB",_playableBehaviour.wiggleRange/100f);
                        m_TrackBinding.cameraSwitcherSettings.material.SetFloat("_CrossFade", 1f-inputWeight);
                        m_TrackBinding.cameraSwitcherSettings.material.SetVector("_OffsetPositionB",offsetPositionB);
                    }
                    else
                    {
                        m_TrackBinding.cameraSwitcherSettings.material.SetTexture("_TextureA",m_TrackBinding.cameraSwitcherSettings.renderTextureA);
                        m_TrackBinding.cameraSwitcherSettings.material.SetFloat("_WigglePowerA",playableBehaviour.wiggle ? 1f:0f);
                        m_TrackBinding.cameraSwitcherSettings.material.SetTexture("_TextureB",m_TrackBinding.cameraSwitcherSettings.renderTextureA);
                        m_TrackBinding.cameraSwitcherSettings.material.SetFloat("_WigglePowerB",playableBehaviour.wiggle ? 1f:0f);
                        m_TrackBinding.cameraSwitcherSettings.material.SetFloat("_CrossFade", inputWeight);
                        
                        var offsetPositionA = new Vector2(
                            playableBehaviour.offsetPosition.x / m_track.width,
                            playableBehaviour.offsetPosition.y / m_track.height);
                        
                        m_TrackBinding.cameraSwitcherSettings.material.SetFloat("_WigglePowerA",playableBehaviour.wiggle ? 1f:0f);
                        m_TrackBinding.cameraSwitcherSettings.material.SetVector("_NoiseSeedA",playableBehaviour.noiseSeed);
                        m_TrackBinding.cameraSwitcherSettings.material.SetVector("_NoiseScaleA",playableBehaviour.noiseScale);
                        m_TrackBinding.cameraSwitcherSettings.material.SetFloat("_TimeScaleA",playableBehaviour.roughness);
                        m_TrackBinding.cameraSwitcherSettings.material.SetVector("_RangeA",playableBehaviour.wiggleRange/100f);
                        m_TrackBinding.cameraSwitcherSettings.material.SetVector("_OffsetPositionA",offsetPositionA);
                        
                        m_TrackBinding.cameraSwitcherSettings.material.SetFloat("_WigglePowerB",playableBehaviour.wiggle ? 1f:0f);
                        m_TrackBinding.cameraSwitcherSettings.material.SetVector("_NoiseSeedB",playableBehaviour.noiseSeed);
                        m_TrackBinding.cameraSwitcherSettings.material.SetVector("_NoiseScaleB",playableBehaviour.noiseScale);
                        m_TrackBinding.cameraSwitcherSettings.material.SetFloat("_TimeScaleB",playableBehaviour.roughness);
                        m_TrackBinding.cameraSwitcherSettings.material.SetVector("_RangeB",playableBehaviour.wiggleRange/100f);
                        m_TrackBinding.cameraSwitcherSettings.material.SetVector("_OffsetPositionB",offsetPositionA);
                    }
                   
                }
                else
                {
                    m_TrackBinding.cameraSwitcherSettings.material.SetTexture("_TextureA",m_TrackBinding.cameraSwitcherSettings.renderTextureA);
                    m_TrackBinding.cameraSwitcherSettings.material.SetFloat("_WigglePowerA",playableBehaviour.wiggle ? 1f:0f);
                    m_TrackBinding.cameraSwitcherSettings.material.SetTexture("_TextureB",m_TrackBinding.cameraSwitcherSettings.renderTextureA);
                    m_TrackBinding.cameraSwitcherSettings.material.SetFloat("_WigglePowerB",playableBehaviour.wiggle ? 1f:0f);
                    m_TrackBinding.cameraSwitcherSettings.material.SetFloat("_CrossFade", inputWeight);
                        
                    var offsetPositionA = new Vector2(
                        playableBehaviour.offsetPosition.x / m_track.width,
                        playableBehaviour.offsetPosition.y / m_track.height);
                        
                    m_TrackBinding.cameraSwitcherSettings.material.SetFloat("_WigglePowerA",playableBehaviour.wiggle ? 1f:0f);
                    m_TrackBinding.cameraSwitcherSettings.material.SetVector("_NoiseSeedA",playableBehaviour.noiseSeed);
                    m_TrackBinding.cameraSwitcherSettings.material.SetVector("_NoiseScaleA",playableBehaviour.noiseScale);
                    m_TrackBinding.cameraSwitcherSettings.material.SetFloat("_TimeScaleA",playableBehaviour.roughness);
                    m_TrackBinding.cameraSwitcherSettings.material.SetVector("_RangeA",playableBehaviour.wiggleRange/100f);
                    m_TrackBinding.cameraSwitcherSettings.material.SetVector("_OffsetPositionA",offsetPositionA);
                        
                    m_TrackBinding.cameraSwitcherSettings.material.SetFloat("_WigglePowerB",playableBehaviour.wiggle ? 1f:0f);
                    m_TrackBinding.cameraSwitcherSettings.material.SetVector("_NoiseSeedB",playableBehaviour.noiseSeed);
                    m_TrackBinding.cameraSwitcherSettings.material.SetVector("_NoiseScaleB",playableBehaviour.noiseScale);
                    m_TrackBinding.cameraSwitcherSettings.material.SetFloat("_TimeScaleB",playableBehaviour.roughness);
                    m_TrackBinding.cameraSwitcherSettings.material.SetVector("_RangeB",playableBehaviour.wiggleRange/100f);
                    m_TrackBinding.cameraSwitcherSettings.material.SetVector("_OffsetPositionB",offsetPositionA);
                }

                break;
                
            }
            inputPort++;
        }

        if (m_TrackBinding.outPutTarget != null)
        {
            m_TrackBinding.Blit();
        }
        
    }


    public override void OnPlayableDestroy(Playable playable)
    {
        base.OnPlayableDestroy(playable);
        if (m_TrackBinding.cameraSwitcherSettings.material != null) m_TrackBinding.cameraSwitcherSettings.material.SetFloat("_PlayableDirectorTime", 0f);
    }
    
}
