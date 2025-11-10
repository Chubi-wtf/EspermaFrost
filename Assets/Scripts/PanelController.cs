using UnityEngine;

public class PanelController : MonoBehaviour
{
    public PlayerInventory inventory;
    public Animator doorAnimator;
    public bool isLocked = true;

    public void Unlock()
    {
        if (!isLocked) return;

        isLocked = false;

        if (doorAnimator != null)
        {
            doorAnimator.SetTrigger("Open");
        }
    }
}