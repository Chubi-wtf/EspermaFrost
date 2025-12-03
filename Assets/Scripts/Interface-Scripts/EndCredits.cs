using UnityEngine;

public class EndCredits : MonoBehaviour
{
    public AnimationInterfaceManager interfaceManager;
    public FadeOutUI fade;

    private bool CreditsEnd;

    private void Start()
    {
        CreditsEnd = false;
    }

    private void Update()
    {
        if (CreditsEnd == true)
        {
            interfaceManager.BackFromCredits();
            interfaceManager.Button();
            fade.FadeIn();
        }
    }

    public void CreditsEnding()
    {
        CreditsEnd = true;
    }
}
