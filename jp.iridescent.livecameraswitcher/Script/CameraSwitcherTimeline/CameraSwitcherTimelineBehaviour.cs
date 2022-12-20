using System;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;
using UnityEngine.Rendering;

namespace CameraLiveSwitcher
{

    [Serializable]
    public class CameraSwitcherTimelineBehaviour : PlayableBehaviour
    {
        public Camera camera;

        public override void OnPlayableCreate(Playable playable)
        {

        }
    }
}