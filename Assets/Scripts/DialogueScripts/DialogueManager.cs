using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DialogueManager : MonoBehaviour
{
    [Header("UI / SYSTEM")]
    public DialogueScriptable initialDialogue;
    public TextMeshProUGUI dialogueText, nameText;
    public Image spriteImage;

    [Header("DIALOGUE")]
    public bool isTalking;
    public int phraseIndex;
    public bool isTypeWriterEnded;

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.F))
        {
            Talk();
        }
    }

    private void Talk()
    {
        if (isTalking)
        {
            if (phraseIndex < initialDialogue.conversation.Length - 1)
            {
                if (isTypeWriterEnded)
                {
                    phraseIndex++;
                    RefreshPhrase();
                }
                else
                {
                    StopCoroutine("TypeWriterDialogue");
                    dialogueText.text = initialDialogue.conversation[phraseIndex].phrase;
                    isTypeWriterEnded = true;
                }
            }
            else
            {
                if (isTypeWriterEnded)
                {
                    nameText.text = string.Empty;
                    dialogueText.text = string.Empty;
                    spriteImage.sprite = null;
                    phraseIndex = 0;
                    isTalking = false;
                }
                else
                {
                    StopCoroutine("TypeWriterDialogue");
                    dialogueText.text = initialDialogue.conversation[phraseIndex].phrase;
                    isTypeWriterEnded = true;
                }
            }
        }
        else
        {
            RefreshPhrase();
            isTalking = true;
        }
    }

    private void RefreshPhrase()
    {
        nameText.text = initialDialogue.conversation[phraseIndex].characterName;
        StartCoroutine("TypeWriterDialogue");
        spriteImage.sprite = initialDialogue.conversation[phraseIndex].characterSprite;
    }

    private IEnumerator TypeWriterDialogue()
    {
        isTypeWriterEnded = false;
        dialogueText.text = string.Empty;
        foreach (char character in initialDialogue.conversation[phraseIndex].phrase)
        {
            dialogueText.text += character;
            yield return new WaitForSeconds(0.1f);
        }
        isTypeWriterEnded = true;
    }
}