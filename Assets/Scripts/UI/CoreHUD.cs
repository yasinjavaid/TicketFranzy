using TMPro;

using UnityEngine;

public class CoreHUD : SingletonMB<CoreHUD>
{
    [SerializeField] CanvasFader mainCanvasFader;
    [SerializeField] TextMeshProUGUI tmp_Tickets;

    private int _tickets;

    public static bool IsVisible
    {
        get => Instance && Instance.mainCanvasFader && Instance.mainCanvasFader.IsVisible;
        set { if (Instance && Instance.mainCanvasFader) Instance.mainCanvasFader.IsVisible = value; }
    }

    private void Update()
    {
        if (_tickets != GameManager.Tickets)
            tmp_Tickets.text = $"{_tickets = GameManager.Tickets} Tickets";
    }

    public static void FadeIn() { if (Instance && Instance.mainCanvasFader) Instance.mainCanvasFader.FadeIn(); }

    public static void FadeOut() { if (Instance && Instance.mainCanvasFader) Instance.mainCanvasFader.FadeOut(); }
}