using UnityEngine;
using System.Collections;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class ReflectionProbeController : MonoBehaviour
{

    [SerializeField]
    ReflectionProbe probe;
    
    public Camera cam;

    [SerializeField]
    bool debug;

    void Start()
    {
        if(!probe)
        {
            probe = GetComponent<ReflectionProbe>();
        }
    }

    void Update()
    {
        
        if (!debug)
        {
            probe.transform.position = new Vector3(
                cam.transform.position.x,
                cam.transform.position.y * -1,
                cam.transform.position.z
            );
        }
        else
        {
#if UNITY_EDITOR
            var cam = SceneView.lastActiveSceneView.camera;
            probe.transform.position = new Vector3(
                cam.transform.position.x,
                cam.transform.position.y * -1,
                cam.transform.position.z
            );
#endif
        }



        probe.RenderProbe();
    }
}