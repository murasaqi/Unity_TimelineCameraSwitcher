using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;
namespace CameraLiveSwitcher
{

    [CustomEditor(typeof(CameraSwitcherTimelineClip))]
    public class CameraSwitcherTimelineClipEditor:Editor
    {
        public override void OnInspectorGUI()
        {
            BeginInspector();
        }
        
        private void BeginInspector()
        {
            serializedObject.Update();
            
            var cameraSwitcherTimelineClip = serializedObject.targetObject  as CameraSwitcherTimelineClip;
            
            EditorGUI.BeginChangeCheck();
            EditorGUILayout.PropertyField(serializedObject.FindProperty("newExposedReference"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("behaviour"));
            if (EditorGUI.EndChangeCheck())
            {
                serializedObject.ApplyModifiedProperties();
            }
           
            
            DrawAddPropertyButton(cameraSwitcherTimelineClip);
            // DrawPropertyInInspector(stageLightProfile.FindProperty("stageLightProperties"));
        }
        
        private void DrawAddPropertyButton(CameraSwitcherTimelineClip cameraSwitcherTimelineClip)
        {
            EditorGUI.BeginChangeCheck();


            // var propertyTypes = SlmUtility.GetTypes(typeof(SlmAdditionalProperty));

            // propertyTypes.Remove(typeof(RollProperty));
            var selectList = new List<string>();
            
            CameraLiveSwitcherUtility.CameraPostProductionTypes.ForEach(t =>
            {
                if(t != typeof(CameraPostProductionBase))selectList.Add(t.Name);
            });
            
            
            
            // var typeDict = new Dictionary<string, Type>();
            
            selectList.Insert(0,"Add Effect");
            foreach (var property in cameraSwitcherTimelineClip.behaviour.cameraPostProductions
                         )
            {
                if(property == null) continue;
                if (selectList.Find(x => x== property.GetType().Name) != null)
                {
                    selectList.Remove(property.GetType().Name);
                }
                    
                
            }
            EditorGUI.BeginDisabledGroup(selectList.Count  <= 1);
            var select = EditorGUILayout.Popup(0, selectList.ToArray());
            EditorGUI.EndDisabledGroup();
            if (EditorGUI.EndChangeCheck())
            {
                var type = CameraLiveSwitcherUtility.GetTypeByClassName(selectList[select]);
                var property = Activator.CreateInstance(type) as CameraPostProductionBase;
                cameraSwitcherTimelineClip.behaviour.cameraPostProductions.Add(property);
            }
            
            
        }
    }
}