using TMPro;

using UnityEngine;

public class SinkItScreen : MonoBehaviour
{
    [SerializeField] protected SinkIt sinkIt;
    [SerializeField] protected TextMeshProUGUI tmp_Timer;
    [SerializeField] protected TextMeshProUGUI tmp_Cups;
    [SerializeField] protected TextMeshProUGUI tmp_TotalShots;
    [SerializeField] protected TextMeshProUGUI tmp_BallsRemaining;
    [SerializeField] protected TextMeshProUGUI tmp_Credits;
    [SerializeField] protected GameObject[] go_balls;

    private void Update()
    {
        for (int i = 0; i < go_balls.Length; i++)
            go_balls[i].SetActive(sinkIt.CompletedCups.Contains(i));

        tmp_Timer.text = sinkIt.SecondsRemainig.ToString();
        tmp_Cups.text = sinkIt.CompletedCups.Count.ToString();
        tmp_TotalShots.text = sinkIt.TotalShots.ToString();
        tmp_BallsRemaining.text = (sinkIt.BallsPerGame - sinkIt.TotalShots).ToString();
    }
}
