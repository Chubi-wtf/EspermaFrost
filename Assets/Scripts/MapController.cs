using UnityEngine;

public class MapController : MonoBehaviour
{
    [Header("Configuración UI")]
    public GameObject mapUI; // Arrastra aquí el Panel o Canvas de tu mapa

    private bool isMapOpen = false;

    private void Start()
    {
        // REQUISITO: Aseguramos que el mapa empiece desactivado
        if (mapUI != null)
        {
            mapUI.SetActive(false);
        }

        isMapOpen = false;
    }

    private void Update()
    {
        // Detectar la tecla TAB
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            ToggleMap();
        }
    }

    private void ToggleMap()
    {
        // Invertimos el estado (si es true pasa a false, y viceversa)
        isMapOpen = !isMapOpen;

        // 1. Activar/Desactivar la imagen del mapa
        if (mapUI != null)
        {
            mapUI.SetActive(isMapOpen);
        }

        // 2. Controlar el cursor y el movimiento del jugador
        if (isMapOpen)
        {
            // --- ESTADO: MAPA ABIERTO ---

            // Liberamos el mouse (por si el mapa tiene botones o marcadores)
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;

            // Bloqueamos el movimiento del jugador (Integración con tu script)
            if (PlayerMovement.Instance != null)
                PlayerMovement.Instance.canMove = false;

            // OPCIONAL: Si quieres que el juego se pause totalmente (el tiempo se detiene):
            // Time.timeScale = 0f; 
        }
        else
        {
            // --- ESTADO: MAPA CERRADO ---

            // Bloqueamos el mouse de nuevo para jugar
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;

            // Reactivamos el movimiento
            if (PlayerMovement.Instance != null)
                PlayerMovement.Instance.canMove = true;

            // OPCIONAL: Si pausaste el tiempo, reactívalo aquí:
            // Time.timeScale = 1f;
        }
    }
}