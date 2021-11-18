using System;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

[Serializable]
public class CameraSwitcherControlBehaviour : PlayableBehaviour
{
    public bool wiggle;
    [HideInInspector][SerializeField] public Camera camera;
    [SerializeField] public Vector2 noiseSeed = Vector2.one;
    [SerializeField] public Vector2 noiseScale = Vector2.one;
    [SerializeField] public float roughness = 1;
    [SerializeField] public Vector2 wiggleRange  = new Vector2(5,5);
    // [SerializeField] public Vector2 offsetPosition = Vector2.zero;
    public override void OnPlayableCreate (Playable playable)
    {
       
    }
}
