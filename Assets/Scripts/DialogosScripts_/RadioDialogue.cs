using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Collections.Generic;

public class RadioDialogue : MonoBehaviour
{
    #region Variables de Configuración

    [Header("═══ DIÁLOGO POR RADIO ═══")]
    [Tooltip("Solo añade las líneas en orden. Usa 'Natasha' o 'Carlos' como nombre.")]
    public List<RadioLine> lines = new List<RadioLine>();

    [System.Serializable]
    public class RadioLine
    {
        public string speaker = "Natasha";
        [TextArea(3, 6)]
        public string text;
    }

    #endregion

    #region Referencias de UI - Natasha

    [Header("Panel Izquierdo - Natasha")]
    public GameObject natashaPanel;
    public Image natashaImage;
    public TextMeshProUGUI natashaNameText;
    public TextMeshProUGUI natashaDialogueText;

    #endregion

    #region Referencias de UI - Carlos

    [Header("Panel Derecho - Carlos")]
    public GameObject carlosPanel;
    public Image carlosImage;
    public TextMeshProUGUI carlosNameText;
    public TextMeshProUGUI carlosDialogueText;

    #endregion

    #region Opciones y Sprites

    [Header("Opciones de Personajes")]
    public Sprite natashaSprite;
    public Sprite carlosSprite;

    [Header("Configuración de Timing")]
    public float typingSpeed = 0.04f;
    [Tooltip("Tiempo automático entre mensajes (si no pulsas)")]
    public float timeBetweenLines = 1.5f;

    #endregion

    #region Control de Interacción

    [Header("Configuración de Interacción")]
    [Tooltip("Si está activado, solo se puede iniciar con 'E' (PlayerInteraction)")]
    public bool requiresInteraction = true;
    [Tooltip("Si está activado, se puede activar entrando en el trigger")]
    public bool triggerActivation = false;
    [Tooltip("Si está activado, el diálogo solo se reproduce una vez")]
    public bool playOnce = false;
    private bool hasBeenPlayed = false;

    #endregion

    #region Variables Internas

    private int currentLine = 0;
    private bool inDialogue = false;

    #endregion

    #region Métodos de Unity

    void Start()
    {
        // Asegurarse de que los paneles estén ocultos al inicio
        if (natashaPanel != null) natashaPanel.SetActive(false);
        if (carlosPanel != null) carlosPanel.SetActive(false);
    }

    void Update()
    {
        if (!inDialogue) return;

        // Permitir avanzar con click o espacio
        if (Input.GetMouseButtonDown(0) || Input.GetKeyDown(KeyCode.Space))
        {
            NextLine();
        }
    }

    #endregion

    #region Sistema de Activación por Trigger (Opcional)

    private void OnTriggerEnter(Collider col)
    {
        if (!triggerActivation) return;
        if (col.CompareTag("Player"))
        {
            StartDialogue();
        }
    }

    private void OnTriggerEnter2D(Collider2D col)
    {
        if (!triggerActivation) return;
        if (col.CompareTag("Player"))
        {
            StartDialogue();
        }
    }

    #endregion

    #region Sistema de Diálogo Principal

    /// <summary>
    /// Método público para iniciar el diálogo (llamado desde PlayerInteraction)
    /// </summary>
    public void StartDialogue()
    {
        // Verificar si ya se reprodujo
        if (playOnce && hasBeenPlayed)
        {
            Debug.Log("Este diálogo ya fue reproducido.");
            return;
        }

        // Verificar si ya está en diálogo o no hay líneas
        if (inDialogue || lines.Count == 0)
        {
            Debug.Log("Ya está en diálogo o no hay líneas configuradas.");
            return;
        }

        // Marcar como reproducido
        if (playOnce) hasBeenPlayed = true;

        // Iniciar diálogo
        inDialogue = true;
        currentLine = 0;
        Time.timeScale = 0f; // Pausa el juego durante la radio

        Debug.Log("Iniciando diálogo por radio...");
        ShowCurrentLine();
    }

    void ShowCurrentLine()
    {
        // Ocultar ambos paneles
        if (natashaPanel != null) natashaPanel.SetActive(false);
        if (carlosPanel != null) carlosPanel.SetActive(false);

        var line = lines[currentLine];
        bool isNatasha = line.speaker.Trim().ToLower() == "natasha";

        if (isNatasha)
        {
            if (natashaPanel != null) natashaPanel.SetActive(true);
            if (natashaImage != null) natashaImage.sprite = natashaSprite;
            if (natashaNameText != null) natashaNameText.text = "Natasha";
            if (natashaDialogueText != null)
                StartCoroutine(TypeText(natashaDialogueText, line.text));
        }
        else
        {
            if (carlosPanel != null) carlosPanel.SetActive(true);
            if (carlosImage != null) carlosImage.sprite = carlosSprite;
            if (carlosNameText != null) carlosNameText.text = "Carlos";
            if (carlosDialogueText != null)
                StartCoroutine(TypeText(carlosDialogueText, line.text));
        }
    }

    void NextLine()
    {
        StopAllCoroutines();

        // Si aún está escribiendo → terminar de golpe
        if (natashaPanel != null && natashaPanel.activeSelf && natashaDialogueText != null)
            natashaDialogueText.maxVisibleCharacters = 9999;
        if (carlosPanel != null && carlosPanel.activeSelf && carlosDialogueText != null)
            carlosDialogueText.maxVisibleCharacters = 9999;

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

    void EndDialogue()
    {
        inDialogue = false;
        if (natashaPanel != null) natashaPanel.SetActive(false);
        if (carlosPanel != null) carlosPanel.SetActive(false);
        Time.timeScale = 1f;

        Debug.Log("Diálogo por radio finalizado.");
    }

    #endregion

    #region Sistema de Escritura Animada

    IEnumerator TypeText(TextMeshProUGUI tmpText, string fullText)
    {
        tmpText.text = fullText;
        tmpText.maxVisibleCharacters = 0;

        for (int i = 0; i <= fullText.Length; i++)
        {
            tmpText.maxVisibleCharacters = i;
            yield return new WaitForSecondsRealtime(typingSpeed);
        }

        // Después de terminar de escribir, espera un poco y pasa automáticamente
        yield return new WaitForSecondsRealtime(timeBetweenLines);
        if (inDialogue) NextLine();
    }

    IEnumerator WaitAndShowNext()
    {
        yield return new WaitForSecondsRealtime(0.5f); // Pequeña pausa entre mensajes
        ShowCurrentLine();
    }

    #endregion

    #region Métodos de Utilidad (Debug)

    /// <summary>
    /// Para probar el diálogo directamente desde el Inspector
    /// </summary>
    [ContextMenu("Test Dialogue")]
    public void TestDialogue()
    {
        StartDialogue();
    }

    /// <summary>
    /// Para resetear el diálogo y poder reproducirlo de nuevo
    /// </summary>
    [ContextMenu("Reset Dialogue")]
    public void ResetDialogue()
    {
        hasBeenPlayed = false;
        Debug.Log("Diálogo reseteado. Se puede reproducir de nuevo.");
    }

    #endregion
}