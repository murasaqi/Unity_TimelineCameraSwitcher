using System;
using System.Collections.Generic;using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Playables;
using UnityEngine.Rendering;
using UnityEngine.Serialization;

#if USE_URP
using UnityEngine.Rendering.Universal;
#elif USE_HDRP
using UnityEngine.Rendering.HighDefinition;
#endif
using UnityEngine.Timeline;
using Random = System.Random;


[Serializable]
public class BokehProp
{
    // [SerializeField] public DepthOfFieldMode depthOfFieldMode;
    [SerializeField] public float focusDistance = 1;
    [SerializeField, Range(1, 300)] public float focalLength = 20;
    [SerializeField, Range(1,32)] public float aperture = 10;
    [SerializeField, Range(1,9)] public int bladeCount =4;
    [SerializeField, Range(0, 1)] public float bladeCurvature = 0;
    [SerializeField, Range(-180, 180)] public float bladeRotation = 0;

    public void Reset()
    {
        focusDistance = 0;
        focalLength = 0; 
        aperture = 0;
        bladeCount =4;
        bladeCurvature = 0;
        bladeRotation = 0;
    }
}

[Serializable]
public class GaussianProp
{
    [SerializeField] public float start =1;
    [SerializeField] public float end =10;
    [SerializeField, Range(0.5f, 1.5f)] public float maxRadius = 1;
    [SerializeField] public bool highQualitySampling = false;

    public void Reset()
    {
        start =0;
        end =0;
        maxRadius = 0;
        highQualitySampling = false;
    }
}

[Serializable]
public enum QualitySetting
{
    Custom = 3,
    Low = 0,
    Medium = 1,
    High =2
}

[Serializable]
public class BluerProps
{
    [Range(3,10)]public int sampleCount =5;
    [Range(0,8)]public float maxRadius = 0;
}

[Serializable]
public class RangeProps
{
    public float start =0;
    public float end = 10;
}



#if USE_HDRP
[Serializable]
public class PhysicalCameraProps
{
    // [SerializeField] public DepthOfFieldMode depthOfFieldMode;

    public void Reset()
    {
        nearBluer.sampleCount = 0;
        nearBluer.maxRadius = 0;
        farBluer.sampleCount = 0;
        farBluer.maxRadius = 0;
        focusLength = 0f;
        focusDistance = 0f;
    }
    [SerializeField] public FocusDistanceMode focusDistanceMode = FocusDistanceMode.Volume;
    [SerializeField] public float focusDistance = 10;
    [SerializeField] public float focusLength = 50;
    [SerializeField] public QualitySetting quality = QualitySetting.Custom;
    [SerializeField] public BluerProps nearBluer =new BluerProps()
    {
        sampleCount = 4,
        maxRadius = 5,
    };
    [SerializeField] public BluerProps farBluer =new BluerProps()
    {
        sampleCount = 9,
        maxRadius = 8,
    };
    
}
[Serializable]
public class ManualRangeProps
{
    
    public void Reset()
    {
        farRange.start = 0;
        farRange.end = 0;
        nearRange.start = 0;
        nearRange.end = 0;
        nearBluer.sampleCount = 0;
        nearBluer.maxRadius = 0;
        farBluer.sampleCount = 0;
        farBluer.maxRadius = 0;
        focusLength = 0f;
        focusDistance = 0f;
    }
    [SerializeField] public float focusDistance = 10;
    [SerializeField] public float focusLength = 50;
    [SerializeField] public RangeProps nearRange = new RangeProps();
    [SerializeField] public RangeProps farRange = new RangeProps()
    {
        start = 10,
        end =  20
    };
    [SerializeField] public QualitySetting quality = QualitySetting.Custom;
    [SerializeField] public BluerProps nearBluer =new BluerProps()
    {
        sampleCount = 4,
        maxRadius = 5,
    };
    [SerializeField] public BluerProps farBluer =new BluerProps()
    {
        sampleCount = 9,
        maxRadius = 8,
    };
    
}

#endif

[Serializable]
public class LookAtProps
{
    [SerializeField] public ExposedReference<Transform> target;
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
    [HideInInspector][SerializeField] public Camera camera;
    
    [SerializeField] public bool wiggle;
    [SerializeField] public WigglerProps wigglerProps;
    [SerializeField] public bool dofOverride = false;
    [SerializeField] public VolumeProfile volumeProfile;
    [HideInInspector] public DepthOfFieldMode mode;
    [SerializeField] public int volumeLayerMaskIndex = 0;
    [HideInInspector] public List<string> volumeLayerListNames = new List<string>(){"default",{"A"},{"B"}};
#if USE_URP
    [SerializeField] public BokehProp bokehProps;
    [SerializeField] public GaussianProp gaussianProps;
#elif USE_HDRP
    [HideInInspector] public HDAdditionalCameraData hdAdditionalCameraData;

    [SerializeField] public PhysicalCameraProps physicalCameraProps;
    [SerializeField] public ManualRangeProps manualRangeProps;
#endif
    // [SerializeField] public bool fadeCurveOverride = false;
    [SerializeField] public bool lookAt;
    [SerializeField] public LookAtProps lookAtProps;

    [SerializeField] public bool colorBlend;
    [SerializeField] public ColorBlendProps colorBlendProps;
    // public AnimationCurve fadeCurve = new AnimationCurve(new Keyframe(0, 0), new Keyframe(1, 1));
    public override void OnPlayableCreate (Playable playable)
    {
       
    }
}
