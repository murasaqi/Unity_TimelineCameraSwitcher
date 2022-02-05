

using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Playables;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.Timeline;


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

    private DepthOfField dof;
    
    private void InitRenderTexture(bool isA)
    {
        var depth = 0;
        if (m_TrackBinding.depthList == DepthList.AtLeast16) depth = 16;
        if (m_TrackBinding.depthList == DepthList.AtLeast24_WidthStencil) depth = 24;
        
        if (isA)
        {
            m_TrackBinding.renderTextureA.Release();
            m_TrackBinding.renderTextureA.format= m_TrackBinding.renderTextureFormat;
            m_TrackBinding.renderTextureA.depth = depth;
            m_TrackBinding.renderTextureA.width = m_TrackBinding.width;
            m_TrackBinding.renderTextureA.height = m_TrackBinding.height;
        }else
        {
            m_TrackBinding.renderTextureB.Release();
            m_TrackBinding.renderTextureB.format= m_TrackBinding.renderTextureFormat;
            m_TrackBinding.renderTextureB.depth = depth;
            m_TrackBinding.renderTextureB.width = m_TrackBinding.width;
            m_TrackBinding.renderTextureB.height = m_TrackBinding.height;
            // m_TrackBinding.material.SetTexture("");
        }


    }
    public override void ProcessFrame(Playable playable, FrameData info, object playerData)
    {
        m_TrackBinding = playerData as CameraSwitcherControl;
        if (m_TrackBinding == null)
            return;
        if (!m_FirstFrameHappened)
        {
            InitRenderTexture(true);
            InitRenderTexture(false);
            
            m_FirstFrameHappened = true;
        }
        
        if(m_TrackBinding.renderTextureA.format != m_TrackBinding.renderTextureFormat) InitRenderTexture(true);
        if(m_TrackBinding.renderTextureB.format != m_TrackBinding.renderTextureFormat) InitRenderTexture(false);
        if(m_TrackBinding.renderTextureA.width != m_TrackBinding.width || m_TrackBinding.renderTextureA.height != m_TrackBinding.height) InitRenderTexture(true);
        if(m_TrackBinding.renderTextureB.width != m_TrackBinding.width || m_TrackBinding.renderTextureB.height != m_TrackBinding.height) InitRenderTexture(false);
        if (m_TrackBinding.prerenderingFrameCount != m_TrackBinding.prerenderingFrameCount) m_TrackBinding.prerenderingFrameCount = m_TrackBinding.prerenderingFrameCount;
        
        var timelineAsset = director.playableAsset as TimelineAsset;
       
        fps = timelineAsset != null ? timelineAsset.editorSettings.fps : 60;
        var offsetStartTime = (1f / fps) * m_TrackBinding.prerenderingFrameCount;
        
        
        m_TrackBinding.material.SetFloat("_PlayableDirectorTime",(float)director.time);
        _cameras.Clear();

        if (dof == null && m_TrackBinding.volume != null)
        {
            m_TrackBinding.volume.TryGet<DepthOfField>(out dof);
        }
        

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
    

#endif
        
        int inputPort = 0;
        var currentTime = (float)m_Director.time;
        var wiggler = Vector4.zero;
        var wigglerRange = Vector2.zero;
        DepthOfFieldMode depthOfFieldMode;
        var focusDistance = 0f;
        var focalLength = 0f;
        var aperture = 0f;
        var bladeCount = 0;
        var bladeCurvature = 0f;
        var bladeRotation = 0f;


        var currentClips = new List<CameraSwitcherControlBehaviour>();
        // var currentDirectorTime = m_Director.time - offsetStartTime;
        foreach (TimelineClip clip in m_Clips)
        {
            
            var inputWeight = playable.GetInputWeight(inputPort);
           
            var scriptPlayable =  (ScriptPlayable<CameraSwitcherControlBehaviour>)playable.GetInput(inputPort);
            var playableBehaviour = scriptPlayable.GetBehaviour();


            var cameraSwitcherControlClip = clip.asset as CameraSwitcherControlClip;
           if (inputWeight > 0)
           {
               
               
               if (playableBehaviour.dofOverride)
               {
                   depthOfFieldMode = playableBehaviour.dofControlProps.depthOfFieldMode;
                   focusDistance += playableBehaviour.dofControlProps.focusDistance*inputWeight;
                   focalLength += playableBehaviour.dofControlProps.focalLength*inputWeight;
                   aperture += playableBehaviour.dofControlProps.aperture * inputWeight;
                   bladeCount += Mathf.FloorToInt(playableBehaviour.dofControlProps.bladeCount*inputWeight);
                   bladeCurvature += playableBehaviour.dofControlProps.bladeCurvature * inputWeight;
                   bladeRotation += playableBehaviour.dofControlProps.bladeRotation * inputWeight;
               
                   currentClips.Add(playableBehaviour);     
               }

              
               if (cameraSwitcherControlClip.lookAt)
               {
                   var lookAt = playableBehaviour.camera.GetComponent<LookAtConstraint>();
                   
                   if (lookAt == null) lookAt = playableBehaviour.camera.gameObject.AddComponent<LookAtConstraint>();
                   if (lookAt && playableBehaviour.target)
                   {


                       if (lookAt.sourceCount > 0 &&  lookAt.GetSource(0).sourceTransform != playableBehaviour.target)
                       {
                           while (lookAt.sourceCount > 0)
                           {
                               lookAt.RemoveSource(0);
                           }
                           var source = new ConstraintSource();
                           source.sourceTransform = playableBehaviour.target;
                           source.weight = 1;
                           lookAt.AddSource(source);
                       }
                           // lookAt.enabled = true;
                      
                           
                       lookAt.enabled = true;
                       lookAt.locked = playableBehaviour.lookAtProps.Lock;
                       lookAt.constraintActive = playableBehaviour.lookAtProps.IsActive;
                       lookAt.weight = playableBehaviour.lookAtProps.Weight;
                       lookAt.roll = playableBehaviour.lookAtProps.Roll;
                       lookAt.useUpObject = playableBehaviour.lookAtProps.UseUpObject;
                       lookAt.rotationOffset = playableBehaviour.lookAtProps.RotationOffset;
                       lookAt.rotationAtRest = playableBehaviour.lookAtProps.RotationAtReset;
                   }
               }
               else
               {
                   // if (lookAt) lookAt.enabled = false;
               }
              
           }

            if (clip.start <= m_Director.time && m_Director.time <= clip.start + clip.duration )
            {
                ResetCameraTarget();
                if (playableBehaviour.camera != null)
                {
                    // Debug.Log(playableBehaviour.camera.name);
                    playableBehaviour.camera.enabled = true;
                    playableBehaviour.camera.targetTexture = m_TrackBinding.renderTextureA;
                    m_TrackBinding.cameraA = playableBehaviour.camera;

                }


             
               

                if (inputPort + 1 < m_Clips.Count() )
                {
                    var nextClip = m_Clips.ToList()[inputPort + 1];
                    var _scriptPlayable =  (ScriptPlayable<CameraSwitcherControlBehaviour>)playable.GetInput(inputPort+1);
                    var _playableBehaviour = _scriptPlayable.GetBehaviour();
                    
                   
                    if (nextClip.start-offsetStartTime <= m_Director.time && m_Director.time < nextClip.start + clip.duration )
                    {
                        currentClips.Add(_playableBehaviour);     
                        m_TrackBinding.cameraB = _playableBehaviour.camera;
                        // blending中で、次のカメラが同じじゃないとき
                        if (_playableBehaviour.camera != playableBehaviour.camera)
                        {
                            _playableBehaviour.camera.enabled = true;
                            _playableBehaviour.camera.targetTexture = m_TrackBinding.renderTextureB;
                            m_TrackBinding.material.SetTexture("_TextureA", m_TrackBinding.renderTextureA);
                            m_TrackBinding.material.SetTexture("_TextureB", m_TrackBinding.renderTextureB);
                            m_TrackBinding.material.SetVector("_WigglerValueA", CalcNoise(playableBehaviour,currentTime));
                            m_TrackBinding.material.SetVector("_ClipSizeA", playableBehaviour.wiggleRange/ 100f);
                            m_TrackBinding.material.SetColor("_MultiplyColorA", playableBehaviour.multiplyColor);
                            m_TrackBinding.material.SetVector("_ClipSizeB", _playableBehaviour.wiggleRange / 100f);
                            m_TrackBinding.material.SetVector("_WigglerValueB", CalcNoise(_playableBehaviour,currentTime));
                            m_TrackBinding.material.SetColor("_MultiplyColorB", playableBehaviour.multiplyColor);
                            
                            if (playableBehaviour.fadeCurveOverride)
                            {
                                m_TrackBinding.material.SetFloat("_CrossFade", 1f - playableBehaviour.fadeCurve.Evaluate(inputWeight));
                            }
                            else
                            {
                                m_TrackBinding.material.SetFloat("_CrossFade", 1f - inputWeight);
                            }
                          
                            
                          
                        }
                        // blending中で、次のカメラが同じときは値もBlendingしてあげる
                        else
                        {
                            // var nextInputWeight = Mathf.Clamp(1f - inputWeight,0f,1f);
                            SelectSingleCamera(playableBehaviour, inputWeight, currentTime,_playableBehaviour);
                            m_TrackBinding.cameraB = _playableBehaviour.camera;
                            var invWeight = 1f - inputWeight;
                            if (_playableBehaviour.dofOverride)
                            {
                                depthOfFieldMode = _playableBehaviour.dofControlProps.depthOfFieldMode;
                                focusDistance += _playableBehaviour.dofControlProps.focusDistance*invWeight;
                                focalLength += _playableBehaviour.dofControlProps.focalLength*invWeight;
                                aperture += _playableBehaviour.dofControlProps.aperture * invWeight;
                                bladeCount += Mathf.FloorToInt(_playableBehaviour.dofControlProps.bladeCount*invWeight);
                                bladeCurvature += _playableBehaviour.dofControlProps.bladeCurvature * invWeight;
                                bladeRotation += _playableBehaviour.dofControlProps.bladeRotation * invWeight;
               

                            }
                        }

                    }
                    else
                    {
                        SelectSingleCamera(playableBehaviour,inputWeight,currentTime,_playableBehaviour);
                        m_TrackBinding.cameraB = _playableBehaviour.camera;
                        // wigglerRange = new Vector2(
                        //     Math.Max(playableBehaviour.wiggleRange.x, playableBehaviour.wiggleRange.y),
                        //     Math.Max(playableBehaviour.wiggleRange.x, playableBehaviour.wiggleRange.y)
                        // );
                        // wiggler = CalcNoise(playableBehaviour, playableBehaviour,currentTime);
                    }
                   
                }
                else
                {
                    SelectSingleCamera(playableBehaviour,inputWeight,currentTime);
                    m_TrackBinding.cameraB = playableBehaviour.camera;
                    // wigglerRange = new Vector2(
                    //     Math.Max(playableBehaviour.wiggleRange.x, playableBehaviour.wiggleRange.y),
                    //     Math.Max(playableBehaviour.wiggleRange.x, playableBehaviour.wiggleRange.y)
                    // );
                    // wiggler = CalcNoise(playableBehaviour, playableBehaviour,currentTime);
                }

                break;
                
            }
            inputPort++;
        }
        

        if (dof)
        {
            if (m_TrackBinding.dofControl)
            {
               
                if (currentClips.Count > 0)
                {
                    
                    dof.focusDistance.value = focusDistance;
                    dof.focalLength.value = focalLength;
                    dof.aperture.value = aperture;
                    dof.bladeCount.value = bladeCount;
                    dof.bladeCurvature.value = bladeCurvature;
                    dof.bladeRotation.value = bladeRotation;
                
                }
                else
                {
                    dof.focusDistance.value = m_TrackBinding.baseDofValues.focusDistance;
                    dof.focalLength.value = m_TrackBinding.baseDofValues.focalLength;
                    dof.aperture.value = m_TrackBinding.baseDofValues.aperture;
                    dof.bladeCount.value = m_TrackBinding.baseDofValues.bladeCount;
                    dof.bladeCurvature.value = m_TrackBinding.baseDofValues.bladeCurvature;
                    dof.bladeRotation.value = m_TrackBinding.baseDofValues.bladeRotation;
                }
            }
        }
        
        // m_TrackBinding.Render();
        // m_TrackBinding.ReleaseRenderTarget();
        // if(dof)
        
        
    }
  
    private void SelectSingleCamera(CameraSwitcherControlBehaviour playableBehaviour,float inputWeight,float currentTime, CameraSwitcherControlBehaviour nextPlayableBehaviour = null)
    {
        
        
        m_TrackBinding.material.SetTexture("_TextureA",m_TrackBinding.renderTextureA);
        m_TrackBinding.material.SetTexture("_TextureB",m_TrackBinding.renderTextureA);
        m_TrackBinding.material.SetFloat("_CrossFade", inputWeight);
       
 
        if (nextPlayableBehaviour != null)
        {
            var nextInputWeight = Mathf.Clamp(1f - inputWeight,0,1);
            var wiggleRange = playableBehaviour.wiggleRange * inputWeight + nextPlayableBehaviour.wiggleRange * nextInputWeight;
            var noise = CalcNoise(playableBehaviour, nextPlayableBehaviour, currentTime, inputWeight);
            var color =playableBehaviour.multiplyColor*inputWeight+nextPlayableBehaviour.multiplyColor* nextInputWeight;
            // Debug.Log(nextInputWeight);

            m_TrackBinding.material.SetVector("_WigglerValueA", noise);
            m_TrackBinding.material.SetVector("_ClipSizeA", wiggleRange/100f);
            m_TrackBinding.material.SetColor("_MultiplyColorA", color);
            m_TrackBinding.material.SetVector("_WigglerValueB", noise);
            m_TrackBinding.material.SetVector("_ClipSizeB", wiggleRange/100f);
            m_TrackBinding.material.SetColor("_MultiplyColorB", color);

        }
        else
        {
            
            m_TrackBinding.material.SetVector("_WigglerValueA", CalcNoise(playableBehaviour,currentTime));
            m_TrackBinding.material.SetVector("_ClipSizeA", playableBehaviour.wiggleRange/100f);
            m_TrackBinding.material.SetColor("_MultiplyColorA", playableBehaviour.multiplyColor);
            m_TrackBinding.material.SetVector("_WigglerValueB", CalcNoise(playableBehaviour,currentTime));
            m_TrackBinding.material.SetVector("_ClipSizeB", playableBehaviour.wiggleRange/100f);
            m_TrackBinding.material.SetColor("_MultiplyColorB", playableBehaviour.multiplyColor);

        }
       
        // m_TrackBinding.material.SetVector("_OffsetPositionB",offsetPositionA);
        
    }


    // public Vector4 CalcNoise(CameraSwitcherControlBehaviour a, CameraSwitcherControlBehaviour b, float currentTime)
    // {
    //     var isWiggle = new Vector2(
    //         a.wiggle ? 1 : 0,
    //         b.wiggle ? 1 : 0
    //     );
    //     var wiggler = new Vector4(
    //         (Mathf.PerlinNoise(a.noiseSeed.x * a.noiseScale.x, currentTime*a.roughness + a.noiseScale.y*a.noiseSeed.y)-0.5f) * a.wiggleRange.x/100f*isWiggle.x,
    //         (Mathf.PerlinNoise(a.noiseSeed.x * a.noiseScale.x + currentTime*a.roughness, a.noiseScale.y*a.noiseSeed.y)-0.5f) * a.wiggleRange.y/100f*isWiggle.x,
    //         (Mathf.PerlinNoise(b.noiseSeed.x * b.noiseScale.x, currentTime*b.roughness + b.noiseScale.y*b.noiseSeed.y)-0.5f) * b.wiggleRange.x/100f*isWiggle.y,
    //         (Mathf.PerlinNoise(b.noiseSeed.x * b.noiseScale.x + currentTime*b.roughness, b.noiseScale.y*b.noiseSeed.y)-0.5f) * b.wiggleRange.y/100f*isWiggle.y
    //     );
    //
    //     return wiggler;
    // }
    
    
    public Vector2 CalcNoise(CameraSwitcherControlBehaviour a, float currentTime)
    {

        var isWiggle = a.wiggle ? 1 : 0f;
        var wiggler = new Vector2(
            (Mathf.PerlinNoise(a.noiseSeed.x * a.noiseScale.x, currentTime*a.roughness + a.noiseScale.y*a.noiseSeed.y)-0.5f) * a.wiggleRange.x/100f*isWiggle,
            (Mathf.PerlinNoise(a.noiseSeed.x * a.noiseScale.x + currentTime*a.roughness, a.noiseScale.y*a.noiseSeed.y)-0.5f) * a.wiggleRange.y/100f*isWiggle
        );

        return wiggler;
    }

    public Vector2 CalcNoise(CameraSwitcherControlBehaviour a, CameraSwitcherControlBehaviour b, float currentTime,float inputWeight)
    {
        var wiggler =Vector2.Lerp(CalcNoise(a, currentTime), CalcNoise(b, currentTime), 1f-inputWeight);

        return wiggler;
    }

    public override void OnPlayableDestroy(Playable playable)
    {
        base.OnPlayableDestroy(playable);
        if (m_TrackBinding != null) m_TrackBinding.material.SetFloat("_PlayableDirectorTime", 0f);
    }
    
}
