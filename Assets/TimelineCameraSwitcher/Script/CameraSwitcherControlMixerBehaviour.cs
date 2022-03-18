
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
       
        Camera prevCamera = null;
        foreach (TimelineClip clip in m_Clips)
        {
            var scriptPlayable =  (ScriptPlayable<CameraSwitcherControlBehaviour>)playable.GetInput(i);
            var playableBehaviour = scriptPlayable.GetBehaviour();
            if (playableBehaviour.camera != null)
            {

               
                prevCamera = playableBehaviour.camera;
                
                _cameras.Add(playableBehaviour.camera);
                playableBehaviour.camera.enabled = false;
                var c = clip.asset as CameraSwitcherControlClip;
                c.volume.enabled = false;
            }
            i++;
        }
        i = 0;
        
        foreach (TimelineClip clip in m_Clips)
        {
            var scriptPlayable =  (ScriptPlayable<CameraSwitcherControlBehaviour>)playable.GetInput(i);
            var playableBehaviour = scriptPlayable.GetBehaviour();
            if (i == 0)
            {
                playableBehaviour.camera.gameObject.layer = m_TrackBinding.cameraALayer;
                playableBehaviour.hdAdditionalCameraData.volumeLayerMask = Add(
                    playableBehaviour.hdAdditionalCameraData.volumeLayerMask, m_TrackBinding.cameraALayer);
                playableBehaviour.hdAdditionalCameraData.volumeLayerMask = Remove(
                    playableBehaviour.hdAdditionalCameraData.volumeLayerMask, m_TrackBinding.cameraBLayer);
            }
            else
            {
                if (clip.start <= m_Director.time && m_Director.time < clip.start + clip.duration)
                {
                    if (i - 1 > 0)
                    {
                        
                        var prevScriptPlayable =  (ScriptPlayable<CameraSwitcherControlBehaviour>)playable.GetInput(i-1);
                        var prevPlayableBehaviour = prevScriptPlayable.GetBehaviour();

                       
                        var isA = !prevPlayableBehaviour.camera.gameObject.layer.Equals(m_TrackBinding.cameraALayer);
                        // Debug.Log($"{playableBehaviour.camera.name} isA: {isA}");
                        if(prevPlayableBehaviour.camera == playableBehaviour.camera) isA = prevPlayableBehaviour.camera.gameObject.layer.Equals(m_TrackBinding.cameraALayer);
                        playableBehaviour.camera.gameObject.layer =
                            isA ? m_TrackBinding.cameraALayer : m_TrackBinding.cameraBLayer;
                        playableBehaviour.hdAdditionalCameraData.volumeLayerMask = Add(
                            playableBehaviour.hdAdditionalCameraData.volumeLayerMask,isA ? m_TrackBinding.cameraALayer : m_TrackBinding.cameraBLayer);
                        playableBehaviour.hdAdditionalCameraData.volumeLayerMask = Remove(
                            playableBehaviour.hdAdditionalCameraData.volumeLayerMask, isA ? m_TrackBinding.cameraBLayer : m_TrackBinding.cameraALayer);
                    }
                    if (i+1 < m_Clips.Count)
                    {
                        var nextScriptPlayable =  (ScriptPlayable<CameraSwitcherControlBehaviour>)playable.GetInput(i+1);
                        var nextPlayableBehaviour = nextScriptPlayable.GetBehaviour();

                        var isA = !playableBehaviour.camera.gameObject.layer.Equals(m_TrackBinding.cameraALayer);
                        if (nextPlayableBehaviour.camera == playableBehaviour.camera)isA = playableBehaviour.camera.gameObject.layer.Equals(m_TrackBinding.cameraALayer);
                        // Debug.Log($"{nextPlayableBehaviour.camera.name} isA: {isA}");
                        if (isA)
                        {
                            nextPlayableBehaviour.camera.gameObject.layer = m_TrackBinding.cameraALayer;
                            nextPlayableBehaviour.hdAdditionalCameraData.volumeLayerMask = Add(
                                nextPlayableBehaviour.hdAdditionalCameraData.volumeLayerMask, m_TrackBinding.cameraALayer);
                            nextPlayableBehaviour.hdAdditionalCameraData.volumeLayerMask = Remove(
                                nextPlayableBehaviour.hdAdditionalCameraData.volumeLayerMask, m_TrackBinding.cameraBLayer);
                        }
                        else
                        {
                            nextPlayableBehaviour.camera.gameObject.layer = m_TrackBinding.cameraBLayer;
                            nextPlayableBehaviour.hdAdditionalCameraData.volumeLayerMask = Add(
                                nextPlayableBehaviour.hdAdditionalCameraData.volumeLayerMask, m_TrackBinding.cameraBLayer);
                            nextPlayableBehaviour.hdAdditionalCameraData.volumeLayerMask = Remove(
                                nextPlayableBehaviour.hdAdditionalCameraData.volumeLayerMask, m_TrackBinding.cameraALayer);
                            
                            
                        }

                        // } 
                       
                    }
                    break;

                }
     
            }
           
            i++;
        }

#if UNITY_EDITOR
    

#endif
        
        int inputPort = 0;
        var currentTime = (float)m_Director.time;
        var wiggler = Vector4.zero;
        var wigglerRange = Vector2.zero;
#if USE_URP
        
        // var focusDistance = 0f;
        // var focalLength = 0f;
        // var aperture = 0f;
        // var bladeCount = 0;
        // var bladeCurvature = 0f;
        // var bladeRotation = 0f;
        var bokehProps = new BokehProp();
        bokehProps.Reset();
        var gaussianProps = new GaussianProp();
        gaussianProps.Reset();

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


           
         
            

           var cameraSwitcherControlClip = clip.asset as CameraSwitcherControlClip;
           if (inputWeight > 0)
           {


               if( cameraSwitcherControlClip.volume != null)cameraSwitcherControlClip.volume.enabled = playableBehaviour.dofOverride;
               if (playableBehaviour.dofOverride)
               {
                   DepthOfField dof;

#if USE_URP
                  

                   // depthOfFieldMode = playableBehaviour.depthOfFieldMode;
                   bokehProps.focusDistance += playableBehaviour.bokehProps.focusDistance*inputWeight;
                   bokehProps.focalLength += playableBehaviour.bokehProps.focalLength*inputWeight;
                   bokehProps.aperture += playableBehaviour.bokehProps.aperture * inputWeight;
                   bokehProps.bladeCount += Mathf.FloorToInt(playableBehaviour.bokehProps.bladeCount*inputWeight);
                   bokehProps.bladeCurvature += playableBehaviour.bokehProps.bladeCurvature * inputWeight;
                   bokehProps.bladeRotation += playableBehaviour.bokehProps.bladeRotation * inputWeight;

                   gaussianProps.start += playableBehaviour.gaussianProps.start * inputWeight;
                   gaussianProps.end += playableBehaviour.gaussianProps.end * inputWeight;
                   gaussianProps.maxRadius += playableBehaviour.gaussianProps.maxRadius * inputWeight;
                   gaussianProps.highQualitySampling = playableBehaviour.gaussianProps.highQualitySampling;
                       
#elif USE_HDRP

                   // DepthOfField dof;
                   if (playableBehaviour.volumeProfile != null)
                   {
                       playableBehaviour.volumeProfile.TryGet<DepthOfField>(out dof);
                       if (dof != null)
                       {
                           
                          
                           if (playableBehaviour.physicalCameraProps.focusDistanceMode == FocusDistanceMode.Camera )
                           {
                               // var hdAdditionalCameraData = playableBehaviour.camera.GetComponents<HDAdditionalCameraData>();
                               playableBehaviour.hdAdditionalCameraData.physicalParameters.focusDistance =
                                   playableBehaviour.physicalCameraProps.focusDistance;
                           }
                           else
                           {
                               dof.focusDistance.value = playableBehaviour.physicalCameraProps.focusDistance;
                           }

                           physicalCameraProps.focusDistanceMode =
                               playableBehaviour.physicalCameraProps.focusDistanceMode;
                           physicalCameraProps.focusDistance +=
                               playableBehaviour.physicalCameraProps.focusDistance * inputWeight;
                           
                           physicalCameraProps.focusLength +=
                               playableBehaviour.physicalCameraProps.focusLength * inputWeight;

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

                           manualRangeProps.focusLength +=
                               playableBehaviour.manualRangeProps.focusLength * inputWeight;
                           manualRangeProps.focusDistance +=
                               playableBehaviour.manualRangeProps.focusDistance * inputWeight;
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

            if (clip.start <= m_Director.time && m_Director.time < clip.start + clip.duration )
            {
                ResetCameraTarget();
                if (playableBehaviour.camera != null)
                {
                    playableBehaviour.camera.enabled = true;
                   
                    playableBehaviour.camera.targetTexture = m_TrackBinding.renderTextureA;
                    m_TrackBinding.cameraA = playableBehaviour.camera;
                    
                

                }


                // if (playableBehaviour.dofOverride)
                // {
                  
                // }
                prevCamera = playableBehaviour.camera;
             
               

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
                            m_TrackBinding.material.SetFloat("_CrossFade", 1f - inputWeight);
                            

#if  USE_URP
                            
                            // bokehProps.focusDistance += _playableBehaviour.bokehProps.focusDistance*invWeight;
                            // bokehProps.focalLength += _playableBehaviour.bokehProps.focalLength*invWeight;
                            // bokehProps.aperture += _playableBehaviour.bokehProps.aperture * invWeight;
                            // bokehProps.bladeCount += Mathf.FloorToInt(_playableBehaviour.bokehProps.bladeCount*invWeight);
                            // bokehProps.bladeCurvature += _playableBehaviour.bokehProps.bladeCurvature * invWeight;
                            // bokehProps.bladeRotation += _playableBehaviour.bokehProps.bladeRotation * invWeight;
                            //
                            // gaussianProps.start += _playableBehaviour.gaussianProps.start * inputWeight;
                            // gaussianProps.end += _playableBehaviour.gaussianProps.end * inputWeight;
                            // gaussianProps.maxRadius += _playableBehaviour.gaussianProps.maxRadius * inputWeight;
                            // gaussianProps.highQualitySampling = _playableBehaviour.gaussianProps.highQualitySampling;
                            
                            
#elif USE_HDRP
                            // var cameraSwitcherControlClip = clip.asset as CameraSwitcherControlClip;
                            var cameraSwitcherControlNextClip = nextClip.asset as CameraSwitcherControlClip;
                            cameraSwitcherControlNextClip.volume.enabled = _playableBehaviour.dofOverride;
                            cameraSwitcherControlClip.volume.enabled = playableBehaviour.dofOverride;

                            CheckVolumeProfile(_playableBehaviour, cameraSwitcherControlNextClip);
                            SetVolumeValues(_playableBehaviour, _playableBehaviour.manualRangeProps, _playableBehaviour.physicalCameraProps);
                            CheckVolumeProfile(playableBehaviour, cameraSwitcherControlClip);
                            SetVolumeValues(playableBehaviour,playableBehaviour.manualRangeProps,playableBehaviour.physicalCameraProps);
                            
#endif


                        }
                        // blending中で、次のカメラが同じときは値もBlendingしてあげる
                        else
                        {
                            // var nextInputWeight = Mathf.Clamp(1f - inputWeight,0f,1f);
                            SelectSingleCamera(playableBehaviour, inputWeight, currentTime,_playableBehaviour);
                            m_TrackBinding.cameraB = _playableBehaviour.camera;
                            var invWeight = 1f - inputWeight;
                            var cameraSwitcherControlNextClip = nextClip.asset as CameraSwitcherControlClip;
                            if (_playableBehaviour.dofOverride)
                            {

#if USE_URP

                                // depthOfFieldMode = _playableBehaviour.bokehProps.depthOfFieldMode;
                                bokehProps.focusDistance += _playableBehaviour.bokehProps.focusDistance*invWeight;
                                bokehProps.focalLength += _playableBehaviour.bokehProps.focalLength*invWeight;
                                bokehProps.aperture += _playableBehaviour.bokehProps.aperture * invWeight;
                                bokehProps.bladeCount += Mathf.FloorToInt(_playableBehaviour.bokehProps.bladeCount*invWeight);
                                bokehProps.bladeCurvature += _playableBehaviour.bokehProps.bladeCurvature * invWeight;
                                bokehProps.bladeRotation += _playableBehaviour.bokehProps.bladeRotation * invWeight;
                         
                                gaussianProps.start += _playableBehaviour.gaussianProps.start * inputWeight;
                                gaussianProps.end += _playableBehaviour.gaussianProps.end * inputWeight;
                                gaussianProps.maxRadius += _playableBehaviour.gaussianProps.maxRadius * inputWeight;
                                gaussianProps.highQualitySampling = _playableBehaviour.gaussianProps.highQualitySampling;
                                CheckVolumeProfile(_playableBehaviour,cameraSwitcherControlNextClip);
                                SetVolumeValues(_playableBehaviour,bokehProps,gaussianProps);

#elif USE_HDRP
                                
                                DepthOfField dof;
                                if (_playableBehaviour.volumeProfile != null)
                                {
                                    _playableBehaviour.volumeProfile.TryGet<DepthOfField>(out dof);


                                    physicalCameraProps.focusDistanceMode =
                                        _playableBehaviour.physicalCameraProps.focusDistanceMode;
                                    physicalCameraProps.focusDistance +=
                                        _playableBehaviour.physicalCameraProps.focusDistance * invWeight;
                                    physicalCameraProps.focusLength +=
                                        _playableBehaviour.physicalCameraProps.focusLength * invWeight;
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

                                    manualRangeProps.focusLength +=
                                        _playableBehaviour.manualRangeProps.focusLength * invWeight;
                                    manualRangeProps.focusDistance +=
                                        _playableBehaviour.manualRangeProps.focusDistance * invWeight;
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
                               
                                CheckVolumeProfile(_playableBehaviour,cameraSwitcherControlNextClip);
                                SetVolumeValues(_playableBehaviour,manualRangeProps,physicalCameraProps);
                               
#endif               

                            }
                        }

                    }
                    else
                    {
                        SelectSingleCamera(playableBehaviour,inputWeight,currentTime,_playableBehaviour);
                        CheckVolumeProfile(playableBehaviour,cameraSwitcherControlClip);

#if USE_URP
                        SetVolumeValues(playableBehaviour,playableBehaviour.bokehProps,playableBehaviour.gaussianProps);
#elif USE_HDRP
SetVolumeValues(playableBehaviour,playableBehaviour.manualRangeProps,playableBehaviour.physicalCameraProps);
#endif
                        
                        m_TrackBinding.cameraB = _playableBehaviour.camera;
                    }
                   
                }
                else
                {
                    SelectSingleCamera(playableBehaviour,inputWeight,currentTime);
#if USE_URP
                        SetVolumeValues(playableBehaviour,playableBehaviour.bokehProps,playableBehaviour.gaussianProps);
#elif USE_HDRP
                    SetVolumeValues(playableBehaviour,playableBehaviour.manualRangeProps,playableBehaviour.physicalCameraProps);
#endif
                    m_TrackBinding.cameraB = playableBehaviour.camera;
                }
                
                // Debug.Log($"{physicalCameraProps.focusLength},{manualRangeProps.focusLength}");

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



    private void CheckVolumeProfile(CameraSwitcherControlBehaviour playableBehaviour, CameraSwitcherControlClip clip)
    {
        if (playableBehaviour.dofOverride)
        {
            if (playableBehaviour.volumeProfile == null && clip.volume.profile != null)
            {
                playableBehaviour.volumeProfile = clip.volume.profile;    
            }

            if (playableBehaviour.volumeProfile != null)
            {
                clip.volume.profile = playableBehaviour.volumeProfile;
            }
            
        }
    }

#if USE_URP
     private void SetVolumeValues(CameraSwitcherControlBehaviour behaviour, BokehProp bokehProp, GaussianProp gaussianProp)
    {
        
        
        
        if (behaviour.volumeProfile != null)
        {
            DepthOfField dof;
            behaviour.volumeProfile.TryGet<DepthOfField>(out dof);
            if(dof == null) return;
            dof.mode.value = behaviour.mode;
            if (behaviour.dofOverride)
            {

                if (dof.mode.value == DepthOfFieldMode.Bokeh)
                {

                    dof.focusDistance.value = bokehProp.focusDistance;
                    dof.focalLength.value = bokehProp.focalLength;
                    dof.aperture.value = bokehProp.aperture;
                    dof.bladeCount.value = bokehProp.bladeCount;
                    dof.bladeCurvature.value = bokehProp.bladeCurvature;
                    dof.bladeRotation.value = bokehProp.bladeRotation;

                }

                if (dof.mode.value == DepthOfFieldMode.Gaussian)
                {
                  
                }
            }

            else
            {
                
                if (dof.mode.value == DepthOfFieldMode.Bokeh)
                {
                    
                    
                    dof.focusDistance.value = m_TrackBinding.bokehBaseValues.focusDistance;
                    dof.focalLength.value = m_TrackBinding.bokehBaseValues.focalLength;
                    dof.aperture.value = m_TrackBinding.bokehBaseValues.aperture;
                    dof.bladeCount.value = m_TrackBinding.bokehBaseValues.bladeCount;
                    dof.bladeCurvature.value = m_TrackBinding.bokehBaseValues.bladeCurvature;
                    dof.bladeRotation.value = m_TrackBinding.bokehBaseValues.bladeRotation;

                }

                if (dof.mode.value == DepthOfFieldMode.Gaussian)
                {
                  
                }

            }
        }
    }
#elif USE_HDRP

    private void SetVolumeValues(CameraSwitcherControlBehaviour behaviour, ManualRangeProps manualRangeProps, PhysicalCameraProps physicalCameraProps)
    {
        
        // Debug.Log(behaviour.hdAdditionalCameraData.volumeLayerMask.value);
        
        
        if (behaviour.volumeProfile != null)
        {
            DepthOfField dof;
            behaviour.volumeProfile.TryGet<DepthOfField>(out dof);
            if(dof == null) return;
            dof.focusMode.value = behaviour.mode;
            if (behaviour.dofOverride)
            {
                
                
                
                if (dof.focusMode.value == DepthOfFieldMode.UsePhysicalCamera)
                {
                    if (behaviour.physicalCameraProps.focusDistanceMode == FocusDistanceMode.Camera )
                    {
                        behaviour.hdAdditionalCameraData.physicalParameters.focusDistance = physicalCameraProps.focusDistance;
                        // behaviour.hdAdditionalCameraData.volumeLayerMask.value
                    }
                    else
                    {
                        dof.focusDistance.value = behaviour.manualRangeProps.focusDistance;
                   
                    }
                    
                    dof.focusDistanceMode.value = physicalCameraProps.focusDistanceMode;
                    dof.focusDistance.value = physicalCameraProps.focusDistance;
                    dof.quality.value = (int) physicalCameraProps.quality;
                    dof.nearMaxBlur = physicalCameraProps.nearBluer.maxRadius;
                    dof.nearSampleCount = physicalCameraProps.nearBluer.sampleCount;
                    dof.farMaxBlur = physicalCameraProps.farBluer.maxRadius;
                    dof.farSampleCount = physicalCameraProps.farBluer.sampleCount;
                    behaviour.camera.focalLength = physicalCameraProps.focusLength;

                }

                if (dof.focusMode.value == DepthOfFieldMode.Manual)
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
                    behaviour.camera.focalLength = manualRangeProps.focusLength;
                    behaviour.hdAdditionalCameraData.physicalParameters.focusDistance = manualRangeProps.focusDistance;
                    // Debug.Log( behaviour.camera.focalLength);
                    // behaviour.camera.
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
#endif
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


            var blendNumB = nextPlayableBehaviour.colorBlend ? 1 : 0;
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
    
    public static bool Contains(LayerMask self, int layerId)
    {
        return ((1 << layerId) & self) != 0;
    }

    public static bool Contains(LayerMask self, string layerName)
    {
        int layerId = LayerMask.NameToLayer(layerName);
        return ((1 << layerId) & self) != 0;
    }

    public static LayerMask Add(LayerMask self, LayerMask layerId)
    {
        return self | (1 << layerId);
    }

    public static LayerMask Toggle(LayerMask self, LayerMask layerId)
    {
        return self ^ (1 << layerId);
    }

    public static LayerMask Remove(LayerMask self, LayerMask layerId)
    {
        return self & ~(1 << layerId);
    }
    
}
