using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

public class UIChatPanel : MonoBehaviour
{
    [SerializeField] private TMP_InputField messageInputField;
    [SerializeField] private Button sendButton;
    [SerializeField] private Button backButton;
    [SerializeField] private TextMeshProUGUI characterNameText;
    [SerializeField] private TextMeshProUGUI chatDisplayText;
    [SerializeField] private ScrollRect chatScrollRect;
    
    private CharacterData currentCharacter;
    public event Action<string, CharacterData> OnMessageSent;
    
    private void Awake()
    {
        sendButton.onClick.AddListener(SendMessage);
        backButton.onClick.AddListener(CloseChat);
        messageInputField.onSubmit.AddListener(OnInputSubmit);
    }
    
    public void OpenChatForCharacter(CharacterData character)
    {
        currentCharacter = character;
        characterNameText.text = character.characterName;
        gameObject.SetActive(true);
        ClearChat();
        messageInputField.text = "";
        messageInputField.Select();
    }
    
    public void CloseChat()
    {
        gameObject.SetActive(false);
        currentCharacter = null;
    }
    
    private void SendMessage()
    {
        if (string.IsNullOrWhiteSpace(messageInputField.text) || currentCharacter == null)
            return;
        
        string message = messageInputField.text;
        OnMessageSent?.Invoke(message, currentCharacter);
        
        AddMessageToChat("You", message);
        messageInputField.text = "";
        messageInputField.Select();
    }
    
    private void OnInputSubmit(string text)
    {
        if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
        {
            SendMessage();
        }
    }
    
    public void AddMessageToChat(string sender, string message)
    {
        chatDisplayText.text += $"{sender}: {message}\n";
        Canvas.ForceUpdateCanvases();
        chatScrollRect.verticalNormalizedPosition = 0f;
    }
    
    private void ClearChat()
    {
        chatDisplayText.text = "";
    }
    
    private void OnDestroy()
    {
        sendButton.onClick.RemoveListener(SendMessage);
        backButton.onClick.RemoveListener(CloseChat);
        messageInputField.onSubmit.RemoveListener(OnInputSubmit);
    }
}
