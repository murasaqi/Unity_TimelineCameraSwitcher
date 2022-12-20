using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;
using UnityEngine.Rendering;

namespace CameraLiveSwitcher
{
    [TrackColor(0.8042867f, 0.3647798f, 1f)]
    [TrackClipType(typeof(CameraSwitcherTimelineClip))]
    [TrackBindingType(typeof(CameraMixer))]
    public class CameraSwitcherTimelineTrack : TrackAsset
    {
        public override Playable CreateTrackMixer(PlayableGraph graph, GameObject go, int inputCount)
        {
            var mixer= ScriptPlayable<CameraSwitcherTimelineMixerBehaviour>.Create(graph, inputCount);
            mixer.GetBehaviour().timelineClips = m_Clips;
            return mixer;
        }
    }

}