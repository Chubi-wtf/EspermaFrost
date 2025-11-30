using UnityEngine;

[CreateAssetMenu(fileName = "New Dialogue", menuName = "ScriptableObjects/Dialogue")]

public class DialogueScriptable : ScriptableObject
{
    [Header("Dialogue")]
    public DIALOGUE[] conversation;
}
