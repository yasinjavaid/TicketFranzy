
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class ControlPromptUpdater : MonoBehaviour
{
    public Image image;

    public Sprite switchSprite;
    public Sprite pcSprite;

    public UnityEvent onSwitchEnabled;
    public UnityEvent onPCEnabled;

    public UnityEvent onSwitchDisabled;
    public UnityEvent onPCDisabled;

    protected string displayScheme;

    private void Update()
    {
        string currentScheme = GameInput.Instance.GetCurrentControlScheme;
        if (currentScheme != displayScheme)
        {
            switch (currentScheme)
            {
                case "Switch": Enable(switchSprite, onSwitchEnabled); break;
                case "PC": Enable(pcSprite, onPCEnabled); break;
            }
            currentScheme = displayScheme;
        }
    }

    private void Enable(Sprite sprite, UnityEvent enableEvent)
    {
        if (image && sprite) image.sprite = sprite;
        InvokeOnDisabled();
        enableEvent?.Invoke();
    }

    private void InvokeOnDisabled()
    {
        switch (displayScheme)
        {
            case "Switch": onSwitchDisabled?.Invoke(); break;
            case "PC": onPCDisabled?.Invoke(); break;
        }
    }
}