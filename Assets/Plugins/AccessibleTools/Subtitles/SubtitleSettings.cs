using System;

namespace AccessibleTools.Subtitles
{
    public class SubtitleSettings
    {
        private bool _showSubtitles = true;
        public bool ShowSubtitles
        {
            get => _showSubtitles;
            set
            {
                _showSubtitles = value;
                ShowSubtitlesChanged?.Invoke(_showSubtitles);
            }
        }
        public Action<bool> ShowSubtitlesChanged;
        
        
        private bool _showClosedCaptions = false;
        public bool ShowClosedCaptions
        {
            get => _showClosedCaptions;
            set
            {
                _showClosedCaptions = value;
                ShowClosedCaptionsChanged?.Invoke(_showClosedCaptions);
            }
        }
        public Action<bool> ShowClosedCaptionsChanged;
        
        
        private bool _showSpeakers = true;
        public bool ShowSpeakers
        {
            get => _showSpeakers;
            set
            {
                _showSpeakers = value;
                ShowSpeakersChanged?.Invoke(_showSpeakers);
            }
        }
        public Action<bool> ShowSpeakersChanged;
        
        
        private bool _colorizeSpeakers = true;
        public bool ColorizeSpeakers
        {
            get => _colorizeSpeakers;
            set
            {
                _colorizeSpeakers = value;
                ColorizeSpeakersChanged?.Invoke(_showClosedCaptions);
            }
        }
        public Action<bool> ColorizeSpeakersChanged;
        
        
        private float _subtitleBackgroundAlpha = 0.5f;
        public float SubtitleBackgroundAlpha
        {
            get => _subtitleBackgroundAlpha;
            set
            {
                _subtitleBackgroundAlpha = value;
                SubtitleBackgroundAlphaChanged?.Invoke(_subtitleBackgroundAlpha);
            }
        }
        public Action<float> SubtitleBackgroundAlphaChanged;
    }
}