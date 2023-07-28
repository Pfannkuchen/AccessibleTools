using System;
using System.Reflection;
using UnityEngine;
using UnityEditor;

namespace AccessibleTools.Subtitles
{
    public class SubtitleBundleCreatorEditorWindow : EditorWindow
    {
        // Add menu named "My Window" to the Window menu
        [MenuItem("AccessibleTools/SubtitleBundle Creator")]
        static void Init()
        {
            // Get existing open window or if none, make a new one:
            SubtitleBundleCreatorEditorWindow window = (SubtitleBundleCreatorEditorWindow)EditorWindow.GetWindow(typeof(SubtitleBundleCreatorEditorWindow));
            window.Show();
        }


        private const int WaveformHeight = 180;


        private int WaveformWidth => (int)position.width - 32;


        private SubtitleBundle _selectedBundle;
        private bool _showTabContent = true;
        private bool _showTabAnalysis = false;
        private bool _showTabTranscript = false;
        private float _audioPlaybackTime = 0f;
        private float _currentAudioPlaybackMarker;


        private GUIStyle _colorStyle;
        private GUIStyle ColorStyle
        {
            get
            {
                if (_colorStyle == null)
                {
                    _colorStyle = new GUIStyle();
                    _colorStyle.richText = true;
                    _colorStyle.padding = new RectOffset(0, 0, 2, 0);
                }

                return _colorStyle;
            }
        }

        private Texture2D _audioWaveform;
        //private Texture2D _previewAudioWaveform;

        private Material _previewAudioMaterial;
        private bool _isPlayingAudio;
        private Material PreviewAudioMaterial
        {
            get
            {
                if (_previewAudioMaterial == null)
                {
                    Shader shader = Shader.Find("Hidden/Internal-Colored");
                    _previewAudioMaterial = new Material(shader);
                }

                return _previewAudioMaterial;
            }
        }


        #region EditorWindow

        private void OnEnable()
        {
            if (_selectedBundle == null)
            {
                return;
            }

            SortSubtitleLines(_selectedBundle);
            UpdateAudioWaveform();
        }

        private void OnDisable()
        {
            DestroyImmediate(_previewAudioMaterial);

            SortSubtitleLines(_selectedBundle);
        }

        #endregion


        private static Texture2D GetAudioWaveform(AudioClip aud, int width, int height, Color color)
        {
            int step = Mathf.CeilToInt((aud.samples * aud.channels) / width);
            float[] samples = new float[aud.samples * aud.channels];

            //workaround to prevent the error in the function getData
            //when Audio Importer loadType is "compressed in memory"
            string path = AssetDatabase.GetAssetPath(aud);
            //AudioImporter audioImporter = AssetImporter.GetAtPath(path) as AudioImporter;
            AssetDatabase.ImportAsset(path);

            //getData after the loadType changed
            aud.GetData(samples, 0);

            //restore the loadType (end of workaround)
            AssetDatabase.ImportAsset(path);

            Texture2D img = new Texture2D(width, height, TextureFormat.RGBA32, false);

            Color[] xy = new Color[width * height];
            for (int x = 0; x < width * height; x++)
            {
                xy[x] = new Color(0, 0, 0, 0);
            }

            img.SetPixels(xy);

            int i = 0;
            while (i < width)
            {
                int barHeight = Mathf.CeilToInt(Mathf.Clamp(Mathf.Abs(samples[i * step]) * height, 0, height));
                int add = samples[i * step] > 0 ? 1 : -1;
                for (int j = 0; j < barHeight; j++)
                {
                    img.SetPixel(i, Mathf.FloorToInt(height / 2) - (Mathf.FloorToInt(barHeight / 2) * add) + (j * add), color);
                }

                ++i;
            }

            img.Apply();
            return img;
        }
        
        public void Update()
        {
            if (_isPlayingAudio)
            {
                // This is necessary to make the framerate normal for the editor window.
                Repaint();
            }
        }
   

        private void OnGUI()
        {
            EditorGUI.BeginChangeCheck();

            GUILayout.Label("Selected SubtitleBundle", EditorStyles.boldLabel);
            _selectedBundle = (SubtitleBundle)EditorGUILayout.ObjectField(_selectedBundle, typeof(SubtitleBundle), false);

            if (_selectedBundle == null) return;

            if (_selectedBundle.TimingReference != null && _audioWaveform == null) UpdateAudioWaveform();

            AudioClip previewAudioClip = (AudioClip)EditorGUILayout.ObjectField(_selectedBundle.TimingReference, typeof(AudioClip), false);
            if (EditorGUI.EndChangeCheck())
            {
                _selectedBundle.TimingReference = previewAudioClip;
                UpdateAudioWaveform();
            }

            /*
            if (_selectedBundle.TimingReference != null && _previewAudioWaveform != null)
            {
                GUILayout.Space(300);
                EditorGUI.DrawPreviewTexture(new Rect(4, 60, Screen.width - 8, 300), _previewAudioWaveform);
    
                GUI.Label(new Rect(8, 334, Screen.width - 12, 30), _selectedBundle.TimingReference.length.ToString() + " seconds");
            }
            */

            if (_selectedBundle.TimingReference != null && _audioWaveform != null)
            {
                GUILayout.Space(WaveformHeight);
                EditorGUI.DrawPreviewTexture(new Rect(4, 60, Screen.width - 8, 180), _audioWaveform);

                string timeString = _currentAudioPlaybackMarker.ToString("F2") + " / " + _selectedBundle.TimingReference.length.ToString("F2");
                GUI.Label(new Rect(8, WaveformHeight + 34, Screen.width - 12, 30), timeString + " seconds");
            }

            GUILayout.Space(2);

            /*
            // edit SubtitleBundle.BundleName
            EditorGUI.BeginChangeCheck();
            string _bundleName = EditorGUILayout.TextField("Bundle Name (keyword)", _selectedBundle.BundleName);
            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(_selectedBundle, "Changed SubtitleBundle.BundleName");
                _selectedBundle.BundleName = _bundleName;
            }
    
            GUILayout.Space(16);
            */
            
            
            
            EditorGUILayout.BeginHorizontal();

            if (GUILayout.Button("Play", GUILayout.MaxWidth(60f)))
            {
                PlayClip(_selectedBundle.TimingReference);
            }
            
            if (GUILayout.Button("Stop", GUILayout.MaxWidth(60f)))
            {
                StopAllClips();
            }

            EditorGUILayout.EndHorizontal();
            
            
            
            GUILayout.Space(8);
            
            

            // Tab selection
            EditorGUILayout.BeginHorizontal();
            if (_showTabContent) GUI.color = Color.gray;
            if (GUILayout.Button("Content"))
            {
                _showTabContent = !_showTabContent;
                SortSubtitleLines(_selectedBundle);
            }

            GUI.color = Color.white;
            if (_showTabAnalysis) GUI.color = Color.gray;
            if (GUILayout.Button("Analysis"))
            {
                _showTabAnalysis = !_showTabAnalysis;
                SortSubtitleLines(_selectedBundle);
            }

            GUI.color = Color.white;
            if (_showTabTranscript) GUI.color = Color.gray;
            if (GUILayout.Button("Transcript"))
            {
                _showTabTranscript = !_showTabTranscript;
                SortSubtitleLines(_selectedBundle);
            }

            GUI.color = Color.white;
            EditorGUILayout.EndHorizontal();

            // draw timings on top of waveform
            for (int i = 0; i < _selectedBundle.Lines.Count; i++)
            {
                if (_selectedBundle.Lines[i] == null)
                {
                    continue;
                }

                // draw line
                if (_selectedBundle.TimingReference != null && _audioWaveform != null && Event.current.type == EventType.Repaint)
                {
                    GL.Clear(true, false, Color.black);
                    PreviewAudioMaterial.SetPass(0);

                    GL.Begin(GL.QUADS);
                    Color col = _selectedBundle.Lines[i].SpeakingCharacter.GetColor();
                    col.a = 0.5f;
                    GL.Color(col);
                    float pixelPos = Mathf.Lerp(4, Screen.width - 4, _selectedBundle.Lines[i].TimeStamp / _selectedBundle.TimingReference.length);
                    float durationPos = Mathf.Lerp(4, Screen.width - 4, (_selectedBundle.Lines[i].TimeStamp + _selectedBundle.Lines[i].Duration) / _selectedBundle.TimingReference.length);
                    GL.Vertex3(pixelPos, 60, 0);
                    GL.Vertex3(pixelPos, WaveformHeight + 60, 0);
                    GL.Vertex3(durationPos, WaveformHeight + 60, 0);
                    GL.Vertex3(durationPos, 60, 0);
                    GL.End();

                    GUI.Label(new Rect(pixelPos + 2, 55, Screen.width - 12, 30), i.ToString());
                }
            }
            
            
            
            if (_isPlayingAudio)
            {
                _currentAudioPlaybackMarker = Time.realtimeSinceStartup - _audioPlaybackTime;
                _currentAudioPlaybackMarker = Mathf.Clamp(_currentAudioPlaybackMarker, 0f, _selectedBundle.TimingReference.length);
            }
            // draw audio playback line
            if (_selectedBundle.TimingReference != null && _audioWaveform != null && Event.current.type == EventType.Repaint)
            {
                GL.Clear(true, false, Color.black);
                PreviewAudioMaterial.SetPass(0);

                GL.Begin(GL.QUADS);
                GL.Color(Color.white);
                float pixelPos = Mathf.Lerp(4, Screen.width - 4, _currentAudioPlaybackMarker / _selectedBundle.TimingReference.length);
                GL.Vertex3(pixelPos, 60, 0);
                GL.Vertex3(pixelPos, WaveformHeight + 60, 0);
                GL.Vertex3(pixelPos + 2, WaveformHeight + 60, 0);
                GL.Vertex3(pixelPos + 2, 60, 0);
                GL.End();
            }
            
            GUILayout.BeginHorizontal();

            // Content editing
            if (_showTabContent)
            {
                GUILayout.BeginVertical("HelpBox");

                // header labels
                EditorGUILayout.BeginHorizontal();
                GUILayout.Label("Index & Start", EditorStyles.boldLabel, GUILayout.Width(120));
                GUILayout.Label("Duration", EditorStyles.boldLabel, GUILayout.Width(160));
                GUILayout.Label("Speaker", EditorStyles.boldLabel, GUILayout.Width(120));
                GUILayout.Label("Content", EditorStyles.boldLabel);
                EditorGUILayout.EndHorizontal();

                for (int i = 0; i < _selectedBundle.Lines.Count; i++)
                {
                    if (_selectedBundle.Lines[i] == null)
                    {
                        continue;
                    }

                    EditorGUILayout.BeginHorizontal();

                    // edit SubtitleLine.TimeStamp
                    EditorGUI.BeginChangeCheck();
                    float originalValue = EditorGUIUtility.labelWidth;
                    EditorGUIUtility.labelWidth = 40;
                    float timeStamp = Mathf.Max(EditorGUILayout.FloatField(i.ToString(), _selectedBundle.Lines[i].TimeStamp, GUILayout.MaxWidth(120)), 0f);
                    if (EditorGUI.EndChangeCheck())
                    {
                        Undo.RecordObject(_selectedBundle, "Changed SubtitleLine.TimeStamp");
                        _selectedBundle.Lines[i].TimeStamp = timeStamp;
                    }

                    EditorGUIUtility.labelWidth = originalValue;

                    // edit SubtitleLine.Duration
                    EditorGUI.BeginChangeCheck();
                    float duration = EditorGUILayout.Slider(_selectedBundle.Lines[i].Duration, 1.5f, 8f, GUILayout.Width(160));
                    if (EditorGUI.EndChangeCheck())
                    {
                        Undo.RecordObject(_selectedBundle, "Changed SubtitleLine.Duration");
                        _selectedBundle.Lines[i].Duration = duration;
                    }

                    // edit SubtitleLine.SpeakingCharacter
                    GUI.color = _selectedBundle.Lines[i].SpeakingCharacter.GetColor();

                    EditorGUI.BeginChangeCheck();
                    Speaker speaker = (Speaker)EditorGUILayout.ObjectField(_selectedBundle.Lines[i].SpeakingCharacter, typeof(Speaker), GUILayout.Width(120));
                    //Speaker _speakerCharacter = (Speaker)EditorGUILayout.EnumPopup(_selectedBundle.Lines[i].SpeakingCharacter, GUILayout.Width(120));
                    if (EditorGUI.EndChangeCheck())
                    {
                        Undo.RecordObject(_selectedBundle, "Changed SubtitleLine.SpeakingCharacter");
                        _selectedBundle.Lines[i].SpeakingCharacter = speaker;
                    }

                    // edit SubtitleLine.Content
                    EditorGUI.BeginChangeCheck();
                    string content = EditorGUILayout.TextField(_selectedBundle.Lines[i].Content);
                    if (EditorGUI.EndChangeCheck())
                    {
                        Undo.RecordObject(_selectedBundle, "Changed SubtitleLine.Content");
                        _selectedBundle.Lines[i].Content = content;
                    }

                    GUI.color = new Color(0.2f, 1f, 0.6f);
                    if (GUILayout.Button("Add", GUILayout.Width(40)))
                    {
                        Undo.RecordObject(_selectedBundle, "Changed SubtitleLine.TimeStamp");
                        _selectedBundle.Lines.Insert(i, _selectedBundle.Lines[i].GetClone());
                        return;
                    }

                    GUI.color = new Color(1f, 0.4f, 0.2f);
                    if (GUILayout.Button("Del", GUILayout.Width(40)))
                    {
                        Undo.RecordObject(_selectedBundle, "Changed SubtitleLine.TimeStamp");
                        _selectedBundle.Lines.RemoveAt(i);
                        return;
                    }

                    GUI.color = Color.white;

                    EditorGUILayout.EndHorizontal();
                }

                GUILayout.EndVertical();
            }

            // Analysis viewing
            if (_showTabAnalysis)
            {
                GUILayout.BeginVertical("HelpBox");

                // header labels
                EditorGUILayout.BeginHorizontal();
                //GUILayout.Label("Index", EditorStyles.boldLabel, GUILayout.Width(40));
                GUILayout.Label("Lines", EditorStyles.boldLabel, GUILayout.Width(40));
                GUILayout.Label("CPL", EditorStyles.boldLabel, GUILayout.Width(40));
                GUILayout.Label("CPS", EditorStyles.boldLabel, GUILayout.Width(40));
                EditorGUILayout.EndHorizontal();

                for (int i = 0; i < _selectedBundle.Lines.Count; i++)
                {
                    if (_selectedBundle.Lines[i] == null)
                    {
                        continue;
                    }

                    EditorGUILayout.BeginHorizontal();

                    //EditorGUILayout.LabelField(i.ToString(), GUILayout.Width(40), GUILayout.Height(19));

                    // lines of this subtitle
                    string[] lines = _selectedBundle.Lines[i].GetLines();
                    if (lines.Length <= 0 || lines.Length >= 3)
                    {
                        EditorGUILayout.LabelField(lines.Length.ToString().WrapInColor(Color.red), ColorStyle, GUILayout.Width(40), GUILayout.Height(19));
                    }
                    else
                    {
                        EditorGUILayout.LabelField(lines.Length.ToString(), GUILayout.Width(40), GUILayout.Height(19));
                    }

                    // characters per line
                    float cpl = (float)_selectedBundle.Lines[i].Content.Length / (float)lines.Length;
                    if (cpl <= 0f || cpl >= 37f)
                    {
                        EditorGUILayout.LabelField(cpl.ToString("F2").WrapInColor(Color.red), ColorStyle, GUILayout.Width(40), GUILayout.Height(19));
                    }
                    else
                    {
                        EditorGUILayout.LabelField(cpl.ToString("F2"), GUILayout.Width(40), GUILayout.Height(19));
                    }

                    // characters per second
                    float cps = (float)_selectedBundle.Lines[i].Content.Length / _selectedBundle.Lines[i].Duration;
                    if (cps <= 0f || cps >= 20f)
                    {
                        EditorGUILayout.LabelField(cps.ToString("F2").WrapInColor(Color.red), ColorStyle, GUILayout.Width(40), GUILayout.Height(19));
                    }
                    else
                    {
                        EditorGUILayout.LabelField(cps.ToString("F2"), GUILayout.Width(40), GUILayout.Height(19));
                    }

                    EditorGUILayout.EndHorizontal();
                }

                GUILayout.EndVertical();
            }

            // Transcript viewing
            if (_showTabTranscript)
            {
                GUILayout.BeginVertical("HelpBox");

                // header labels
                EditorGUILayout.BeginHorizontal();
                //GUILayout.Label("Index", EditorStyles.boldLabel, GUILayout.Width(40));
                GUILayout.Label("Time Stamp", EditorStyles.boldLabel, GUILayout.Width(120));
                GUILayout.Label("Content", EditorStyles.boldLabel);
                EditorGUILayout.EndHorizontal();

                for (int i = 0; i < _selectedBundle.Lines.Count; i++)
                {
                    if (_selectedBundle.Lines[i] == null)
                    {
                        continue;
                    }

                    EditorGUILayout.BeginHorizontal();

                    //EditorGUILayout.LabelField(i.ToString(), GUILayout.Width(40), GUILayout.Height(19));

                    EditorGUILayout.LabelField(_selectedBundle.Lines[i].TimeStamp.ToString("F2") + " -> " + _selectedBundle.Lines[i].TimeStampEnd.ToString("F2"), GUILayout.Width(120), GUILayout.Height(19));

                    GUIStyle style = new GUIStyle();
                    style.richText = true;
                    EditorGUILayout.LabelField(_selectedBundle.Lines[i].GetContentStyledInline(), style, GUILayout.Height(19));

                    EditorGUILayout.EndHorizontal();
                }

                GUILayout.EndVertical();
            }

            GUILayout.EndHorizontal();
        }

        private void SortSubtitleLines(SubtitleBundle bundle)
        {
            if (bundle == null) return;

            bundle.Lines.Sort();
        }

        private void UpdateAudioWaveform()
        {
            _audioWaveform = (_selectedBundle.TimingReference == null) ? null : GetAudioWaveform(_selectedBundle.TimingReference, WaveformWidth, WaveformHeight, Color.white);
        }
        
        public void PlayClip(AudioClip clip, int startSample = 0, bool loop = false)
        {
            StopAllClips();
            
            Assembly unityEditorAssembly = typeof(AudioImporter).Assembly;
     
            Type audioUtilClass = unityEditorAssembly.GetType("UnityEditor.AudioUtil");
            MethodInfo method = audioUtilClass.GetMethod(
                "PlayPreviewClip",
                BindingFlags.Static | BindingFlags.Public,
                null,
                new Type[] { typeof(AudioClip), typeof(int), typeof(bool) },
                null
            );
 
            Debug.Log(method);
            method.Invoke(
                null,
                new object[] { clip, startSample, loop }
            );

            _isPlayingAudio = true;
            _audioPlaybackTime = Time.realtimeSinceStartup;
        }
 
        public void StopAllClips()
        {
            Assembly unityEditorAssembly = typeof(AudioImporter).Assembly;
 
            Type audioUtilClass = unityEditorAssembly.GetType("UnityEditor.AudioUtil");
            MethodInfo method = audioUtilClass.GetMethod(
                "StopAllPreviewClips",
                BindingFlags.Static | BindingFlags.Public,
                null,
                new Type[] { },
                null
            );
 
            Debug.Log(method);
            method.Invoke(
                null,
                new object[] { }
            );
            
            _isPlayingAudio = false;
        }
    }
}