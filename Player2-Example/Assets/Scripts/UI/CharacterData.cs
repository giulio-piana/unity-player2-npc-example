using UnityEngine;

[CreateAssetMenu(fileName = "New Character", menuName = "Character Data")]
public class CharacterData : ScriptableObject
{
    public string characterName;
    public Sprite characterImage;
    public string characterId;
}
