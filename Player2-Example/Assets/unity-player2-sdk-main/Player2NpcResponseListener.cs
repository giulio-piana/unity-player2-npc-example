using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Networking;
using UnityEngine.Serialization;

[Serializable]
public class NpcApiChatResponse
{
    public string npcID;
    public string message;
    public SingleTextToSpeechData audio;
    public List<FunctionCall> command;
}

[Serializable]
public class SingleTextToSpeechData
{
    public string data;
}

[Serializable]
public class FunctionCall
{
    public string name;
    public string arguments;
}

[Serializable]
public class NpcResponseEvent : UnityEvent<NpcApiChatResponse> { }

public class Player2NpcResponseListener : MonoBehaviour
{
    [Header("Configuration")]
    [SerializeField] private string baseUrl = "http://localhost:4315/v1";
    [SerializeField] private string gameId = "unity";
    
    
    private Coroutine _listeningCoroutine;

    private Dictionary<string, UnityEvent<NpcApiChatResponse>> _responseEvents = new Dictionary<string, UnityEvent<NpcApiChatResponse>>();
    
    
    void Start()
    {
        StartListening();
    }

    public void RegisterNpc(string npcId, UnityEvent<NpcApiChatResponse> onNpcResponse)
    {
        _responseEvents.Add(npcId, onNpcResponse);
    }

    public void StartListening()
    {
        if (_listeningCoroutine != null)
            StopCoroutine(_listeningCoroutine);
        
        _listeningCoroutine = StartCoroutine(ListenForResponses());
    }

    public void StopListening()
    {
        if (_listeningCoroutine != null)
        {
            StopCoroutine(_listeningCoroutine);
            _listeningCoroutine = null;
        }
    }

    private IEnumerator ListenForResponses()
    {
        string url = $"{baseUrl}/npc/games/{gameId}/npcs/responses";
        
        using (UnityWebRequest request = UnityWebRequest.Get(url))
        {
            request.SendWebRequest();
            
            string buffer = "";
            
            while (!request.isDone)
            {
                if (request.downloadHandler.text.Length > buffer.Length)
                {
                    string newData = request.downloadHandler.text.Substring(buffer.Length);
                    buffer = request.downloadHandler.text;
                    
                    string[] lines = newData.Split('\n');
                    foreach (string line in lines)
                    {
                        if (!string.IsNullOrWhiteSpace(line))
                        {
                            try
                            {
                                NpcApiChatResponse response = JsonUtility.FromJson<NpcApiChatResponse>(line);
                                _responseEvents[response.npcID]?.Invoke(response);
                            }
                            catch (Exception e)
                            {
                                Debug.LogError($"Failed to parse response: {e.Message}");
                            }
                        }
                    }
                }
                
                yield return new WaitForSeconds(0.1f);
            }
        }
    }

    void OnDestroy()
    {
        StopListening();
    }
}