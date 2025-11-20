using UnityEngine;
using System.Collections;

public class ScriptCasilleroProvisional : MonoBehaviour
{
    [Header("Referencias Globales")]
    public Transform playerTransform;
    public GameObject playerBody;
    public Camera playerCamera;

    [Header("Referencias Internas (Auto-detectables)")]
    public Camera lockerCamera;        // La cámara dentro DE ESTE casillero
    public Transform cameraHidePosition; // Dónde se pondrá la cámara (opcional)

    [Header("Configuración")]
    public KeyCode interactionKey = KeyCode.E;
    public float interactionDistance = 3.0f;
    public LayerMask interactionLayer;

    [Header("Estado")]
    public bool isHidden = false;

    private PlayerMovement playerMovement;

    void Start()
    {
        // 1. AUTO-CONFIGURACIÓN DEL JUGADOR
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
        if (playerCamera == null) playerCamera = Camera.main;

        // 2. AUTO-CONFIGURACIÓN INTERNA (Crucial para Prefabs)
        // Busca la cámara que está DENTRO de este casillero específico
        if (lockerCamera == null)
        {
            lockerCamera = GetComponentInChildren<Camera>(true); // 'true' para buscar aunque esté desactivada
        }

        // Si no se asignó posición de cámara, usa la posición de la cámara encontrada o el propio casillero
        if (cameraHidePosition == null)
        {
            if (lockerCamera != null) cameraHidePosition = lockerCamera.transform;
            else cameraHidePosition = this.transform;
        }

        // Asegurarse de que la cámara del locker empiece apagada
        if (lockerCamera != null) lockerCamera.gameObject.SetActive(false);
    }

    void Update()
    {
        // Lógica para SALIR (Solo si este es el casillero donde estás escondido)
        if (isHidden)
        {
            if (Input.GetKeyDown(interactionKey) || Input.GetKeyDown(KeyCode.Space))
            {
                StartCoroutine(ExitLocker());
            }
            return;
        }

        // Lógica para ENTRAR (Se ejecuta en todos los casilleros, pero filtra por raycast)
        if (Input.GetKeyDown(interactionKey))
        {
            CheckForInteraction();
        }
    }

    void CheckForInteraction()
    {
        if (playerCamera == null) return;

        Ray ray = new Ray(playerCamera.transform.position, playerCamera.transform.forward);
        RaycastHit hit;

        // Lanzamos el rayo
        if (Physics.Raycast(ray, out hit, interactionDistance, interactionLayer))
        {
            // CORRECCIÓN IMPORTANTE: 
            // Buscamos si el objeto golpeado tiene ESTE script, o si es un hijo de este script
            ScriptCasilleroProvisional casilleroGolpeado = hit.collider.GetComponentInParent<ScriptCasilleroProvisional>();

            // Verificamos si el casillero golpeado SOY YO (this)
            if (casilleroGolpeado == this)
            {
                StartCoroutine(HideInLocker());
            }
        }
    }

    IEnumerator HideInLocker()
    {
        isHidden = true;

        // Desactivar jugador
        if (playerBody != null) playerBody.SetActive(false);
        if (playerMovement != null) playerMovement.enabled = false;
        if (playerCamera != null) playerCamera.gameObject.SetActive(false);

        // Activar cámara de ESTE casillero
        if (lockerCamera != null)
        {
            lockerCamera.gameObject.SetActive(true);
            // Opcional: Forzar posición si es necesario
            // lockerCamera.transform.position = cameraHidePosition.position;
            // lockerCamera.transform.rotation = cameraHidePosition.rotation;
        }
        else
        {
            Debug.LogError("¡Falta la cámara dentro del prefab del casillero!");
        }

        Debug.Log("Escondido en: " + gameObject.name);
        yield return null;
    }

    IEnumerator ExitLocker()
    {
        // Desactivar cámara de este casillero
        if (lockerCamera != null) lockerCamera.gameObject.SetActive(false);

        // Reactivar jugador
        if (playerCamera != null) playerCamera.gameObject.SetActive(true);
        if (playerBody != null) playerBody.SetActive(true);
        if (playerMovement != null) playerMovement.enabled = true;

        isHidden = false;
        yield return null;
    }
}