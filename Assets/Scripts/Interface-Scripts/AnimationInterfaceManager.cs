using UnityEngine;
using UnityEngine.SceneManagement;

public class AnimationInterfaceManager : MonoBehaviour
{
    public Animator anim, anim2;

    public void Button()
    {
        anim.SetTrigger("ButtonAnim");
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
