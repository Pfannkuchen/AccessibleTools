using System;

namespace AccessibleTools.Subtitles
{
    [Serializable]
    public class SubtitleLine : IComparable
    {
        public float TimeStamp; // if not defined, will show right after previous Subtitle
        public float Duration; // if not defined, will show until next SubtitleLine is showing or for a maximum duration
        public Speaker SpeakingCharacter; // speaking character
        public string Content; // single line of content

        public SubtitleLine()
        {
            this.TimeStamp = 0f;
            this.Duration = 0f;
            this.SpeakingCharacter = null;
            this.Content = "";
        }

        public SubtitleLine GetClone()
        {
            SubtitleLine line = new SubtitleLine
            {
                TimeStamp = this.TimeStamp,
                Duration = this.Duration,
                SpeakingCharacter = this.SpeakingCharacter,
                Content = this.Content
            };
            return line;
        }

        public int CompareTo(object other)
        {
            return TimeStamp.CompareTo(((SubtitleLine)other).TimeStamp);
        }

        /// <summary>
        /// Returns the end time stamp by adding the Duration to the (start) TimeStamp.
        /// </summary>
        public float TimeStampEnd => TimeStamp + Duration;

        /// <summary>
        /// Separates the Content string into single lines.
        /// </summary>
        /// <returns></returns>
        public string[] GetLines() => Content.Split(new[] { '|' });
    }
}