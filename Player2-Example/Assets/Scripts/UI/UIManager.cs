using UnityEngine;

public class UIManager : MonoBehaviour
{
    [SerializeField] private UICharacterSelectionPanel characterSelectionPanel;
    [SerializeField] private UIChatPanel chatPanel;
    
    private void Start()
    {
        characterSelectionPanel.OnCharacterSelected += HandleCharacterSelected;
        chatPanel.OnMessageSent += HandleMessageSent;
        
        chatPanel.gameObject.SetActive(false);
    }
    
    private void HandleCharacterSelected(CharacterData character)
    {
        chatPanel.OpenChatForCharacter(character);
    }
    
    private void HandleMessageSent(string message, CharacterData character)
    {
        Debug.Log($"Message sent to {character.characterName}: {message}");
        
        // Simulate NPC response
        chatPanel.AddMessageToChat(character.characterName, "Hello! How can I help you?");
    }
    
    private void OnDestroy()
    {
        if (characterSelectionPanel != null)
            characterSelectionPanel.OnCharacterSelected -= HandleCharacterSelected;
        
        if (chatPanel != null)
            chatPanel.OnMessageSent -= HandleMessageSent;
    }
}
