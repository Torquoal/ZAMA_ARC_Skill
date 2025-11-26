using System.Collections.Generic;

namespace ZAMAEmotionModel
{
    /// <summary>
    /// Represents a user-defined emotion event that can be triggered.
    /// Each event has a keyword (trigger name) and associated valence/arousal values.
    /// </summary>
    public class UserEmotionEvent
    {
        /// <summary>Keyword that triggers this emotion event (case-insensitive)</summary>
        public string Keyword { get; set; }
        
        /// <summary>Valence value (-10 to +10) indicating how positive/negative the event is</summary>
        public float Valence { get; set; }
        
        /// <summary>Arousal value (-10 to +10) indicating how energetic/calm the event is</summary>
        public float Arousal { get; set; }
    }

    /// <summary>
    /// Configuration class that persists emotion model settings with the ARC project.
    /// All user-defined events, personality settings, and cooldown values are stored here.
    /// </summary>
    public class Configuration
    {
        /// <summary>List of all user-registered emotion events</summary>
        public List<UserEmotionEvent> Events { get; set; } = new List<UserEmotionEvent>();
        
        /// <summary>Long-term personality valence (-10 to +10). Default: 5 (slightly positive)</summary>
        public float TemperamentValence { get; set; } = 5f;
        
        /// <summary>Long-term personality arousal (-10 to +10). Default: 0 (neutral energy)</summary>
        public float TemperamentArousal { get; set; } = 0f;
        
        /// <summary>Whether to randomize initial mood when personality is set. Default: true</summary>
        public bool RandomizePersonality { get; set; } = true;
        
        /// <summary>Whether mood can shift over time based on events. Default: true</summary>
        public bool AllowMoodShift { get; set; } = true;
        
        /// <summary>Whether personality can drift over time based on events. Default: true</summary>
        public bool AllowPersonalityShift { get; set; } = true;
        
        /// <summary>Minimum cooldown duration (ms) when arousal is at maximum (+10). Default: 500ms</summary>
        public int MinCooldownMs { get; set; } = 500;
        
        /// <summary>Maximum cooldown duration (ms) when arousal is at minimum (-10). Default: 10000ms (10s)</summary>
        public int MaxCooldownMs { get; set; } = 10000;
    }
}
