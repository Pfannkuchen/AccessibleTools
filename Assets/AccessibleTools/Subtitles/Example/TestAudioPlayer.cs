using System.Linq;
using UnityEngine;

namespace AccessibleTools.Subtitles
{
    [RequireComponent(typeof(AudioSource))]
    public class TestAudioPlayer : MonoBehaviour
    {
        [SerializeField] private AudioEntry[] _entries;
        private AudioEntry[] Entries => _entries;

        private AudioSource _source;

        // Start is called before the first frame update
        private void Start()
        {
            _source = GetComponent<AudioSource>();
        }

        private void OnGUI()
        {
            AudioEntry[] cleanEntries = Entries.Where(x => x != null).ToArray();

            for (int i = 0; i < cleanEntries.Count(); i++)
            {
                if (GUI.Button(new Rect(10f + i * 50f, 10f, 40f, 40f), i.ToString()))
                {
                    _source.PlayOneShot(cleanEntries[i].Clip);
                    SubtitleService.Play(cleanEntries[i].Subtitles, cleanEntries[i].SubtitlePriority);
                }
            }
        }
    }
}