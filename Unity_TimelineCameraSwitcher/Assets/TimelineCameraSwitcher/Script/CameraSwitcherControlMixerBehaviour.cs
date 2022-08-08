using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Playables;
using UnityEngine.Rendering;

#if USE_URP
using UnityEngine.Rendering.Universal;
#endif

#if USE_HDRP
using UnityEngine.Rendering.HighDefinition;
#endif
using UnityEngine.Timeline;


internal class ClipInfo
{
    public CameraSwitcherControlBehaviour behaviour;
    public CameraSwitcherControlClip clip;
    public float inputWeigh;
    public int clipIndex;
    public TimelineClip timelineClip;
    public VolumeProfile tmpVolumeProfile;

    public ClipInfo()
    {
        tmpVolumeProfile = new VolumeProfile();
        var dof = tmpVolumeProfile.Add<DepthOfField>();
        tmpVolumeProfile.name = Guid.NewGuid().ToString();
        
        dof.mode = new DepthOfFieldModeParameter( DepthOfFieldMode.Bokeh ,true);
        dof.focusDistance.overrideState = true;
        dof.focalLength.overrideState = true;
        dof.aperture.overrideState = true;
        dof.bladeCount.overrideState = true;
        dof.bladeCurvature.overrideState = true;
        dof.bladeRotation.overrideState = true;
        dof.gaussianStart.overrideState = true;
        dof.gaussianEnd.overrideState = true;
        dof.gaussianMaxRadius.overrideState = true;
        dof.highQualitySampling.overrideState = true;
        
        
    }
}

public class CameraSwitcherControlMixerBehaviour : PlayableBehaviour
{
    public List<TimelineClip> clips;

    public PlayableDirector director;
    public CameraSwitcherControlTrack track;

    private bool firstFrameHappened;

    public TextMeshProUGUI cameraNamePreviewGUI;
    public float fps;
    private StringBuilder _stringBuilder;


    private double offsetStartTime;
    

    private CameraSwitcherControl trackBinding;

    private List<ClipInfo> clipInfos = new List<ClipInfo>();

    private ClipInfo A;
    private ClipInfo B;

    
    private ClipInfo preA;
    private ClipInfo preB;
    public bool disableWiggler;
    
    private void InitBehaviour(CameraSwitcherControlBehaviour cameraSwitcherControlBehaviour)
    {
        if (cameraSwitcherControlBehaviour.camera != null)
        {
            cameraSwitcherControlBehaviour.camera.enabled = false;
            cameraSwitcherControlBehaviour.universalAdditionalCameraData = cameraSwitcherControlBehaviour.camera.gameObject.GetComponent<UniversalAdditionalCameraData>();
            cameraSwitcherControlBehaviour.universalAdditionalCameraData.volumeLayerMask = Remove(
                cameraSwitcherControlBehaviour.universalAdditionalCameraData.volumeLayerMask,
                trackBinding.cameraALayer);
            cameraSwitcherControlBehaviour.universalAdditionalCameraData.volumeLayerMask = Remove(
                cameraSwitcherControlBehaviour.universalAdditionalCameraData.volumeLayerMask,
                trackBinding.cameraBLayer);
            
        }
    }

    private void InitPlayables(Playable playable)
    {
        clipInfos.Clear();
        if(clips == null || clips.Count != playable.GetInputCount())
            return;
        for (int i = 0; i < playable.GetInputCount(); i++)
        {
            var scriptPlayable = (ScriptPlayable<CameraSwitcherControlBehaviour>) playable.GetInput(i);
            var cameraSwitcherControlBehaviour = scriptPlayable.GetBehaviour();
            InitBehaviour(cameraSwitcherControlBehaviour);
            clipInfos.Add(new ClipInfo()
            {
                behaviour =     cameraSwitcherControlBehaviour,
                clip = clips[i].asset as CameraSwitcherControlClip,
                inputWeigh = playable.GetInputWeight(i),
                clipIndex = i,
                timelineClip = clips[i]
            });
                
        }
    }

    private void Init(Playable playable)
    {
        InitPlayables(playable);
        var timelineAsset = director.playableAsset as TimelineAsset;
        fps = timelineAsset != null ? timelineAsset.editorSettings.fps : 60;
        offsetStartTime = (1f / fps) * trackBinding.prerenderingFrameCount;
        firstFrameHappened = true;
    }
    

  
    private bool CheckClipOnFrame(TimelineClip clip, double offsetTime = 0)
    {
        if (clip.start <= director.time+offsetTime && director.time+offsetTime < clip.start + clip.duration)
        {
            return true;
        }
        else
        {
            return false;
        }
    }
    
    private void SetLayer(ClipInfo clipInfo,bool isA )
    {
        var cameraSwitcherControlBehaviour = clipInfo.behaviour;
        
        var camera = cameraSwitcherControlBehaviour.camera;
        
        camera.gameObject.layer = isA ? trackBinding.cameraALayer : trackBinding.cameraBLayer;
        
        
        var volumeLayerMask = cameraSwitcherControlBehaviour.universalAdditionalCameraData.volumeLayerMask;
        cameraSwitcherControlBehaviour.universalAdditionalCameraData.volumeLayerMask = Add(
            volumeLayerMask,
            isA ? trackBinding.cameraALayer : trackBinding.cameraBLayer);
        volumeLayerMask = cameraSwitcherControlBehaviour.universalAdditionalCameraData.volumeLayerMask;
        cameraSwitcherControlBehaviour.universalAdditionalCameraData.volumeLayerMask = Remove(
            volumeLayerMask,
            isA ? trackBinding.cameraBLayer : trackBinding.cameraALayer);
        
    }



    private void Refresh()
    {
        A = null;
        B = null;
        _stringBuilder.Clear();
        
        foreach (var pair in clipInfos)
        {
            pair.behaviour.camera.enabled = false;
            pair.behaviour.camera.targetTexture = null;
            pair.clip.lookAtConstraint.enabled = false;
            pair.clip.volume.enabled = false;
            // CheckVolumeProfile(pair);
        }

    }
    public override void ProcessFrame(Playable playable, FrameData info, object playerData)
    {
        if (_stringBuilder == null) _stringBuilder = new StringBuilder();
        trackBinding = playerData as CameraSwitcherControl;
        if (trackBinding == null)
            return;


        if (!firstFrameHappened)
        {
            Init(playable);
        }


        
        Refresh();


        if (cameraNamePreviewGUI != null)
        {
            var dateTime = new TimeSpan(0, 0, (int) director.time);
            _stringBuilder.Append(dateTime.ToString(@"hh\:mm\:ss"));
            _stringBuilder.Append(" ");
            _stringBuilder.Append((Mathf.CeilToInt(fps * (float) director.time)));
            _stringBuilder.Append("f ");

        }
        
        
        for (int i = 0; i < clipInfos.Count; i++)
        {
            var clipInfo = clipInfos[i];
            clipInfo.inputWeigh = playable.GetInputWeight(i);

            if (CheckClipOnFrame(clipInfo.timelineClip))
            {
                A = clipInfo;
                var nextIndex = i + 1;
                if (nextIndex < clipInfos.Count)
                {
                    var nextClipInfo = clipInfos[nextIndex];
                    nextClipInfo.inputWeigh = playable.GetInputWeight(nextIndex);
                    if (CheckClipOnFrame(nextClipInfo.timelineClip, trackBinding.prerenderingFrameCount * offsetStartTime))
                    {
                       B = nextClipInfo;
                  
                    }
                }
                
                break;

            }
        }
        
        if(A == null && B == null)
        {
            if (preA != null)
            {
                A = preA;
                B = preB;
            }
        }
        
        SetRenderTexture();
        SetLayer();
        SetVolume();
        SetLookAt();
        BlendCameraAB(playable);
        if (cameraNamePreviewGUI != null) cameraNamePreviewGUI.text = _stringBuilder.ToString();

        preA = A;
        preB = B;

    }
   

    private void SetLayer()
    {
        if (A != null && B != null)
        {
            if (A.behaviour.camera == B.behaviour.camera)
            {
                SetLayer(A, true);
            }
            else
            {
                SetLayer(A, true);
                SetLayer(B, false);
            }
            
        }else
        {
            if (A != null)
            {
                SetLayer(A, true);
            }
        }
    }

    private void SetVolume()
    {
        if (A != null && B != null)
        {
            if (A.behaviour.camera == B.behaviour.camera)
            {
                A.clip.volume.enabled = true;
                A.clip.volume.profile = trackBinding.volumeProfileA;
            }
            else
            {
                A.clip.volume.enabled = true;
                A.clip.volume.profile = trackBinding.volumeProfileA;
                B.clip.volume.enabled = true;
                B.clip.volume.profile = trackBinding.volumeProfileB;
            }
            
        }else
        {
            if (A != null)
            {
                A.clip.volume.profile = trackBinding.volumeProfileA;
                A.clip.volume.enabled = true;
            }
        }
    }

    
    
    private void SetRenderTexture()
    {

        if (A != null && B != null)
        {
            if (A.behaviour.camera == B.behaviour.camera)
            {
                A.behaviour.camera.enabled = true;
                A.behaviour.camera.targetTexture = trackBinding.renderTextureA;
            }
            else
            {
                A.behaviour.camera.enabled = true;
                A.behaviour.camera.targetTexture = trackBinding.renderTextureA;
                
                B.behaviour.camera.enabled = true;
                B.behaviour.camera.targetTexture = trackBinding.renderTextureB;
            }
            
        }else
        {
            if (A != null)
            {
                A.behaviour.camera.enabled = true;
                A.behaviour.camera.targetTexture = trackBinding.renderTextureA;
            }
          
        }
           
    }
    
    private void SetLookAt()
    {

        if (A != null && B != null)
        {
            if (A.behaviour.camera == B.behaviour.camera)
            {
                if (A.behaviour.lookAt || B.behaviour.lookAt)
                {
                    InitLookAt(A);
                    
                }
                
            }
            else
            {
                InitLookAt(A);
                InitLookAt(B);
            }
            
        }else
        {
            if (A != null)
            {
                InitLookAt(A);
            }
          
        }
           
    }

    private void BlendCameraAB(Playable playable)
    {
        if(A == null && B == null) return;
        
        trackBinding.material.SetFloat("_PlayableDirectorTime", (float) director.time);


        if (A != null && B != null)
        {
            // Clipが2つBlendされた状態で、且つAとBが同じカメラになっている場合、
            if (A.behaviour.camera == B.behaviour.camera)
            {
                var bokehProps = new BokehProp();
                var gaussianProps = new GaussianProp();
             
                var useVolumeOverride = A.behaviour.dofOverride || B.behaviour.dofOverride;
                bokehProps = A.behaviour.bokehProps * A.inputWeigh+ (B.behaviour.bokehProps * (B.inputWeigh));
                gaussianProps = A.behaviour.gaussianProps * A.inputWeigh + B.behaviour.gaussianProps * (B.inputWeigh);
                SetVolumeValues(trackBinding.volumeProfileA,A, bokehProps, gaussianProps);
                
                BlendLookAt();
                trackBinding.material.SetTexture("_TextureA", trackBinding.renderTextureA);
                trackBinding.material.SetTexture("_TextureB", trackBinding.renderTextureA);
                trackBinding.material.SetVector("_ClipSizeA", BlendClopSize(A,B));
                trackBinding.material.SetVector("_WigglerValueA",BlendNoise(A,B));
                trackBinding.material.SetInt("_BlendA", GetBlendNum(A));
                trackBinding.material.SetInt("_BlendB", GetBlendNum(B));
                trackBinding.material.SetColor("_MultiplyColorA", A.behaviour.colorBlendProps.color);
                trackBinding.material.SetColor("_MultiplyColorB", B.behaviour.colorBlendProps.color);
                trackBinding.material.SetFloat("_CrossFade", 0);
                
                
            }
            // AとBで違うカメラをDisolveしたいとき
            else
            {
                SetVolumeValues(trackBinding.volumeProfileA, A, A.behaviour.bokehProps, A.behaviour.gaussianProps);
                SetVolumeValues(trackBinding.volumeProfileB, B, B.behaviour.bokehProps, B.behaviour.gaussianProps);
                
                InitLookAt(A);
                InitLookAt(B);
                trackBinding.material.SetTexture("_TextureA", trackBinding.renderTextureA);
                trackBinding.material.SetVector("_ClipSizeA", GetClopSize(A));
                trackBinding.material.SetVector("_WigglerValueA",CalcNoise(A));
                trackBinding.material.SetColor("_MultiplyColorA", A.behaviour.colorBlendProps.color);
                trackBinding.material.SetInt("_BlendA", GetBlendNum(A));
                
                trackBinding.material.SetTexture("_TextureB", trackBinding.renderTextureB);
                trackBinding.material.SetVector("_ClipSizeB", GetClopSize(B));
                trackBinding.material.SetVector("_WigglerValueB",CalcNoise(B));
                trackBinding.material.SetColor("_MultiplyColorB", B.behaviour.colorBlendProps.color);
                trackBinding.material.SetInt("_BlendB", GetBlendNum(B));
                
                trackBinding.material.SetFloat("_CrossFade", 1f-A.inputWeigh);
                
            }     
        }
        // 一つしかカメラが無いとき
        else
        {
            InitLookAt(A);
            SetVolumeValues(trackBinding.volumeProfileA,A, A.behaviour.bokehProps, A.behaviour.gaussianProps);
            trackBinding.material.SetTexture("_TextureA", trackBinding.renderTextureA);
            trackBinding.material.SetVector("_ClipSizeA", GetClopSize(A));
            trackBinding.material.SetVector("_WigglerValueA", CalcNoise(A));
            trackBinding.material.SetColor("_MultiplyColorA", A.behaviour.colorBlendProps.color);
            trackBinding.material.SetInt("_BlendA", GetBlendNum(A));
            trackBinding.material.SetFloat("_CrossFade", 0);
           
        }
       
    }

    private int GetBlendNum(ClipInfo clipInfo)
    {
        var blendNumB = clipInfo.behaviour.colorBlend ? 1 : 0;
        blendNumB += blendNumB == 0 ? 0 : (int) clipInfo.behaviour.colorBlendProps.blendMode;
        return blendNumB;
    }
    

   
    private Color BlendColor(ClipInfo A, ClipInfo B)
    {
        var colorA = A.behaviour.colorBlendProps.color;
        var colorB = B.behaviour.colorBlendProps.color;
        return Color.Lerp(colorA, colorB, A.inputWeigh);
    }

    private Vector2 BlendClopSize(ClipInfo A, ClipInfo B)
    {
        return Vector2.Lerp(GetClopSize(A), GetClopSize(B), 1f-A.inputWeigh);
    }

    private Vector2 GetClopSize(ClipInfo clipInfo)
    {
        return  clipInfo.behaviour.wiggle ? clipInfo.behaviour.wigglerProps.wiggleRange / 100f : Vector2.zero;
    }
    private Vector2 CalcNoise(ClipInfo clipInfo)
    {
        var currentTime = (float) director.time;
        if (!clipInfo.behaviour.wiggle) return Vector2.zero;
        var wiggler = new Vector2(
            (Mathf.PerlinNoise(clipInfo.behaviour.wigglerProps.noiseSeed.x * clipInfo.behaviour.wigglerProps.noiseScale.x,
                 currentTime *clipInfo.behaviour. wigglerProps.roughness + clipInfo.behaviour.wigglerProps.noiseScale.y * clipInfo.behaviour.wigglerProps.noiseSeed.y) -
             0.5f) * clipInfo.behaviour.wigglerProps.wiggleRange.x / 100f,
            (Mathf.PerlinNoise(
                clipInfo.behaviour.wigglerProps.noiseSeed.x * clipInfo.behaviour.wigglerProps.noiseScale.x + currentTime * clipInfo.behaviour.wigglerProps.roughness,
                clipInfo.behaviour.wigglerProps.noiseScale.y * clipInfo.behaviour.wigglerProps.noiseSeed.y) - 0.5f) * clipInfo.behaviour.wigglerProps.wiggleRange.y /
            100f
        );

        return wiggler;
    }

    private Vector2 BlendNoise(ClipInfo a, ClipInfo b)
    {
        var wiggler = Vector2.Lerp(CalcNoise(a), CalcNoise(b), 1f - a.inputWeigh);

        return wiggler;
    }


    private void BlendLookAt()
    {
        var clipInfo = A.inputWeigh > 0.5f ? A : B; 
        var target = clipInfo.clip.target;
        SetLookAtTarget(A.clip.lookAtConstraint, target);
        clipInfo.clip.lookAtConstraint.enabled = clipInfo.behaviour.lookAt;
        var lookAt = clipInfo.clip.lookAtConstraint;
        lookAt.locked = clipInfo.behaviour.lookAtProps.Lock;
        lookAt.constraintActive = clipInfo.behaviour.lookAtProps.IsActive;
        lookAt.weight = A.behaviour.lookAtProps.Weight * A.inputWeigh + B.behaviour.lookAtProps.Weight * (1f - A.inputWeigh);
        lookAt.roll = A.behaviour.lookAtProps.Roll * A.inputWeigh + B.behaviour.lookAtProps.Roll * (1f - A.inputWeigh);
        lookAt.useUpObject = A.behaviour.lookAtProps.UseUpObject;
        lookAt.rotationOffset = A.behaviour.lookAtProps.RotationOffset * A.inputWeigh + B.behaviour.lookAtProps.RotationOffset * (1f - A.inputWeigh);
        lookAt.rotationAtRest = A.behaviour.lookAtProps.RotationAtReset * A.inputWeigh + B.behaviour.lookAtProps.RotationAtReset * (1f - A.inputWeigh);
    }


    private void SetLookAtTarget(LookAtConstraint lookAt, Transform target)
    {
        if (target != null)
        {
            if (lookAt.sourceCount > 0)
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
                    lookAt.SetSource(0, source);
                }
            }
            else
            {
                var source = new ConstraintSource();
                source.sourceTransform = target;
                source.weight = 1;
                lookAt.AddSource(source);
            }
        }
        else
        {
            while (lookAt.sourceCount > 1)
            {
                lookAt.RemoveSource(0);
            }
        }
    }
    private void InitLookAt(ClipInfo clipInfo)
    {
        
        
       
        var target = clipInfo.clip.target;
        clipInfo.clip.lookAtConstraint.enabled = clipInfo.behaviour.lookAt;
        var lookAt = clipInfo.clip.lookAtConstraint;
        SetLookAtTarget(lookAt, target);
        if (target != null)
        {

            lookAt.locked = clipInfo.behaviour.lookAtProps.Lock;
            lookAt.constraintActive = clipInfo.behaviour.lookAtProps.IsActive;
            lookAt.weight = clipInfo.behaviour.lookAtProps.Weight;
            lookAt.roll = clipInfo.behaviour.lookAtProps.Roll;
            lookAt.useUpObject = clipInfo.behaviour.lookAtProps.UseUpObject;
            lookAt.rotationOffset = clipInfo.behaviour.lookAtProps.RotationOffset;
            lookAt.rotationAtRest = clipInfo.behaviour.lookAtProps.RotationAtReset;
        }
       
            
        
    }


    private VolumeProfile GetVolumeProfile(ClipInfo clipInfo)
    {
        if(clipInfo.behaviour.volumeProfile == null)
        {
            return clipInfo.tmpVolumeProfile;
        }
        else
        {
            return clipInfo.behaviour.volumeProfile;
        }
        



    }

    private void SetVolumeValues(VolumeProfile volumeProfile,ClipInfo clipInfo,BokehProp bokehProp, GaussianProp gaussianProp)
    {

        // if (clipInfo.behaviour.volumeProfile != null)
        // {
        //     CopyVolumeValues(volumeProfile, clipInfo);
        // }
        // else
        // {
            DepthOfField dof;
            volumeProfile.TryGet<DepthOfField>(out dof);
            if (dof == null) return;
           
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
                dof.gaussianStart.value = gaussianProp.start;
                dof.gaussianEnd.value = gaussianProp.end;
                dof.gaussianMaxRadius.value = gaussianProp.maxRadius;
                dof.highQualitySampling.value = gaussianProp.highQualitySampling;
            } 
        // }

           

    }

    private void CopyVolumeValues(VolumeProfile volumeProfile, ClipInfo clipInfo)
    {
        if (clipInfo.behaviour.volumeProfile != null)
        {
            DepthOfField dof;
            volumeProfile.TryGet<DepthOfField>(out dof);
            
            DepthOfField referenceDof;
            clipInfo.behaviour.volumeProfile.TryGet<DepthOfField>(out referenceDof);
            
            if(dof != null && referenceDof != null)
            {
                dof.focusDistance.value = referenceDof.focusDistance.value;
                dof.focalLength.value = referenceDof.focalLength.value;
                dof.aperture.value = referenceDof.aperture.value;
                dof.bladeCount.value = referenceDof.bladeCount.value;
                dof.bladeCurvature.value = referenceDof.bladeCurvature.value;
                dof.bladeRotation.value = referenceDof.bladeRotation.value;
                dof.gaussianStart.value = referenceDof.gaussianStart.value;
                dof.gaussianEnd.value = referenceDof.gaussianEnd.value;
                dof.gaussianMaxRadius.value = referenceDof.gaussianMaxRadius.value;
                dof.highQualitySampling.value = referenceDof.highQualitySampling.value;
            }
        }
    }

  
    public override void OnPlayableDestroy(Playable playable)
    {
        base.OnPlayableDestroy(playable);
        if (trackBinding != null && trackBinding.material != null) trackBinding.material.SetFloat("_PlayableDirectorTime", 0f);
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