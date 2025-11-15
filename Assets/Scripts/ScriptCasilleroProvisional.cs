using UnityEngine;
using System.Collections;

public class ScriptCasilleroProvisional : MonoBehaviour
{
    [Header("Referencias")]
    public Transform playerTransform;
    public GameObject playerBody;
    public Camera playerCamera;        // Cámara principal del jugador
    public Camera lockerCamera;        // Nueva cámara del locker

    [Header("Configuración del Casillero")]
    public Transform cameraHidePosition;
    public KeyCode interactionKey = KeyCode.E;

    [Header("Estado")]
    public bool isHidden = false;
    public bool canInteract = false;

    // Tiempo de transición para suavizar el movimiento
    private float transitionTime = 0.5f;
    private PlayerMovement playerMovement;

    void Start()
    {
        // Buscar automáticamente las referencias si no están asignadas
        if (playerTransform == null)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                playerTransform = player.transform;
                playerBody = player;
                playerMovement = player.GetComponent<PlayerMovement>();
            }
        }

        if (playerCamera == null)
        {
            // Buscar la cámara principal (que debe tener el tag MainCamera)
            playerCamera = Camera.main;
        }

        // Asegurarse de que la cámara del locker esté desactivada al inicio
        if (lockerCamera != null)
        {
            lockerCamera.gameObject.SetActive(false);
        }
    }

    void Update()
    {
        // Verificar si el jugador puede interactuar y presiona la tecla
        if (canInteract && Input.GetKeyDown(interactionKey))
        {
            if (!isHidden)
            {
                // Esconderse en el casillero
                StartCoroutine(HideInLocker());
            }
            else
            {
                // Salir del casillero
                StartCoroutine(ExitLocker());
            }
        }

        // Salir rápido si está escondido (opcional)
        if (isHidden && Input.GetKeyDown(KeyCode.Space))
        {
            StartCoroutine(ExitLocker());
        }
    }

    IEnumerator HideInLocker()
    {
        isHidden = true;

        // Desactivar el cuerpo del jugador y movimiento
        if (playerBody != null)
            playerBody.SetActive(false);

        if (playerMovement != null)
            playerMovement.enabled = false;

        // Cambio de cámaras
        if (playerCamera != null)
            playerCamera.gameObject.SetActive(false);

        if (lockerCamera != null)
        {
            lockerCamera.gameObject.SetActive(true);
            // Posicionar la cámara del locker en la posición correcta
            lockerCamera.transform.position = cameraHidePosition.position;
            lockerCamera.transform.rotation = cameraHidePosition.rotation;
        }

        Debug.Log("Te has escondido en el casillero. Presiona E o Space para salir.");
        yield return null;
    }

    IEnumerator ExitLocker()
    {
        // Cambio de cámaras
        if (lockerCamera != null)
            lockerCamera.gameObject.SetActive(false);

        if (playerCamera != null)
            playerCamera.gameObject.SetActive(true);

        // Reactivar el cuerpo del jugador y movimiento
        if (playerBody != null)
            playerBody.SetActive(true);

        if (playerMovement != null)
            playerMovement.enabled = true;

        isHidden = false;

        Debug.Log("Has salido del casillero.");
        yield return null;
    }

    // Detectar cuando el jugador está cerca del casillero
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            canInteract = true;
            Debug.Log("Presiona E para esconderte en el casillero");
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            canInteract = false;
        }
    }

    // Método para forzar la salida (útil para eventos externos)
    public void ForceExit()
    {
        if (isHidden)
        {
            StartCoroutine(ExitLocker());
        }
    }
}