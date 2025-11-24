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

    // Configuration values that you wish to save with the project can go in here.
    // These details are specified on the getting started guide for creating behavior controls on synthiam.com

    public string TestValue = "this is a default value";
  }
}
