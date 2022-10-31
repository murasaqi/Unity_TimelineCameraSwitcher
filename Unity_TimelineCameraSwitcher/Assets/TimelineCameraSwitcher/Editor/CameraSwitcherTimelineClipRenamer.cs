using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;
using UnityEngine.UIElements;

public class CameraSwitcherTimelineClipRenamer : EditorWindow
{
    
    [MenuItem("Window/CameraSwitcher/Timeline Clip Renamer")]
    public static void ShowWindow()
    {
        var window = GetWindow(typeof(CameraSwitcherTimelineClipRenamer));
        window.titleContent = new GUIContent("Camera Renamer");
    }

    public TimelineAsset timelineAsset;
    
    public PopupField<string> popupField;
    
    public CameraSwitcherControlTrack targetTrack;
    // public CameraSwitcherControlTrack cameraSwitcherControlTrack;
    public Button renameButton;
    
    // public Button renameCameraButton;
    private Dictionary<string, CameraSwitcherControlTrack> cameraSwitcherControlTracks = new Dictionary<string, CameraSwitcherControlTrack>();
    // public List<CameraSwitcherControlTrack> cameraSwitcherControlTracks = new List<CameraSwitcherControlTrack>();
    public void OnEnable()
    {

        var root = rootVisualElement;
        
        var objectField = new ObjectField("Playable Director");
        
        objectField.objectType = typeof(TimelineAsset);
        objectField.RegisterValueChangedCallback(evt =>
        {
            timelineAsset = evt.newValue as TimelineAsset;
            var tracks = timelineAsset.GetOutputTracks();
            foreach (var track in tracks)
            {
                if (track is CameraSwitcherControlTrack)
                {
                    cameraSwitcherControlTracks.Add( track.name, track as CameraSwitcherControlTrack);
                }
            }
            
            
            
            InitPopup();
        });
        
        popupField = new PopupField<string>();
        
        renameButton = new Button(() =>
        {
            if(targetTrack != null)RenameAllTimelineClips(targetTrack);
            
        });
        
        if(targetTrack != null)renameButton.text = "Rename " + targetTrack.name + " Camera";
        else renameButton.text = "Rename";
        
        renameButton.SetEnabled(false);
        
        
     
       
        root.Add(objectField);
        root.Add(popupField);
        
        root.Add(renameButton);
        
    }
    
    public void InitPopup()
    {

        if (cameraSwitcherControlTracks.Count > 0)
        {
            renameButton.SetEnabled(true);
            var options = new List<string>();
            foreach (var cameraSwitcherControlTrack in cameraSwitcherControlTracks)
            {
                options.Add(cameraSwitcherControlTrack.Key);
            }
            popupField.choices = options;
            popupField.RegisterValueChangedCallback(evt =>
            {
                targetTrack = cameraSwitcherControlTracks[evt.newValue];
            });
        }
        else
        {
            renameButton.SetEnabled(false);
            popupField.Clear();
        }
     
        
    }
    
    
    
    
    private Dictionary<CameraSwitcherControlClip,string> newClipNameDict = new Dictionary<CameraSwitcherControlClip,string>();
    [ContextMenu("Rename")]
    public void RenameAllTimelineClips(CameraSwitcherControlTrack cameraSwitcherControlTrack)
    {
        newClipNameDict.Clear();
        var i = 0;
        foreach (var clip in
                 cameraSwitcherControlTrack.GetClips())
        {
            var number = String.Format("{0:D2}", i);
            var startFrame = String.Format("{0:D4}", Mathf.CeilToInt((float)( timelineAsset.editorSettings.frameRate*clip.start)));
            var endFrame = String.Format("{0:D4}", Mathf.CeilToInt((float)(timelineAsset.editorSettings.frameRate*clip.end))-1);
            // clip.displayName = $"C{number}_{startFrame}_{endFrame}";
            // clip.asset.name = clip.displayName;
            // EditorUtility.SetDirty(clip.asset);
            // AssetDatabase.SaveAssets();
            newClipNameDict.Add(clip.asset as CameraSwitcherControlClip,$"C{number}_{startFrame}_{endFrame}");
            i++;
        }
        
        RenameCameraByClipName();
    }

    
    [ContextMenu("RenameCameraByClipName")]
    public void RenameCameraByClipName()
    {
        var cameraClipDic = new Dictionary<Camera, string>();
        var clips = targetTrack.GetClips();
        foreach (var clip in clips)
        {
            CameraSwitcherControlClip asset = clip.asset as CameraSwitcherControlClip;
            Debug.Log($"{asset.template.camera.name},{clip.displayName}");
            
            var newCameraName = newClipNameDict[asset];
            if (cameraClipDic.ContainsKey(asset.template.camera))
            {
                cameraClipDic[asset.template.camera] = $"{cameraClipDic[asset.template.camera]}_{newCameraName}";
            }
            else
            {
                
                cameraClipDic.Add(asset.template.camera,newCameraName);
            }
        }

        foreach (var camera in cameraClipDic.Keys)
        {
            camera.name = cameraClipDic[camera];
        }
        
    }

    public void SortTransform(CameraSwitcherControl cameraSwitcherControl)
    {
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
}
