using System;
using System.Collections.Generic;using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Playables;

#if USE_URP
using UnityEngine.Rendering.Universal;
#elif USE_HDRP
using UnityEngine.Rendering.HighDefinition;
#endif
using UnityEngine.Timeline;
using Random = System.Random;


[Serializable]
public class DofControlProps
{
    [SerializeField] public DepthOfFieldMode depthOfFieldMode;
    [SerializeField] public float focusDistance;
    [SerializeField, Range(1, 300)] public float focalLength;
    [SerializeField, Range(1,32)] public float aperture;
    [SerializeField, Range(1,9)] public int bladeCount;
    [SerializeField, Range(0, 1)] public float bladeCurvature;
    [SerializeField, Range(-180, 180)] public float bladeRotation;
    [SerializeField] public float start;
    [SerializeField] public float end;
    [SerializeField, Range(0.5f, 1.5f)] public float maxRadius;
    [SerializeField] public bool highQualitySampling;
}

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
    [SerializeField] public DofControlProps dofControlProps;
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
