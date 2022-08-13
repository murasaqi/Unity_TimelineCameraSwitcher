using System;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Playables;

#if USE_URP
using UnityEngine.Rendering.Universal;
#elif USE_HDRP
using UnityEngine.Rendering.HighDefinition;
#endif

[Serializable]
public class LookAtProps
{
    // [SerializeField] public Transform target;
    [SerializeField] public bool IsActive = false;
    [SerializeField, Range(0f,1f)] public float Weight;
    [SerializeField] public float Roll;
    [SerializeField] public bool UseUpObject = false;
    // [SerializeField] public tra
    [SerializeField] public bool Lock = false;
    [SerializeField] public Vector3 RotationAtReset;
    [SerializeField] public Vector3 RotationOffset;
    // [SerializeField] public List<ExposedReference<Transform>> Sources;

}

[Serializable]
public class WigglerProps
{
    [SerializeField] public Vector2 noiseSeed = Vector2.one;
    [SerializeField] public Vector2 noiseScale = Vector2.one;
    [SerializeField] public float roughness = 1;
    [SerializeField] public Vector2 wiggleRange  = new Vector2(5,5);
    // [SerializeField] public List<ExposedReference<Transform>> Sources;

}

[Serializable]
public enum BlendMode
{
    Multiply,
    Add,
    Sub,
    Overwrite
}

[Serializable]
public class ColorBlendProps
{
    public BlendMode blendMode = BlendMode.Multiply;
    public Color color = Color.white;
}


[Serializable]
public class CameraSwitcherControlBehaviour : PlayableBehaviour
{
    
    // public Volume volume;
    [HideInInspector][SerializeField] public Camera camera;
  
#if USE_URP
     [HideInInspector] public UniversalAdditionalCameraData universalAdditionalCameraData;
#elif USE_HDRP
#endif
    [SerializeField] public Transform lookAtTarget;
    [SerializeField] public LookAtConstraint lookAtConstraint;
    public override void OnPlayableCreate (Playable playable)
    {
       
    }
}
