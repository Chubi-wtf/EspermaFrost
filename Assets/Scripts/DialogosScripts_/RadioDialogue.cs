using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Collections.Generic;

public class RadioDialogue : MonoBehaviour
{
    [Header("═══ DIÁLOGO POR RADIO ═══")]
    [Tooltip("Solo añade las líneas en orden. Usa 'Natasha' o 'Carlos' como nombre.")]
    public List<RadioLine> lines = new List<RadioLine>();

    [Header("Panel Izquierdo - Natasha")]
    public GameObject natashaPanel;
    public Image natashaImage;
    public TextMeshProUGUI natashaNameText;
    public TextMeshProUGUI natashaDialogueText;

    [Header("Panel Derecho - Carlos")]
    public GameObject carlosPanel;
    public Image carlosImage;
    public TextMeshProUGUI carlosNameText;
    public TextMeshProUGUI carlosDialogueText;

    [Header("Opciones")]
    public Sprite natashaSprite;
    public Sprite carlosSprite;
    public float typingSpeed = 0.04f;
    public float timeBetweenLines = 1.5f; // Tiempo automático entre mensajes (si no pulsas)

    [System.Serializable]
    public class RadioLine
    {
        public string speaker = "Natasha";     // Escribe exactamente: Natasha o Carlos
        [TextArea(3, 6)]
        public string text;
    }

    private int currentLine = 0;
    private bool inDialogue = false;

    void Start()
    {
        natashaPanel.SetActive(false);
        carlosPanel.SetActive(false);
    }

    void Update()
    {
        if (!inDialogue) return;

        if (Input.GetMouseButtonDown(0) || Input.GetKeyDown(KeyCode.Space))
        {
            NextLine();
        }
    }

    public void StartDialogue()
    {
        if (inDialogue || lines.Count == 0) return;

        inDialogue = true;
        currentLine = 0;
        Time.timeScale = 0f; // Pausa el juego durante la radio

        ShowCurrentLine();
    }

    void NextLine()
    {
        StopAllCoroutines();

        // Si aún está escribiendo → terminar de golpe
        if (natashaPanel.activeSelf) natashaDialogueText.maxVisibleCharacters = 9999;
        if (carlosPanel.activeSelf) carlosDialogueText.maxVisibleCharacters = 9999;

        currentLine++;

        if (currentLine < lines.Count)
        {
            StartCoroutine(WaitAndShowNext());
        }
        else
        {
            EndDialogue();
        }
    }

    void ShowCurrentLine()
    {
        // Ocultar ambos paneles
        natashaPanel.SetActive(false);
        carlosPanel.SetActive(false);

        var line = lines[currentLine];
        bool isNatasha = line.speaker.Trim().ToLower() == "natasha";

        if (isNatasha)
        {
            natashaPanel.SetActive(true);
            if (natashaImage) natashaImage.sprite = natashaSprite;
            if (natashaNameText) natashaNameText.text = "Natasha";
            StartCoroutine(TypeText(natashaDialogueText, line.text));
        }
        else
        {
            carlosPanel.SetActive(true);
            if (carlosImage) carlosImage.sprite = carlosSprite;
            if (carlosNameText) carlosNameText.text = "Carlos";
            StartCoroutine(TypeText(carlosDialogueText, line.text));
        }
    }

    IEnumerator TypeText(TextMeshProUGUI tmpText, string fullText)
    {
        tmpText.text = fullText;
        tmpText.maxVisibleCharacters = 0;

        for (int i = 0; i <= fullText.Length; i++)
        {
            tmpText.maxVisibleCharacters = i;
            yield return new WaitForSecondsRealtime(typingSpeed);
        }

        // Después de terminar de escribir, espera un poco y pasa solo (o espera clic)
        yield return new WaitForSecondsRealtime(timeBetweenLines);
        if (inDialogue) NextLine();
    }

    IEnumerator WaitAndShowNext()
    {
        yield return new WaitForSecondsRealtime(0.5f); // Pequeña pausa entre mensajes
        ShowCurrentLine();
    }

    void EndDialogue()
    {
        inDialogue = false;
        natashaPanel.SetActive(false);
        carlosPanel.SetActive(false);
        Time.timeScale = 1f;
    }

    // === PARA PROBAR O LLAMAR DESDE OTRO LADO ===
    private void OnTriggerEnter2D(Collider2D col)
    {
        if (col.CompareTag("Player")) StartDialogue();
    }
    private void OnTriggerEnter(Collider col)
    {
        if (col.CompareTag("Player")) StartDialogue();
    }

}