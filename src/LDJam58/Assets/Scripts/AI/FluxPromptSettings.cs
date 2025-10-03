using UnityEngine;

[CreateAssetMenu(fileName = "FluxPromptSettings", menuName = "AI/Flux Prompt Settings")]
public class FluxPromptSettings : ScriptableObject
{
    [SerializeField] 
    [TextArea(3, 10)]
    private string basePrompt = "A high quality digital artwork";

    public string BasePrompt => basePrompt;
}


