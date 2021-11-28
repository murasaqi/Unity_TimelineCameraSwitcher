using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[CreateAssetMenu(fileName = "CameraSwitcherSetting", menuName = "ScriptableObjects/CameraSwitcherSettings", order = 1)]
public class CameraSwitcherSettings : ScriptableObject
{
    // Start is called before the first frame update
    [SerializeField] public Material material;
    [SerializeField] public Shader compositeShader;
    [SerializeField] public RenderTextureFormat renderTextureFormat = RenderTextureFormat.ARGB64;
    [SerializeField] public int preRenderingFrameCount = 3;
    [SerializeField] public RenderTexture renderTextureA;
    [SerializeField] public RenderTexture renderTextureB;
    [SerializeField] public Vector2Int resolution = new Vector2Int(1920, 1080);
    [SerializeField] public DepthList depth = DepthList.AtLeast24_WidthStencil;


}
