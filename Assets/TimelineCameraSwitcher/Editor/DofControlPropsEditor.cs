using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;

#if USE_URP
using UnityEngine.Rendering.Universal;
#elif USE_HDRP
using UnityEngine.Rendering.HighDefinition;
#endif
using UnityEngine.UIElements;


[CustomEditor(typeof(DofControlProps))]
public class DofControlPropsEditor : Editor
{
    // Start is called before the first frame update
    public override VisualElement CreateInspectorGUI()
    {
        // DofControlProps dofControlProps = target as DofControlProps;
        var treeAsset = Resources.Load<VisualTreeAsset>("CameraSwitcherControlResources/DofParameters");
        var container = treeAsset.Instantiate();

        var modeField = container.Q<EnumField>("DepthOfFieldMode");
        var bokeh = container.Q<Foldout>("Bokeh");
        var gaussian = container.Q<Foldout>("Gaussian");
        var mode = (DepthOfFieldMode) modeField.value;

        CheckDofMode(bokeh,gaussian,mode);
        modeField.RegisterValueChangedCallback((evt => CheckDofMode(bokeh, gaussian, (DepthOfFieldMode) modeField.value)));
        
        return container;
    }

    private void CheckDofMode(Foldout bokeh, Foldout gaussian, DepthOfFieldMode mode)
    {
#if USE_URP
        bokeh.value = mode == DepthOfFieldMode.Bokeh;
        bokeh.SetEnabled(mode == DepthOfFieldMode.Bokeh);
        gaussian.value = mode == DepthOfFieldMode.Gaussian;
        gaussian.SetEnabled(mode == DepthOfFieldMode.Gaussian);
#elif USE_HDRP
#endif

    }

    // Update is called once per frame
    
}