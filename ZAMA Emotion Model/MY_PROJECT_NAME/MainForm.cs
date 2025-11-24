using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows.Forms;
using ARC;

namespace ZAMAEmotionModel {

  public partial class MainForm : ARC.UCForms.FormPluginMaster {

    Configuration _config;
    readonly EmotionEngine _emotionEngine = new EmotionEngine();
    readonly Dictionary<string, UserEmotionEvent> _events = new Dictionary<string, UserEmotionEvent>(StringComparer.OrdinalIgnoreCase);

    public MainForm() {

      InitializeComponent();

      // show a config button in the title bar. Set this to false if you do not have a config form.
      ConfigButton = true;
    }

    /// <summary>
    /// Set the configuration from the project file when loaded.
    /// We'll extract the _config class that's from the project file.
    /// </summary>
    public override void SetConfiguration(ARC.Config.Sub.PluginV1 cf) {

      _config = (Configuration)cf.GetCustomObjectV2(typeof(Configuration)) ?? new Configuration();

      SyncEventsFromConfig();

      base.SetConfiguration(cf);
    }

    /// <summary>
    /// When the project is saving, give it a copy of our config
    /// </summary>
    public override ARC.Config.Sub.PluginV1 GetConfiguration() {

      SyncConfigFromEvents();
      _cf.SetCustomObjectV2(_config);

      return base.GetConfiguration();
    }

    /// <summary>
    /// The user pressed the config button in the title bar. Show the config menu and handle the changes to the config.
    /// </summary>
    public override void ConfigPressed() {

      using (var form = new ConfigForm()) {

        form.SetConfiguration(_config);

        if (form.ShowDialog() != DialogResult.OK)
          return;

        _config = form.GetConfiguration();
      }
    }
        private void MainForm_Load(object sender, EventArgs e)
        {
            ResetOutputLabels();
        }

        private void btnRegister_Click(object sender, EventArgs e)
        {
            string keyword = tbKeyword.Text.Trim();
            if (string.IsNullOrWhiteSpace(keyword))
            {
                MessageBox.Show("Please enter a keyword to register.", "Missing keyword", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (!TryParseFloat(numValence.Text, out float valence))
            {
                MessageBox.Show("Valence must be a number between -10 and 10.", "Invalid valence", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (!TryParseFloat(numArousal.Text, out float arousal))
            {
                MessageBox.Show("Arousal must be a number between -10 and 10.", "Invalid arousal", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            valence = Clamp(valence, -10f, 10f);
            arousal = Clamp(arousal, -10f, 10f);

            var evt = new UserEmotionEvent
            {
                Keyword = keyword,
                Valence = valence,
                Arousal = arousal
            };

            _events[keyword] = evt;
            SyncConfigFromEvents();

            emotionLabel.Text = $"Registered '{keyword}'";
            valenceLabel.Text = $"Valence: {valence:F1}";
            arousalLabel.Text = $"Arousal: {arousal:F1}";
        }

        private void btnTrigger_Click(object sender, EventArgs e)
        {
            string keyword = tbTrigger.Text.Trim();
            if (string.IsNullOrWhiteSpace(keyword))
            {
                MessageBox.Show("Enter a keyword to trigger.", "Missing keyword", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (!_events.TryGetValue(keyword, out var evt))
            {
                MessageBox.Show($"No event registered for '{keyword}'. Register it first.", "Unknown keyword", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            var result = _emotionEngine.CalculateResponse(evt.Valence, evt.Arousal, keyword);

            emotionLabel.Text = $"Emotion: {result.DisplayEmotion}";
            valenceLabel.Text = $"Valence: {result.Valence:F1}";
            arousalLabel.Text = $"Arousal: {result.Arousal:F1}";

            ARC.Scripting.VariableManager.SetVariable("$Emotion.Current", result.DisplayEmotion);
            ARC.Scripting.VariableManager.SetVariable("$Emotion.Valence", result.Valence);
            ARC.Scripting.VariableManager.SetVariable("$Emotion.Arousal", result.Arousal);

            UpdateStateLabels();
        }

        private void SyncEventsFromConfig()
        {
            _events.Clear();

            if (_config?.Events == null)
            {
                return;
            }

            foreach (var evt in _config.Events)
            {
                if (string.IsNullOrWhiteSpace(evt?.Keyword))
                {
                    continue;
                }

                _events[evt.Keyword] = evt;
            }
        }

        private void SyncConfigFromEvents()
        {
            if (_config == null)
            {
                _config = new Configuration();
            }

            _config.Events = _events.Values.ToList();
        }

        private void ResetOutputLabels()
        {
            emotionLabel.Text = "Emotion";
            valenceLabel.Text = "Valence";
            arousalLabel.Text = "Arousal";
            UpdateStateLabels();
        }

        private static bool TryParseFloat(string input, out float value)
        {
            return float.TryParse(input, NumberStyles.Float, CultureInfo.InvariantCulture, out value);
        }

        private static float Clamp(float value, float min, float max)
        {
            return Math.Max(min, Math.Min(max, value));
        }

        private void UpdateStateLabels()
        {
            moodValueLabel.Text = $"Mood: {_emotionEngine.CurrentMood}";
            personalityValueLabel.Text = $"Temperament: {_emotionEngine.CurrentTemperament}";
        }
    }
}
