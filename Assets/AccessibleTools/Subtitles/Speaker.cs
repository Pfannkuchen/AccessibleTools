using UnityEngine;

namespace AccessibleTools.Subtitles
{
    [CreateAssetMenu(fileName = "New Speaker", menuName = "AccessibleTools/Speaker", order = 1)]
    public class Speaker : ScriptableObject
    {
        [SerializeField] private bool _showName = true;
        public bool ShowName => _showName;

        [SerializeField] private string _name = "";
        public string Name => _name;

        [SerializeField] private Color _speakerColor = Color.white;
        public Color SpeakerColor => _speakerColor;
    }
}