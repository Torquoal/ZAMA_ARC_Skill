# ZAMA Emotion Model - Integration Examples

This folder contains example scripts demonstrating how to integrate the ZAMA Emotion Model with other ARC components.

## Example Scripts

### 1. `Example_VoiceToEmotion.ezs`
**Purpose**: Trigger emotion events from voice recognition

**What it does**:
- Watches for voice commands (e.g., "happy", "sad", "excited")
- Triggers corresponding emotion events
- Displays the emotional response

**Setup**:
1. Add a Voice Recognition component to your ARC project
2. Configure it to set a variable when keywords are detected (e.g., `$VoiceCommand`)
3. Update the variable name in the script to match your component
4. Register emotion events (either via UI or script commands)

**Usage**:
- Speak keywords like "happy", "sad", "excited", "angry", or "calm"
- The script will trigger the corresponding emotion event
- Check `$EmotionCurrent`, `$EmotionValence`, `$EmotionArousal` for the response

---

### 2. `Example_EmotionToServo.ezs`
**Purpose**: Continuously control servo motors based on emotion values

**What it does**:
- Continuously reads current emotion values (valence and arousal)
- Maps valence to horizontal servo position (head left/right)
- Maps arousal to vertical servo position (head up/down)
- Calculates body posture based on emotion name
- Updates servo positions at regular intervals

**Setup**:
1. Add a Servo component to your ARC project
2. Configure servo variable names (e.g., `$Servo1`, `$Servo2`)
3. Update the servo variable names and ranges in the script
4. Adjust servo min/max values to match your hardware

**Usage**:
- The script continuously polls emotion values
- Automatically updates servo positions based on current emotion
- Adjust `$updateInterval` to change update frequency

**Customization**:
- Modify the mapping formulas to change how emotions affect servo positions
- Add more servos for additional degrees of freedom
- Create gesture sequences based on emotion combinations

---

### 3. `Example_EventDrivenServo.ezs`
**Purpose**: Move servos only when emotion events are triggered (event-driven)

**What it does**:
- Watches for changes in emotion state (indicating a new event was triggered)
- Moves servos once per event, not continuously
- Maps valence and arousal to servo positions
- Calculates body posture based on emotion name

**Setup**:
1. Add a Servo component to your ARC project
2. Configure servo variable names (e.g., `$Servo1`, `$Servo2`)
3. Update the servo variable names and ranges in the script
4. Register emotion events (via UI or script)

**Usage**:
- Run the script
- Trigger emotion events (via voice, script, or UI)
- Servos move once per event, then wait for the next event
- Perfect for event-driven robot responses

**When to use**:
- Use this when you want servos to move only when events happen
- Use `Example_EmotionToServo.ezs` when you want continuous emotion-based movement

---

### 4. `Example_CompleteIntegration.ezs`
**Purpose**: Complete end-to-end example combining all components

**What it does**:
- Voice recognition triggers emotion events
- Emotion values control servo positions (continuous polling)
- Demonstrates the complete integration flow
- All in one continuous loop

**Setup**:
1. Add Voice Recognition component
2. Add Servo component(s) (optional - script will calculate positions even without hardware)
3. Update all variable names in the script to match your components
4. Register emotion events (see script initialization)

**Usage**:
- Run the script
- Speak emotion keywords ("happy", "sad", etc.)
- Watch servo positions being calculated and updated based on emotions
- Uncomment servo control lines when you have hardware

---

## Common ARC Component Variable Names

When adapting these examples, you may need to adjust variable names. Here are common patterns:

### Voice Recognition Components:
- `$VoiceCommand`
- `$SpeechText`
- `$RecognizedWord`
- `$VoiceKeyword`

### Servo Components:
- `$Servo1`, `$Servo2`, etc.
- `$ServoPosition1`, `$ServoPosition2`
- `$MaestroServo1`, `$MaestroServo2`
- Some components use ControlCommand instead of variables

**Note**: Check your component's documentation for the exact variable names it uses.

---

## Emotion Model Variables Reference

The emotion model exposes these ARC variables:

### Event Response Variables (immediate reaction to last event):
- `$EmotionCurrent` - Current emotion name (e.g., "Happy", "Sad")
- `$EmotionValence` - Valence value (-10 to +10, higher = more positive)
- `$EmotionArousal` - Arousal value (-10 to +10, higher = more energetic)

### Mood Variables (persistent, changes slowly):
- `$EmotionMood` - Current mood name
- `$EmotionMoodValence` - Mood valence value
- `$EmotionMoodArousal` - Mood arousal value
- `$EmotionTemperament` - Long-term personality/temperament

### Command Variable:
- `$Emotion.Command` - Send commands to the emotion model
  - `RegisterEvent:keyword:valence:arousal` - Register a new event
  - `TriggerEvent:keyword` - Trigger an event
  - `GetEmotion` - Update emotion response variables
  - `GetMood` - Update mood variables
  - `ListEvents` - List all registered events (stored in `$EmotionEvents`)

---

## Tips for Integration

1. **Variable Watchers**: Many ARC components support "Variable Watchers" that can trigger actions when variables change. You can use this to automatically trigger emotion events when voice commands are detected.

2. **ControlCommands**: Some components use `ControlCommand()` instead of variables. Check your component's documentation for the correct method.

3. **Timing**: Use `Sleep()` calls to allow the emotion model time to process commands before reading results.

4. **Mapping Ranges**: When mapping emotion values (-10 to +10) to servo positions (0-180) or other ranges, use linear interpolation:
   ```
   output = min + ((value - inputMin) / (inputMax - inputMin)) * (max - min)
   ```

5. **Testing**: Start with simple examples and gradually add complexity. Test each component individually before combining them.

---

## Next Steps

1. **Vision Integration**: Create events triggered by vision detection (e.g., "face_seen", "person_detected")
2. **Touch Sensors**: Create events for physical interactions (e.g., "touched", "petted")
3. **Complex Gestures**: Create sequences of servo movements based on emotion combinations
4. **Display Integration**: Map emotions to LED colors, screen displays, or facial expressions
5. **Sound Integration**: Add audio components and map emotions to sounds (see tips section for mapping formulas)
6. **Learning**: Implement feedback loops where robot behavior affects future emotional responses

---

## Troubleshooting

- **Events not triggering**: Make sure events are registered before trying to trigger them
- **Servos not moving**: Check that servo variable names match your component, and uncomment the servo control lines in the script
- **Values not updating**: `TriggerEvent` automatically updates variables, but you can call `GetEmotion` to refresh if needed
- **Commands not working**: Check that `$Emotion.Command` variable is being set correctly
- **Script syntax errors**: Make sure you're using `REPEATWHILE(1)` and `ENDREPEATWHILE` for loops, and `ENDIF` for IF statements

For more help, check the Visual Studio Output window (Debug) for detailed logging from the emotion model.

