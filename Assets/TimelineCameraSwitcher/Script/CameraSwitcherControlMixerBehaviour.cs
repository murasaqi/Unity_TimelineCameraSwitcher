
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Playables;

#if USE_URP
using UnityEngine.Rendering.Universal;
#endif

#if USE_HDRP
using UnityEngine.Rendering.HighDefinition;
#endif
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


    // private DepthOfField dof;
    
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
#if USE_URP
        var focusDistance = 0f;
        var focalLength = 0f;
        var aperture = 0f;
        var bladeCount = 0;
        var bladeCurvature = 0f;
        var bladeRotation = 0f;

#elif USE_HDRP
        var physicalCameraProps = new PhysicalCameraProps();
        physicalCameraProps.Reset();
        var manualRangeProps = new ManualRangeProps();
        manualRangeProps.Reset();
        // var focusDistance = 0f;
#endif


        var currentClips = new List<CameraSwitcherControlBehaviour>();
        // var currentDirectorTime = m_Director.time - offsetStartTime;
        foreach (TimelineClip clip in m_Clips)
        {
            
            var inputWeight = playable.GetInputWeight(inputPort);
           
            var scriptPlayable =  (ScriptPlayable<CameraSwitcherControlBehaviour>)playable.GetInput(inputPort);
            var playableBehaviour = scriptPlayable.GetBehaviour();


           
            // if (playableBehaviour.volumeProfile != null)
            // {
            //     cameraSwitcherControlClip.mode = m_TrackBinding.depthOfFieldMode;
            // }

           var cameraSwitcherControlClip = clip.asset as CameraSwitcherControlClip;
           if (inputWeight > 0)
           {
               
               

               if (playableBehaviour.dofOverride)
               {
                  
#if USE_URP
                    DepthOfField dof;
                   if (m_TrackBinding.volume != null)
                   {
                       m_TrackBinding.volume.TryGet<DepthOfField>(out dof);
                   }

                   // depthOfFieldMode = playableBehaviour.depthOfFieldMode;
                   focusDistance += playableBehaviour.bokehProps.focusDistance*inputWeight;
                   focalLength += playableBehaviour.bokehProps.focalLength*inputWeight;
                   aperture += playableBehaviour.bokehProps.aperture * inputWeight;
                   bladeCount += Mathf.FloorToInt(playableBehaviour.bokehProps.bladeCount*inputWeight);
                   bladeCurvature += playableBehaviour.bokehProps.bladeCurvature * inputWeight;
                   bladeRotation += playableBehaviour.bokehProps.bladeRotation * inputWeight;
                       
#elif USE_HDRP

                   DepthOfField dof;
                   if (playableBehaviour.volumeProfile != null)
                   {
                       playableBehaviour.volumeProfile.TryGet<DepthOfField>(out dof);
                       Debug.Log(dof);
                       if (dof != null)
                       {
                           
                           if (playableBehaviour.physicalCameraProps.focusDistanceMode == FocusDistanceMode.Camera &&
                               dof.focusMode == DepthOfFieldMode.UsePhysicalCamera)
                           {
                               // var hdAdditionalCameraData = playableBehaviour.camera.GetComponents<HDAdditionalCameraData>();
                               playableBehaviour.hdAdditionalCameraData.physicalParameters.focusDistance =
                                   playableBehaviour.physicalCameraProps.focusDistance;
                           }

                           physicalCameraProps.focusDistanceMode =
                               playableBehaviour.physicalCameraProps.focusDistanceMode;
                           physicalCameraProps.focusDistance +=
                               playableBehaviour.physicalCameraProps.focusDistance * inputWeight;
                           physicalCameraProps.nearBluer.sampleCount +=
                               Mathf.FloorToInt(playableBehaviour.physicalCameraProps.nearBluer.sampleCount *
                                                inputWeight);
                           physicalCameraProps.nearBluer.maxRadius +=
                               Mathf.FloorToInt(playableBehaviour.physicalCameraProps.nearBluer.maxRadius *
                                                inputWeight);

                           physicalCameraProps.farBluer.sampleCount +=
                               Mathf.FloorToInt(
                                   playableBehaviour.physicalCameraProps.farBluer.sampleCount * inputWeight);
                           physicalCameraProps.farBluer.maxRadius +=
                               Mathf.FloorToInt(playableBehaviour.physicalCameraProps.farBluer.maxRadius * inputWeight);


                           manualRangeProps.nearRange.start +=
                               playableBehaviour.manualRangeProps.nearRange.start * inputWeight;
                           manualRangeProps.nearRange.end +=
                               playableBehaviour.manualRangeProps.nearRange.end * inputWeight;

                           manualRangeProps.farRange.start +=
                               playableBehaviour.manualRangeProps.farRange.start * inputWeight;
                           manualRangeProps.farRange.end +=
                               playableBehaviour.manualRangeProps.farRange.end * inputWeight;
                           manualRangeProps.quality = playableBehaviour.manualRangeProps.quality;

                           manualRangeProps.nearBluer.sampleCount +=
                               Mathf.FloorToInt(playableBehaviour.manualRangeProps.nearBluer.sampleCount * inputWeight);
                           manualRangeProps.nearBluer.maxRadius +=
                               Mathf.FloorToInt(playableBehaviour.manualRangeProps.nearBluer.maxRadius * inputWeight);

                           manualRangeProps.farBluer.sampleCount +=
                               Mathf.FloorToInt(playableBehaviour.manualRangeProps.farBluer.sampleCount * inputWeight);
                           manualRangeProps.farBluer.maxRadius +=
                               Mathf.FloorToInt(playableBehaviour.manualRangeProps.farBluer.maxRadius * inputWeight);
                       }
                   }

#endif
                   currentClips.Add(playableBehaviour);     
               }

              
               if (playableBehaviour.lookAt)
               {
                   var lookAt = playableBehaviour.camera.GetComponent<LookAtConstraint>();
                   
                   if (lookAt == null) lookAt = playableBehaviour.camera.gameObject.AddComponent<LookAtConstraint>();
                   var target = playableBehaviour.lookAtProps.target.Resolve(playable.GetGraph().GetResolver());
                   if (lookAt && target)
                   {

                       

                       if (lookAt.sourceCount > 0 )
                       {
                           while (lookAt.sourceCount > 1)
                           {
                               lookAt.RemoveSource(0);
                           }

                           if (lookAt.GetSource(0).sourceTransform != target)
                           {
                               // lookAt.RemoveSource(0);
                               var source = new ConstraintSource();
                               source.sourceTransform = target;
                               source.weight = 1;
                               lookAt.SetSource(0,source);
                           }
                           
                          
                       }
                       else
                       {
                           var source = new ConstraintSource();
                           source.sourceTransform = target;
                           source.weight = 1;
                           lookAt.AddSource(source);
                       }

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
                            m_TrackBinding.material.SetVector("_ClipSizeA", playableBehaviour.wigglerProps.wiggleRange/ 100f);
                            m_TrackBinding.material.SetColor("_MultiplyColorA", playableBehaviour.colorBlendProps.color);
                            m_TrackBinding.material.SetVector("_ClipSizeB", _playableBehaviour.wigglerProps.wiggleRange / 100f);
                            m_TrackBinding.material.SetVector("_WigglerValueB", CalcNoise(_playableBehaviour,currentTime));
                            m_TrackBinding.material.SetColor("_MultiplyColorB", playableBehaviour.colorBlendProps.color);
                            var invWeight = 1f - inputWeight;
                            // if (playableBehaviour.fadeCurveOverride)
                            // {
                            //     m_TrackBinding.material.SetFloat("_CrossFade", 1f - playableBehaviour.fadeCurve.Evaluate(inputWeight));
                            // }
                            // else
                            // {
                                m_TrackBinding.material.SetFloat("_CrossFade", 1f - inputWeight);
                            // }
#if USE_HDRP
                            // var cameraSwitcherControlClip = clip.asset as CameraSwitcherControlClip;
                            var cameraSwitcherControlNextClip = nextClip.asset as CameraSwitcherControlClip;
                            SetVolumeValues(_playableBehaviour, _playableBehaviour.manualRangeProps, _playableBehaviour.physicalCameraProps);
                            if(_playableBehaviour.dofOverride)cameraSwitcherControlNextClip.volume.profile = _playableBehaviour.volumeProfile;
                            SetVolumeValues(playableBehaviour,playableBehaviour.manualRangeProps,playableBehaviour.physicalCameraProps);
                            if (playableBehaviour.dofOverride)
                                cameraSwitcherControlClip.volume.profile = playableBehaviour.volumeProfile;
                            // if(_playableBehaviour.physicalCameraProps.focusDistanceMode == FocusDistanceMode.Camera && dof.focusMode == DepthOfFieldMode.UsePhysicalCamera)
                            // {
                            //     _playableBehaviour.hdAdditionalCameraData.physicalParameters.focusDistance = _playableBehaviour.physicalCameraProps.focusDistance;
                            // }
                            //     physicalCameraProps.focusDistanceMode = _playableBehaviour.physicalCameraProps.focusDistanceMode;
                            //    physicalCameraProps.focusDistance +=
                            //        _playableBehaviour.physicalCameraProps.focusDistance * invWeight;
                            //    physicalCameraProps.nearBluer.sampleCount +=
                            //        Mathf.FloorToInt(_playableBehaviour.physicalCameraProps.nearBluer.sampleCount * invWeight);
                            //    physicalCameraProps.nearBluer.maxRadius +=
                            //        Mathf.FloorToInt( _playableBehaviour.physicalCameraProps.nearBluer.maxRadius * invWeight);
                            //    
                            //    physicalCameraProps.farBluer.sampleCount +=
                            //        Mathf.FloorToInt(_playableBehaviour.physicalCameraProps.farBluer.sampleCount * invWeight);
                            //    physicalCameraProps.farBluer.maxRadius +=
                            //        Mathf.FloorToInt( _playableBehaviour.physicalCameraProps.farBluer.maxRadius * invWeight);
                            //
                            //
                            //    
                            //    manualRangeProps.nearRange.start += _playableBehaviour.manualRangeProps.nearRange.start * invWeight;
                            //    manualRangeProps.nearRange.end += _playableBehaviour.manualRangeProps.nearRange.end * invWeight;
                            //    
                            //    manualRangeProps.farRange.start += _playableBehaviour.manualRangeProps.farRange.start * invWeight;
                            //    manualRangeProps.farRange.end += _playableBehaviour.manualRangeProps.farRange.end * invWeight;
                            //    manualRangeProps.quality = _playableBehaviour.manualRangeProps.quality;
                            //    
                            //    manualRangeProps.nearBluer.sampleCount +=
                            //        Mathf.FloorToInt(_playableBehaviour.manualRangeProps.nearBluer.sampleCount * invWeight);
                            //    manualRangeProps.nearBluer.maxRadius +=
                            //        Mathf.FloorToInt( _playableBehaviour.manualRangeProps.nearBluer.maxRadius * invWeight);
                            //    
                            //    manualRangeProps.farBluer.sampleCount +=
                            //        Mathf.FloorToInt(_playableBehaviour.manualRangeProps.farBluer.sampleCount * invWeight);
                            //    manualRangeProps.farBluer.maxRadius +=
                            //        Mathf.FloorToInt( _playableBehaviour.manualRangeProps.farBluer.maxRadius * invWeight);
#endif


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

#if USE_URP

                                // depthOfFieldMode = _playableBehaviour.bokehProps.depthOfFieldMode;
                                focusDistance += _playableBehaviour.bokehProps.focusDistance*invWeight;
                                focalLength += _playableBehaviour.bokehProps.focalLength*invWeight;
                                aperture += _playableBehaviour.bokehProps.aperture * invWeight;
                                bladeCount += Mathf.FloorToInt(_playableBehaviour.bokehProps.bladeCount*invWeight);
                                bladeCurvature += _playableBehaviour.bokehProps.bladeCurvature * invWeight;
                                bladeRotation += _playableBehaviour.bokehProps.bladeRotation * invWeight;

#elif USE_HDRP
                                
                                DepthOfField dof;
                                if (_playableBehaviour.volumeProfile != null)
                                {
                                    _playableBehaviour.volumeProfile.TryGet<DepthOfField>(out dof);


                                    physicalCameraProps.focusDistanceMode =
                                        _playableBehaviour.physicalCameraProps.focusDistanceMode;
                                    physicalCameraProps.focusDistance +=
                                        _playableBehaviour.physicalCameraProps.focusDistance * invWeight;
                                    physicalCameraProps.nearBluer.sampleCount +=
                                        Mathf.FloorToInt(_playableBehaviour.physicalCameraProps.nearBluer.sampleCount *
                                                         invWeight);
                                    physicalCameraProps.nearBluer.maxRadius +=
                                        Mathf.FloorToInt(_playableBehaviour.physicalCameraProps.nearBluer.maxRadius *
                                                         invWeight);

                                    physicalCameraProps.farBluer.sampleCount +=
                                        Mathf.FloorToInt(_playableBehaviour.physicalCameraProps.farBluer.sampleCount *
                                                         invWeight);
                                    physicalCameraProps.farBluer.maxRadius +=
                                        Mathf.FloorToInt(_playableBehaviour.physicalCameraProps.farBluer.maxRadius *
                                                         invWeight);


                                    manualRangeProps.nearRange.start +=
                                        _playableBehaviour.manualRangeProps.nearRange.start * invWeight;
                                    manualRangeProps.nearRange.end +=
                                        _playableBehaviour.manualRangeProps.nearRange.end * invWeight;

                                    manualRangeProps.farRange.start +=
                                        _playableBehaviour.manualRangeProps.farRange.start * invWeight;
                                    manualRangeProps.farRange.end +=
                                        _playableBehaviour.manualRangeProps.farRange.end * invWeight;
                                    manualRangeProps.quality = _playableBehaviour.manualRangeProps.quality;

                                    manualRangeProps.nearBluer.sampleCount +=
                                        Mathf.FloorToInt(_playableBehaviour.manualRangeProps.nearBluer.sampleCount *
                                                         invWeight);
                                    manualRangeProps.nearBluer.maxRadius +=
                                        Mathf.FloorToInt(_playableBehaviour.manualRangeProps.nearBluer.maxRadius *
                                                         invWeight);

                                    manualRangeProps.farBluer.sampleCount +=
                                        Mathf.FloorToInt(_playableBehaviour.manualRangeProps.farBluer.sampleCount *
                                                         invWeight);
                                    manualRangeProps.farBluer.maxRadius +=
                                        Mathf.FloorToInt(_playableBehaviour.manualRangeProps.farBluer.maxRadius *
                                                         invWeight);

                                    if (_playableBehaviour.physicalCameraProps.focusDistanceMode ==
                                        FocusDistanceMode.Camera && dof.focusMode == DepthOfFieldMode.UsePhysicalCamera)
                                    {
                                        _playableBehaviour.hdAdditionalCameraData.physicalParameters.focusDistance =
                                            Mathf.Lerp(playableBehaviour.physicalCameraProps.focusDistance,
                                                _playableBehaviour.physicalCameraProps.focusDistance, invWeight);
                                    }
                                }
                                
                                SetVolumeValues(_playableBehaviour,manualRangeProps,physicalCameraProps);
#endif               

                            }
                        }

                    }
                    else
                    {
                        SelectSingleCamera(playableBehaviour,inputWeight,currentTime,_playableBehaviour);
                        SetVolumeValues(playableBehaviour,playableBehaviour.manualRangeProps,playableBehaviour.physicalCameraProps);
                        if(playableBehaviour.dofOverride)cameraSwitcherControlClip.volume.profile = playableBehaviour.volumeProfile;
                        m_TrackBinding.cameraB = _playableBehaviour.camera;
                    }
                   
                }
                else
                {
                    SelectSingleCamera(playableBehaviour,inputWeight,currentTime);
                    m_TrackBinding.cameraB = playableBehaviour.camera;
                }

                break;
                
            }
            inputPort++;
        }
        
//
//         if (dof)
//         {
//            
//             if (m_TrackBinding.dofControl)
//             {
//                     
// #if USE_URP               
//                 if (currentClips.Count > 0)
//                 {
//                     
//
//
//                     if (dof.mode == DepthOfFieldMode.Bokeh)
//                     {
//                         dof.focusDistance.value = focusDistance;
//                         dof.focalLength.value = focalLength;
//                         dof.aperture.value = aperture;
//                         dof.bladeCount.value = bladeCount;
//                         dof.bladeCurvature.value = bladeCurvature;
//                         dof.bladeRotation.value = bladeRotation;      
//                     }
//
//                 }
//                 else
//                 {
//                     
//                     if (dof.mode == DepthOfFieldMode.Bokeh)
//                     {
//                         dof.focusDistance.value = m_TrackBinding.bokehBaseValues.focusDistance;
//                         dof.focalLength.value = m_TrackBinding.bokehBaseValues.focalLength;
//                         dof.aperture.value = m_TrackBinding.bokehBaseValues.aperture;
//                         dof.bladeCount.value = m_TrackBinding.bokehBaseValues.bladeCount;
//                         dof.bladeCurvature.value = m_TrackBinding.bokehBaseValues.bladeCurvature;
//                         dof.bladeRotation.value = m_TrackBinding.bokehBaseValues.bladeRotation;
//                     }
//
//                 }
// #elif USE_HDRP
//                 
//                
// #endif   
//             }
//         }
//         
        // m_TrackBinding.Render();
        // m_TrackBinding.ReleaseRenderTarget();
        // if(dof)
        
        
    }


    private void SetVolumeValues(CameraSwitcherControlBehaviour behaviour, ManualRangeProps manualRangeProps, PhysicalCameraProps physicalCameraProps)
    {
        
        if (behaviour.volumeProfile != null)
        {
            DepthOfField dof;
            behaviour.volumeProfile.TryGet<DepthOfField>(out dof);
            if (behaviour.dofOverride)
            {
                
                if(behaviour.physicalCameraProps.focusDistanceMode == FocusDistanceMode.Camera && dof.focusMode == DepthOfFieldMode.UsePhysicalCamera)
                {
                    behaviour.hdAdditionalCameraData.physicalParameters.focusDistance = behaviour.physicalCameraProps.focusDistance;
                }

                if (dof.focusMode == DepthOfFieldMode.UsePhysicalCamera)
                {
                    dof.focusDistanceMode.value = physicalCameraProps.focusDistanceMode;
                    dof.focusDistance.value = physicalCameraProps.focusDistance;
                    dof.quality.value = (int) physicalCameraProps.quality;
                    dof.nearMaxBlur = physicalCameraProps.nearBluer.maxRadius;
                    dof.nearSampleCount = physicalCameraProps.nearBluer.sampleCount;
                    dof.farMaxBlur = physicalCameraProps.farBluer.maxRadius;
                    dof.farSampleCount = physicalCameraProps.farBluer.sampleCount;



                }

                if (dof.focusMode == DepthOfFieldMode.Manual)
                {
                    dof.nearFocusStart.value = manualRangeProps.nearRange.start;
                    dof.nearFocusEnd.value = manualRangeProps.nearRange.end;
                    dof.farFocusStart.value = manualRangeProps.farRange.start;
                    dof.farFocusEnd.value = manualRangeProps.farRange.end;
                    dof.nearMaxBlur = manualRangeProps.nearBluer.maxRadius;
                    dof.nearSampleCount = manualRangeProps.nearBluer.sampleCount;
                    dof.farMaxBlur = manualRangeProps.farBluer.maxRadius;
                    dof.farSampleCount = manualRangeProps.farBluer.sampleCount;
                    dof.quality.value = (int) manualRangeProps.quality;
                }
            }

            else
            {
                
                if(m_TrackBinding.physicalCameraBaseValues.focusDistanceMode == FocusDistanceMode.Camera && dof.focusMode == DepthOfFieldMode.UsePhysicalCamera)
                {
                    behaviour.hdAdditionalCameraData.physicalParameters.focusDistance = m_TrackBinding.physicalCameraBaseValues.focusDistance;
                }
                if (dof.focusMode == DepthOfFieldMode.UsePhysicalCamera)
                {
                    dof.focusDistanceMode.value = m_TrackBinding.physicalCameraBaseValues.focusDistanceMode;
                    dof.focusDistance.value = m_TrackBinding.physicalCameraBaseValues.focusDistance;
                    dof.quality.value = (int) m_TrackBinding.physicalCameraBaseValues.quality;
                    dof.nearMaxBlur = m_TrackBinding.physicalCameraBaseValues.nearBluer.maxRadius;
                    dof.nearSampleCount = m_TrackBinding.physicalCameraBaseValues.nearBluer.sampleCount;
                    dof.farMaxBlur = m_TrackBinding.physicalCameraBaseValues.farBluer.maxRadius;
                    dof.farSampleCount = m_TrackBinding.physicalCameraBaseValues.farBluer.sampleCount;
                }

                if (dof.focusMode == DepthOfFieldMode.Manual)
                {
                    dof.nearFocusStart.value = m_TrackBinding.manualRangeBaseValues.nearRange.start;
                    dof.nearFocusEnd.value = m_TrackBinding.manualRangeBaseValues.nearRange.end;
                    dof.farFocusStart.value = m_TrackBinding.manualRangeBaseValues.farRange.start;
                    dof.farFocusEnd.value = m_TrackBinding.manualRangeBaseValues.farRange.end;
                    dof.nearMaxBlur = m_TrackBinding.manualRangeBaseValues.nearBluer.maxRadius;
                    dof.nearSampleCount = m_TrackBinding.manualRangeBaseValues.nearBluer.sampleCount;
                    dof.farMaxBlur = m_TrackBinding.manualRangeBaseValues.farBluer.maxRadius;
                    dof.farSampleCount = m_TrackBinding.manualRangeBaseValues.farBluer.sampleCount;
                    dof.quality.value = (int) m_TrackBinding.manualRangeBaseValues.quality;
                }

            }
        }
    }
    private void SelectSingleCamera(CameraSwitcherControlBehaviour playableBehaviour,float inputWeight,float currentTime, CameraSwitcherControlBehaviour nextPlayableBehaviour = null)
    {
        
        
        m_TrackBinding.material.SetTexture("_TextureA",m_TrackBinding.renderTextureA);
        m_TrackBinding.material.SetTexture("_TextureB",m_TrackBinding.renderTextureA);
        m_TrackBinding.material.SetFloat("_CrossFade", inputWeight);
       
        var blendNumA = playableBehaviour.colorBlend ? 1 : 0;
        blendNumA += blendNumA == 0 ? 0:(int) playableBehaviour.colorBlendProps.blendMode;
        
        if (nextPlayableBehaviour != null)
        {
            m_TrackBinding.material.SetFloat("_CrossFade", 1f-inputWeight);
            var nextInputWeight = Mathf.Clamp(1f - inputWeight,0,1);
            var wiggleRange = playableBehaviour.wigglerProps.wiggleRange * inputWeight + nextPlayableBehaviour.wigglerProps.wiggleRange * nextInputWeight;
            var noise = CalcNoise(playableBehaviour, nextPlayableBehaviour, currentTime, inputWeight);
            // var color =playableBehaviour.colorBlendProps.color*inputWeight+nextPlayableBehaviour.colorBlendProps.color* nextInputWeight;
            // Debug.Log(nextInputWeight);

            
            
            var blendNumB = nextPlayableBehaviour.colorBlend ? 1 : 0;
            // Debug.Log((int) nextPlayableBehaviour.colorBlendProps.blendMode);
            blendNumB += blendNumB==0? 0 : (int) nextPlayableBehaviour.colorBlendProps.blendMode;
            m_TrackBinding.material.SetVector("_WigglerValueA", noise);
            m_TrackBinding.material.SetVector("_ClipSizeA", wiggleRange/100f);
            m_TrackBinding.material.SetColor("_MultiplyColorA", playableBehaviour.colorBlendProps.color);
            m_TrackBinding.material.SetInt("_BlendA", blendNumA);
            m_TrackBinding.material.SetVector("_WigglerValueB", noise);
            m_TrackBinding.material.SetVector("_ClipSizeB", wiggleRange/100f);
            m_TrackBinding.material.SetColor("_MultiplyColorB", nextPlayableBehaviour.colorBlendProps.color);
            m_TrackBinding.material.SetInt("_BlendB", blendNumB);

        }
        else
        {
            
            m_TrackBinding.material.SetVector("_WigglerValueA", CalcNoise(playableBehaviour,currentTime));
            m_TrackBinding.material.SetVector("_ClipSizeA", playableBehaviour.wigglerProps.wiggleRange/100f);
            m_TrackBinding.material.SetColor("_MultiplyColorA", playableBehaviour.colorBlendProps.color);
            m_TrackBinding.material.SetInt("_BlendA", blendNumA);
            m_TrackBinding.material.SetVector("_WigglerValueB", CalcNoise(playableBehaviour,currentTime));
            m_TrackBinding.material.SetVector("_ClipSizeB", playableBehaviour.wigglerProps.wiggleRange/100f);
            m_TrackBinding.material.SetColor("_MultiplyColorB", playableBehaviour.colorBlendProps.color);
            m_TrackBinding.material.SetInt("_BlendB", blendNumA);

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
            (Mathf.PerlinNoise(a.wigglerProps.noiseSeed.x * a.wigglerProps.noiseScale.x, currentTime*a.wigglerProps.roughness + a.wigglerProps.noiseScale.y*a.wigglerProps.noiseSeed.y)-0.5f) * a.wigglerProps.wiggleRange.x/100f*isWiggle,
            (Mathf.PerlinNoise(a.wigglerProps.noiseSeed.x * a.wigglerProps.noiseScale.x + currentTime*a.wigglerProps.roughness, a.wigglerProps.noiseScale.y*a.wigglerProps.noiseSeed.y)-0.5f) * a.wigglerProps.wiggleRange.y/100f*isWiggle
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
