using System;
using System.Collections.Generic;
using System.Linq;
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

        public List<TimelineClip> timelineClips = new List<TimelineClip>();
        private CameraMixer cameraMixer;
        readonly List<CameraSwitcherClipInfo> clipInfoList = new List<CameraSwitcherClipInfo>();

        private bool isFirstFrameHappened = false;
        // NOTE: This function is called at runtime and edit time.  Keep that in mind when setting the values of properties.
        public override void ProcessFrame(Playable playable, FrameData info, object playerData)
        {
            cameraMixer = playerData as CameraMixer;

            if (!cameraMixer)
                return;
            
            if(isFirstFrameHappened == false)
            {
                isFirstFrameHappened = true;
                
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

                input.camera.enabled = inputWeight > 0;

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
                    // Debug.Log(clip);
                }
                
            }
            
            Mix(clipInfoList);
            cameraMixer.Render();
            cameraMixer.useTimeline = true;
        }
        
        public override void OnPlayableDestroy (Playable playable)
        {
            isFirstFrameHappened = false;
            if(cameraMixer)cameraMixer.useTimeline = false;
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