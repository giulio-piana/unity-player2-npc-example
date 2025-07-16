
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Networking;
using UnityEngine.Serialization;
using UnityEngine.UI;

[Serializable]
public class SpawnNpc
{
    public string shortName;
    public string name;
    public string characterDescription;
    public string systemPrompt;
    public string voiceID;
    public List<Function> commands;
}


[Serializable]
public class ChatRequest
{
    public string sender_name;
    public string sender_message;
    public string game_state_info;
    public string tts;
}

[Serializable]
public class NpcSpawnedEvent : UnityEvent<string> { }

public class Player2Npc : MonoBehaviour
{
    [Header("State Config")] 
    [SerializeField] private NpcManager npcManager;
    [Header("NPC Configuration")]
    [SerializeField] private string shortName = "Victor";
    [SerializeField] private string fullName = "Victor J. Johnson";
    [SerializeField] private string characterDescription = "A crazed scientist on the hunt for gold";
    [SerializeField] private string systemPrompt = "You are a mad scientist obsessed with finding gold.";
    [SerializeField] private bool persistent = false; 
    
    [Header("Events")]
    [SerializeField] private UnityEvent spawnTrigger;

    [SerializeField] private UnityEvent<ChatRequest> inputMessage;
    [SerializeField] private UnityEvent<string> outputMessage;

    

    private string _npcID = null;

    private string _gameID()
    {
        return npcManager.gameId;
    }

    private string _baseUrl() {
        return npcManager.baseUrl;
    }
    private void Start()
    {
        inputMessage.AddListener(SendMessage);
        // Subscribe to spawn trigger if it exists
        if (spawnTrigger != null)
        {
            spawnTrigger.AddListener(SpawnNpc);
        }
        else {
            // If no spawn trigger is set, spawn on start
            SpawnNpc();
        }
    }
    
    private void SpawnNpc()
    {
        var spawnData = new SpawnNpc
        {
            shortName = shortName,
            name = fullName,
            characterDescription = characterDescription,
            systemPrompt = systemPrompt,
            voiceID = "test",
            commands = npcManager.functions
        };

        StartCoroutine(SpawnNpcCoroutine(spawnData));
    }

    private IEnumerator SpawnNpcCoroutine(SpawnNpc spawnData)
    {
        string url = $"{_baseUrl()}/npc/games/{_gameID()}/npcs/spawn";
        string json = JsonUtility.ToJson(spawnData);
        byte[] bodyRaw = Encoding.UTF8.GetBytes(json);

        using (UnityWebRequest request = new UnityWebRequest(url, "POST"))
        {
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");

            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                _npcID = request.downloadHandler.text.Trim('"');
                Debug.Log($"NPC spawned successfully with ID: {_npcID}");
                npcManager.RegisterNpc(_npcID, outputMessage);
            }
            else
            {
                string error = $"Failed to spawn NPC: {request.error}";
                Debug.LogError(error);
            }
        }
    }
    
    private IEnumerator SendChatCoroutine(ChatRequest chatRequest)
    {
        if (string.IsNullOrEmpty(_npcID))
        {
            string error = "NPC ID is not set!";
            Debug.LogError(error);
            yield break;
        }

        string url = $"{_baseUrl()}/npc/games/{_gameID()}/npcs/{_npcID}/chat";
        string json = JsonUtility.ToJson(chatRequest);
        byte[] bodyRaw = Encoding.UTF8.GetBytes(json);

        using (UnityWebRequest request = new UnityWebRequest(url, "POST"))
        {
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");

            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                Debug.Log($"Message sent successfully to NPC {_npcID}");
            }
            else
            {
                string error = $"Failed to send message: {request.error}";
                Debug.LogError(error);
            }
        }
    }

    private void SendMessage(ChatRequest message)
    {
        if (_npcID != null)
        {
            StartCoroutine(SendChatCoroutine(message));
        }
        
    }

    private void OnDestroy()
    {
        if (spawnTrigger != null)
        {
            spawnTrigger.RemoveListener(SpawnNpc);
        }
    }
}
