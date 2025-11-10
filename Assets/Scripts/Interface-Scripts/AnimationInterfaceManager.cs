using UnityEngine;

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
}
