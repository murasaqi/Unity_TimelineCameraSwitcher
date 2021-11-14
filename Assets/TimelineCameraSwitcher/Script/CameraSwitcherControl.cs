
using System;
using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.Experimental.Rendering;

#if UNITY_EDITOR


#endif

[ExecuteInEditMode]
public class CameraSwitcherControl : MonoBehaviour
{
    
    
    [SerializeField] public CameraSwitcherSettings cameraSwitcherSettings;
    [SerializeField] public RenderTexture outPutTarget;
    private float m_fader;


    public void Blit()
    {
        if (outPutTarget)
        {
            Graphics.Blit(null,outPutTarget,cameraSwitcherSettings.material);
        }
    }
}
