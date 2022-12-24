using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

namespace CameraLiveSwitcher
{

    public class CameraSwitcherClipInfo
    {
        public TimelineClip clip;
        public CameraSwitcherTimelineBehaviour behaviour;
        public float inputWeight;
        
        
    }
    public class CameraSwitcherTimelineMixerBehaviour : PlayableBehaviour
    {
        public TextMeshProUGUI debugText;
        private StringBuilder stringBuilder;

        public List<TimelineClip> timelineClips = new List<TimelineClip>();
        private CameraMixer cameraMixer;
        
        readonly List<CameraSwitcherClipInfo> clipInfoList = new List<CameraSwitcherClipInfo>();
        private TimelineAsset timelineAsset;
        private PlayableDirector director;
        private bool isFirstFrameHappened = false;
        // NOTE: This function is called at runtime and edit time.  Keep that in mind when setting the values of properties.
        public override void ProcessFrame(Playable playable, FrameData info, object playerData)
        {
            cameraMixer = playerData as CameraMixer;

            if (!cameraMixer)
                return;
            
            if(isFirstFrameHappened == false)
            {
                stringBuilder = new StringBuilder();
                director =   playable.GetGraph().GetResolver() as PlayableDirector;
                timelineAsset = director.playableAsset as TimelineAsset;

                isFirstFrameHappened = true;

                foreach (var clip in timelineClips)
                {
                    var cameraMixerTimelineClip = clip.asset as CameraSwitcherTimelineClip;
                    var cameraMixerTimelineBehaviour = cameraMixerTimelineClip.behaviour;
                    // remove same type of cameraPostProductions in cameraMixerTimelineBehaviour list
                    var v = cameraMixerTimelineBehaviour.cameraPostProductions.RemoveAll(x => cameraMixerTimelineBehaviour.cameraPostProductions.Any(y => y.GetType() == x.GetType() && x!= y));

                }
                
            }

            int inputCount = playable.GetInputCount();
            clipInfoList.Clear();
            cameraMixer.cameraList.Distinct();
            for (int i = 0; i < inputCount; i++)
            {
                var clip = timelineClips[i];
                float inputWeight = playable.GetInputWeight(i);
                ScriptPlayable<CameraSwitcherTimelineBehaviour> inputPlayable =
                    (ScriptPlayable<CameraSwitcherTimelineBehaviour>)playable.GetInput(i);
                CameraSwitcherTimelineBehaviour input = inputPlayable.GetBehaviour();
                input.cameraPostProductions.Distinct();
                if(input.camera)input.camera.enabled = inputWeight > 0;

                if (input.camera != null && cameraMixer.cameraList.Contains(input.camera) != true)
                {
                    cameraMixer.cameraList.Add(input.camera);
                }
                if (inputWeight > 0)
                {
                    clipInfoList.Add(new CameraSwitcherClipInfo()
                    {
                        clip = clip,
                        behaviour = input,
                        inputWeight = inputWeight
                    });
                }
                
            }
            
            Mix(clipInfoList);
            ApplyPostEffect(clipInfoList);

            if (debugText != null)
            {
                stringBuilder.Clear();
                var dateTime = TimeSpan.FromSeconds(director.time);
                stringBuilder.Append($"[{cameraMixer.gameObject.scene.name}]  ");
                // _stringBuilder.Append(" ");
                stringBuilder.Append(dateTime.ToString(@"hh\:mm\:ss\:ff"));
                stringBuilder.Append(" ");
                stringBuilder.Append((Mathf.CeilToInt((float)timelineAsset.editorSettings.frameRate * (float) director.time)));
                stringBuilder.Append("f  ");
                
                debugText.text = stringBuilder.ToString();
                // if (A != null && A.behaviour.camera != null) _stringBuilder.Append($"{A.behaviour.camera.name}");
                // if (B != null && B.behaviour.camera != null) _stringBuilder.Append($" >> {B.behaviour.camera.name}");

            }
        }
        
        public override void OnPlayableDestroy (Playable playable)
        {
            isFirstFrameHappened = false;
            if(cameraMixer)cameraMixer.useTimeline = false;
        }

        private void ApplyPostEffect(List<CameraSwitcherClipInfo> clipInfos)
        {
            foreach (var clipInfo in clipInfos)
            {
                foreach (var cameraPostProduction in clipInfo.behaviour.cameraPostProductions)
                {
                    cameraPostProduction.UpdateEffect(clipInfo.behaviour.camera);
                }
            }
        }

        private void Mix(List<CameraSwitcherClipInfo> clips)
        {
            if(cameraMixer == null) return;
            if(clips.Count<=0) return;
            // Debug.Log(clips.Count);

            if (clips.Count == 1)
            {
               
                if(clips[0].behaviour.camera == null) return;
                cameraMixer.SetCamera(clips[0].behaviour.camera,null,0);
            }

            if (clips.Count == 2)
            {
                
                // if(clips[0].behaviour.camera == null || clips[0].behaviour.camera == null) return;
                cameraMixer.SetCamera(clips[0].behaviour.camera, clips[1].behaviour.camera, 1f-clips[0].inputWeight);
            }
            
            
        }
    }
}