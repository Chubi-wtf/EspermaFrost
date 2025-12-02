using UnityEngine;
using TMPro;
using System.Collections;

public class CommentableObject : MonoBehaviour
{
    [Header("Comentarios de Natasha (elige uno al azar)")]
    public string[] natashaComments = new string[]
    {
        "La verdad es que jamás entendí por qué están esos numeros por toda la nave.",
        "Recuerda el plan: activa el generador primero. ¿Es eso?",
        "Los numeros siempre los numeros... ¿Qué significan?",
        "Los numeros parecen importantes, pero no sé por qué.",
        "los numeros, por qué siempre debe haber numeros..."
    };

    [Header("Configuración por objeto")]
    public float cooldown = 10f;                // Tiempo mínimo entre comentarios
    public float commentDisplayTime = 3f;       // Duración visible del panel (configurable en Inspector)
    public float typingSpeed = 0.04f;           // Velocidad de escritura (como en RadioDialogue)

    [Header("Referencias UI (global, asigna o busca auto)")]
    public GameObject natashaCommentPanel;      // Panel de Natasha
    public TextMeshProUGUI natashaCommentText;  // Texto TMP para el comentario

    private float lastCommentTime = -999f;

    private void Awake()
    {
        // Buscar automáticamente si no asignas (global para todos los objetos)
        if (natashaCommentPanel == null)
        {
            natashaCommentPanel = GameObject.Find("NatashaCommentPanel");
            if (natashaCommentPanel) natashaCommentPanel.SetActive(false);
            natashaCommentText = natashaCommentPanel?.GetComponentInChildren<TextMeshProUGUI>();
        }

        // LOG: Para chequear si encuentra el panel
        if (natashaCommentPanel == null) Debug.LogError("CommentableObject: No encontró NatashaCommentPanel!");
        else Debug.Log("CommentableObject: Panel encontrado OK.");
    }

    public bool CanComment()
    {
        return Time.time > lastCommentTime + cooldown;
    }

    // Método público que PlayerInteraction llama para mostrar un comentario random
    public void ShowRandomComment()
    {
        // LOG: Se llamó al método
        Debug.Log("ShowRandomComment llamado en " + gameObject.name);

        if (!CanComment())
        {
            Debug.Log("Cooldown activo, no muestra comentario.");
            return;
        }

        string comment = natashaComments[Random.Range(0, natashaComments.Length)];
        lastCommentTime = Time.time;

        // LOG: Iniciando corutina
        Debug.Log("Iniciando corutina con comentario: " + comment);
        StartCoroutine(TypeAndShowComment(comment));
    }

    // Corutina con animación de escritura (como en RadioDialogue)
    private IEnumerator TypeAndShowComment(string text)
    {
        if (natashaCommentPanel) natashaCommentPanel.SetActive(true);
        else Debug.LogError("Panel es null en corutina!");

        if (natashaCommentText != null)
        {
            natashaCommentText.text = text;
            natashaCommentText.maxVisibleCharacters = 0;

            for (int i = 0; i <= text.Length; i++)
            {
                natashaCommentText.maxVisibleCharacters = i;
                yield return new WaitForSecondsRealtime(typingSpeed);
            }
        }
        else Debug.LogError("TMP Text es null!");

        // Espera la duración configurable
        yield return new WaitForSecondsRealtime(commentDisplayTime);

        if (natashaCommentPanel) natashaCommentPanel.SetActive(false);

        // LOG: Fin de corutina
        Debug.Log("Comentario mostrado y ocultado.");
    }
}