using UnityEngine;

namespace AccessibleTools.Subtitles
{
    public static class SubtitleFunctionality
    {
        private static readonly Color AudiocaptionColor = Color.gray;
        public static readonly Color BackgroundColor = Color.black;


        public static SubtitleLine GetSubtitleBundleAtTimeStamp(this SubtitleBundle bundle, float timeStamp)
        {
            if (bundle == null) return null;

            SubtitleLine selectedLine = null;
            for (int i = 0; i < bundle.Lines.Count; i++)
            {
                if (bundle.Lines[i] == null) continue;

                if (timeStamp >= bundle.Lines[i].TimeStamp && timeStamp <= (bundle.Lines[i].TimeStamp + bundle.Lines[i].Duration))
                {
                    if (selectedLine == null || bundle.Lines[i].TimeStamp > selectedLine.TimeStamp)
                    {
                        selectedLine = bundle.Lines[i];
                    }
                }
            }

            return selectedLine;
        }

        public static string GetName(this Speaker speaker) => (speaker == null || !speaker.ShowName) ? "" : speaker.Name;

        public static Color GetColor(this Speaker speaker) => speaker == null ? AudiocaptionColor : speaker.SpeakerColor;

        public static string GetHexColor(this Color color)
        {
            return ColorUtility.ToHtmlStringRGB(color);
        }

        public static string GetHexColorWithAlpha(this Color color)
        {
            return ColorUtility.ToHtmlStringRGBA(color);
        }

        public static string WrapInColor(this string content, Color color)
        {
            if (string.IsNullOrEmpty(content)) return "";
            else
            {
                return "<color=#" + color.GetHexColor() + ">" + content + "</color>";
            }
        }

        public static string WrapInMark(this string content, Color color, float backgroundAlpha, bool paddingEnabled = true)
        {
            if (string.IsNullOrEmpty(content)) return "";
            else
            {
                color.a = backgroundAlpha;
                string colorWithAlpha = color.GetHexColorWithAlpha();
                string padding = paddingEnabled ? " padding=\"50, 50, 20, 20\"" : "";
                return "<mark=#" + colorWithAlpha + padding + ">" + content + "</mark>";
            }
        }

        public static string WrapName(this string name)
        {
            if (string.IsNullOrEmpty(name)) return "";
            else
            {
                return name + ":";
                //return "[" + name + "]"; // deprecated styling
            }
        }

        public static string WrapContent(this string content, Speaker speaker)
        {
            if (speaker == null)
            {
                return ("[" + content + "]").WrapInColor(speaker.GetColor());
            }
            else
            {
                return content;
            }
        }

        public static string GetContentStyledInline(this SubtitleLine subtitleLine)
        {
            string speakerName = subtitleLine.SpeakingCharacter.GetName().WrapName().WrapInColor(subtitleLine.SpeakingCharacter.GetColor());
            if (!string.IsNullOrEmpty(speakerName))
            {
                speakerName += " ";
            }

            return speakerName + subtitleLine.Content.WrapContent(subtitleLine.SpeakingCharacter);
        }

        public static string GetContentStyled(this SubtitleLine subtitleLine, bool showSpeaker, bool colorizeSpeaker)
        {
            string speakerName = GetSpeakerName(subtitleLine.SpeakingCharacter == null ? null : subtitleLine.SpeakingCharacter, showSpeaker, colorizeSpeaker);

            string[] lines = subtitleLine.GetLines();
            return lines.Length switch
            {
                <= 0 => "",
                1 => speakerName + lines[0].WrapContent(subtitleLine.SpeakingCharacter),
                _ => speakerName + lines[0].WrapContent(subtitleLine.SpeakingCharacter) + "\n" + lines[1].WrapContent(subtitleLine.SpeakingCharacter)
            };
        }

        private static string GetSpeakerName(Speaker speaker, bool showSpeaker, bool colorizeSpeaker)
        {
            if (speaker == null) return "";

            if (showSpeaker)
            {
                string speakerName = speaker.GetName().WrapName();
                if (colorizeSpeaker) speakerName = speakerName.WrapInColor(speaker.GetColor());
                
                if (!string.IsNullOrEmpty(speakerName))
                {
                    speakerName += " ";
                }

                return speakerName;
            }

            return "";
        }
    }
}