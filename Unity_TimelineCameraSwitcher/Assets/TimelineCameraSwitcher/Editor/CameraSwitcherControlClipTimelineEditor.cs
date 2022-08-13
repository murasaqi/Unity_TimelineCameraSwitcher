#if UNITY_EDITOR
using System;
using UnityEditor;
using UnityEditor.Timeline;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;
using UnityEngine.UIElements;

[CustomTimelineEditor(typeof(CameraSwitcherControlClip))]
public class CameraSwitcherControlClipTimelineEditor: ClipEditor
{
    [InitializeOnLoad]
    class EditorInitialize
    {
        static EditorInitialize()
        {
            playableDirector = GetMasterDirector();  
           
        } 
        static PlayableDirector GetMasterDirector() { return TimelineEditor.masterDirector; }
    }

    public bool forceUpdate = false;
    private static PlayableDirector playableDirector;
    private Texture2D thumbnailTexture2d;
    private GUIStyle timeCodeStyle = new GUIStyle()
    {
        alignment = TextAnchor.MiddleLeft,
        fontSize = 12,
        normal = new GUIStyleState() { textColor = Color.gray },
        richText = true,
        stretchWidth = true,
        wordWrap = true
    };
    
    public override ClipDrawOptions GetClipOptions(TimelineClip clip)
    {
        var cameraSwitcherControlClip = (CameraSwitcherControlClip) clip.asset;
        var clipOptions = base.GetClipOptions(clip);
        if (cameraSwitcherControlClip != null)
        {
            var director = TimelineEditor.inspectedDirector;
            if (director != null)
            {
                cameraSwitcherControlClip.template.camera =  cameraSwitcherControlClip.camera.Resolve(director);
                var cam = cameraSwitcherControlClip.template.camera;
                if (cam == null)
                    clipOptions.errorText = "A virtual camera must be assigned";
                else
                    clipOptions.tooltip = cam.name;

                
            }
        }
        return clipOptions;
    }
    
    public override void OnClipChanged(TimelineClip clip)
    {
        
        var cameraSwitcherControlClip = (CameraSwitcherControlClip)clip.asset;
        if (cameraSwitcherControlClip == null)
            return;
        if (cameraSwitcherControlClip.template.camera != null)
            clip.displayName = cameraSwitcherControlClip.template.camera.name;
        else
        {
            var director = TimelineEditor.inspectedDirector;
            if (director != null)
            {
                cameraSwitcherControlClip.template.camera = cameraSwitcherControlClip.camera.Resolve(director);
                if (cameraSwitcherControlClip.template.camera != null)
                    clip.displayName = cameraSwitcherControlClip.template.camera.name;
                
            }
            
            
        }
    }
    
    
    
    public override void OnCreate(TimelineClip clip, TrackAsset track, TimelineClip clonedFrom)
    {
        base.OnCreate(clip, track, clonedFrom);
       
    }

    private static string GetTimeCode(double time, float fps, DrawTimeMode drawTimeMode)
    {
        var dateTime = TimeSpan.FromSeconds( time );
        switch (drawTimeMode)
        {
            
            case DrawTimeMode.Duration:
                return String.Format("{0:0.##} s", time);
            
            case DrawTimeMode.Frame:
                var frameCount = (int) (time * fps);
                return $"{frameCount} f";
            case DrawTimeMode.TimeCode:
                return dateTime.ToString(@"mm\:ss\:ff");
            
        }
        

        return "";
    }

    public override void DrawBackground(TimelineClip clip, ClipBackgroundRegion region)
    {
        

        var cameraSwitcherControlClip = clip.asset as CameraSwitcherControlClip;
       var drawTimeMode = cameraSwitcherControlClip.drawTimeMode;
       
        if (cameraSwitcherControlClip.thumbnailRenderTexture != null)
        {
            GUI.DrawTexture(region.position, cameraSwitcherControlClip.thumbnailRenderTexture);
        }

        if (drawTimeMode != DrawTimeMode.None)
        {
            var fps = (float)clip.GetParentTrack().timelineAsset.editorSettings.frameRate;
            var r = new Rect(region.position.x+10,  region.position.y+region.position.height*0.4f, region.position.width, region.position.height);
            GUI.Label(r,GetTimeCode(clip.start,fps,drawTimeMode),timeCodeStyle);

        
            var endFrameText = GetTimeCode(clip.end,fps,drawTimeMode);
            r = new Rect(region.position.x+region.position.width-endFrameText.Length*8, region.position.y-region.position.height*0.4f, region.position.width, region.position.height);
            GUI.Label(r,endFrameText,timeCodeStyle);    
        }
        
    }
    
    
    
}

#endif