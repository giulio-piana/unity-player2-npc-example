# Player2 Modified SDK

A Unity SDK for integrating intelligent NPCs with text-to-speech capabilities into your Unity games using the Player2 platform.

## Features

- **NPC Spawning and Management**: Create and manage AI-powered NPCs with custom personalities
- **Real-time Chat System**: Send messages to NPCs and receive intelligent responses
- **Text-to-Speech Integration**: Convert NPC responses to audio with customizable voice settings
- **ScriptableObject Configuration**: Easy NPC configuration through Unity's inspector
- **Function Calling**: NPCs can trigger custom functions in your game
- **Async Web Requests**: Non-blocking communication with the Player2 API
- **Response Listening**: Real-time listening for NPC responses via WebSocket-like polling

## Installation

1. Copy the `Assets/Scripts/player2-modified-sdk` folder into your Unity project
2. Ensure you have the Newtonsoft.Json package installed via Package Manager
3. Set up your Player2 server endpoint and game ID

## Quick Start

### 1. Setup NPC Manager

Create a GameObject with the `NpcManager` component:

```csharp
// Configure in inspector or via code
npcManager.gameId = "your-game-id";
npcManager.baseUrl = "http://localhost:4315/v1";
```

### 2. Create NPC Configuration

Create an NPC configuration ScriptableObject:

1. Right-click in Project window → Create → ScriptableObjects → NPCConfiguration
2. Configure your NPC properties:

```csharp
// Example NPC Configuration
shortName = "Victor";
fullName = "Victor J. Johnson";
characterDescription = "A crazed scientist on the hunt for gold";
systemPrompt = "You are a mad scientist obsessed with finding gold.";
```

### 3. Setup Player2Npc Component

Add the `Player2Npc` component to a GameObject and configure:

```csharp
[SerializeField] private NpcManager npcManager;          // Reference to NPC Manager
[SerializeField] NPCConfigurationSO npcConfiguration;   // NPC configuration asset
[SerializeField] private UnityEvent spawnTrigger;       // Optional spawn trigger
[SerializeField] private UnityEvent<ChatRequest> inputMessage;   // Input message event
[SerializeField] private UnityEvent<string> outputMessage;      // Output message event
```

## Usage Examples

### Basic NPC Interaction

```csharp
using Player2ModifiedSdk;

public class PlayerController : MonoBehaviour
{
    [SerializeField] private Player2Npc npcComponent;
    
    void Start()
    {
        // Subscribe to NPC responses
        npcComponent.outputMessage.AddListener(OnNpcResponse);
    }
    
    public void SendMessageToNpc(string message)
    {
        var chatRequest = new ChatRequest
        {
            sender_name = "Player",
            sender_message = message,
            game_state_info = "Player is standing near the laboratory",
            tts = "local_client"
        };
        
        npcComponent.inputMessage.Invoke(chatRequest);
    }
    
    private void OnNpcResponse(string response)
    {
        Debug.Log($"NPC responded: {response}");
        // Update UI, trigger animations, etc.
    }
}
```

### Text-to-Speech Usage

```csharp
public class NpcVoiceController : MonoBehaviour
{
    [SerializeField] private Player2Npc npcComponent;
    
    public void SpeakText(string text)
    {
        // Basic TTS
        npcComponent.RequestTextToSpeech(text);
        
        // Advanced TTS with custom settings
        npcComponent.RequestTextToSpeech(
            text: "Hello, adventurer!",
            playInApp: false,
            voiceIds: new List<string> { "01955d76-ed5b-73e0-a88d-cbeb3c5b499d" },
            voiceGender: "female",
            voiceLanguage: "en_US",
            speed: 1.2f
        );
    }
}
```

### Custom Function Integration

Configure functions that NPCs can call in your game:

```csharp
// In NpcManager, add functions
public List<Function> functions = new List<Function>
{
    new Function
    {
        name = "give_item",
        description = "Give an item to the player",
        parameters = new Parameters
        {
            type = "object",
            Properties = new Dictionary<string, object>
            {
                { "item_name", new { type = "string", description = "Name of the item" } },
                { "quantity", new { type = "integer", description = "Number of items" } }
            },
            required = new List<string> { "item_name", "quantity" }
        }
    }
};
```

### UI Integration Example

```csharp
public class ChatUI : MonoBehaviour
{
    [SerializeField] private InputField messageInput;
    [SerializeField] private Text chatDisplay;
    [SerializeField] private Player2Npc npcComponent;
    
    void Start()
    {
        npcComponent.outputMessage.AddListener(DisplayNpcMessage);
    }
    
    public void SendMessage()
    {
        string message = messageInput.text;
        if (string.IsNullOrEmpty(message)) return;
        
        DisplayPlayerMessage(message);
        
        var chatRequest = new ChatRequest
        {
            sender_name = "Player",
            sender_message = message,
            game_state_info = GetCurrentGameState(),
            tts = "local_client"
        };
        
        npcComponent.inputMessage.Invoke(chatRequest);
        messageInput.text = "";
    }
    
    private void DisplayPlayerMessage(string message)
    {
        chatDisplay.text += $"\nPlayer: {message}";
    }
    
    private void DisplayNpcMessage(string message)
    {
        chatDisplay.text += $"\nNPC: {message}";
    }
    
    private string GetCurrentGameState()
    {
        return "Player is in the main town square";
    }
}
```

## API Reference

### Core Classes

#### Player2Npc
Main component for NPC interaction.

**Key Methods:**
- `SendMessage(ChatRequest message)` - Send a message to the NPC
- `RequestTextToSpeech(string text, ...)` - Convert text to speech
- `SpawnNpc()` - Spawn the NPC (called automatically on Start)

#### ChatRequest
```csharp
public class ChatRequest
{
    public string sender_name;      // Name of the message sender
    public string sender_message;   // The actual message content
    public string game_state_info;  // Current game state context
    public string tts;              // TTS preference ("local_client" or "play_in_app")
}
```

#### NPCConfigurationSO
ScriptableObject for NPC configuration.

**Properties:**
- `shortName` - Short identifier for the NPC
- `fullName` - Full display name
- `characterDescription` - NPC's character traits
- `systemPrompt` - AI behavior instructions
- `persistent` - Whether NPC state persists between sessions

#### NpcManager
Manages multiple NPCs and global settings.

**Configuration:**
- `gameId` - Your game's unique identifier
- `baseUrl` - Player2 API endpoint
- `functions` - Available functions NPCs can call

### Events

#### UnityEvents Available
- `spawnTrigger` - Triggers NPC spawning
- `inputMessage` - Handles incoming chat messages
- `outputMessage` - Handles outgoing NPC responses

### TextToSpeechRequest Options

```csharp
public class TextToSpeechRequest
{
    public string audio_format = "wav";           // "wav" or "mp3"
    public bool play_in_app = false;              // Play in Player2 app vs return audio data
    public float speed = 1f;                      // Speech speed multiplier
    public string text;                           // Text to convert
    public string voice_gender = "female";       // "male" or "female"
    public List<string> voice_ids;               // Specific voice IDs to use
    public string voice_language = "en_US";      // Language code
}
```

## Testing

The SDK includes built-in testing methods accessible via context menu:

1. **Right-click on Player2Npc component** → Player2Npc/Send Test Message
2. **Right-click on Player2Npc component** → Player2Npc/Test Text to Speech

## Configuration

### Server Setup
Ensure your Player2 server is running and accessible. Update the `baseUrl` in NpcManager:

```csharp
// Local development
baseUrl = "http://localhost:4315/v1";

// Production
baseUrl = "https://your-player2-server.com/v1";
```

### Audio Requirements
For TTS functionality, ensure your GameObject has an `AudioSource` component attached.

## Dependencies

- **Newtonsoft.Json** - For JSON serialization
- **Unity 2021.3+** - Recommended Unity version
- **Player2 Server** - Backend API server

## Troubleshooting

### Common Issues

1. **NPC not spawning**
   - Check that `gameId` and `baseUrl` are correctly configured
   - Verify Player2 server is running and accessible
   - Check Unity console for error messages

2. **TTS not working**
   - Ensure AudioSource component is attached to the GameObject
   - Check that the voice ID exists and is valid
   - Verify audio file permissions in persistent data path

3. **Messages not being sent**
   - Confirm NPC has been spawned successfully (check console logs)
   - Verify ChatRequest fields are properly populated
   - Check network connectivity to Player2 server

### Debug Mode
Enable verbose logging by adding this to your NpcManager:

```csharp
void Start()
{
    Debug.unityLogger.logEnabled = true;
}
```

## License

This modified SDK is provided as-is for integration with the Player2 platform. Please refer to the original Player2 SDK license for terms and conditions.

## Support

For issues related to:
- **SDK Integration**: Check the Unity console for detailed error messages
- **Player2 Platform**: Refer to Player2 documentation
- **API Endpoints**: Verify server configuration and network connectivity

## Version History

- **v1.0** - Initial modified SDK with TTS integration and ScriptableObject configuration
- **v1.1** - Added async web request support and improved error handling
- **v1.2** - Enhanced response listening and function calling capabilities
