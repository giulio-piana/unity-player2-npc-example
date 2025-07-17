# Player2 NPC SDK

A Unity SDK for integrating AI-powered NPCs into your game using the Player2 platform. This SDK provides a complete solution for spawning, managing, and communicating with intelligent NPCs that can respond to player messages and execute custom functions.

## Features

- **NPC Spawning**: Create and spawn AI NPCs with custom personalities and behaviors
- **Real-time Communication**: Send messages to NPCs and receive responses in real-time
- **Function Calling**: NPCs can execute custom functions based on conversation context
- **Event-driven Architecture**: Unity Events for easy integration with existing game systems
- **Audio Support**: Built-in text-to-speech integration
- **Persistent NPCs**: Optional persistence across game sessions

## Quick Start

### 1. Setup

1. Add the `player2-modified-sdk` folder to your Unity project's Scripts directory
2. Ensure your project has TextMeshPro imported (required dependency)
3. Make sure you have a Player2 server running (default: `http://localhost:4315`)

### 2. Basic Setup

Create an empty GameObject in your scene and add the required components:

```csharp
// 1. Add NpcManager component
// 2. Add Player2NpcResponseListener component
// 3. Add Player2Npc component for each NPC
```

### 3. Configure the NPC Manager

The `NpcManager` is the central component that manages all NPCs in your scene:

```csharp
[Header("Config")]
public string gameId = "your-game-id";          // Your unique game identifier
public string baseUrl = "http://localhost:4315/v1"; // Player2 server URL
public List<Function> functions;                // Available functions for NPCs
```

### 4. Configure Individual NPCs

Each `Player2Npc` component represents a single NPC character:

```csharp
[Header("NPC Configuration")]
public string shortName = "Victor";             // Short identifier
public string fullName = "Victor J. Johnson";  // Full character name
public string characterDescription = "A crazed scientist on the hunt for gold";
public string systemPrompt = "You are a mad scientist obsessed with finding gold.";
public bool persistent = false;                // Whether NPC persists across sessions
```

## Core Components

### NpcManager

The central manager that coordinates all NPC operations.

**Key Methods:**
- `RegisterNpc(string id, UnityEvent<string> onNpcResponse)`: Register an NPC to receive responses

**Configuration:**
- `gameId`: Unique identifier for your game
- `baseUrl`: Player2 server endpoint
- `functions`: List of available functions NPCs can call

### Player2Npc

Individual NPC controller that handles spawning and communication.

**Key Events:**
- `spawnTrigger`: UnityEvent to trigger NPC spawning
- `inputMessage`: UnityEvent<ChatRequest> for sending messages to NPC
- `outputMessage`: UnityEvent<string> for receiving NPC responses

**Key Methods:**
- `SpawnNpc()`: Spawns the NPC on the server
- `SendMessage(ChatRequest message)`: Sends a chat message to the NPC

### Player2NpcResponseListener

Handles real-time response listening from the Player2 server.

**Key Methods:**
- `RegisterNpc(string npcId, UnityEvent<NpcApiChatResponse> onNpcResponse)`: Register for NPC responses
- `StartListening()`: Begin listening for responses
- `StopListening()`: Stop listening for responses

## Data Structures

### Function

Defines available functions that NPCs can call:

```csharp
[Serializable]
public class Function
{
    public string name;              // Function name
    public string description;       // What the function does
    public Parameters parameters;    // Function parameters
}
```

### ChatRequest

Structure for sending messages to NPCs:

```csharp
[Serializable]
public class ChatRequest
{
    public string sender_name;       // Name of the message sender
    public string sender_message;    // The actual message
    public string game_state_info;   // Current game state context
    public string tts;              // Text-to-speech settings
}
```

### NpcApiChatResponse

Response received from NPCs:

```csharp
[Serializable]
public class NpcApiChatResponse
{
    public string npcID;                    // ID of the responding NPC
    public string message;                  // NPC's text response
    public SingleTextToSpeechData audio;    // Audio data for TTS
    public List<FunctionCall> command;      // Functions to execute
}
```

## Usage Examples

### Basic NPC Setup

```csharp
// 1. Create GameObject with NpcManager
GameObject manager = new GameObject("NPC Manager");
NpcManager npcManager = manager.AddComponent<NpcManager>();
npcManager.gameId = "my-game";
npcManager.baseUrl = "http://localhost:4315/v1";

// 2. Add Response Listener
manager.AddComponent<Player2NpcResponseListener>();

// 3. Create NPC GameObject
GameObject npcObject = new GameObject("Merchant NPC");
Player2Npc npc = npcObject.AddComponent<Player2Npc>();
```

### Sending Messages to NPCs

```csharp
public class PlayerController : MonoBehaviour
{
    [SerializeField] private Player2Npc targetNpc;
    
    public void TalkToNpc(string playerMessage)
    {
        ChatRequest request = new ChatRequest
        {
            sender_name = "Player",
            sender_message = playerMessage,
            game_state_info = "Player is in the marketplace",
            tts = "enabled"
        };
        
        targetNpc.inputMessage.Invoke(request);
    }
}
```

### Handling NPC Responses

```csharp
public class DialogueUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI dialogueText;
    [SerializeField] private Player2Npc npc;
    
    void Start()
    {
        // Subscribe to NPC responses
        npc.outputMessage.AddListener(OnNpcResponse);
    }
    
    void OnNpcResponse(string response)
    {
        dialogueText.text = response;
        // Handle the NPC's response (display dialogue, trigger animations, etc.)
    }
}
```

### Custom Function Implementation

```csharp
// 1. Define the function in NpcManager
Function giveItemFunction = new Function
{
    name = "give_item",
    description = "Give an item to the player",
    parameters = new Parameters
    {
        type = "object",
        Properties = new Dictionary<string, object>
        {
            ["item_name"] = new { type = "string", description = "Name of the item to give" },
            ["quantity"] = new { type = "integer", description = "Number of items to give" }
        },
        required = new List<string> { "item_name", "quantity" }
    }
};

// 2. Add to NpcManager's functions list
npcManager.functions.Add(giveItemFunction);

// 3. Handle function calls in your game logic
public void HandleFunctionCall(FunctionCall functionCall)
{
    if (functionCall.name == "give_item")
    {
        // Parse arguments and execute the function
        var args = JsonUtility.FromJson<GiveItemArgs>(functionCall.arguments);
        inventory.AddItem(args.item_name, args.quantity);
    }
}
```

## Advanced Configuration

### Server Configuration

Update the server settings in `NpcManager`:

```csharp
// For production server
npcManager.baseUrl = "https://your-production-server.com/v1";

// For local development
npcManager.baseUrl = "http://localhost:4315/v1";
```

### Game State Integration

Provide context to NPCs by including game state information:

```csharp
ChatRequest contextualRequest = new ChatRequest
{
    sender_name = "Player",
    sender_message = "What items do you have for sale?",
    game_state_info = $"Player level: {playerLevel}, Gold: {playerGold}, Location: {currentLocation}",
    tts = "enabled"
};
```

### Event-Driven NPC Spawning

Use Unity Events to control when NPCs spawn:

```csharp
public class NPCSpawner : MonoBehaviour
{
    [SerializeField] private Player2Npc npc;
    
    public void OnPlayerEnterArea()
    {
        // Trigger NPC spawn when player enters area
        npc.spawnTrigger.Invoke();
    }
}
```

## Troubleshooting

### Common Issues

1. **NPC not spawning**
   - Check that the Player2 server is running
   - Verify the `baseUrl` and `gameId` configuration
   - Check Unity Console for error messages

2. **No responses received**
   - Ensure `Player2NpcResponseListener` is active
   - Verify NPC registration in `NpcManager`
   - Check network connectivity to the server

3. **Function calls not working**
   - Verify function definitions in `NpcManager.functions`
   - Check that function names match exactly
   - Ensure proper JSON formatting in function parameters

### Debug Tips

- Enable Unity Console logging to see detailed error messages
- Use the Network tab in Unity's Profiler to monitor API calls
- Check server logs for additional debugging information

## API Reference

### NpcManager
- `gameId: string` - Game identifier
- `baseUrl: string` - Server endpoint
- `functions: List<Function>` - Available functions
- `RegisterNpc(string, UnityEvent<string>)` - Register NPC for responses

### Player2Npc
- `shortName: string` - NPC identifier
- `fullName: string` - Character name
- `characterDescription: string` - Character description
- `systemPrompt: string` - AI behavior prompt
- `persistent: bool` - Persistence setting
- `spawnTrigger: UnityEvent` - Spawn trigger event
- `inputMessage: UnityEvent<ChatRequest>` - Message input
- `outputMessage: UnityEvent<string>` - Response output

### Player2NpcResponseListener
- `baseUrl: string` - Server endpoint
- `gameId: string` - Game identifier
- `RegisterNpc(string, UnityEvent<NpcApiChatResponse>)` - Register for responses
- `StartListening()` - Begin listening
- `StopListening()` - Stop listening

## Requirements

- Unity 2019.4 or later
- TextMeshPro package
- Player2 server running and accessible
- Internet connection (for server communication)

## License

This SDK is part of the Player2 platform. Please refer to your Player2 license agreement for usage terms.
