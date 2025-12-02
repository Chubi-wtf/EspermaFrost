using UnityEngine;

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

    [Header("Tiempo mínimo entre comentarios (segundos)")]
    public float cooldown = 10f; // Para que no hable todo el tiempo

    private float lastCommentTime = -999f;

    public bool CanComment()
    {
        return Time.time > lastCommentTime + cooldown;
    }

    public string GetRandomComment()
    {
        lastCommentTime = Time.time;
        return natashaComments[Random.Range(0, natashaComments.Length)];
    }
}