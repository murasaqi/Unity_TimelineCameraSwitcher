using System;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Playables;

#if USE_HDRP
using UnityEngine.Rendering.HighDefinition;
#elif USE_URP
using UnityEngine.Rendering.Universal;
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

    public LookAtProps(LookAtProps lookAtProps = null)
    {
        if(lookAtProps != null)
        {
            IsActive = lookAtProps.IsActive;
            Weight = lookAtProps.Weight;
            Roll = lookAtProps.Roll;
            UseUpObject = lookAtProps.UseUpObject;
            Lock = lookAtProps.Lock;
            RotationAtReset = lookAtProps.RotationAtReset;
            RotationOffset = lookAtProps.RotationOffset;
        }
        else
        {
            IsActive = true;
            Weight = 1f;
            Roll = 0f;
            UseUpObject = false;
            Lock = false;
            RotationAtReset = Vector3.zero;
            RotationOffset = Vector3.zero;
        }
    }
    // [SerializeField] public List<ExposedReference<Transform>> Sources;

}

[Serializable]
public class WigglerProps
{
    [SerializeField] public Vector2 noiseSeed = Vector2.one;
    [SerializeField] public Vector2 noiseScale = Vector2.one;
    [SerializeField] public float roughness = 1;
    [SerializeField] public Vector2 wiggleRange  = new Vector2(5,5);

    public WigglerProps(WigglerProps wigglerProps = null)
    {
        if (wigglerProps != null)
        {
            noiseSeed = wigglerProps.noiseSeed;
            noiseScale = wigglerProps.noiseScale;
            roughness = wigglerProps.roughness;
            wiggleRange = wigglerProps.wiggleRange;
            
        }
        else
        {
            noiseSeed = Vector2.one;
            noiseScale = Vector2.one;
            roughness = 1;
            wiggleRange = new Vector2(5,5);
        }
    }
    // [SerializeField] public List<ExposedReference<Transform>> Sources;

}

[Serializable] 
public class FocusProps
{
    [SerializeField] public float focalLength = 30;
    public FocusProps(FocusProps focusProps = null)
    {
        if (focusProps != null)
        {
            focalLength = focusProps.focalLength;
        }
        else
        {
            focalLength = 30;
        }
    }
}


[Serializable]
public class PositionProps
{
    [SerializeField] public Vector3 position;
    
    public PositionProps(PositionProps positionProps = null)
    {
        if (positionProps != null)
        {
            position = positionProps.position;
        }
        else
        {
            position = Vector3.zero;
        }
    }   
}

[Serializable]
public class RotationProps
{
    [SerializeField] public Vector3 rotation;
  
    public RotationProps(RotationProps rotationProps = null)
    {
        if (rotationProps != null)
        {
            rotation = rotationProps.rotation;
        }
        else
        {
            rotation = Vector3.zero;
        }
    }   
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

    public ColorBlendProps(ColorBlendProps colorBlendProps = null)
    {
        if (colorBlendProps != null)
        {
            blendMode = colorBlendProps.blendMode;
            color = colorBlendProps.color;
        }
        else
        {
            blendMode = BlendMode.Multiply;
            color = Color.white;
        }
    }
}


[Serializable]
public class CameraSwitcherControlBehaviour : PlayableBehaviour
{
    
    // public Volume volume;
    [HideInInspector][SerializeField] public Camera camera;

#if USE_HDRP
    [HideInInspector] public HDAdditionalCameraData hdAdditionalCameraData;
#elif USE_URP
     [HideInInspector] public UniversalAdditionalCameraData universalAdditionalCameraData;
#endif
    [SerializeField] public Transform lookAtTarget;
    [SerializeField] public LookAtConstraint lookAtConstraint;
    public override void OnPlayableCreate (Playable playable)
    {
       
    }
}
