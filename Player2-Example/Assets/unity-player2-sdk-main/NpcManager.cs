using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;


[Serializable]
public class Function
{
    public string name;
    public string description;
    public Parameters parameters;
}

[Serializable]
public class Parameters
{
    public string type;
    public Dictionary<string, object> Properties;
    public List<string> required;
}
public class NpcManager : MonoBehaviour
{
    [Header("Config")]
    [SerializeField] public string gameId = "your-game-id";

    
    private UnityEvent<Function> _functionEvent;
    
    private Player2NpcResponseListener _responseListener;
    
    [Header("Functions")] [SerializeField] public List<Function> functions;

    
    public string baseUrl = "http://localhost:4315/v1";

    public void RegisterNpc(string id, UnityEvent<string> onNpcResponse)
    {
        UnityEvent<NpcApiChatResponse> onNpcApiResponse = new UnityEvent<NpcApiChatResponse>();
        onNpcApiResponse.AddListener((response) =>
        {
            onNpcResponse.Invoke(response.message);
        });
        _responseListener.RegisterNpc(id, onNpcApiResponse);
    }
}
