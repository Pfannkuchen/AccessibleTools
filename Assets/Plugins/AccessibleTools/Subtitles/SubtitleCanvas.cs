using UnityEngine;
using TMPro;

namespace AccessibleTools.Subtitles
{
    public class SubtitleCanvas : MonoBehaviour
    {
        [Tooltip("Reference to the text background.")]
        [SerializeField] private TMP_Text _textBackground;
        private TMP_Text TextBackground => _textBackground;
        
        [Tooltip("Reference to the readable text content.")]
        [SerializeField] private TMP_Text _textContent;
        private TMP_Text TextContent => _textContent;

        
        /// <summary>
        /// Returns if the SubtitleCanvas is currently displaying a SubtitleBundle.
        /// </summary>
        public bool IsPlaying => _currentBundle != null;
        
        
        // runtime
        private SubtitleBundle _currentBundle;
        private int _currentPriority;
        private SubtitleLine _currentLine;
        private float _currentTimeStamp;
        private bool _paused;

        
        // cached settings
        private bool _showSubtitles;
        private bool _showClosedCaptions;
        private bool _showSpeakers;
        private bool _colorizeSpeakers;
        private float _backgroundAlpha;


        public void OnEnable()
        {
            Stop();

            SubtitleService.Settings.ShowSubtitlesChanged += OnShowSubtitlesChanged;
            OnShowSubtitlesChanged(SubtitleService.Settings.ShowSubtitles);
            
            SubtitleService.Settings.ShowClosedCaptionsChanged += OnShowClosedCaptionsChanged;
            OnShowClosedCaptionsChanged(SubtitleService.Settings.ShowClosedCaptions);
            
            SubtitleService.Settings.ShowSpeakersChanged += OnShowSpeakersChanged;
            OnShowSpeakersChanged(SubtitleService.Settings.ShowSpeakers);
            
            SubtitleService.Settings.ColorizeSpeakersChanged += OnColorizeSpeakersChanged;
            OnColorizeSpeakersChanged(SubtitleService.Settings.ColorizeSpeakers);
            
            SubtitleService.Settings.SubtitleBackgroundAlphaChanged += OnSubtitleBackgroundAlphaChanged;
            OnSubtitleBackgroundAlphaChanged(SubtitleService.Settings.SubtitleBackgroundAlpha);
        }

        public void Kill()
        {
            SubtitleService.Settings.ShowSubtitlesChanged -= OnShowSubtitlesChanged;
            SubtitleService.Settings.ShowClosedCaptionsChanged -= OnShowClosedCaptionsChanged;
            SubtitleService.Settings.ShowSpeakersChanged -= OnShowSpeakersChanged;
            SubtitleService.Settings.ColorizeSpeakersChanged -= OnColorizeSpeakersChanged;
            SubtitleService.Settings.SubtitleBackgroundAlphaChanged -= OnSubtitleBackgroundAlphaChanged;
        }

        private void OnShowSubtitlesChanged(bool show)
        {
            _showSubtitles = show;
        }

        private void OnShowClosedCaptionsChanged(bool show)
        {
            _showClosedCaptions = show;
        }

        private void OnShowSpeakersChanged(bool show)
        {
            _showSpeakers = show;
        }

        private void OnColorizeSpeakersChanged(bool colorize)
        {
            _colorizeSpeakers = colorize;
        }

        private void OnSubtitleBackgroundAlphaChanged(float alpha)
        {
            _backgroundAlpha = alpha;
        }
        

        /// <summary>Displays a subtitles depending on their priority.</summary>
        /// <param name="bundle">Text and timing data that make up a sequence of subtitles.</param>
        /// <param name="priority">Display priority of the subtitle sequence.</param>
        public void Play(SubtitleBundle bundle, int priority)
        {
            if (bundle == null) return; // most audio playbacks don't have subtitles assigned

            // check if the incoming Subtitles have priority
            if (IsPlaying && priority < _currentPriority)
            {
                Debug.Log($"New Subtitles have lower priority ({priority}) than already playing Subtitles ({_currentPriority}).");
                return;
            }

            _currentPriority = priority; // set priority
            _currentBundle = bundle; // set current bundle
            _currentLine = null;
            _currentTimeStamp = 0f; // play from the start

            if(TextContent != null) TextContent.gameObject.SetActive(false);
            if(TextBackground != null) TextBackground.gameObject.SetActive(false);
        }

        public void Pause() => _paused = true;

        public void UnPause() => _paused = false;

        public void Stop()
        {
            _currentBundle = null;

            TextContent.gameObject.SetActive(false);
            TextBackground.gameObject.SetActive(false);
        }

        private void Update()
        {
            if (_currentBundle == null) return;

            _currentTimeStamp += Time.deltaTime * (_paused ? 0f : 1f);

            SubtitleLine selectedLine = _currentBundle.GetSubtitleBundleAtTimeStamp(_currentTimeStamp);
            if(_currentLine == selectedLine) return;
            else _currentLine = selectedLine;

            if (LineCanBePlayed(_currentLine))
            {
                TextContent.gameObject.SetActive(true);
                TextContent.text = _currentLine.GetContentStyled(_showSpeakers, _colorizeSpeakers);
                
                TextBackground.gameObject.SetActive(true);
                TextBackground.text = _currentLine.GetContentStyled(_showSpeakers, _colorizeSpeakers).WrapInMark(SubtitleFunctionality.BackgroundColor, _backgroundAlpha);
            }
            else
            {
                TextContent.gameObject.SetActive(false);
                TextBackground.gameObject.SetActive(false);
            }

            if (_currentTimeStamp >= _currentBundle.TimeStampEnd())
            {
                Stop();
            }
        }

        private bool LineCanBePlayed(SubtitleLine line)
        {
            if (!_showSubtitles) return false;
            if (line == null) return false;
            if (line.SpeakingCharacter == null && !_showClosedCaptions) return false;

            return true;
        }
    }
}