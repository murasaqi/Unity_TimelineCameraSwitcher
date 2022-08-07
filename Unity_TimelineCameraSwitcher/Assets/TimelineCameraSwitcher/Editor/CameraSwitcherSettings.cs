using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
// using UnityEngine.Rendering.Universal;

// [CreateAssetMenu(fileName = "CameraSwitcherSetting", menuName = "ScriptableObjects/CameraSwitcherControlSettings", order = 1)]
public class CameraSwitcherSettings : ScriptableObject
{
    // Start is called before the first frame update
    [SerializeField] public Material material;
    [SerializeField] public RenderTexture renderTextureA;
    [SerializeField] public RenderTexture renderTextureB;
    [SerializeField] public VolumeProfile volume;
    [SerializeField] public Vector2Int resolution = new Vector2Int(1920,1080);
    [SerializeField] public List<Vector2Int> resolutionList = new List<Vector2Int>()
    {
        new Vector2Int(1280,720),
        new Vector2Int(1600,900),
        new Vector2Int(1920,1080),
        new Vector2Int(2560,1440),
        new Vector2Int(3840,2160),
    };

    [SerializeField] public DepthList depthList =DepthList.AtLeast24_WidthStencil;

}
