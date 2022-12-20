using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;
using UnityEngine.Rendering;

namespace CameraLiveSwitcher
{

    [Serializable]
    public class CameraSwitcherTimelineBehaviour : PlayableBehaviour
    {
        [HideInInspector]public Camera camera;
        [SerializeReference] public List<CameraPostProductionBase> cameraPostProductions = new List<CameraPostProductionBase>();
        public override void OnPlayableCreate(Playable playable)
        {
           
        }
    }
}