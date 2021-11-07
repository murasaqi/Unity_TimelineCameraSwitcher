using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[CreateAssetMenu(fileName = "CameraSwitcherSetting", menuName = "ScriptableObjects/CameraSwitcherSettings", order = 1)]
public class CameraSwitcherSettings : ScriptableObject
{
    // Start is called before the first frame update
    [SerializeField] public Material material;
    [SerializeField] public Shader shader;
    [SerializeField] public RenderTexture renderTextureA;
    [SerializeField] public RenderTexture renderTextureB;
    

}
