using UnityEngine;

namespace AccessibleTools.Subtitles
{
    [CreateAssetMenu(fileName = "New AudioEntry", menuName = "AccessibleTools/AudioEntry", order = 1)]
    public class AudioEntry : ScriptableObject
    {
        [SerializeField] private AudioClip _clip;
        public AudioClip Clip => _clip;

        [SerializeField] private SubtitleBundle _subtitles;
        public SubtitleBundle Subtitles => _subtitles;
        
        [SerializeField] private int _subtitlePriority;
        public int SubtitlePriority => _subtitlePriority;
    }
}