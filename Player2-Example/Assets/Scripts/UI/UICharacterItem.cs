using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

public class UICharacterItem : MonoBehaviour
{
    [SerializeField] private Image characterImage;
    [SerializeField] private TextMeshProUGUI characterNameText;
    [SerializeField] private Button chatButton;
    
    private CharacterData characterData;
    public event Action<CharacterData> OnChatButtonClicked;
    
    private void Awake()
    {
        chatButton.onClick.AddListener(OnChatButtonPressed);
    }
    
    public void Setup(CharacterData data)
    {
        characterData = data;
        characterImage.sprite = data.characterImage;
        characterNameText.text = data.characterName;
    }
    
    private void OnChatButtonPressed()
    {
        OnChatButtonClicked?.Invoke(characterData);
    }
    
    private void OnDestroy()
    {
        chatButton.onClick.RemoveListener(OnChatButtonPressed);
    }
}
