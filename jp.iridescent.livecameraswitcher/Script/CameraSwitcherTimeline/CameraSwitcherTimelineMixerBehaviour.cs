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
        
        public CameraSwitcherClipInfo(TimelineClip clip, CameraSwitcherTimelineBehaviour behaviour, float inputWeight)
        {
            this.clip = clip;
            this.behaviour = behaviour;
            this.inputWeight = inputWeight;
        }
        
        
    }
    public class CameraSwitcherTimelineMixerBehaviour : PlayableBehaviour
    {
        public TextMeshProUGUI debugText;
        private StringBuilder stringBuilder;

        public List<TimelineClip> timelineClips = new List<TimelineClip>();
        private CameraMixer cameraMixer;
        
        readonly List<CameraSwitcherClipInfo> cameraQue = new List<CameraSwitcherClipInfo>();
        private TimelineAsset timelineAsset;
        internal PlayableDirector director;
        private bool isFirstFrameHappened = false;
        // NOTE: This function is called at runtime and edit time.  Keep that in mind when setting the values of properties.
        public override void ProcessFrame(Playable playable, FrameData info, object playerData)
        {
            cameraMixer = playerData as CameraMixer;

            if (!cameraMixer || !director)
                return;
            
            if(isFirstFrameHappened == false)
            {
                stringBuilder = new StringBuilder();
                
                timelineAsset = director.playableAsset as TimelineAsset;

                isFirstFrameHappened = true;

                cameraMixer.cameraList.Distinct();
                foreach (var clip in timelineClips)
                {
                    var cameraMixerTimelineClip = clip.asset as CameraSwitcherTimelineClip;
                    var cameraMixerTimelineBehaviour = cameraMixerTimelineClip.behaviour;
                    var v = cameraMixerTimelineBehaviour.cameraPostProductions.RemoveAll(x => cameraMixerTimelineBehaviour.cameraPostProductions.Any(y => y.GetType() == x.GetType() && x!= y));

                }
                
            }

            // var _director =   playable.GetGraph().GetResolver() as PlayableDirector;
            // Debug.Log($"{director.name}:{director.time},{_director.name}:{_director.time}");
            int inputCount = playable.GetInputCount();
            cameraQue.Clear();
            for (int i = 0; i < inputCount; i++)
            {
                var clip = timelineClips[i];
                float inputWeight = playable.GetInputWeight(i);
                ScriptPlayable<CameraSwitcherTimelineBehaviour> inputPlayable =
                    (ScriptPlayable<CameraSwitcherTimelineBehaviour>)playable.GetInput(i);
                CameraSwitcherTimelineBehaviour input = inputPlayable.GetBehaviour();
                input.cameraPostProductions.Distinct();
                
                
                if (input.camera != null && cameraMixer.cameraList.Contains(input.camera) != true)
                {
                    cameraMixer.cameraList.Add(input.camera);
                }

                if (inputWeight > 0)
                {
                    cameraQue.Add(new CameraSwitcherClipInfo(clip, input, inputWeight));
                }
                
                // if (clip.start <= director.time && director.time < clip.start + clip.duration)
                // {
                //     cameraQue.Add(new CameraSwitcherClipInfo(clip, input, inputWeight));
                //     Debug.Log($"time: {clip.displayName} {input.camera.name}, {clip.start}, {clip.duration}");
                //    
                // }
                
            }
            
            
            SetCameraQueue(cameraQue);
            ApplyPostEffect(cameraQue);

            if (debugText != null)
            {
                stringBuilder.Clear();
                var dateTime = TimeSpan.FromSeconds(director.time);
                stringBuilder.Append($"[{cameraMixer.gameObject.scene.name}]  ");
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

        private void SetCameraQueue(List<CameraSwitcherClipInfo> clips)
        {
            if(cameraMixer == null) return;
            if(clips.Count<=0) return;
            
            if (clips.Count == 1)
            {
               
                if(clips[0].behaviour.camera == null) return;
                cameraMixer.SetCameraQueue(clips[0].behaviour.camera,null,0);
            }
            else if (clips.Count == 2)
            {
                if(clips[0].behaviour.camera == null || clips[1].behaviour.camera == null) return;
                cameraMixer.SetCameraQueue(clips[0].behaviour.camera, clips[1].behaviour.camera, 1f-clips[0].inputWeight);
            }
            
            
        }
    }
}