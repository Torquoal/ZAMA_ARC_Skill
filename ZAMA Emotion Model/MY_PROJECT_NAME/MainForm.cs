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
    bool _isLoadingConfig;
    private System.Windows.Forms.Timer _commandWatcherTimer;
    private string _lastCommandValue = string.Empty;
    private EmotionEngine.EmotionalResponseResult? _lastResponse = null; // Store last calculated response

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
      ApplyConfigToUi();
      UpdateEngineFromConfig();

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
            
            // Initialize emotion variables with actual engine values
            // Set initial response to match mood (no events triggered yet)
            _lastResponse = new EmotionEngine.EmotionalResponseResult(
                _emotionEngine.CurrentMood,
                _emotionEngine.MoodValence,
                _emotionEngine.MoodArousal,
                "Initial"
            );
            UpdateEmotionVariables();
            
            // Set up timer to watch for commands via ARC variable
            _commandWatcherTimer = new System.Windows.Forms.Timer();
            _commandWatcherTimer.Interval = 200; // Check every 200ms
            _commandWatcherTimer.Tick += CommandWatcherTimer_Tick;
            _commandWatcherTimer.Start();
        }

        private void CommandWatcherTimer_Tick(object sender, EventArgs e)
        {
            // Watch $Emotion.Command variable for incoming commands
            string commandVar = null;
            try
            {
                commandVar = ARC.Scripting.VariableManager.GetVariable("$Emotion.Command") as string;
            }
            catch
            {
                // Variable doesn't exist yet, that's fine - just return
                return;
            }
            
            if (string.IsNullOrWhiteSpace(commandVar) || commandVar == _lastCommandValue)
            {
                return; // No new command
            }

            System.Diagnostics.Debug.WriteLine($"CommandWatcherTimer_Tick: Raw commandVar='{commandVar}' (length={commandVar?.Length ?? 0})");
            _lastCommandValue = commandVar;
            
            // Remove outer quotes if the entire string is quoted
            if (commandVar.StartsWith("\"") && commandVar.EndsWith("\""))
            {
                commandVar = commandVar.Substring(1, commandVar.Length - 2);
                System.Diagnostics.Debug.WriteLine($"CommandWatcherTimer_Tick: Removed outer quotes, now='{commandVar}'");
            }
            
            // Parse command: format is "COMMAND:param1:param2:param3"
            // Examples:
            //   "TriggerEvent:happy"
            //   "RegisterEvent:excited:9:8"
            //   "GetEmotion"
            string[] parts = commandVar.Split(':');
            System.Diagnostics.Debug.WriteLine($"CommandWatcherTimer_Tick: Split into {parts.Length} parts: [{string.Join("|", parts)}]");
            if (parts.Length == 0)
            {
                return;
            }

            string cmd = parts[0].Trim();
            // Remove quotes if present (shouldn't be needed after outer quote removal, but just in case)
            if (cmd.StartsWith("\"") && cmd.EndsWith("\""))
            {
                cmd = cmd.Substring(1, cmd.Length - 2);
            }
            else if (cmd.StartsWith("\""))
            {
                cmd = cmd.Substring(1);
            }
            else if (cmd.EndsWith("\""))
            {
                cmd = cmd.Substring(0, cmd.Length - 1);
            }
            
            string[] parameters = new string[parts.Length - 1];
            for (int i = 1; i < parts.Length; i++)
            {
                string param = parts[i].Trim();
                // Remove quotes from parameters too
                if (param.StartsWith("\"") && param.EndsWith("\""))
                {
                    param = param.Substring(1, param.Length - 2);
                }
                else if (param.StartsWith("\""))
                {
                    param = param.Substring(1);
                }
                else if (param.EndsWith("\""))
                {
                    param = param.Substring(0, param.Length - 1);
                }
                parameters[i - 1] = param;
            }

            System.Diagnostics.Debug.WriteLine($"CommandWatcherTimer_Tick: Parsed command='{cmd}', parameters=[{string.Join(", ", parameters)}]");

            // Process the command on UI thread to ensure thread safety
            try
            {
                if (InvokeRequired && IsHandleCreated)
                {
                    // Use synchronous Invoke so command completes before we continue
                    Invoke(new Action(() => ProcessCommand(cmd, parameters)));
                }
                else if (!InvokeRequired)
                {
                    ProcessCommand(cmd, parameters);
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine($"Cannot process command '{cmd}': Form handle not created yet");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error processing command '{cmd}': {ex.Message}\n{ex.StackTrace}");
            }
            
            // Clear the variable after processing
            try
            {
                ARC.Scripting.VariableManager.SetVariable("$Emotion.Command", string.Empty);
            }
            catch
            {
                // Ignore errors clearing variable
            }
        }

        private void ProcessCommand(string command, string[] parameters)
        {
            System.Diagnostics.Debug.WriteLine($"ProcessCommand: command='{command}', parameters.Length={parameters?.Length ?? 0}");
            if (parameters != null && parameters.Length > 0)
            {
                System.Diagnostics.Debug.WriteLine($"ProcessCommand: parameters = [{string.Join("|", parameters)}]");
            }
            
            // Handle "TriggerEvent" command
            if (string.Equals(command, "TriggerEvent", StringComparison.OrdinalIgnoreCase))
            {
                System.Diagnostics.Debug.WriteLine($"TriggerEvent command received. Parameters: {string.Join(", ", parameters ?? new string[0])}");
                if (parameters != null && parameters.Length > 0)
                {
                    string keyword = parameters[0].Trim();
                    System.Diagnostics.Debug.WriteLine($"TriggerEvent: About to trigger event '{keyword}' (trimmed)");
                    // We're already on UI thread from ProcessCommand
                    TriggerEventInternal(keyword);
                    System.Diagnostics.Debug.WriteLine($"TriggerEvent: Completed trigger for '{keyword}'");
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine($"TriggerEvent: FAILED - No parameters provided");
                }
                return;
            }

            // Handle "RegisterEvent" command: RegisterEvent keyword valence arousal
            System.Diagnostics.Debug.WriteLine($"ProcessCommand: Checking if command '{command}' matches 'RegisterEvent' (case-insensitive)");
            if (string.Equals(command, "RegisterEvent", StringComparison.OrdinalIgnoreCase))
            {
                System.Diagnostics.Debug.WriteLine($"RegisterEvent command MATCHED! Parameters: {string.Join(", ", parameters ?? new string[0])}");
                if (parameters != null && parameters.Length >= 3)
                {
                    string keyword = parameters[0].Trim();
                    System.Diagnostics.Debug.WriteLine($"RegisterEvent: keyword='{keyword}' (trimmed), valence='{parameters[1]}', arousal='{parameters[2]}'");
                    
                    if (TryParseFloat(parameters[1], out float valence) && 
                        TryParseFloat(parameters[2], out float arousal))
                    {
                        var evt = new UserEmotionEvent
                        {
                            Keyword = keyword,
                            Valence = Clamp(valence, -10f, 10f),
                            Arousal = Clamp(arousal, -10f, 10f)
                        };
                        _events[keyword] = evt;
                        System.Diagnostics.Debug.WriteLine($"RegisterEvent: SUCCESS - Added event '{keyword}' (V={evt.Valence}, A={evt.Arousal}). Dictionary now has {_events.Count} events: {string.Join(", ", _events.Keys)}");
                        SyncConfigFromEvents();
                        
                        // Refresh grid - we're already on UI thread from ProcessCommand
                        RefreshEventsGrid();
                        System.Diagnostics.Debug.WriteLine($"RegisterEvent: Grid refreshed. Grid has {eventsGrid?.Rows?.Count ?? 0} rows.");
                    }
                    else
                    {
                        System.Diagnostics.Debug.WriteLine($"RegisterEvent: FAILED - Could not parse valence='{parameters[1]}' or arousal='{parameters[2]}'");
                    }
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine($"RegisterEvent: FAILED - Invalid parameters. Length={parameters?.Length ?? 0}");
                }
                return;
            }

            // Handle "GetEmotion" command - updates ARC variables with current emotion state
            // Returns both the immediate response (last event) and persistent mood state
            if (string.Equals(command, "GetEmotion", StringComparison.OrdinalIgnoreCase))
            {
                System.Diagnostics.Debug.WriteLine($"GetEmotion command received. _lastResponse.HasValue={_lastResponse.HasValue}");
                if (_lastResponse.HasValue)
                {
                    System.Diagnostics.Debug.WriteLine($"GetEmotion: _lastResponse contains Emotion={_lastResponse.Value.DisplayEmotion}, Valence={_lastResponse.Value.Valence}, Arousal={_lastResponse.Value.Arousal}");
                }
                // We're already on UI thread from ProcessCommand
                UpdateEmotionVariables();
                return;
            }

            // Handle "GetMood" command - returns only the persistent mood state (not event response)
            if (string.Equals(command, "GetMood", StringComparison.OrdinalIgnoreCase))
            {
                ARC.Scripting.VariableManager.SetVariable("$EmotionMood", _emotionEngine.CurrentMood ?? "Neutral");
                ARC.Scripting.VariableManager.SetVariable("$EmotionMoodValence", _emotionEngine.MoodValence.ToString("F3", CultureInfo.InvariantCulture));
                ARC.Scripting.VariableManager.SetVariable("$EmotionMoodArousal", _emotionEngine.MoodArousal.ToString("F3", CultureInfo.InvariantCulture));
                ARC.Scripting.VariableManager.SetVariable("$EmotionTemperament", _emotionEngine.CurrentTemperament ?? "Neutral");
                return;
            }

            // Handle "ListEvents" command - returns list of registered events
            if (string.Equals(command, "ListEvents", StringComparison.OrdinalIgnoreCase))
            {
                System.Diagnostics.Debug.WriteLine($"ListEvents: Dictionary has {_events.Count} events");
                foreach (var kvp in _events)
                {
                    System.Diagnostics.Debug.WriteLine($"  Event: '{kvp.Key}' -> Valence={kvp.Value.Valence}, Arousal={kvp.Value.Arousal}");
                }
                var eventList = string.Join(";", _events.Keys);
                ARC.Scripting.VariableManager.SetVariable("$EmotionEvents", eventList);
                return;
            }
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
            RefreshEventsGrid();

            emotionLabel.Text = $"Registered '{keyword}'";
            valenceLabel.Text = $"Valence: {valence:F1}";
            arousalLabel.Text = $"Arousal: {arousal:F1}";
        }

        private void UpdateEmotionVariables()
        {
            // Update ARC variables with current mood state (persistent, changes slowly)
            ARC.Scripting.VariableManager.SetVariable("$EmotionMood", _emotionEngine.CurrentMood ?? "Neutral");
            ARC.Scripting.VariableManager.SetVariable("$EmotionMoodValence", _emotionEngine.MoodValence.ToString("F3", CultureInfo.InvariantCulture));
            ARC.Scripting.VariableManager.SetVariable("$EmotionMoodArousal", _emotionEngine.MoodArousal.ToString("F3", CultureInfo.InvariantCulture));
            ARC.Scripting.VariableManager.SetVariable("$EmotionTemperament", _emotionEngine.CurrentTemperament ?? "Neutral");
            
            // Also update response variables (immediate reaction to last event)
            if (_lastResponse.HasValue)
            {
                System.Diagnostics.Debug.WriteLine($"UpdateEmotionVariables: Using _lastResponse - Emotion={_lastResponse.Value.DisplayEmotion}, Valence={_lastResponse.Value.Valence}, Arousal={_lastResponse.Value.Arousal}");
                ARC.Scripting.VariableManager.SetVariable("$EmotionCurrent", _lastResponse.Value.DisplayEmotion ?? "Neutral");
                ARC.Scripting.VariableManager.SetVariable("$EmotionValence", _lastResponse.Value.Valence.ToString("F3", CultureInfo.InvariantCulture));
                ARC.Scripting.VariableManager.SetVariable("$EmotionArousal", _lastResponse.Value.Arousal.ToString("F3", CultureInfo.InvariantCulture));
            }
            else
            {
                System.Diagnostics.Debug.WriteLine($"UpdateEmotionVariables: _lastResponse is null, using mood values - Mood={_emotionEngine.CurrentMood}, Valence={_emotionEngine.MoodValence}, Arousal={_emotionEngine.MoodArousal}");
                // No event triggered yet, response equals mood
                ARC.Scripting.VariableManager.SetVariable("$EmotionCurrent", _emotionEngine.CurrentMood ?? "Neutral");
                ARC.Scripting.VariableManager.SetVariable("$EmotionValence", _emotionEngine.MoodValence.ToString("F3", CultureInfo.InvariantCulture));
                ARC.Scripting.VariableManager.SetVariable("$EmotionArousal", _emotionEngine.MoodArousal.ToString("F3", CultureInfo.InvariantCulture));
            }
        }

        private void TriggerEventInternal(string keyword)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(keyword))
                {
                    System.Diagnostics.Debug.WriteLine("TriggerEventInternal: Empty keyword");
                    return;
                }

                System.Diagnostics.Debug.WriteLine($"TriggerEventInternal: Looking for event '{keyword}'. Dictionary has {_events.Count} events: {string.Join(", ", _events.Keys)}");

                if (!_events.TryGetValue(keyword, out var evt))
                {
                    // Event not found - log for debugging
                    System.Diagnostics.Debug.WriteLine($"Event '{keyword}' not found in dictionary. Registered events: {string.Join(", ", _events.Keys)}");
                    return;
                }

                System.Diagnostics.Debug.WriteLine($"TriggerEventInternal: Found event '{keyword}' with valence={evt.Valence}, arousal={evt.Arousal}");

                var result = _emotionEngine.CalculateResponse(evt.Valence, evt.Arousal, keyword);

                System.Diagnostics.Debug.WriteLine($"TriggerEventInternal: Calculated result - Emotion={result.DisplayEmotion}, Valence={result.Valence}, Arousal={result.Arousal}");

                // Store the result so GetEmotion can return it
                _lastResponse = result;

                emotionLabel.Text = $"Emotion: {result.DisplayEmotion}";
                valenceLabel.Text = $"Valence: {result.Valence:F1}";
                arousalLabel.Text = $"Arousal: {result.Arousal:F1}";

                // Update variables immediately with the response
                UpdateEmotionVariables();

                UpdateStateLabels();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error in TriggerEventInternal: {ex.Message}\n{ex.StackTrace}");
            }
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

            TriggerEventInternal(keyword);
        }

        private void SyncEventsFromConfig()
        {
            System.Diagnostics.Debug.WriteLine($"SyncEventsFromConfig: Clearing _events (had {_events.Count} events)");
            _events.Clear();

            if (_config?.Events == null)
            {
                System.Diagnostics.Debug.WriteLine($"SyncEventsFromConfig: _config.Events is null");
                RefreshEventsGrid();
                return;
            }

            System.Diagnostics.Debug.WriteLine($"SyncEventsFromConfig: Loading {_config.Events.Count} events from config");
            foreach (var evt in _config.Events)
            {
                if (string.IsNullOrWhiteSpace(evt?.Keyword))
                {
                    continue;
                }

                _events[evt.Keyword] = evt;
                System.Diagnostics.Debug.WriteLine($"SyncEventsFromConfig: Loaded event '{evt.Keyword}' (V={evt.Valence}, A={evt.Arousal})");
            }
            System.Diagnostics.Debug.WriteLine($"SyncEventsFromConfig: Dictionary now has {_events.Count} events: {string.Join(", ", _events.Keys)}");
            RefreshEventsGrid();
        }

        private void SyncConfigFromEvents()
        {
            if (_config == null)
            {
                _config = new Configuration();
            }

            _config.Events = _events.Values.ToList();
            RefreshEventsGrid();
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

        private void RefreshEventsGrid()
        {
            if (eventsGrid == null)
            {
                return;
            }

            eventsGrid.Rows.Clear();
            foreach (var evt in _events.Values.OrderBy(e => e.Keyword, StringComparer.OrdinalIgnoreCase))
            {
                eventsGrid.Rows.Add(evt.Keyword, evt.Valence.ToString("F1", CultureInfo.InvariantCulture), evt.Arousal.ToString("F1", CultureInfo.InvariantCulture));
            }
        }

        private void ApplyConfigToUi()
        {
            if (_config == null)
            {
                return;
            }

            _isLoadingConfig = true;
            personalityValenceText.Text = _config.TemperamentValence.ToString("F1", CultureInfo.InvariantCulture);
            personalityArousalText.Text = _config.TemperamentArousal.ToString("F1", CultureInfo.InvariantCulture);
            chkRandomPersonality.Checked = _config.RandomizePersonality;
            chkMoodShift.Checked = _config.AllowMoodShift;
            chkPersonalityShift.Checked = _config.AllowPersonalityShift;
            _isLoadingConfig = false;
        }

        private void UpdateEngineFromConfig()
        {
            if (_config == null)
            {
                return;
            }

            _emotionEngine.SetShiftOptions(_config.AllowMoodShift, _config.AllowPersonalityShift);
            _emotionEngine.SetRandomizePersonality(_config.RandomizePersonality, false);
            _emotionEngine.SetPersonality(_config.TemperamentValence, _config.TemperamentArousal, true);
            UpdateStateLabels();
        }

        private void btnApplyPersonality_Click(object sender, EventArgs e)
        {
            if (_config == null)
            {
                _config = new Configuration();
            }

            if (!TryParseFloat(personalityValenceText.Text, out float valence))
            {
                MessageBox.Show("Personality valence must be a number between -10 and 10.", "Invalid personality valence", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (!TryParseFloat(personalityArousalText.Text, out float arousal))
            {
                MessageBox.Show("Personality arousal must be a number between -10 and 10.", "Invalid personality arousal", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            valence = Clamp(valence, -10f, 10f);
            arousal = Clamp(arousal, -10f, 10f);

            _config.TemperamentValence = valence;
            _config.TemperamentArousal = arousal;

            _emotionEngine.SetPersonality(valence, arousal, true);
            UpdateStateLabels();
            UpdateEmotionVariables(); // Update ARC variables immediately
        }

        private void chkRandomPersonality_CheckedChanged(object sender, EventArgs e)
        {
            if (_isLoadingConfig)
            {
                return;
            }

            if (_config == null)
            {
                _config = new Configuration();
            }

            _config.RandomizePersonality = chkRandomPersonality.Checked;
            _emotionEngine.SetRandomizePersonality(_config.RandomizePersonality);
            UpdateStateLabels();
        }

        private void chkMoodShift_CheckedChanged(object sender, EventArgs e)
        {
            if (_isLoadingConfig)
            {
                return;
            }

            if (_config == null)
            {
                _config = new Configuration();
            }

            _config.AllowMoodShift = chkMoodShift.Checked;
            _emotionEngine.SetShiftOptions(_config.AllowMoodShift, _config.AllowPersonalityShift);
        }

        private void chkPersonalityShift_CheckedChanged(object sender, EventArgs e)
        {
            if (_isLoadingConfig)
            {
                return;
            }

            if (_config == null)
            {
                _config = new Configuration();
            }

            _config.AllowPersonalityShift = chkPersonalityShift.Checked;
            _emotionEngine.SetShiftOptions(_config.AllowMoodShift, _config.AllowPersonalityShift);
        }


        private void btnLoadEvent_Click(object sender, EventArgs e)
        {
            if (eventsGrid.SelectedRows.Count == 0)
            {
                MessageBox.Show("Select an event to load.", "No selection", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            string keyword = eventsGrid.SelectedRows[0].Cells[0].Value as string;
            if (string.IsNullOrWhiteSpace(keyword))
            {
                return;
            }

            if (_events.TryGetValue(keyword, out var evt))
            {
                tbKeyword.Text = evt.Keyword;
                numValence.Text = evt.Valence.ToString("F1", CultureInfo.InvariantCulture);
                numArousal.Text = evt.Arousal.ToString("F1", CultureInfo.InvariantCulture);
            }
        }

        private void btnDeleteEvent_Click(object sender, EventArgs e)
        {
            if (eventsGrid.SelectedRows.Count == 0)
            {
                MessageBox.Show("Select an event to delete.", "No selection", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            string keyword = eventsGrid.SelectedRows[0].Cells[0].Value as string;
            if (string.IsNullOrWhiteSpace(keyword))
            {
                return;
            }

            if (MessageBox.Show($"Delete event '{keyword}'?", "Confirm delete", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
            {
                return;
            }

            if (_events.Remove(keyword))
            {
                SyncConfigFromEvents();
                RefreshEventsGrid();
            }
        }

        private void label5_Click(object sender, EventArgs e)
        {

        }

        private void label6_Click(object sender, EventArgs e)
        {

        }

        private void eventsGrid_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {

        }

        private void groupBox2_Enter(object sender, EventArgs e)
        {

        }

        private void label4_Click(object sender, EventArgs e)
        {

        }
    }
}
