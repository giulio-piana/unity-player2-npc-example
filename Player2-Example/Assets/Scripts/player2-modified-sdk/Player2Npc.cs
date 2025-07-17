using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Networking;
using Newtonsoft.Json;
using System.IO;

namespace Player2ModifiedSdk
{
    [Serializable]
    public class SpawnNpc
    {
        public string short_name;
        public string name;
        public string character_description;
        public string system_prompt;
        public string voice_id;
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

    [Serializable]
    public class TextToSpeechRequest
    {
        public string audio_format = "wav"; // mp3 or mpeg
        public bool play_in_app = false;
        public float speed = 1f;
        public string text;
        public string voice_gender = "female";
        public List<string> voice_ids;
        public string voice_language = "en_US";
    }
    
    [Serializable]
    public class TTSResponse
    {
        public string data;
    }

    public class Player2Npc : MonoBehaviour
    {
        [Header("State Config")]
        [SerializeField] private NpcManager npcManager;

        [Header("NPC Configuration")]
        [SerializeField] NPCConfigurationSO npcConfiguration;

        [Header("Events")]
        [SerializeField] private UnityEvent spawnTrigger;

        [SerializeField] private UnityEvent<ChatRequest> inputMessage;
        [SerializeField] private UnityEvent<string> outputMessage;



        private string _npcID = null;

        private string _gameID()
        {
            return npcManager.gameId;
        }

        private string _baseUrl()
        {
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
            else
            {
                // If no spawn trigger is set, spawn on start
                SpawnNpc();
            }

            SpawnNpc();
        }

        private void SpawnNpc()
        {
            var spawnData = new SpawnNpc
            {
                short_name = npcConfiguration.shortName,
                name = npcConfiguration.fullName,
                character_description = npcConfiguration.characterDescription,
                system_prompt = npcConfiguration.systemPrompt,
                voice_id = "test",
                commands = npcManager.functions
            };

            SpawnNpcCoroutine(spawnData);
        }

        private void SpawnNpcCoroutine(SpawnNpc spawnData)
        {
            string json = JsonConvert.SerializeObject(spawnData);
            string url = $"{_baseUrl()}/npc/games/{_gameID()}/npcs/spawn";

            WebRequestHelper.SendWebRequestAsync(
                url,
                json,
                response =>
                {
                    _npcID = response.Trim('"');
                    Debug.Log($"NPC spawned successfully with ID: {_npcID}");
                    npcManager.RegisterNpc(_npcID, outputMessage);

                },
                error =>
                {
                    Debug.LogError($"Failed to spawn NPC: {error}");
                },
                CancellationToken.None
            ).ConfigureAwait(false);
        }

        private void SendChatRequest(ChatRequest chatRequest)
        {
            Debug.Log($"Sending chat request to NPC {_npcID}: {chatRequest.sender_message}");

            if (string.IsNullOrEmpty(_npcID))
            {
                string error = "NPC ID is not set!";
                Debug.LogError(error);
                return;
            }

            string json = JsonConvert.SerializeObject(chatRequest);
            string url = $"{_baseUrl()}/npc/games/{_gameID()}/npcs/{_npcID}/chat";

            WebRequestHelper.SendWebRequestAsync(
                url,
                json,
                response =>
                {
                    Debug.Log($"Message sent successfully to NPC {_npcID}");
                },
                error =>
                {
                    Debug.LogError(error);
                },
                CancellationToken.None
            ).ConfigureAwait(false);
        }

        private void SendMessage(ChatRequest message)
        {
            Debug.Log($"Sending message to NPC {_npcID}: {message.sender_message}");

            if (_npcID != null)
            {
                SendChatRequest(message);
            }

        }

        public void RequestTextToSpeech(string text, bool playInApp = false, List<string> voiceIds = null, string voiceGender = "female", string voiceLanguage = "en_US", float speed = 1f)
        {
            var ttsRequest = new TextToSpeechRequest
            {
                text = text,
                play_in_app = playInApp,
                voice_ids = voiceIds ?? new List<string> { "01955d76-ed5b-73e0-a88d-cbeb3c5b499d" },
                voice_gender = voiceGender,
                voice_language = voiceLanguage,
                speed = speed
            };

            string json = JsonConvert.SerializeObject(ttsRequest);
            string url = $"{_baseUrl()}/tts/speak";

            WebRequestHelper.SendWebRequestAsync(
                url,
                json,
                response =>
                {
                    if (playInApp)
                    {
                        Debug.Log("TTS audio played in Player2 app");
                    }
                    else
                    {
                        Debug.Log("TTS audio data received as base64 mp3");
                        // Handle base64 audio data if needed
                        var resp = JsonConvert.DeserializeObject<TTSResponse>(response);                    
                        StartCoroutine(ProcessBase64Audio(resp.data));
                        
                    }
                },
                error =>
                {
                    Debug.LogError($"Failed to convert text to speech: {error}");
                },
                CancellationToken.None
            ).ConfigureAwait(false);
        }

        IEnumerator ProcessBase64Audio(string base64Data)
        {
            byte[] newBytes = Convert.FromBase64String(base64Data);
            var tempPath = Application.persistentDataPath + "tmpBase64.wav";
            File.WriteAllBytes(tempPath, newBytes);
            UnityWebRequest request = UnityWebRequestMultimedia.GetAudioClip(tempPath, AudioType.WAV);
            yield return request.SendWebRequest();
            if (request.result.Equals(UnityWebRequest.Result.ConnectionError))
            {
                Debug.LogError(request.error);
            }
            else
            {
                AudioSource audioSource = GetComponent<AudioSource>();
                audioSource.clip = DownloadHandlerAudioClip.GetContent(request);
                audioSource.Play();
            }
            File.Delete(tempPath);

        }
        private void OnDestroy()
        {
            if (spawnTrigger != null)
            {
                spawnTrigger.RemoveListener(SpawnNpc);
            }
        }

        [ContextMenu("Player2Npc/Send Test Message")]
        public void sendTestMessage()
        {
            SendMessage(new ChatRequest
            {
                sender_name = "Player",
                sender_message = "Hello NPC!",
                game_state_info = "Initial game state",
                tts = "local_client"
            });
        }

        [ContextMenu("Player2Npc/Test Text to Speech")]
        public void testTextToSpeech()
        {
            RequestTextToSpeech("Hello, how are you doing today?");
        }
    }
}