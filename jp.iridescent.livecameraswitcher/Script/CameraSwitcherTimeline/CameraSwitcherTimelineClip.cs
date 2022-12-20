using System;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;
using UnityEngine.Rendering;

namespace CameraLiveSwitcher
{

    [Serializable]
    public class CameraSwitcherTimelineClip : PlayableAsset, ITimelineClipAsset
    {
        public CameraSwitcherTimelineBehaviour behaviour = new CameraSwitcherTimelineBehaviour();
        public ExposedReference<Camera> newExposedReference;
        // public Camera camera;
        public ClipCaps clipCaps
        {
            get { return ClipCaps.Blending; }
        }

        public override Playable CreatePlayable(PlayableGraph graph, GameObject owner)
        {
            var playable = ScriptPlayable<CameraSwitcherTimelineBehaviour>.Create(graph, behaviour);
            CameraSwitcherTimelineBehaviour clone = playable.GetBehaviour();
            clone.camera = newExposedReference.Resolve(graph.GetResolver());
            return playable;
        }
    }

}