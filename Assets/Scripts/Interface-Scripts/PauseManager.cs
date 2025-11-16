using UnityEngine;
using UnityEngine.SceneManagement;

public class PauseManager : MonoBehaviour
{
    [Header("Referencias de menús")]
    public GameObject menuConfig;
    public GameObject pausaUI;
    public GameObject menuPausa;
    public GameObject hudParaOcultar;


    private bool isPause = false;
    private bool config = false;

    void Start()
    {
        pausaUI.SetActive(false);
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Q))
        {
            if (isPause)
                Reanudar();
            else
                Pausar();
        }
    }

    public void Pausar()
    {        
        hudParaOcultar.SetActive(false);
        pausaUI.SetActive(true);
        Time.timeScale = 0f;
        isPause = true;
        Cursor.lockState = CursorLockMode.None;

    }

    public void Reanudar()
    {
        pausaUI.SetActive(false);
        menuConfig.SetActive(false);
        hudParaOcultar.SetActive(true);
        Time.timeScale = 1f;
        isPause = false;
        Cursor.lockState = CursorLockMode.Locked;

    }

    public void SalirAlMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("Título 1");
    }

    public void Configuracion()
    {
        menuConfig.SetActive(true);
        menuPausa.SetActive(false);
    }

    public void VolverAPausa()
    {
        menuConfig.SetActive(false);
        menuPausa.SetActive(true);
    }
}
