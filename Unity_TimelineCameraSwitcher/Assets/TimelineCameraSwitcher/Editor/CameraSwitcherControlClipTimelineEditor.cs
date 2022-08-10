using UnityEditor;
using UnityEditor.Timeline;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

[CustomTimelineEditor(typeof(CameraSwitcherControlClip))]
public class CameraSwitcherControlClipTimelineEditor: ClipEditor
{
    [InitializeOnLoad]
    class EditorInitialize
    {
        static EditorInitialize()
        {
            playableDirector = GetMasterDirector();  } 
        static PlayableDirector GetMasterDirector() { return TimelineEditor.masterDirector; }
    }

    private static PlayableDirector playableDirector;
    
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
    GUIContent kUndamped = new GUIContent("UNCACHED");
    
    public override void DrawBackground(TimelineClip clip, ClipBackgroundRegion region)
    {
        base.DrawBackground(clip, region);

        // if (Application.isPlaying || !TargetPositionCache.UseCache 
        //     || TargetPositionCache.CacheMode == TargetPositionCache.Mode.Disabled
        //     || TimelineEditor.inspectedDirector == null)
        // {
        //     return;
        // }
        //
        // // Draw the cache indicator over the cached region
        // var cacheRange = TargetPositionCache.CacheTimeRange;
        // if (!cacheRange.IsEmpty)
        // {
        //     cacheRange.Start = (float)TimelineGlobalToLocalTime(cacheRange.Start);
        //     cacheRange.End = (float)TimelineGlobalToLocalTime(cacheRange.End);
        //
        //     // Clip cacheRange to rect
        //     float start = (float)region.startTime;
        //     float end = (float)region.endTime;
        //     cacheRange.Start = Mathf.Max((float)clip.ToLocalTime(cacheRange.Start), start);
        //     cacheRange.End = Mathf.Min((float)clip.ToLocalTime(cacheRange.End), end);
        //         
        //     var r = region.position;
        //     var a = r.x + r.width * (cacheRange.Start - start) / (end - start);
        //     var b = r.x + r.width * (cacheRange.End - start) / (end - start);
        //     r.x = a; r.width = b-a;
        //     r.y += r.height; r.height *= 0.2f; r.y -= r.height;
        //     EditorGUI.DrawRect(r, new Color(0.1f, 0.2f, 0.8f, 0.6f));
        // }
        //
        // // Draw the "UNCACHED" indicator, if appropriate
        // if (!TargetPositionCache.IsRecording && !TargetPositionCache.CurrentPlaybackTimeValid)
        // {
        //     var r = region.position;
        //     var t = clip.ToLocalTime(TimelineGlobalToLocalTime(TimelineEditor.masterDirector.time));
        //     var pos = r.x + r.width 
        //         * (float)((t - region.startTime) / (region.endTime - region.startTime));
        //
        //     var s = EditorStyles.miniLabel.CalcSize(kUndamped);
        //     r.width = s.x; r.x = pos - r.width / 2;
        //     var c = GUI.color;
        //     GUI.color = Color.yellow;
        //     EditorGUI.LabelField(r, kUndamped, EditorStyles.miniLabel);
        //     GUI.color = c;
        // }
    }
    
}
