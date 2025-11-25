using System.Collections.Generic;

namespace ZAMAEmotionModel {

  public class UserEmotionEvent
  {
    public string Keyword { get; set; }
    public float Valence { get; set; }
    public float Arousal { get; set; }
  }

  public class Configuration {

    public List<UserEmotionEvent> Events { get; set; } = new List<UserEmotionEvent>();
    public float TemperamentValence { get; set; } = 5f;
    public float TemperamentArousal { get; set; } = 0f;
    public bool RandomizePersonality { get; set; } = true;
    public bool AllowMoodShift { get; set; } = true;
    public bool AllowPersonalityShift { get; set; } = true;
    public string WatchVariableName { get; set; } = "$Emotion.Trigger";
    public bool ClearVariableAfterUse { get; set; } = true;

    // Configuration values that you wish to save with the project can go in here.
    // These details are specified on the getting started guide for creating behavior controls on synthiam.com

    public string TestValue = "this is a default value";
  }
}
