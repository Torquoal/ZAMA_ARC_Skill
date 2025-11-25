using System;
using System.Collections.Generic;

namespace ZAMAEmotionModel
{
    /// <summary>
    /// Standalone port of the Unity EmotionModel logic that can run inside an ARC skill.
    /// Keeps the valence/arousal maths but drops all Unity dependencies.
    /// </summary>
    public class EmotionEngine
    {
        private readonly Random _random = new Random();

        private float _longTermValence = 5f;
        private float _longTermArousal = 0f;
        private float _moodValence;
        private float _moodArousal;

        private float _eventWeight = 0.7f;
        private float _moodWeight = 0.3f;
        private float _stochasticVariability = 1f;
        private bool _randomizePersonality = true;
        private bool _allowMoodShift = true;
        private bool _allowPersonalityShift = true;
        private const float MoodShiftGain = 0.01f;
        private const float PersonalityShiftGain = 0.001f;

        private readonly Dictionary<string, EmotionalResponseValues> _moodBaseValues = new Dictionary<string, EmotionalResponseValues>(StringComparer.OrdinalIgnoreCase)
        {
            { "Excited", new EmotionalResponseValues { Valence = 8f, Arousal = 8f } },
            { "Happy", new EmotionalResponseValues { Valence = 8f, Arousal = 4f } },
            { "Relaxed", new EmotionalResponseValues { Valence = 4f, Arousal = -4f } },
            { "Energetic", new EmotionalResponseValues { Valence = 4f, Arousal = 8f } },
            { "Neutral", new EmotionalResponseValues { Valence = 0f, Arousal = 0f } },
            { "Tired", new EmotionalResponseValues { Valence = -2f, Arousal = -6f } },
            { "Annoyed", new EmotionalResponseValues { Valence = -4f, Arousal = 4f } },
            { "Sad", new EmotionalResponseValues { Valence = -6f, Arousal = -2f } },
            { "Gloomy", new EmotionalResponseValues { Valence = -8f, Arousal = -8f } }
        };

        public EmotionEngine()
        {
            InitializeCurrentMood();
        }

        public string CurrentMood { get; private set; }
        public float MoodValence => _moodValence;
        public float MoodArousal => _moodArousal;
        public float LongTermValence => _longTermValence;
        public float LongTermArousal => _longTermArousal;
        public string CurrentTemperament => ClassifyMood(_longTermValence, _longTermArousal);

        public void SetPersonality(float valence, float arousal, bool reinitializeMood = true)
        {
            _longTermValence = Clamp(valence, -10f, 10f);
            _longTermArousal = Clamp(arousal, -10f, 10f);

            if (reinitializeMood)
            {
                InitializeCurrentMood();
            }
        }

        public void SetRandomizePersonality(bool randomize, bool reinitializeMood = true)
        {
            _randomizePersonality = randomize;

            if (reinitializeMood)
            {
                InitializeCurrentMood();
            }
        }

        public void SetShiftOptions(bool allowMoodShift, bool allowPersonalityShift)
        {
            _allowMoodShift = allowMoodShift;
            _allowPersonalityShift = allowPersonalityShift;
        }

        public EmotionalResponseResult CalculateResponse(float eventValence, float eventArousal, string triggerEvent)
        {
            var moodValues = _moodBaseValues.TryGetValue(CurrentMood, out var moodBase)
                ? moodBase
                : _moodBaseValues["Neutral"];

            var eventValues = new EmotionalResponseValues
            {
                Valence = Clamp(eventValence, -10f, 10f),
                Arousal = Clamp(eventArousal, -10f, 10f),
            };

            var combined = new EmotionalResponseValues
            {
                Valence = _moodWeight * moodValues.Valence + _eventWeight * eventValues.Valence,
                Arousal = _moodWeight * moodValues.Arousal + _eventWeight * eventValues.Arousal
            };

            var fuzzedValence = combined.Valence + RandomRange(-_stochasticVariability, _stochasticVariability);
            var fuzzedArousal = combined.Arousal + RandomRange(-_stochasticVariability, _stochasticVariability);

            // Small drift to mood/personality so repeated triggers change state over time.
            if (_allowMoodShift)
            {
                _moodValence = Clamp(_moodValence + fuzzedValence * MoodShiftGain, -10f, 10f);
                _moodArousal = Clamp(_moodArousal + fuzzedArousal * MoodShiftGain, -10f, 10f);
            }

            if (_allowPersonalityShift)
            {
                _longTermValence = Clamp(_longTermValence + fuzzedValence * PersonalityShiftGain, -10f, 10f);
                _longTermArousal = Clamp(_longTermArousal + fuzzedArousal * PersonalityShiftGain, -10f, 10f);
            }

            CurrentMood = ClassifyMood(_moodValence, _moodArousal);

            var displayEmotion = ResolveDisplayEmotion(fuzzedValence, fuzzedArousal);

            return new EmotionalResponseResult(displayEmotion, fuzzedValence, fuzzedArousal, triggerEvent);
        }

        public void RefreshMoodFromCurrentSettings()
        {
            InitializeCurrentMood();
        }

        private void InitializeCurrentMood()
        {
            if (_randomizePersonality)
            {
                _moodValence = Clamp(_longTermValence + RandomRange(-2f, 2f), -10f, 10f);
                _moodArousal = Clamp(_longTermArousal + RandomRange(-2f, 2f), -10f, 10f);
            }
            else
            {
                _moodValence = _longTermValence;
                _moodArousal = _longTermArousal;
            }

            CurrentMood = ClassifyMood(_moodValence, _moodArousal);
        }

        private string ClassifyMood(float valence, float arousal)
        {
            if (valence > 3)
            {
                if (arousal > 3) return "Excited";
                if (arousal > -3) return "Happy";
                return "Relaxed";
            }

            if (valence >= -3)
            {
                if (arousal > 3) return "Energetic";
                if (arousal > -3) return "Neutral";
                return "Tired";
            }

            if (arousal > 3) return "Annoyed";
            if (arousal > -3) return "Sad";
            return "Gloomy";
        }

        private string ResolveDisplayEmotion(float valence, float arousal)
        {
            if (arousal > 6)
            {
                if (valence > 6) return "Excited";
                if (valence > 3) return "Excited";
                if (valence > -3) return "Surprised";
                if (valence > -8) return "Tense";
                return "Scared";
            }

            if (arousal > 3)
            {
                if (valence > 3) return "Happy";
                if (valence > -3) return "Energetic";
                if (valence > -6) return "Annoyed";
                return "Angry";
            }

            if (arousal > -3)
            {
                if (valence > 3) return "Happy";
                if (valence > -3) return "Neutral";
                if (valence > -6) return "Sad";
                return "Miserable";
            }

            if (arousal > -6)
            {
                if (valence > 3) return "Relaxed";
                if (valence > -3) return "Tired";
                if (valence > -6) return "Sad";
                return "Sad";
            }

            if (valence > 3) return "Relaxed";
            if (valence > -3) return "Tired";
            if (valence > -6) return "Gloomy";
            return "Gloomy";
        }

        private float RandomRange(float min, float max)
        {
            return (float)(_random.NextDouble() * (max - min) + min);
        }

        private static float Clamp(float value, float min, float max)
        {
            return Math.Max(min, Math.Min(max, value));
        }

        private struct EmotionalResponseValues
        {
            public float Valence;
            public float Arousal;
        }

        public readonly struct EmotionalResponseResult
        {
            public EmotionalResponseResult(string displayEmotion, float valence, float arousal, string trigger)
            {
                DisplayEmotion = displayEmotion;
                Valence = valence;
                Arousal = arousal;
                TriggerEvent = trigger;
            }

            public string DisplayEmotion { get; }
            public float Valence { get; }
            public float Arousal { get; }
            public string TriggerEvent { get; }
        }
    }
}
