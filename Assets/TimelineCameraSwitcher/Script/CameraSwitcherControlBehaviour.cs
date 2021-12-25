using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Playables;
using UnityEngine.Rendering.Universal;
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
}

[Serializable]
public class LookAtProps
{
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
public class CameraSwitcherControlBehaviour : PlayableBehaviour
{
    
    public bool wiggle;
    [HideInInspector][SerializeField] public Camera camera;
    [HideInInspector][SerializeField] public Transform target;
    [SerializeField] public Vector2 noiseSeed = Vector2.one;
    [SerializeField] public Vector2 noiseScale = Vector2.one;
    [SerializeField] public float roughness = 1;
    [SerializeField] public Vector2 wiggleRange  = new Vector2(5,5);
    [SerializeField] public bool dofOverride = false;
    // [SerializeField] public DepthOfFieldMode depthOfFieldMode;
    // [SerializeField] public float focusDistance;
    // [SerializeField, Range(1, 300)] public float focalLength;
    // [SerializeField, Range(1,32)] public float aperture;
    // [SerializeField, Range(1,9)] public int bladeCount;
    // [SerializeField, Range(0, 1)] public float bladeCurvature;
    // [SerializeField, Range(-180, 180)] public float bladeRotation;

    [SerializeField] public DofControlProps dofControlProps;
    [SerializeField] public LookAtProps lookAtProps;
    
    // [SerializeField] public Vector2 offsetPosition = Vector2.zero;
    public override void OnPlayableCreate (Playable playable)
    {
       
    }
}
