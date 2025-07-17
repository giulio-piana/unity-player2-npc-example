using UnityEngine;
using System.Collections.Generic;
using System;

public class UICharacterSelectionPanel : MonoBehaviour
{
    [SerializeField] private Transform scrollViewContent;
    [SerializeField] private UICharacterItem characterItemPrefab;
    [SerializeField] private List<CharacterData> characterDataList;
    
    public event Action<CharacterData> OnCharacterSelected;
    
    private List<UICharacterItem> instantiatedItems = new List<UICharacterItem>();
    
    private void Start()
    {
        PopulateCharacterList();
    }
    
    private void PopulateCharacterList()
    {
        ClearExistingItems();
        
        foreach (CharacterData characterData in characterDataList)
        {
            UICharacterItem item = Instantiate(characterItemPrefab, scrollViewContent);
            item.Setup(characterData);
            item.OnChatButtonClicked += HandleCharacterSelected;
            instantiatedItems.Add(item);
        }
    }
    
    private void HandleCharacterSelected(CharacterData characterData)
    {
        OnCharacterSelected?.Invoke(characterData);
    }
    
    private void ClearExistingItems()
    {
        foreach (UICharacterItem item in instantiatedItems)
        {
            if (item != null)
            {
                item.OnChatButtonClicked -= HandleCharacterSelected;
                Destroy(item.gameObject);
            }
        }
        instantiatedItems.Clear();
    }
    
    public void RefreshCharacterList()
    {
        PopulateCharacterList();
    }
}
