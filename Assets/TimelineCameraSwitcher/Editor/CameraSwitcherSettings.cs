using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

// [CreateAssetMenu(fileName = "CameraSwitcherSetting", menuName = "ScriptableObjects/CameraSwitcherControlSettings", order = 1)]
public class CameraSwitcherSettings : ScriptableObject
{
    // Start is called before the first frame update
    [SerializeField] public Material material;
    [SerializeField] public RenderTexture renderTextureA;
    [SerializeField] public RenderTexture renderTextureB;
    [SerializeField] public Volume volume;

}
