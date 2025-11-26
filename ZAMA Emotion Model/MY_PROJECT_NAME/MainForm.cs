using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Windows.Forms;
using ARC;

namespace ZAMAEmotionModel
{
    /// <summary>
    /// Main form for the ZAMA Emotion Model ARC skill.
    /// 
    /// This form provides:
    /// - UI for registering and managing emotion events
    /// - Controls for setting robot personality/temperament
    /// - Display of current mood and emotional responses
    /// - Integration with ARC via variable watcher for script commands
    /// - Cooldown system configuration and monitoring
    /// 
    /// The skill communicates with other ARC components via:
    /// - Input: Watches $Emotion.Command variable for commands from scripts
    /// - Output: Sets $EmotionCurrent, $EmotionValence, $EmotionArousal, etc. for other components to read
    /// </summary>
    public partial class MainForm : ARC.UCForms.FormPluginMaster
    {
        // Configuration and state management
        private Configuration _config;  // Persisted configuration loaded from ARC project
        private readonly EmotionEngine _emotionEngine = new EmotionEngine();  // Core emotion calculation engine
        private readonly Dictionary<string, UserEmotionEvent> _events = new Dictionary<string, UserEmotionEvent>(StringComparer.OrdinalIgnoreCase);  // In-memory event registry
        private bool _isLoadingConfig;  // Flag to prevent event handlers from firing during config load
        
        // ARC integration - command watcher
        private System.Windows.Forms.Timer _commandWatcherTimer;  // Polls $Emotion.Command for script commands
        private string _lastCommandValue = string.Empty;  // Tracks last processed command to avoid duplicates
        
        // Cooldown system UI updates
        private System.Windows.Forms.Timer _cooldownUpdateTimer;  // Updates cooldown status display
        
        // Response tracking
        private EmotionEngine.EmotionalResponseResult? _lastResponse = null;  // Stores last calculated event response

        /// <summary>
        /// Initializes the main form and enables the config button in the title bar.
        /// </summary>
        public MainForm()
        {
            InitializeComponent();
            ConfigButton = true;  // Show config button in ARC title bar
        }

        /// <summary>
        /// Called by ARC when a project is loaded.
        /// Restores configuration from the project file and applies it to the UI and engine.
        /// </summary>
        /// <param name="cf">ARC configuration object containing persisted data</param>
        public override void SetConfiguration(ARC.Config.Sub.PluginV1 cf)
        {
            // Load configuration from project file, or create new if none exists
            _config = (Configuration)cf.GetCustomObjectV2(typeof(Configuration)) ?? new Configuration();

            // Synchronize in-memory event dictionary with loaded configuration
            SyncEventsFromConfig();
            
            // Apply configuration values to UI controls
            ApplyConfigToUi();
            
            // Apply configuration to emotion engine
            UpdateEngineFromConfig();

            base.SetConfiguration(cf);
        }

        /// <summary>
        /// Called by ARC when a project is being saved.
        /// Saves current configuration (including events) to the project file.
        /// </summary>
        /// <returns>ARC configuration object with current settings</returns>
        public override ARC.Config.Sub.PluginV1 GetConfiguration()
        {
            // Sync events from in-memory dictionary to config before saving
            SyncConfigFromEvents();
            
            // Store configuration in ARC project file
            _cf.SetCustomObjectV2(_config);

            return base.GetConfiguration();
        }

        /// <summary>
        /// Called when user clicks the config button in the title bar.
        /// Opens the configuration form for advanced settings.
        /// </summary>
        public override void ConfigPressed()
        {
            using (var form = new ConfigForm())
            {
                form.SetConfiguration(_config);

                if (form.ShowDialog() != DialogResult.OK)
                    return;

                _config = form.GetConfiguration();
            }
        }
        /// <summary>
        /// Called when the form loads. Initializes timers, UI state, and ARC variable integration.
        /// </summary>
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
            // Scripts write commands to $Emotion.Command, this timer processes them
            _commandWatcherTimer = new System.Windows.Forms.Timer();
            _commandWatcherTimer.Interval = 200;  // Check every 200ms
            _commandWatcherTimer.Tick += CommandWatcherTimer_Tick;
            _commandWatcherTimer.Start();
            
            // Set up timer to update cooldown status display
            _cooldownUpdateTimer = new System.Windows.Forms.Timer();
            _cooldownUpdateTimer.Interval = 100;  // Update every 100ms for smooth countdown
            _cooldownUpdateTimer.Tick += CooldownUpdateTimer_Tick;
            _cooldownUpdateTimer.Start();
            
            // Initialize cooldown text boxes with current values (from config or defaults)
            if (_config != null)
            {
                cooldownMinText.Text = _config.MinCooldownMs.ToString();
                cooldownMaxText.Text = _config.MaxCooldownMs.ToString();
                _emotionEngine.MinCooldownMs = _config.MinCooldownMs;
                _emotionEngine.MaxCooldownMs = _config.MaxCooldownMs;
            }
            else
            {
                cooldownMinText.Text = _emotionEngine.MinCooldownMs.ToString();
                cooldownMaxText.Text = _emotionEngine.MaxCooldownMs.ToString();
            }
            
            UpdateCooldownStatus();
        }

        /// <summary>
        /// Timer tick handler that updates the cooldown status display.
        /// </summary>
        private void CooldownUpdateTimer_Tick(object sender, EventArgs e)
        {
            UpdateCooldownStatus();
        }

        /// <summary>
        /// Updates the cooldown status label to show remaining time or ready state.
        /// </summary>
        private void UpdateCooldownStatus()
        {
            if (_emotionEngine.CanProcessEvent())
            {
                int currentCooldown = _emotionEngine.GetCurrentCooldownMs();
                float currentArousal = _emotionEngine.MoodArousal;
                cooldownStatusLabel.Text = $"Cooldown: Ready (Current: {currentCooldown}ms @ Arousal: {currentArousal:F1})";
                cooldownStatusLabel.ForeColor = Color.Green;
            }
            else
            {
                int remaining = _emotionEngine.GetRemainingCooldownMs();
                float currentArousal = _emotionEngine.MoodArousal;
                int currentCooldown = _emotionEngine.GetCurrentCooldownMs();
                cooldownStatusLabel.Text = $"Cooldown: {remaining}ms / {currentCooldown}ms (Arousal: {currentArousal:F1})";
                cooldownStatusLabel.ForeColor = Color.Orange;
            }
        }

        /// <summary>
        /// Handles the Apply Cooldown button click. Validates and applies new cooldown settings.
        /// </summary>
        private void btnApplyCooldown_Click(object sender, EventArgs e)
        {
            if (int.TryParse(cooldownMinText.Text, out int minCooldown) &&
                int.TryParse(cooldownMaxText.Text, out int maxCooldown))
            {
                if (minCooldown < 0)
                {
                    MessageBox.Show("Minimum cooldown must be 0 or greater.", "Invalid Value", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
                
                if (maxCooldown < minCooldown)
                {
                    MessageBox.Show("Maximum cooldown must be greater than or equal to minimum cooldown.", "Invalid Value", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
                
                _emotionEngine.MinCooldownMs = minCooldown;
                _emotionEngine.MaxCooldownMs = maxCooldown;
                
                // Save to config
                if (_config != null)
                {
                    _config.MinCooldownMs = minCooldown;
                    _config.MaxCooldownMs = maxCooldown;
                }
                
                MessageBox.Show($"Cooldown settings updated:\nMin: {minCooldown}ms\nMax: {maxCooldown}ms", "Settings Applied", MessageBoxButtons.OK, MessageBoxIcon.Information);
                UpdateCooldownStatus();
            }
            else
            {
                MessageBox.Show("Please enter valid numbers for cooldown values.", "Invalid Input", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }


        /// <summary>
        /// Timer tick handler that watches for commands from ARC scripts.
        /// Scripts write commands to $Emotion.Command in the format "COMMAND:param1:param2".
        /// This method polls that variable, parses commands, and executes them.
        /// </summary>
        private void CommandWatcherTimer_Tick(object sender, EventArgs e)
        {
            // Read command from ARC variable
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
            
            // Skip if no command or same as last processed command (avoid duplicate processing)
            if (string.IsNullOrWhiteSpace(commandVar) || commandVar == _lastCommandValue)
            {
                return;
            }

            System.Diagnostics.Debug.WriteLine($"CommandWatcherTimer_Tick: Raw commandVar='{commandVar}' (length={commandVar?.Length ?? 0})");
            _lastCommandValue = commandVar;
            
            // Remove outer quotes if the entire string is quoted (EZ-Script may add quotes)
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

            // Extract command name and remove any quotes
            string cmd = RemoveQuotes(parts[0].Trim());
            
            // Extract parameters and remove quotes from each
            string[] parameters = new string[parts.Length - 1];
            for (int i = 1; i < parts.Length; i++)
            {
                parameters[i - 1] = RemoveQuotes(parts[i].Trim());
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
            
            // Clear the variable after processing so same command isn't processed again
            try
            {
                ARC.Scripting.VariableManager.SetVariable("$Emotion.Command", string.Empty);
            }
            catch
            {
                // Ignore errors clearing variable
            }
        }

        /// <summary>
        /// Helper method to remove surrounding quotes from a string.
        /// Handles cases where quotes are on both ends, just start, or just end.
        /// </summary>
        private static string RemoveQuotes(string str)
        {
            if (string.IsNullOrEmpty(str))
                return str;
            
            // Remove quotes from both ends
            if (str.StartsWith("\"") && str.EndsWith("\""))
            {
                return str.Substring(1, str.Length - 2);
            }
            // Remove quote from start only
            else if (str.StartsWith("\""))
            {
                return str.Substring(1);
            }
            // Remove quote from end only
            else if (str.EndsWith("\""))
            {
                return str.Substring(0, str.Length - 1);
            }
            
            return str;
        }

        /// <summary>
        /// Processes a command received from an ARC script.
        /// Commands are case-insensitive and support various operations:
        /// - TriggerEvent:keyword - Triggers an emotion event
        /// - RegisterEvent:keyword:valence:arousal - Registers a new event
        /// - GetEmotion - Updates ARC variables with current emotion state
        /// - GetMood - Updates ARC variables with persistent mood state
        /// - ListEvents - Returns list of registered events
        /// </summary>
        /// <param name="command">Command name (case-insensitive)</param>
        /// <param name="parameters">Command parameters</param>
        private void ProcessCommand(string command, string[] parameters)
        {
            System.Diagnostics.Debug.WriteLine($"ProcessCommand: command='{command}', parameters.Length={parameters?.Length ?? 0}");
            if (parameters != null && parameters.Length > 0)
            {
                System.Diagnostics.Debug.WriteLine($"ProcessCommand: parameters = [{string.Join("|", parameters)}]");
            }
            
            // Handle "TriggerEvent" command: TriggerEvent:keyword
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

            // Handle "RegisterEvent" command: RegisterEvent:keyword:valence:arousal
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
            // Updates both immediate response (last event) and persistent mood state
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

            // Handle "GetMood" command - updates ARC variables with persistent mood state only (not event response)
            if (string.Equals(command, "GetMood", StringComparison.OrdinalIgnoreCase))
            {
                ARC.Scripting.VariableManager.SetVariable("$EmotionMood", _emotionEngine.CurrentMood ?? "Neutral");
                ARC.Scripting.VariableManager.SetVariable("$EmotionMoodValence", _emotionEngine.MoodValence.ToString("F3", CultureInfo.InvariantCulture));
                ARC.Scripting.VariableManager.SetVariable("$EmotionMoodArousal", _emotionEngine.MoodArousal.ToString("F3", CultureInfo.InvariantCulture));
                ARC.Scripting.VariableManager.SetVariable("$EmotionTemperament", _emotionEngine.CurrentTemperament ?? "Neutral");
                return;
            }

            // Handle "ListEvents" command - returns list of registered events in $EmotionEvents variable
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


        /// <summary>
        /// Handles the Register button click. Registers a new emotion event from UI input fields.
        /// </summary>
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

        /// <summary>
        /// Updates ARC variables with current emotion state.
        /// Sets both mood variables (persistent state) and response variables (immediate event reaction).
        /// Other ARC components can read these variables to react to the robot's emotional state.
        /// </summary>
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

        /// <summary>
        /// Internal method to trigger an emotion event by keyword.
        /// Looks up the event, calculates the emotional response, updates UI, and stores the result.
        /// </summary>
        /// <param name="keyword">Event keyword to trigger</param>
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

                EmotionEngine.EmotionalResponseResult result;
                try
                {
                    result = _emotionEngine.CalculateResponse(evt.Valence, evt.Arousal, keyword);
                }
                catch (InvalidOperationException ex)
                {
                    // Cooldown is active - event was blocked
                    System.Diagnostics.Debug.WriteLine($"TriggerEventInternal: {ex.Message}");
                    return; // Silently ignore the event
                }

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

        /// <summary>
        /// Handles the Trigger button click. Triggers an emotion event from the trigger text box.
        /// </summary>
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

        /// <summary>
        /// Synchronizes the in-memory event dictionary from the configuration.
        /// Called when configuration is loaded from a project file.
        /// </summary>
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

        /// <summary>
        /// Synchronizes the configuration from the in-memory event dictionary.
        /// Called before saving configuration to ensure all registered events are persisted.
        /// </summary>
        private void SyncConfigFromEvents()
        {
            if (_config == null)
            {
                _config = new Configuration();
            }

            _config.Events = _events.Values.ToList();
            RefreshEventsGrid();
        }

        /// <summary>
        /// Resets the output labels to their default state.
        /// </summary>
        private void ResetOutputLabels()
        {
            emotionLabel.Text = "Emotion";
            valenceLabel.Text = "Valence";
            arousalLabel.Text = "Arousal";
            UpdateStateLabels();
        }

        /// <summary>
        /// Helper method to parse a float value using invariant culture.
        /// </summary>
        private static bool TryParseFloat(string input, out float value)
        {
            return float.TryParse(input, NumberStyles.Float, CultureInfo.InvariantCulture, out value);
        }

        /// <summary>
        /// Helper method to clamp a value between min and max.
        /// </summary>
        private static float Clamp(float value, float min, float max)
        {
            return Math.Max(min, Math.Min(max, value));
        }

        /// <summary>
        /// Updates the mood and temperament display labels.
        /// </summary>
        private void UpdateStateLabels()
        {
            moodValueLabel.Text = $"Mood: {_emotionEngine.CurrentMood}";
            personalityValueLabel.Text = $"Temperament: {_emotionEngine.CurrentTemperament}";
        }

        /// <summary>
        /// Refreshes the events grid with current registered events.
        /// Events are displayed sorted alphabetically by keyword.
        /// </summary>
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

        /// <summary>
        /// Applies configuration values to UI controls.
        /// Sets the _isLoadingConfig flag to prevent event handlers from firing during load.
        /// </summary>
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
            cooldownMinText.Text = _config.MinCooldownMs.ToString();
            cooldownMaxText.Text = _config.MaxCooldownMs.ToString();
            _isLoadingConfig = false;
        }

        /// <summary>
        /// Applies configuration values to the emotion engine.
        /// Called when configuration is loaded from a project file.
        /// </summary>
        private void UpdateEngineFromConfig()
        {
            if (_config == null)
            {
                return;
            }

            _emotionEngine.SetShiftOptions(_config.AllowMoodShift, _config.AllowPersonalityShift);
            _emotionEngine.SetRandomizePersonality(_config.RandomizePersonality, false);
            _emotionEngine.SetPersonality(_config.TemperamentValence, _config.TemperamentArousal, true);
            _emotionEngine.MinCooldownMs = _config.MinCooldownMs;
            _emotionEngine.MaxCooldownMs = _config.MaxCooldownMs;
            UpdateStateLabels();
        }

        /// <summary>
        /// Handles the Apply Personality button click. Applies new personality/temperament values.
        /// </summary>
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

        /// <summary>
        /// Handles the Randomize Personality checkbox change. Enables/disables personality randomization.
        /// </summary>
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

        /// <summary>
        /// Handles the Allow Mood Shift checkbox change. Enables/disables mood drift over time.
        /// </summary>
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

        /// <summary>
        /// Handles the Allow Personality Shift checkbox change. Enables/disables personality drift over time.
        /// </summary>
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

        /// <summary>
        /// Handles the Load Event button click. Loads selected event from grid into edit fields.
        /// </summary>
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

        /// <summary>
        /// Handles the Delete Event button click. Removes selected event after confirmation.
        /// </summary>
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

    }
}
