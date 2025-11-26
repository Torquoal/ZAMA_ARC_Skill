using System;
using System.Collections.Generic;

namespace ZAMAEmotionModel
{
    /// <summary>
    /// Core emotion calculation engine for the ZAMA Emotion Model.
    /// 
    /// This is a standalone port of the Unity EmotionModel logic that can run inside an ARC skill.
    /// It implements a valence-arousal emotion model where:
    /// - Valence: How positive/negative an emotion is (-10 to +10)
    /// - Arousal: How energetic/calm an emotion is (-10 to +10)
    /// 
    /// The engine maintains three emotional states:
    /// 1. Event Response: Immediate reaction to a triggered event (calculated on-demand)
    /// 2. Mood: Short-term emotional state that changes gradually (updates with each event)
    /// 3. Temperament/Personality: Long-term baseline that drifts very slowly over time
    /// 
    /// Features:
    /// - Combines event values with current mood using weighted averages
    /// - Applies stochastic variability for natural variation
    /// - Supports mood and personality drift over time
    /// - Implements arousal-based cooldown system (lower arousal = longer cooldown)
    /// </summary>
    public class EmotionEngine
    {
        private readonly Random _random = new Random();

        // Long-term personality/temperament (baseline emotional state)
        private float _longTermValence = 5f;  // Default: slightly positive
        private float _longTermArousal = 0f;  // Default: neutral energy
        
        // Short-term mood (current emotional state, changes faster than personality)
        private float _moodValence;
        private float _moodArousal;

        // Weighting factors for combining event and mood values
        // Higher event weight = events have more immediate impact
        // Higher mood weight = current mood has more influence on response
        private float _eventWeight = 0.7f;  // 70% weight on event
        private float _moodWeight = 0.3f;   // 30% weight on mood
        
        // Stochastic variability: random variation added to responses for naturalness
        private float _stochasticVariability = 1f;  // ±1 unit of random variation
        
        // Behavior flags
        private bool _randomizePersonality = true;  // Randomize initial mood when personality is set
        private bool _allowMoodShift = true;       // Allow mood to drift over time
        private bool _allowPersonalityShift = true; // Allow personality to drift over time
        
        // Drift rates: how much mood/personality change per event
        // Mood changes 100x faster than personality (0.01 vs 0.001)
        private const float MoodShiftGain = 0.01f;        // Mood drift per event
        private const float PersonalityShiftGain = 0.001f; // Personality drift per event
        
        // Cooldown system: lower arousal = longer cooldown between responses
        private DateTime _lastEventTriggerTime = DateTime.MinValue;
        private int _minCooldownMs = 500;   // Minimum cooldown at high arousal (+10)
        private int _maxCooldownMs = 10000;  // Maximum cooldown at low arousal (-10)

        /// <summary>
        /// Gets or sets the minimum cooldown duration (milliseconds) when arousal is at maximum (+10)
        /// </summary>
        public int MinCooldownMs
        {
            get => _minCooldownMs;
            set => _minCooldownMs = Math.Max(0, value);
        }

        /// <summary>
        /// Gets or sets the maximum cooldown duration (milliseconds) when arousal is at minimum (-10)
        /// </summary>
        public int MaxCooldownMs
        {
            get => _maxCooldownMs;
            set => _maxCooldownMs = Math.Max(_minCooldownMs, value);
        }

        /// <summary>
        /// Base valence/arousal values for each mood classification.
        /// These define the "center" of each emotional state in the valence-arousal space.
        /// Used as the mood component when calculating emotional responses.
        /// </summary>
        private readonly Dictionary<string, EmotionalResponseValues> _moodBaseValues = new Dictionary<string, EmotionalResponseValues>(StringComparer.OrdinalIgnoreCase)
        {
            { "Excited", new EmotionalResponseValues { Valence = 8f, Arousal = 8f } },    // Very positive, very energetic
            { "Happy", new EmotionalResponseValues { Valence = 8f, Arousal = 4f } },      // Very positive, moderately energetic
            { "Relaxed", new EmotionalResponseValues { Valence = 4f, Arousal = -4f } },    // Positive, calm
            { "Energetic", new EmotionalResponseValues { Valence = 4f, Arousal = 8f } },   // Positive, very energetic
            { "Neutral", new EmotionalResponseValues { Valence = 0f, Arousal = 0f } },      // Neutral on both axes
            { "Tired", new EmotionalResponseValues { Valence = -2f, Arousal = -6f } },      // Slightly negative, very low energy
            { "Annoyed", new EmotionalResponseValues { Valence = -4f, Arousal = 4f } },    // Negative, energetic (agitated)
            { "Sad", new EmotionalResponseValues { Valence = -6f, Arousal = -2f } },       // Negative, low energy
            { "Gloomy", new EmotionalResponseValues { Valence = -8f, Arousal = -8f } }     // Very negative, very low energy
        };

        /// <summary>
        /// Initializes a new EmotionEngine instance with default personality settings.
        /// </summary>
        public EmotionEngine()
        {
            InitializeCurrentMood();
        }

        // Public properties for querying current emotional state
        
        /// <summary>Current mood classification name (e.g., "Happy", "Sad", "Neutral")</summary>
        public string CurrentMood { get; private set; }
        
        /// <summary>Current mood valence value (-10 to +10)</summary>
        public float MoodValence => _moodValence;
        
        /// <summary>Current mood arousal value (-10 to +10)</summary>
        public float MoodArousal => _moodArousal;
        
        /// <summary>Long-term personality/temperament valence value (-10 to +10)</summary>
        public float LongTermValence => _longTermValence;
        
        /// <summary>Long-term personality/temperament arousal value (-10 to +10)</summary>
        public float LongTermArousal => _longTermArousal;
        
        /// <summary>Current temperament classification name based on long-term values</summary>
        public string CurrentTemperament => ClassifyMood(_longTermValence, _longTermArousal);

        /// <summary>
        /// Sets the long-term personality/temperament values.
        /// </summary>
        /// <param name="valence">Personality valence (-10 to +10)</param>
        /// <param name="arousal">Personality arousal (-10 to +10)</param>
        /// <param name="reinitializeMood">If true, recalculates current mood based on new personality</param>
        public void SetPersonality(float valence, float arousal, bool reinitializeMood = true)
        {
            _longTermValence = Clamp(valence, -10f, 10f);
            _longTermArousal = Clamp(arousal, -10f, 10f);

            if (reinitializeMood)
            {
                InitializeCurrentMood();
            }
        }

        /// <summary>
        /// Enables or disables personality randomization on initialization.
        /// When enabled, initial mood is randomized within ±2 units of personality values.
        /// </summary>
        /// <param name="randomize">True to enable randomization</param>
        /// <param name="reinitializeMood">If true, recalculates current mood immediately</param>
        public void SetRandomizePersonality(bool randomize, bool reinitializeMood = true)
        {
            _randomizePersonality = randomize;

            if (reinitializeMood)
            {
                InitializeCurrentMood();
            }
        }

        /// <summary>
        /// Sets whether mood and personality can drift over time based on events.
        /// </summary>
        /// <param name="allowMoodShift">If true, mood gradually shifts toward event responses</param>
        /// <param name="allowPersonalityShift">If true, personality very slowly drifts toward event responses</param>
        public void SetShiftOptions(bool allowMoodShift, bool allowPersonalityShift)
        {
            _allowMoodShift = allowMoodShift;
            _allowPersonalityShift = allowPersonalityShift;
        }

        /// <summary>
        /// Calculates the cooldown duration based on current mood arousal level.
        /// Lower arousal = longer cooldown, higher arousal = shorter cooldown.
        /// </summary>
        /// <param name="arousal">Current mood arousal value (-10 to +10)</param>
        /// <returns>Cooldown duration in milliseconds</returns>
        private int CalculateCooldown(float arousal)
        {
            // Map arousal (-10 to +10) to cooldown (_maxCooldownMs to _minCooldownMs)
            // Low arousal (-10) = _maxCooldownMs
            // High arousal (+10) = _minCooldownMs
            // Neutral (0) = average of min and max
            float arousalPercent = (arousal - (-10f)) / (10f - (-10f)); // 0.0 to 1.0
            int cooldownMs = _minCooldownMs + (int)((1f - arousalPercent) * (_maxCooldownMs - _minCooldownMs));
            return cooldownMs;
        }

        /// <summary>
        /// Checks if enough time has passed since the last event trigger based on current arousal.
        /// </summary>
        /// <returns>True if cooldown has expired and event can be processed, false otherwise</returns>
        public bool CanProcessEvent()
        {
            float currentArousal = _moodArousal;
            int cooldownMs = CalculateCooldown(currentArousal);
            TimeSpan timeSinceLastTrigger = DateTime.Now - _lastEventTriggerTime;
            
            return timeSinceLastTrigger.TotalMilliseconds >= cooldownMs;
        }

        /// <summary>
        /// Gets the remaining cooldown time in milliseconds, or 0 if cooldown has expired.
        /// </summary>
        public int GetRemainingCooldownMs()
        {
            float currentArousal = _moodArousal;
            int cooldownMs = CalculateCooldown(currentArousal);
            TimeSpan timeSinceLastTrigger = DateTime.Now - _lastEventTriggerTime;
            int remaining = cooldownMs - (int)timeSinceLastTrigger.TotalMilliseconds;
            return remaining > 0 ? remaining : 0;
        }

        /// <summary>
        /// Gets the current cooldown duration in milliseconds based on current arousal.
        /// </summary>
        public int GetCurrentCooldownMs()
        {
            float currentArousal = _moodArousal;
            return CalculateCooldown(currentArousal);
        }

        /// <summary>
        /// Calculates the emotional response to a triggered event.
        /// 
        /// Process:
        /// 1. Checks cooldown - if active, throws exception
        /// 2. Gets current mood base values
        /// 3. Combines event values (70%) with mood values (30%) using weighted average
        /// 4. Applies stochastic variability for natural variation
        /// 5. Updates mood and personality with small drift amounts (if enabled)
        /// 6. Classifies and returns the resulting emotion
        /// 
        /// </summary>
        /// <param name="eventValence">Valence value of the triggering event (-10 to +10)</param>
        /// <param name="eventArousal">Arousal value of the triggering event (-10 to +10)</param>
        /// <param name="triggerEvent">Name/keyword of the triggering event (for logging)</param>
        /// <returns>EmotionalResponseResult containing the calculated emotion response</returns>
        /// <exception cref="InvalidOperationException">Thrown if cooldown is still active</exception>
        public EmotionalResponseResult CalculateResponse(float eventValence, float eventArousal, string triggerEvent)
        {
            // Check cooldown before processing - prevents too-frequent emotional responses
            if (!CanProcessEvent())
            {
                int remaining = GetRemainingCooldownMs();
                System.Diagnostics.Debug.WriteLine($"EmotionEngine: Event '{triggerEvent}' ignored due to cooldown. Current arousal={_moodArousal:F2}, remaining={remaining}ms");
                throw new InvalidOperationException($"Event cooldown active. Remaining: {remaining}ms. Current arousal: {_moodArousal:F2}");
            }

            // Get base values for current mood classification
            var moodValues = _moodBaseValues.TryGetValue(CurrentMood, out var moodBase)
                ? moodBase
                : _moodBaseValues["Neutral"];

            // Clamp event values to valid range
            var eventValues = new EmotionalResponseValues
            {
                Valence = Clamp(eventValence, -10f, 10f),
                Arousal = Clamp(eventArousal, -10f, 10f),
            };

            // Combine event and mood using weighted average
            // Event has 70% weight (immediate impact), mood has 30% (persistent influence)
            var combined = new EmotionalResponseValues
            {
                Valence = _moodWeight * moodValues.Valence + _eventWeight * eventValues.Valence,
                Arousal = _moodWeight * moodValues.Arousal + _eventWeight * eventValues.Arousal
            };

            // Add random variation for natural, non-deterministic responses
            var fuzzedValence = combined.Valence + RandomRange(-_stochasticVariability, _stochasticVariability);
            var fuzzedArousal = combined.Arousal + RandomRange(-_stochasticVariability, _stochasticVariability);

            // Apply gradual drift to mood and personality so repeated events cause long-term changes
            // Mood shifts 100x faster than personality (0.01 vs 0.001 gain)
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

            // Update mood classification based on new mood values
            CurrentMood = ClassifyMood(_moodValence, _moodArousal);

            // Resolve display emotion name (may differ from mood classification)
            var displayEmotion = ResolveDisplayEmotion(fuzzedValence, fuzzedArousal);

            // Update cooldown timer after successful processing
            _lastEventTriggerTime = DateTime.Now;

            return new EmotionalResponseResult(displayEmotion, fuzzedValence, fuzzedArousal, triggerEvent);
        }

        /// <summary>
        /// Recalculates current mood based on current personality settings.
        /// Useful when personality or randomization settings change.
        /// </summary>
        public void RefreshMoodFromCurrentSettings()
        {
            InitializeCurrentMood();
        }

        /// <summary>
        /// Initializes the current mood based on personality settings.
        /// If randomization is enabled, mood starts within ±2 units of personality.
        /// Otherwise, mood starts exactly at personality values.
        /// </summary>
        private void InitializeCurrentMood()
        {
            if (_randomizePersonality)
            {
                // Add random variation within ±2 units of personality for natural variation
                _moodValence = Clamp(_longTermValence + RandomRange(-2f, 2f), -10f, 10f);
                _moodArousal = Clamp(_longTermArousal + RandomRange(-2f, 2f), -10f, 10f);
            }
            else
            {
                // Start mood exactly at personality values
                _moodValence = _longTermValence;
                _moodArousal = _longTermArousal;
            }

            CurrentMood = ClassifyMood(_moodValence, _moodArousal);
        }

        /// <summary>
        /// Classifies a valence/arousal pair into a mood name.
        /// Uses a simple grid-based classification:
        /// - High valence (>3): Excited, Happy, or Relaxed (based on arousal)
        /// - Medium valence (-3 to 3): Energetic, Neutral, or Tired (based on arousal)
        /// - Low valence (<-3): Annoyed, Sad, or Gloomy (based on arousal)
        /// </summary>
        /// <param name="valence">Valence value (-10 to +10)</param>
        /// <param name="arousal">Arousal value (-10 to +10)</param>
        /// <returns>Mood classification name</returns>
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

        /// <summary>
        /// Resolves the display emotion name from valence/arousal values.
        /// This is more granular than mood classification and provides specific emotion names
        /// for display purposes (e.g., "Surprised", "Tense", "Scared", "Miserable").
        /// Uses a finer-grained grid than ClassifyMood().
        /// </summary>
        /// <param name="valence">Valence value (-10 to +10)</param>
        /// <param name="arousal">Arousal value (-10 to +10)</param>
        /// <returns>Display emotion name</returns>
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

        /// <summary>
        /// Generates a random float value within the specified range.
        /// </summary>
        /// <param name="min">Minimum value (inclusive)</param>
        /// <param name="max">Maximum value (inclusive)</param>
        /// <returns>Random value between min and max</returns>
        private float RandomRange(float min, float max)
        {
            return (float)(_random.NextDouble() * (max - min) + min);
        }

        /// <summary>
        /// Clamps a value to be within the specified min/max range.
        /// </summary>
        /// <param name="value">Value to clamp</param>
        /// <param name="min">Minimum allowed value</param>
        /// <param name="max">Maximum allowed value</param>
        /// <returns>Clamped value</returns>
        private static float Clamp(float value, float min, float max)
        {
            return Math.Max(min, Math.Min(max, value));
        }

        /// <summary>
        /// Internal structure for holding valence/arousal pairs.
        /// Used for mood base values and intermediate calculations.
        /// </summary>
        private struct EmotionalResponseValues
        {
            public float Valence;
            public float Arousal;
        }

        /// <summary>
        /// Result of an emotional response calculation.
        /// Contains the display emotion name, calculated valence/arousal values, and the trigger event.
        /// </summary>
        public readonly struct EmotionalResponseResult
        {
            public EmotionalResponseResult(string displayEmotion, float valence, float arousal, string trigger)
            {
                DisplayEmotion = displayEmotion;
                Valence = valence;
                Arousal = arousal;
                TriggerEvent = trigger;
            }

            /// <summary>Display name of the emotion (e.g., "Happy", "Excited", "Sad")</summary>
            public string DisplayEmotion { get; }
            
            /// <summary>Calculated valence value (-10 to +10)</summary>
            public float Valence { get; }
            
            /// <summary>Calculated arousal value (-10 to +10)</summary>
            public float Arousal { get; }
            
            /// <summary>Keyword/name of the event that triggered this response</summary>
            public string TriggerEvent { get; }
        }
    }
}
