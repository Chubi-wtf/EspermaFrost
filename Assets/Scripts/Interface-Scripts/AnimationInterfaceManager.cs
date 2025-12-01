using UnityEngine;
using UnityEngine.SceneManagement;

public class AnimationInterfaceManager : MonoBehaviour
{
    public Animator anim, anim2, anim3;
    public GameObject CreditsBack;


    private void Start()
    {
        CreditsBack.SetActive(false);
    }
    public void Button()
    {
        anim.SetTrigger("ButtonAnim");
    }

    public void Credits()
    {
        anim3.SetTrigger("CreditAnim");
        CreditsBack.SetActive(true);
    }

    public void BackFromCredits()
    {
        anim3.SetTrigger("CreditAnim");
        CreditsBack.SetActive(false);

    }

    public void Window()
    {
        anim2.SetTrigger("WindowAnim");
    }

    public void Jugar()
    {
        SceneManager.LoadScene("Escena 1");
    }
    public void SalirDelJuego()
    {
        Application.Quit();
    }
}
