// using System;
// using System.Collections.Generic;
// using UnityEditor;
// using UnityEditor.UIElements;
// using UnityEngine;
// using UnityEngine.Timeline;
// using UnityEngine.UIElements;
//
// namespace Editor
// {
//     public class SwitcherCameraRenamer:EditorWindow
//     {
//         
//         [MenuItem("Window/CameraSwitcher/SwitcherCameraRenamer")]
//         public static void ShowWindow()
//         {
//             GetWindow(typeof(SwitcherCameraRenamer));
//         }
//
//         public TimelineAsset timelineAsset;
//         public List<CameraSwitcher> cameraSwitchers = new List<CameraSwitcher>();
//         public void OnEnable()
//         {
//             var root = rootVisualElement;
//             var objectField = new ObjectField("Timeline");
//         
//             objectField.objectType = typeof(TimelineAsset);
//             objectField.RegisterValueChangedCallback(evt =>
//             {
//                 timelineAsset = evt.newValue as TimelineAsset;
//                 var tracks = timelineAsset.GetOutputTracks();
//                 foreach (var track in tracks)
//                 {
//                     if (track is CameraSwitcherControlTrack)
//                     {
//                         cameraSwitcherControlTracks.Add( track.name, track as CameraSwitcherControlTrack);
//                     }
//                 }
//             
//             
//             
//                 InitPopup();
//             });
//         
//             popupField = new PopupField<string>();
//
//             
//             
//         }
//
//         public static void Rename()
//         {
//             var cameras = GameObject.FindObjectsOfType<Camera>();
//             foreach (var camera in cameras)
//             {
//                 if (camera.gameObject.GetComponent<SwitcherCamera>() != null)
//                 {
//                     camera.gameObject.name = "SwitcherCamera";
//                 }
//             }
//         }
//         
//         
//             
//     }
// }