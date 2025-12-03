using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SplashScreen : MonoBehaviour
{
    private Image _logo;
    private bool _loadFinish;
    private bool _endLogo;

    private void Awake()
    {
        _logo = GetComponent<Image>();
        _loadFinish = false;
        _endLogo = false;
        _logo.color = new Color(_logo.color.r, _logo.color.g, _logo.color.b, 0f);
    }

    private void Start()
    {
#if UNITY_EDITOR
        PlayerPrefs.DeleteAll();
        #endif

        _loadFinish = true;
    }

    private void Update()
    {
        if (_loadFinish && _endLogo)
        {
            SceneManager.LoadSceneAsync("Título 1");
        }
    }

    public void EndAnimationLogo()
    {
        _endLogo = true;
    }
}
