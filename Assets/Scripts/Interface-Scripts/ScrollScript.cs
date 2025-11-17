using UnityEngine;
using UnityEngine.UI;

public class ScrollScript : MonoBehaviour
{
    public int totalLevels;

    public Scrollbar aBar;

    public int valorFocus;
    public bool elegir = false;

    int totalFilas;

    private void Start()
    {
        float res = totalLevels / 4.0f;
        totalFilas = Mathf.CeilToInt(res);
    }

    private void Update()
    {
        if(elegir)
        {
         elegir = false;
            FocusButton();
        }
    }

    public void FocusButton()
    {
        float auxRes = (valorFocus - 1) / 4.0f;
        auxRes = totalFilas - auxRes;
        int mover = Mathf.CeilToInt(auxRes);
        float normalizar = mover / totalFilas;
        aBar.value = normalizar;
    }
}

