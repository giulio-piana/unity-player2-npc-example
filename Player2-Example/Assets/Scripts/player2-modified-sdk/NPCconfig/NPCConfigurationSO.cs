using UnityEngine;


[CreateAssetMenu(fileName = "NPCConfiguration", menuName = "ScriptableObjects/NPCConfiguration")]
public class NPCConfigurationSO : ScriptableObject
{
    [Header("NPC Configuration")]
    [SerializeField] public string shortName = "Victor";
    [SerializeField] public string fullName = "Victor J. Johnson";
    [SerializeField] public string characterDescription = "A crazed scientist on the hunt for gold";
    [SerializeField] public string systemPrompt = "You are a mad scientist obsessed with finding gold.";
    [SerializeField] public bool persistent = false;
}
