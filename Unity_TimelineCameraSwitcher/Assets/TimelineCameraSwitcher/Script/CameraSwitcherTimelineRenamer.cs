using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

[ExecuteAlways]
public class CameraSwitcherTimelineRenamer : MonoBehaviour
{

    [SerializeField] private PlayableDirector playableDirector;

    [SerializeField] private CameraSwitcherControl cameraSwitcherControl;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    [ContextMenu("Rename")]
    public void RenameAllTimelineClips()
    {
        if(playableDirector != null)
        {
            var timelineAsset = playableDirector.playableAsset as TimelineAsset;
            foreach (TrackAsset  trackAsset in timelineAsset.GetOutputTracks())
        
            if (trackAsset.GetType() == typeof(CameraSwitcherControlTrack))
            {
            
                var i = 0;
                foreach (var clip in
                         trackAsset.GetClips())
                {
                    var number = String.Format("{0:D2}", i);
                    var startFrame = String.Format("{0:D4}", Mathf.CeilToInt((float)( timelineAsset.editorSettings.frameRate*clip.start)));
                    var endFrame = String.Format("{0:D4}", Mathf.CeilToInt((float)(timelineAsset.editorSettings.frameRate*clip.end))-1);
                    clip.displayName = $"C{number}_{startFrame}_{endFrame}";
                    i++;
                }
             
            }
        }
    }

    [ContextMenu("RenameCameraByClipName")]
    public void RenameCameraByClipName()
    {
        var cameraClipDic = new Dictionary<Camera, string>();
        if(playableDirector == null) return;
        foreach (TrackAsset  trackAsset in (playableDirector.playableAsset as TimelineAsset).GetOutputTracks())
        {
            if (trackAsset.GetType() == typeof(CameraSwitcherControlTrack))
            {
                foreach (var clip in trackAsset.GetClips())
                {
                    CameraSwitcherControlClip asset = clip.asset as CameraSwitcherControlClip;
                    Debug.Log($"{asset.targetCamera.name},{clip.displayName}");
                    if (cameraClipDic.ContainsKey(asset.targetCamera))
                    {
                        cameraClipDic[asset.targetCamera] = $"{cameraClipDic[asset.targetCamera]}_{clip.displayName}";
                    }
                    else
                    {
                        cameraClipDic.Add(asset.targetCamera, clip.displayName);
                    }
                }
                   
            }
        }

        foreach (var camera in cameraClipDic.Keys)
        {
            camera.name = cameraClipDic[camera];
        }
        
        var childrens = new List<Transform>();
        foreach (Transform t in cameraSwitcherControl.transform)
        {
            childrens.Add(t);
        }
        childrens.Sort(delegate(Transform x, Transform y)
        {
            if (x == null && y == null) return 0;
           
            else return x.name.CompareTo(y.name);
        });


        var i = 0;
        foreach (var c in childrens)
        {
            c.SetSiblingIndex(i);
            i++;
        }
    }

    // アルファベット順に並べ替え
   
    static void SortByName()
    {
        Sort((a, b) => string.Compare(a.name, b.name));
    }
    
    static void Sort(Comparison<Transform> compare)
    {
        
        // foreach (Transform child in cameraSwitcherControl.gameObject.transform)
        // {

        var childrens = new List<Transform>();
       
        //     var sorted = cameraSwitcherControl.gameObject.transform.chil
        //     sorted.Sort(compare);
        //
        //     var indices = sorted.Select(s => s.GetSiblingIndex()).OrderBy(s => s).ToList();
        //
        //     for (int i = 0; i < sorted.Count; i++)
        //     {
        //         Undo.SetTransformParent(sorted[i], sorted[i].parent, "Sort");
        //         sorted[i].SetSiblingIndex(indices[i]);
        //     }
        // // }
    }
    // Update is called once per frame
    void Update()
    {
        
    }
}
