using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TMPro;
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
    public VolumeProfile defaultProfile;
    public ClipInfo()
    {

        

    }

    public void Init()
    {
        clip.defaultVolume.profile = defaultProfile;
        
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
    private bool isUpdateThumbnail = false;
    private Dictionary<Camera, VolumeProfile> cameraVolumeProfiles = new Dictionary<Camera, VolumeProfile>();

    private void InitBehaviour(CameraSwitcherControlBehaviour cameraSwitcherControlBehaviour)
    {
        if (cameraSwitcherControlBehaviour.camera != null)
        {
            cameraSwitcherControlBehaviour.camera.enabled = false;
#if USE_URP
            cameraSwitcherControlBehaviour.universalAdditionalCameraData = cameraSwitcherControlBehaviour.camera.gameObject.GetComponent<UniversalAdditionalCameraData>();
            cameraSwitcherControlBehaviour.universalAdditionalCameraData.volumeLayerMask = Remove(
                cameraSwitcherControlBehaviour.universalAdditionalCameraData.volumeLayerMask,
                trackBinding.cameraALayer);
            cameraSwitcherControlBehaviour.universalAdditionalCameraData.volumeLayerMask = Remove(
                cameraSwitcherControlBehaviour.universalAdditionalCameraData.volumeLayerMask,
                trackBinding.cameraBLayer);
            
#elif USE_HDRP

            cameraSwitcherControlBehaviour.hdAdditionalCameraData = cameraSwitcherControlBehaviour.camera.gameObject.GetComponent<HDAdditionalCameraData>();
            cameraSwitcherControlBehaviour.hdAdditionalCameraData.volumeLayerMask = Remove(
                cameraSwitcherControlBehaviour.hdAdditionalCameraData.volumeLayerMask,
                trackBinding.cameraALayer);
            cameraSwitcherControlBehaviour.hdAdditionalCameraData.volumeLayerMask = Remove(
                cameraSwitcherControlBehaviour.hdAdditionalCameraData.volumeLayerMask,
                trackBinding.cameraBLayer);
#endif
            
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

            var cameraSwitcherControlClip = clips[i].asset as CameraSwitcherControlClip;
            if(cameraSwitcherControlClip != null && cameraSwitcherControlClip.thumbnailRenderTexture == null) cameraSwitcherControlClip.InitThumbnail();
            clipInfos.Add(new ClipInfo()
            {
                behaviour =     cameraSwitcherControlBehaviour, 
                clip =cameraSwitcherControlClip,
                inputWeigh = playable.GetInputWeight(i),
                clipIndex = i,
                timelineClip = clips[i],
                defaultProfile = cameraVolumeProfiles.ContainsKey(cameraSwitcherControlBehaviour.camera) ? cameraVolumeProfiles[cameraSwitcherControlBehaviour.camera] : null
            });
            
            clipInfos.Last().Init();

        }
    }



    private void BlitThumbnail(ClipInfo clipInfo, RenderTexture source)
    {
        trackBinding.BlitThumbnail(clipInfo.clip.thumbnailRenderTexture,track.thumbnailColor);
    }
   
    
    

    private void Init(Playable playable)
    {
        var timelineAsset = director.playableAsset as TimelineAsset;
        fps = timelineAsset != null ? (float)timelineAsset.editorSettings.frameRate : 60;
        offsetStartTime = (1f / fps) * trackBinding.prerenderingFrameCount;
        firstFrameHappened = true;
        
        cameraVolumeProfiles.Clear();

        foreach (var cameraVolumeProfileSetting in trackBinding.cameraVolumeProfileSettings)
        {
            cameraVolumeProfiles.Add(cameraVolumeProfileSetting.camera, cameraVolumeProfileSetting.volumeProfile);
        }
        
        InitPlayables(playable);

        // if (thumbnailMat == null)
        // {
        //     thumbnailMat = Resources.Load<Material>("CameraSwitcherControlResources/ThumbnailMat");
        //     thumbnailMat.SetTexture("_MainTex",trackBinding.renderTextureA);
        //
        // }
        
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

#if USE_URP
        var volumeLayerMask = cameraSwitcherControlBehaviour.universalAdditionalCameraData.volumeLayerMask;
        cameraSwitcherControlBehaviour.universalAdditionalCameraData.volumeLayerMask = Add(
            volumeLayerMask,
            isA ? trackBinding.cameraALayer : trackBinding.cameraBLayer);
        volumeLayerMask = cameraSwitcherControlBehaviour.universalAdditionalCameraData.volumeLayerMask;
        cameraSwitcherControlBehaviour.universalAdditionalCameraData.volumeLayerMask = Remove(
            volumeLayerMask,
            isA ? trackBinding.cameraBLayer : trackBinding.cameraALayer);
#elif USE_HDRP
        var volumeLayerMask = cameraSwitcherControlBehaviour.hdAdditionalCameraData.volumeLayerMask;
        cameraSwitcherControlBehaviour.hdAdditionalCameraData.volumeLayerMask = Add(
            volumeLayerMask,
            isA ? trackBinding.cameraALayer : trackBinding.cameraBLayer);
        volumeLayerMask = cameraSwitcherControlBehaviour.hdAdditionalCameraData.volumeLayerMask;
        cameraSwitcherControlBehaviour.hdAdditionalCameraData.volumeLayerMask = Remove(
            volumeLayerMask,
            isA ? trackBinding.cameraBLayer : trackBinding.cameraALayer);
#endif
        
        
        
    }


    
    private void Refresh()
    {

        A = null;
        B = null;
        _stringBuilder.Clear();

        var index = 0;
        foreach (var pair in clipInfos)
        {
            
            if (pair.behaviour.camera == null)
            {
               Debug.LogWarning($"{pair.timelineClip.displayName} Camera is null");
                continue;
            }
            pair.clip.clipIndex = index;
            pair.clip.mixer = this;
            if (pair.clip.isUpdateThumbnail) isUpdateThumbnail = true;
            pair.clip.drawTimeMode = track.drawTimeMode;
            pair.timelineClip.displayName = pair.behaviour.camera != null ? pair.behaviour.camera.name : "!CAMERA NULL";
            pair.behaviour.camera.enabled = false;
            pair.behaviour.camera.targetTexture = null;
            pair.behaviour.lookAtConstraint.enabled = false;
            pair.clip.defaultVolume.profile = cameraVolumeProfiles.ContainsKey(pair.behaviour.camera) ? cameraVolumeProfiles[pair.behaviour.camera] : null;
            pair.clip.defaultVolume.enabled = false;
            index++;
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
     
        if (cameraNamePreviewGUI != null)
        {
            var dateTime = TimeSpan.FromSeconds(director.time);
            _stringBuilder.Append($"[{trackBinding.gameObject.scene.name}]  ");
            // _stringBuilder.Append(" ");
            _stringBuilder.Append(dateTime.ToString(@"hh\:mm\:ss\:ff"));
            _stringBuilder.Append(" ");
            _stringBuilder.Append((Mathf.CeilToInt(fps * (float) director.time)));
            _stringBuilder.Append("f  ");
            if (A != null && A.behaviour.camera != null) _stringBuilder.Append($"{A.behaviour.camera.name}");
            if (B != null && B.behaviour.camera != null) _stringBuilder.Append($" >> {B.behaviour.camera.name}");

        }

        
        SetRenderTexture();
        SetLayer();
        SetVolume();
        SetLookAt();
        CheckThumbnail();
        BlendCamera();
        if (cameraNamePreviewGUI != null) cameraNamePreviewGUI.text = _stringBuilder.ToString();

        preA = A;
        preB = B;
        
     

    }

    public void SyncProfileSameCameraClip(int clipIndex)
    {
        
        var referenceClipInfo = clipInfos[clipIndex];
        
        var camera = clipInfos[clipIndex].behaviour.camera;


        foreach (var clipInfo in clipInfos)
        {
            if (clipInfo.behaviour.camera == camera)
            {
                CopyClipValues(referenceClipInfo, clipInfo);
            }
        }
    }

    private void CopyClipValues(ClipInfo from, ClipInfo to)
    {
        to.clip.wiggle = from.clip.wiggle;
        to.clip.wigglerProps = new WigglerProps(from.clip.wigglerProps);
        to.clip.colorBlend = from.clip.colorBlend;
        to.clip.colorBlendProps = new ColorBlendProps(from.clip.colorBlendProps);
        to.clip.lookAt = from.clip.lookAt;
        to.clip.lookAtProps = new LookAtProps(from.clip.lookAtProps);
        to.clip.lookAtTarget = from.clip.lookAtTarget;
        to.clip.volumeOverride = from.clip.volumeOverride;
        to.clip.volumeProfile = from.clip.volumeProfile;
        
        to.behaviour.lookAtTarget = from.behaviour.lookAtTarget;

    }


    private void CheckThumbnail()
    {
#if UNITY_EDITOR

        var startRange = 0.3f;
        var endRange = 1f - startRange;
        var step = 2;
        if (track.drawThumbnail)
        {
            if (A != null)
            {
                if (A.timelineClip.start+ A.timelineClip.duration* startRange < director.time && 
                    director.time < A.timelineClip.start + A.timelineClip.duration*endRange)
                {
                    if((int)(fps*director.time) %step ==0)BlitThumbnail(A,A.behaviour.camera.targetTexture);
                }         
            }

            if (B != null)
            {
                if (B.timelineClip.start+ B.timelineClip.duration* startRange < director.time && 
                    director.time < B.timelineClip.start + B.timelineClip.duration*endRange)
                {
                    if((int)(fps*director.time) %step ==0)BlitThumbnail(B,B.behaviour.camera.targetTexture);
                }     
            }
           
        }
                
#endif
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
            trackBinding.volumeA.profile = A.clip.volumeProfile;
            trackBinding.volumeB.profile = B.clip.volumeProfile;
            A.clip.defaultVolume.enabled = true;
            B.clip.defaultVolume.enabled = true;
            if (A.behaviour.camera == B.behaviour.camera)
            {
                
                var isOverride = A.clip.volumeOverride || B.clip.volumeOverride;
                if (isOverride)
                {
                    trackBinding.volumeA.gameObject.layer = trackBinding.cameraALayer;
                    trackBinding.volumeB.gameObject.layer = trackBinding.cameraALayer;
                    trackBinding.volumeA.enabled = true;
                    trackBinding.volumeB.enabled = true;
                    trackBinding.volumeA.weight = A.inputWeigh;
                    trackBinding.volumeB.weight = B.inputWeigh;
                }
            }
            // AとBで違うカメラがBlendingされているとき
            else
            {
                trackBinding.volumeA.weight = 1f;
                trackBinding.volumeB.weight = 1f;
                trackBinding.volumeA.enabled = A.clip.volumeOverride;
                trackBinding.volumeB.enabled = B.clip.volumeOverride;
                trackBinding.volumeA.gameObject.layer = trackBinding.cameraALayer;
                trackBinding.volumeB.gameObject.layer = trackBinding.cameraBLayer;

            }
            
        }else
        {
            if (A != null)
            {
                trackBinding.volumeA.enabled = A.clip.volumeOverride;
                trackBinding.volumeA.profile = A.clip.volumeProfile;
                trackBinding.volumeB.enabled = false;
                trackBinding.volumeA.weight = 1f;   
                
                A.clip.defaultVolume.enabled = true;
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
                if (A.clip.lookAt|| B.clip.lookAt)
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

    

    private void BlendCamera()
    {
        if(A == null && B == null) return;
        
        trackBinding.material.SetFloat("_PlayableDirectorTime", (float) director.time);


        if (A != null && B != null)
        {
            // Clipが2つBlendされた状態で、且つAとBが同じカメラになっている場合、
            if (A.behaviour.camera == B.behaviour.camera)
            {

                BlendLookAt();
                trackBinding.material.SetTexture("_TextureA", trackBinding.renderTextureA);
                trackBinding.material.SetTexture("_TextureB", trackBinding.renderTextureA);
                trackBinding.material.SetVector("_ClipSizeA", BlendClopSize(A,B));
                trackBinding.material.SetVector("_WigglerValueA",BlendNoise(A,B));
                trackBinding.material.SetInt("_BlendA", GetBlendNum(A));
                trackBinding.material.SetInt("_BlendB", GetBlendNum(B));
                trackBinding.material.SetColor("_MultiplyColorA", A.clip.colorBlendProps.color);
                trackBinding.material.SetColor("_MultiplyColorB", B.clip.colorBlendProps.color);
                trackBinding.material.SetFloat("_CrossFade", 0);
                
                
            }
            // AとBで違うカメラをDisolveしたいとき
            else
            {
              
                InitLookAt(A);
                InitLookAt(B);
                trackBinding.material.SetTexture("_TextureA", trackBinding.renderTextureA);
                trackBinding.material.SetVector("_ClipSizeA", GetClopSize(A));
                trackBinding.material.SetVector("_WigglerValueA",CalcNoise(A));
                trackBinding.material.SetColor("_MultiplyColorA", A.clip.colorBlendProps.color);
                trackBinding.material.SetInt("_BlendA", GetBlendNum(A));
                
                trackBinding.material.SetTexture("_TextureB", trackBinding.renderTextureB);
                trackBinding.material.SetVector("_ClipSizeB", GetClopSize(B));
                trackBinding.material.SetVector("_WigglerValueB",CalcNoise(B));
                trackBinding.material.SetColor("_MultiplyColorB", B.clip.colorBlendProps.color);
                trackBinding.material.SetInt("_BlendB", GetBlendNum(B));
                trackBinding.material.SetFloat("_CrossFade", 1f-A.inputWeigh);
                
            }     
        }
        // 一つしかカメラが無いとき
        else
        {
            InitLookAt(A);
            trackBinding.material.SetTexture("_TextureA", trackBinding.renderTextureA);
            trackBinding.material.SetVector("_ClipSizeA", GetClopSize(A));
            trackBinding.material.SetVector("_WigglerValueA", CalcNoise(A));
            trackBinding.material.SetColor("_MultiplyColorA", A.clip.colorBlendProps.color);
            trackBinding.material.SetInt("_BlendA", GetBlendNum(A));
            trackBinding.material.SetFloat("_CrossFade", 0);
           
        }
       
    }

    private int GetBlendNum(ClipInfo clipInfo)
    {
        var blendNumB = clipInfo.clip.colorBlend ? 1 : 0;
        blendNumB += blendNumB == 0 ? 0 : (int) clipInfo.clip.colorBlendProps.blendMode;
        return blendNumB;
    }
    

    private Vector2 BlendClopSize(ClipInfo A, ClipInfo B)
    {
        return Vector2.Lerp(GetClopSize(A), GetClopSize(B), 1f-A.inputWeigh);
    }

    private Vector2 GetClopSize(ClipInfo clipInfo)
    {
        return  clipInfo.clip.wiggle ? clipInfo.clip.wigglerProps.wiggleRange / 100f : Vector2.zero;
    }
    private Vector2 CalcNoise(ClipInfo clipInfo)
    {
        var currentTime = (float) director.time;
        if (!clipInfo.clip.wiggle) return Vector2.zero;
        var wiggler = new Vector2(
            (Mathf.PerlinNoise(clipInfo.clip.wigglerProps.noiseSeed.x * clipInfo.clip.wigglerProps.noiseScale.x,
                 currentTime *clipInfo.clip. wigglerProps.roughness + clipInfo.clip.wigglerProps.noiseScale.y * clipInfo.clip.wigglerProps.noiseSeed.y) -
             0.5f) * clipInfo.clip.wigglerProps.wiggleRange.x / 100f,
            (Mathf.PerlinNoise(
                clipInfo.clip.wigglerProps.noiseSeed.x * clipInfo.clip.wigglerProps.noiseScale.x + currentTime * clipInfo.clip.wigglerProps.roughness,
                clipInfo.clip.wigglerProps.noiseScale.y * clipInfo.clip.wigglerProps.noiseSeed.y) - 0.5f) * clipInfo.clip.wigglerProps.wiggleRange.y /
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
        var target = clipInfo.behaviour.lookAtTarget;
        SetLookAtTarget(A.behaviour.lookAtConstraint, target);
        clipInfo.behaviour.lookAtConstraint.enabled = clipInfo.clip.lookAt;
        var lookAt = clipInfo.behaviour.lookAtConstraint;
        lookAt.locked = clipInfo.clip.lookAtProps.Lock;
        lookAt.constraintActive = clipInfo.clip.lookAtProps.IsActive;
        lookAt.weight = A.clip.lookAtProps.Weight * A.inputWeigh + B.clip.lookAtProps.Weight * (1f - A.inputWeigh);
        lookAt.roll = A.clip.lookAtProps.Roll * A.inputWeigh + B.clip.lookAtProps.Roll * (1f - A.inputWeigh);
        lookAt.useUpObject = A.clip.lookAtProps.UseUpObject;
        lookAt.rotationOffset = A.clip.lookAtProps.RotationOffset * A.inputWeigh + B.clip.lookAtProps.RotationOffset * (1f - A.inputWeigh);
        lookAt.rotationAtRest = A.clip.lookAtProps.RotationAtReset * A.inputWeigh + B.clip.lookAtProps.RotationAtReset * (1f - A.inputWeigh);
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
        var lookAt = clipInfo.behaviour.lookAtConstraint;
        if(lookAt == null) return;
        lookAt.enabled = clipInfo.clip.lookAt;
        var target = clipInfo.behaviour.lookAtTarget;
        SetLookAtTarget(lookAt, target);
        lookAt.locked = clipInfo.clip.lookAtProps.Lock;
        lookAt.constraintActive = clipInfo.clip.lookAtProps.IsActive;
        lookAt.weight = clipInfo.clip.lookAtProps.Weight;
        lookAt.roll = clipInfo.clip.lookAtProps.Roll;
        lookAt.useUpObject = clipInfo.clip.lookAtProps.UseUpObject;
        lookAt.rotationOffset = clipInfo.clip.lookAtProps.RotationOffset;
        lookAt.rotationAtRest = clipInfo.clip.lookAtProps.RotationAtReset;

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