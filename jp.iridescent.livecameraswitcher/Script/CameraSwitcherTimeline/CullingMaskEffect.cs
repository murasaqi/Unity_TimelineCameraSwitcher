using System;
using UnityEngine;

namespace CameraLiveSwitcher
{
    [Serializable]
    public class CullingMaskEffect:CameraPostProductionBase
    {
        // [HideInInspector]public Camera camera;
        public LayerMask cullingMask = -1;

        public override void UpdateEffect(Camera camera)
        {
            if(camera == null || cullingMask == null)
                return;
            #if USE_HDRP
            var hdrpCamera = camera.GetComponent<UnityEngine.Rendering.HighDefinition.HDAdditionalCameraData>();
            camera.cullingMask = cullingMask;
            #endif
        }
        
        public override void Initialize()
        {
        }
        
    }
}