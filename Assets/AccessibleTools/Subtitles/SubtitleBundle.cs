using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

namespace AccessibleTools.Subtitles
{
    [CreateAssetMenu(fileName = "New SubtitleBundle", menuName = "AccessibleTools/SubtitleBundle", order = 1)]
    [System.Serializable]
    public class SubtitleBundle : ScriptableObject
    {
        [SerializeField] AudioClip _timingReference;
        public AudioClip TimingReference
        {
            get => _timingReference;
            set => _timingReference = value;
        }

        /*
        [SerializeField] string bundleName;
        public string BundleName
        {
            get
            {
                return bundleName;
            }
            set
            {
                bundleName = value;
            }
        }
        */

        [FormerlySerializedAs("lines")] [SerializeField] private List<SubtitleLine> _lines;
        public List<SubtitleLine> Lines
        {
            get => _lines ??= new List<SubtitleLine>();
            set => _lines = value;
        }

        public float TimeStampEnd()
        {
            float latest = 0f;
            for (int i = 0; i < Lines.Count; i++)
            {
                if (Lines[i] == null) continue;

                if (Lines[i].TimeStampEnd > latest)
                {
                    latest = Lines[i].TimeStampEnd;
                }
            }

            return latest;
        }
    }
}