using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Windows.Speech;

namespace AccessibleTools.Input
{
    public class SpeechToInput : MonoBehaviour
    {
        private KeywordRecognizer _keywordRecognizer;
        private Dictionary<string, Action> _actions;

        private void Start()
        {
            _actions = new Dictionary<string, Action>
            {
                { "left", Left },
                { "right", Right },
                { "up", Up },
                { "down", Down }
            };

            _keywordRecognizer = new KeywordRecognizer(_actions.Keys.ToArray());
            _keywordRecognizer.OnPhraseRecognized += RecognizedSpeech;
            _keywordRecognizer.Start();
        }

        private void RecognizedSpeech(PhraseRecognizedEventArgs speech)
        {
            Debug.Log(speech.text);
            _actions[speech.text]?.Invoke();
        }

        private void Left()
        {
            transform.Translate(-1, 0, 0);
        }
        
        private void Right()
        {
            transform.Translate(1, 0, 0);
        }
        
        private void Up()
        {
            transform.Translate(0, 0, 1);
        }
        
        private void Down()
        {
            transform.Translate(0, 0, -1);
        }
    }
}
