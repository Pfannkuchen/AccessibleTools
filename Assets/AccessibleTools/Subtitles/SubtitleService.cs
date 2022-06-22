using System.Collections.Generic;
using AccessibleTools.Subtitles;
using UnityEngine;

namespace AccessibleTools
{
    public static class SubtitleService
    {
        private static SubtitleSettings _settings;
        public static SubtitleSettings Settings => _settings ??= new SubtitleSettings();


        private static SubtitleCanvas _canvas;
        private static SubtitleCanvas Canvas => _canvas ??= Object.Instantiate(Resources.Load<SubtitleCanvas>("SubtitleCanvasPrefab"));
        
        
        private static List<SubtitleLine> _lineQueue;
        private static List<SubtitleLine> LineQueue => _lineQueue ??= new List<SubtitleLine>();


        /// <summary>Returns if a SubtitleBundle or SubtitleLine is currently being shown.</summary>
        public static bool IsPlaying => (Canvas != null) && Canvas.IsPlaying;

        /// <summary>Returns the total duration in seconds until the contents of LineQueue have all been shown.</summary>
        public static float GetPlayTimeRemaining()
        {
            float completeDuration = 0f;

            for (int i = 0; i < LineQueue.Count; i++)
            {
                if (LineQueue[i] == null) continue;
                completeDuration += LineQueue[i].Duration;
            }

            return completeDuration;
        }

        
        public static void Play(SubtitleBundle bundle, int priority)
        {
            if(Canvas != null) Canvas.Play(bundle, priority);
        }
        
        public static void Pause()
        {
            if(Canvas != null) Canvas.Pause();
        }
        
        public static void UnPause()
        {
            if(Canvas != null) Canvas.UnPause();
        }

        public static void Stop()
        {
            if (Canvas != null) Canvas.Stop();
        }
    }
}