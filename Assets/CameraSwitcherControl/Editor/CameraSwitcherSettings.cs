using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[CreateAssetMenu(fileName = "CameraSwitcherSetting", menuName = "ScriptableObjects/CameraSwitcherSettings", order = 1)]
public class CameraSwitcherSettings : ScriptableObject
{
    // Start is called before the first frame update
    [SerializeField] public Material material;
    [SerializeField] public Shader compositeShader;
    // [SerializeField] public Shader wigglerShader;
    [SerializeField] public int preRenderingFrameCount = 3;
    [SerializeField] public RenderTexture renderTextureA;
    [SerializeField] public RenderTexture renderTextureB;
    [SerializeField] public Vector2 resolution = new Vector2(1920, 1080);


}
