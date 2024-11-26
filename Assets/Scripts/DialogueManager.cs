using UnityEngine;
using TextMeshPro = TMPro.TextMeshProUGUI;

public class DialogueManager : MonoBehaviour
{
    [SerializeField] public TextMeshPro Title;
    [SerializeField] public TextMeshPro Text;

    [SerializeField] CanvasGroup _uiGroup;

    bool _fadeOut = false;
    bool _fadeIn = false;

    public bool FadeOut
    {
        get { return _fadeOut; }
        set
        {
            if (value) _uiGroup.alpha = 1f;
            _fadeOut = value;
        }
    }

    public bool FadeIn
    {
        get { return _fadeIn; }
        set
        {
            if (value) _uiGroup.alpha = 0f;
            _fadeIn = value;
        }
    }

    public void DelayedFadeout(float seconds)
    {
        Invoke("BeginFadeout", seconds);
    }

    void BeginFadeout()
    {
        FadeOut = true;
    }

    public void FixedUpdate()
    {
        if (FadeIn)
        {
            _uiGroup.alpha += Time.fixedTime * 0.006f;
            if (_uiGroup.alpha >= 1f) FadeIn = false;
        }
        if (FadeOut)
        {
            _uiGroup.alpha -= Time.fixedTime * 0.006f;
            if (_uiGroup.alpha <= 0f) FadeOut = false;
        }
    }
}
