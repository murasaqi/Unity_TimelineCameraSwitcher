
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.Playables;
// using UnityEngine.Rendering.PostProcessing;
using UnityEngine.Timeline;
using UnityEngine.UI;

public class CameraSwitcherControlMixerBehaviour : PlayableBehaviour
{
    
    private List<TimelineClip> m_Clips;
    private List<Camera> _cameras = new List<Camera>();
    private PlayableDirector m_Director;
    private Material m_CompositeMaterial;
    private CameraSwitcherControlTrack m_track;
    // private TimelineClip m_preClip = null;
    
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

    public Material compositeMaterial
    {
        get => m_CompositeMaterial;
        set => m_CompositeMaterial = value;
    }

    public RawImage rawImage;
    
    

    public void ResetCameraTarget()
    {
        foreach (var camera in _cameras)
        {
            Debug.Log(camera.name);
            camera.targetTexture = null;
        }
    }
    public override void ProcessFrame(Playable playable, FrameData info, object playerData)
    {
        if (!compositeMaterial)
            return;

        compositeMaterial.SetFloat("_PlayableDirectorTime",(float)director.time);
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
                // if (playableBehaviour.wiggle) isWiggle = playableBehaviour.wiggle;
                    
                if (playableBehaviour.camera != null)
                {
                    // Debug.Log(playableBehaviour.camera.name);
                    playableBehaviour.camera.enabled = true;
                    playableBehaviour.camera.targetTexture = track.textureA;
                    
                }

                if (inputPort + 1 < m_Clips.Count())
                {
                    var nextClip = m_Clips.ToList()[inputPort + 1];
                    var _scriptPlayable =  (ScriptPlayable<CameraSwitcherControlBehaviour>)playable.GetInput(inputPort+1);
                    var _playableBehaviour = _scriptPlayable.GetBehaviour();
                    
                    
                    if (nextClip.start <= m_Director.time && m_Director.time <= nextClip.start + clip.duration )
                    {
                        _playableBehaviour.camera.enabled = true;
                        _playableBehaviour.camera.targetTexture = track.textureB;
                        compositeMaterial.SetTexture("_TextureA",track.textureA);
                        var offsetPositionA = new Vector2(
                            playableBehaviour.offsetPosition.x / rawImage.rectTransform.rect.width,
                            playableBehaviour.offsetPosition.y / rawImage.rectTransform.rect.height);
                        compositeMaterial.SetFloat("_WigglePowerA",playableBehaviour.wiggle ? 1f:0f);
                        compositeMaterial.SetVector("_NoiseSeedA",playableBehaviour.noiseSeed);
                        compositeMaterial.SetVector("_NoiseScaleA",playableBehaviour.noiseScale);
                        compositeMaterial.SetFloat("_TimeScaleA",playableBehaviour.roughness);
                        compositeMaterial.SetVector("_RangeA",playableBehaviour.wiggleRange/100f);
                        compositeMaterial.SetVector("_OffsetPositionA",offsetPositionA);

                        
                        var offsetPositionB = new Vector2(
                            _playableBehaviour.offsetPosition.x / rawImage.rectTransform.rect.width,
                            _playableBehaviour.offsetPosition.y / rawImage.rectTransform.rect.height);
                        compositeMaterial.SetTexture("_TextureB",track.textureB);
                        compositeMaterial.SetFloat("_WigglePowerB",_playableBehaviour.wiggle ? 1f:0f);
                        compositeMaterial.SetVector("_NoiseSeedB",_playableBehaviour.noiseSeed);
                        compositeMaterial.SetVector("_NoiseScaleB",_playableBehaviour.noiseScale);
                        compositeMaterial.SetFloat("_TimeScaleB",_playableBehaviour.roughness);
                        compositeMaterial.SetVector("_RangeB",_playableBehaviour.wiggleRange/100f);
                        compositeMaterial.SetFloat("_CrossFade", 1f-inputWeight);
                        compositeMaterial.SetVector("_OffsetPositionB",offsetPositionB);
                    }
                    else
                    {
                        compositeMaterial.SetTexture("_TextureA",track.textureA);
                        compositeMaterial.SetFloat("_WigglePowerA",playableBehaviour.wiggle ? 1f:0f);
                        compositeMaterial.SetTexture("_TextureB",track.textureA);
                        compositeMaterial.SetFloat("_WigglePowerB",playableBehaviour.wiggle ? 1f:0f);
                        compositeMaterial.SetFloat("_CrossFade", inputWeight);
                        
                        var offsetPositionA = new Vector2(
                            playableBehaviour.offsetPosition.x / rawImage.rectTransform.rect.width,
                            playableBehaviour.offsetPosition.y / rawImage.rectTransform.rect.height);
                        
                        compositeMaterial.SetFloat("_WigglePowerA",playableBehaviour.wiggle ? 1f:0f);
                        compositeMaterial.SetVector("_NoiseSeedA",playableBehaviour.noiseSeed);
                        compositeMaterial.SetVector("_NoiseScaleA",playableBehaviour.noiseScale);
                        compositeMaterial.SetFloat("_TimeScaleA",playableBehaviour.roughness);
                        compositeMaterial.SetVector("_RangeA",playableBehaviour.wiggleRange/100f);
                        compositeMaterial.SetVector("_OffsetPositionA",offsetPositionA);
                        
                        compositeMaterial.SetFloat("_WigglePowerB",playableBehaviour.wiggle ? 1f:0f);
                        compositeMaterial.SetVector("_NoiseSeedB",playableBehaviour.noiseSeed);
                        compositeMaterial.SetVector("_NoiseScaleB",playableBehaviour.noiseScale);
                        compositeMaterial.SetFloat("_TimeScaleB",playableBehaviour.roughness);
                        compositeMaterial.SetVector("_RangeB",playableBehaviour.wiggleRange/100f);
                        compositeMaterial.SetVector("_OffsetPositionB",offsetPositionA);
                    }
                   
                }
                else
                {
                    compositeMaterial.SetTexture("_TextureA",track.textureA);
                    compositeMaterial.SetFloat("_WigglePowerA",playableBehaviour.wiggle ? 1f:0f);
                    compositeMaterial.SetTexture("_TextureB",track.textureA);
                    compositeMaterial.SetFloat("_WigglePowerB",playableBehaviour.wiggle ? 1f:0f);
                    compositeMaterial.SetFloat("_CrossFade", inputWeight);
                        
                    var offsetPositionA = new Vector2(
                        playableBehaviour.offsetPosition.x / rawImage.rectTransform.rect.width,
                        playableBehaviour.offsetPosition.y / rawImage.rectTransform.rect.height);
                        
                    compositeMaterial.SetFloat("_WigglePowerA",playableBehaviour.wiggle ? 1f:0f);
                    compositeMaterial.SetVector("_NoiseSeedA",playableBehaviour.noiseSeed);
                    compositeMaterial.SetVector("_NoiseScaleA",playableBehaviour.noiseScale);
                    compositeMaterial.SetFloat("_TimeScaleA",playableBehaviour.roughness);
                    compositeMaterial.SetVector("_RangeA",playableBehaviour.wiggleRange/100f);
                    compositeMaterial.SetVector("_OffsetPositionA",offsetPositionA);
                        
                    compositeMaterial.SetFloat("_WigglePowerB",playableBehaviour.wiggle ? 1f:0f);
                    compositeMaterial.SetVector("_NoiseSeedB",playableBehaviour.noiseSeed);
                    compositeMaterial.SetVector("_NoiseScaleB",playableBehaviour.noiseScale);
                    compositeMaterial.SetFloat("_TimeScaleB",playableBehaviour.roughness);
                    compositeMaterial.SetVector("_RangeB",playableBehaviour.wiggleRange/100f);
                    compositeMaterial.SetVector("_OffsetPositionB",offsetPositionA);
                }

                // Debug.Log($"<color=#00BFFF>{inputWeight} {clip.displayName}</color>");
                break;
                
            }
            inputPort++;
        }
        
        
        
        
    }
    
}
