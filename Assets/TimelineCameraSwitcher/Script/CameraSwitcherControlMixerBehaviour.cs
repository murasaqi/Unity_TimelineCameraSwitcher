// using UnityEngin

using System;
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
    
   
    private CameraSwitcherControl m_TrackBinding;
    
    public void ResetCameraTarget()
    {
        foreach (var camera in _cameras)
        {
            // Debug.Log(camera.name);
            camera.targetTexture = null;
        }
    }
    
    // private void InitRenderTexture(bool isA)
    // {
    //     var depth = 0;
    //     if (m_track.depthList == DepthList.AtLeast16) depth = 16;
    //     if (m_track.depthList == DepthList.AtLeast24_WidthStencil) depth = 24;
    //     
    //     if (isA)
    //     {
    //         m_TrackBinding.renderTextureA.Release();
    //         m_TrackBinding.renderTextureA.format= m_track.renderTextureFormat;
    //         m_TrackBinding.renderTextureA.depth = depth;
    //         m_TrackBinding.renderTextureA.width = m_track.width;
    //         m_TrackBinding.renderTextureA.height = m_track.height;
    //     }else
    //     {
    //         m_TrackBinding.renderTextureB.Release();
    //         m_TrackBinding.renderTextureB.format= m_track.renderTextureFormat;
    //         m_TrackBinding.renderTextureB.depth = depth;
    //         m_TrackBinding.renderTextureB.width = m_track.width;
    //         m_TrackBinding.renderTextureB.height = m_track.height;
    //         // m_TrackBinding.material.SetTexture("");
    //     }
    //
    //     if (m_TrackBinding.outPutRenderTarget != null)
    //     {
    //         m_TrackBinding.outPutRenderTarget.Release();
    //         m_TrackBinding.outPutRenderTarget.format= m_track.renderTextureFormat;
    //         m_TrackBinding.outPutRenderTarget.depth = depth;
    //         m_TrackBinding.outPutRenderTarget.width = m_track.width;
    //         m_TrackBinding.outPutRenderTarget.height = m_track.height;
    //     }
    //     
    //     
    // }
    public override void ProcessFrame(Playable playable, FrameData info, object playerData)
    {
        m_TrackBinding = playerData as CameraSwitcherControl;
        if (m_TrackBinding == null)
            return;
        if (!m_FirstFrameHappened)
        {
            m_TrackBinding.InitRenderTexture();
            
            if (m_TrackBinding != null)
            {
                m_TrackBinding.Blit();
            }
            m_FirstFrameHappened = true;
        }
        
        // if(m_TrackBinding.renderTextureA.format != m_track.renderTextureFormat) InitRenderTexture(true);
        // if(m_TrackBinding.renderTextureB.format != m_track.renderTextureFormat) InitRenderTexture(false);
        // if(m_TrackBinding.renderTextureA.width != m_track.width || m_TrackBinding.cameraSwitcherSettings.renderTextureA.height != m_track.height) InitRenderTexture(true);
        // if(m_TrackBinding.renderTextureB.width != m_track.width || m_TrackBinding.cameraSwitcherSettings.renderTextureB.height != m_track.height) InitRenderTexture(false);
        // if (m_track.m_prerenderingFrameCount != m_TrackBinding.preRenderingFrameCount) m_TrackBinding.preRenderingFrameCount = m_track.m_prerenderingFrameCount;
        var timelineAsset = director.playableAsset as TimelineAsset;
       
        fps = timelineAsset != null ? timelineAsset.editorSettings.fps : 60;
        var offsetStartTime = (1f / fps) * m_TrackBinding.preRenderingFrameCount;
        
        
        m_TrackBinding.material.SetFloat("_PlayableDirectorTime",(float)director.time);
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
        // // var missingCameraCount = 0;
        // if (m_track.findMissingCameraInHierarchy)
        // {
        //    
        //     
        //    
        //     foreach (var clip in m_Clips)
        //     {
        //         var scriptPlayable =  (ScriptPlayable<CameraSwitcherControlBehaviour>)playable.GetInput(i);
        //         var playableBehaviour = scriptPlayable.GetBehaviour();
        //
        //         var missing = false;
        //         if (playableBehaviour.camera == null)
        //         {
        //             Debug.Log($"<color=#DA70D6>[frame: {Mathf.CeilToInt((float)clip.start*fps )} clip: {clip.displayName}] Camera reference is null.</color>");
        //             missing = true;
        //         }
        //         else
        //         {
        //             if (clip.displayName != playableBehaviour.camera.name)
        //             {
        //                 Debug.Log($"<color=#9370DB>[frame: {Mathf.CeilToInt((float)clip.start*fps )} clip: {clip.displayName}] Name does not match the camera</color>");
        //                 missing = true;
        //             }
        //         }
        //
        //         if (missing && m_track.fixMissingPrefabByCameraName)
        //         {
        //             
        //             var cam = GameObject.Find(clip.displayName);
        //             if (cam!=null)
        //             {
        //                 var serializedObject = new SerializedObject(clip.asset, director);
        //                 var serializedProperty = serializedObject.FindProperty("camera");
        //                 // if(cam.GetComponent<Camera>())Debug.Log(cam);
        //                 Debug.Log(cam.name);
        //                 // playableBehaviour.camera = cam.GetComponent<Camera>();
        //                 if(cam.GetComponent<Camera>())serializedProperty.exposedReferenceValue = cam.GetComponent<Camera>();
        //                 serializedObject.ApplyModifiedProperties();
        //                 Debug.Log($"<color=#00BFFF>Fix {clip.displayName}</color>");
        //             }
        //         }
        //
        //         i++;
        //     }
        //    
        // }

#endif
        
       
        int inputPort = 0;

        var currentTime = (float)m_Director.time;
        var wiggler = Vector4.zero;
        var wigglerRange = Vector2.zero;
        // var currentDirectorTime = m_Director.time - offsetStartTime;
        foreach (TimelineClip clip in m_Clips)
        {
            
            var inputWeight = playable.GetInputWeight(inputPort);
           
            var scriptPlayable =  (ScriptPlayable<CameraSwitcherControlBehaviour>)playable.GetInput(inputPort);
            var playableBehaviour = scriptPlayable.GetBehaviour();

          

            if (clip.start <= m_Director.time && m_Director.time <= clip.start + clip.duration )
            {
                ResetCameraTarget();
                var renderQue = new CameraSwitcherRenderQue();
                m_TrackBinding.AddRenderQue(renderQue);
                if (playableBehaviour.camera != null)
                {
                    playableBehaviour.camera.targetTexture = m_TrackBinding.renderTextureA;
                    
                }

               

                // 次のクリップが存在しているとき
                if (inputPort + 1 < m_Clips.Count())
                {
                    var nextClip = m_Clips.ToList()[inputPort + 1];
                    var _scriptPlayable =  (ScriptPlayable<CameraSwitcherControlBehaviour>)playable.GetInput(inputPort+1);
                    var _playableBehaviour = _scriptPlayable.GetBehaviour();
                    
                    
                    // 次のクリップをすでに踏んでいる場合（preRenderFrameCountを含む）
                    if (nextClip.start-offsetStartTime <= m_Director.time && m_Director.time <= nextClip.start + clip.duration )
                    {
                        
                        m_TrackBinding.material.SetTexture("_TextureA",m_TrackBinding.renderTextureA);
                        m_TrackBinding.material.SetFloat("_WigglePowerA",playableBehaviour.wiggle ? 1f:0f);
                        m_TrackBinding.material.SetVector("_NoiseSeedA",playableBehaviour.noiseSeed);
                        m_TrackBinding.material.SetVector("_NoiseScaleA",playableBehaviour.noiseScale);
                        m_TrackBinding.material.SetFloat("_TimeScaleA",playableBehaviour.roughness);
                        m_TrackBinding.material.SetVector("_RangeA",playableBehaviour.wiggleRange/100f);
                        renderQue.AddQue(playableBehaviour.camera, m_TrackBinding.renderTextureA);
                        
                       
                        // _playableBehaviour.camera.enabled = true;
                        _playableBehaviour.camera.targetTexture = m_TrackBinding.renderTextureB;
                        m_TrackBinding.material.SetTexture("_TextureB",m_TrackBinding.renderTextureB);
                        m_TrackBinding.material.SetFloat("_WigglePowerB",_playableBehaviour.wiggle ? 1f:0f);
                        m_TrackBinding.material.SetVector("_NoiseSeedB",_playableBehaviour.noiseSeed);
                        m_TrackBinding.material.SetVector("_NoiseScaleB",_playableBehaviour.noiseScale);
                        m_TrackBinding.material.SetFloat("_TimeScaleB",_playableBehaviour.roughness);
                        m_TrackBinding.material.SetVector("_RangeB",_playableBehaviour.wiggleRange/100f);
                        m_TrackBinding.material.SetFloat("_CrossFade", 1f-inputWeight);
                        renderQue.AddQue(_playableBehaviour.camera, m_TrackBinding.renderTextureB);
                      

                        wigglerRange = new Vector2(
                            Math.Max(playableBehaviour.wiggleRange.x, playableBehaviour.wiggleRange.y),
                            Math.Max(_playableBehaviour.wiggleRange.x, _playableBehaviour.wiggleRange.y)
                        );
                        wiggler = CalcNoise(playableBehaviour, _playableBehaviour,currentTime);
                        
                       
                    }
                    else
                    {
                        m_TrackBinding.material.SetTexture("_TextureA",m_TrackBinding.renderTextureA);
                        m_TrackBinding.material.SetFloat("_WigglePowerA",playableBehaviour.wiggle ? 1f:0f);
                        m_TrackBinding.material.SetTexture("_TextureB",m_TrackBinding.renderTextureA);
                        m_TrackBinding.material.SetFloat("_WigglePowerB",playableBehaviour.wiggle ? 1f:0f);
                        m_TrackBinding.material.SetFloat("_CrossFade", inputWeight);

                        m_TrackBinding.material.SetFloat("_WigglePowerA",playableBehaviour.wiggle ? 1f:0f);
                        m_TrackBinding.material.SetVector("_NoiseSeedA",playableBehaviour.noiseSeed);
                        m_TrackBinding.material.SetVector("_NoiseScaleA",playableBehaviour.noiseScale);
                        m_TrackBinding.material.SetFloat("_TimeScaleA",playableBehaviour.roughness);
                        m_TrackBinding.material.SetVector("_RangeA",playableBehaviour.wiggleRange/100f);
                        // m_TrackBinding.material.SetVector("_OffsetPositionA",offsetPositionA);
                        
                        m_TrackBinding.material.SetFloat("_WigglePowerB",playableBehaviour.wiggle ? 1f:0f);
                        m_TrackBinding.material.SetVector("_NoiseSeedB",playableBehaviour.noiseSeed);
                        m_TrackBinding.material.SetVector("_NoiseScaleB",playableBehaviour.noiseScale);
                        m_TrackBinding.material.SetFloat("_TimeScaleB",playableBehaviour.roughness);
                        m_TrackBinding.material.SetVector("_RangeB",playableBehaviour.wiggleRange/100f);
                        // m_TrackBinding.material.SetVector("_OffsetPositionB",offsetPositionA);
                        
                        renderQue.AddQue(playableBehaviour.camera, m_TrackBinding.renderTextureA);
                        wigglerRange = new Vector2(
                            Math.Max(playableBehaviour.wiggleRange.x, playableBehaviour.wiggleRange.y),
                            Math.Max(playableBehaviour.wiggleRange.x, playableBehaviour.wiggleRange.y)
                        );
                        wiggler = CalcNoise(playableBehaviour, playableBehaviour,currentTime);
                    }
                   
                }
                else
                {
                    m_TrackBinding.material.SetTexture("_TextureA",m_TrackBinding.renderTextureA);
                    m_TrackBinding.material.SetFloat("_WigglePowerA",playableBehaviour.wiggle ? 1f:0f);
                    m_TrackBinding.material.SetTexture("_TextureB",m_TrackBinding.renderTextureA);
                    m_TrackBinding.material.SetFloat("_WigglePowerB",playableBehaviour.wiggle ? 1f:0f);
                    m_TrackBinding.material.SetFloat("_CrossFade", inputWeight);
                        
                    
                    m_TrackBinding.material.SetFloat("_WigglePowerA",playableBehaviour.wiggle ? 1f:0f);
                    m_TrackBinding.material.SetVector("_NoiseSeedA",playableBehaviour.noiseSeed);
                    m_TrackBinding.material.SetVector("_NoiseScaleA",playableBehaviour.noiseScale);
                    m_TrackBinding.material.SetFloat("_TimeScaleA",playableBehaviour.roughness);
                    m_TrackBinding.material.SetVector("_RangeA",playableBehaviour.wiggleRange/100f);
                    
                        
                    m_TrackBinding.material.SetFloat("_WigglePowerB",playableBehaviour.wiggle ? 1f:0f);
                    m_TrackBinding.material.SetVector("_NoiseSeedB",playableBehaviour.noiseSeed);
                    m_TrackBinding.material.SetVector("_NoiseScaleB",playableBehaviour.noiseScale);
                    m_TrackBinding.material.SetFloat("_TimeScaleB",playableBehaviour.roughness);
                    m_TrackBinding.material.SetVector("_RangeB",playableBehaviour.wiggleRange/100f);
                    // m_TrackBinding.material.SetVector("_OffsetPositionB",offsetPositionA);
                    renderQue.AddQue(playableBehaviour.camera, m_TrackBinding.renderTextureA);
                   
                    wigglerRange = new Vector2(
                        Math.Max(playableBehaviour.wiggleRange.x, playableBehaviour.wiggleRange.y),
                        Math.Max(playableBehaviour.wiggleRange.x, playableBehaviour.wiggleRange.y)
                    );
                    wiggler = CalcNoise(playableBehaviour, playableBehaviour,currentTime);
                }

                break;
                
            }
            inputPort++;
        }

        m_TrackBinding.material.SetVector("_Wiggler",wiggler);
        m_TrackBinding.material.SetVector("_WigglerRange",wigglerRange/100f);

        //
        m_TrackBinding.Render();

    }


    public Vector4 CalcNoise(CameraSwitcherControlBehaviour a, CameraSwitcherControlBehaviour b, float currentTime)
    {
        var isWiggle = new Vector2(
            a.wiggle ? 1 : 0,
            b.wiggle ? 1 : 0
        );
        var wiggler = new Vector4(
            (Mathf.PerlinNoise(a.noiseSeed.x * a.noiseScale.x, currentTime*a.roughness + a.noiseScale.y*a.noiseSeed.y)-0.5f) * a.wiggleRange.x/100f*isWiggle.x,
            (Mathf.PerlinNoise(a.noiseSeed.x * a.noiseScale.x + currentTime*a.roughness, a.noiseScale.y*a.noiseSeed.y)-0.5f) * a.wiggleRange.y/100f*isWiggle.x,
            (Mathf.PerlinNoise(b.noiseSeed.x * b.noiseScale.x, currentTime*b.roughness + b.noiseScale.y*b.noiseSeed.y)-0.5f) * b.wiggleRange.x/100f*isWiggle.y,
            (Mathf.PerlinNoise(b.noiseSeed.x * b.noiseScale.x + currentTime*b.roughness, b.noiseScale.y*b.noiseSeed.y)-0.5f) * b.wiggleRange.y/100f*isWiggle.y
        );

        return wiggler;
    }

    public override void OnPlayableDestroy(Playable playable)
    {
        base.OnPlayableDestroy(playable);
        if (m_TrackBinding != null) m_TrackBinding.material.SetFloat("_PlayableDirectorTime", 0f);

        if (m_TrackBinding != null)
        {
            // m_TrackBinding.ren>
        }
    }
    
}
