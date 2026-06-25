using TMPro;
using UnityEngine;

public class HUDController : MonoBehaviour
{
    [Header("Vitals Texts")]
    public TMP_Text hrText;
    public TMP_Text spo2Text;
    public TMP_Text rrText;
    public TMP_Text bpText;
    public TMP_Text tempText;

    [Header("Score")]
    public TMP_Text scoreText;

    public void UpdateVitals(int hr, int spo2, int rr, string bp, float temp)
    {
        hrText.text = "HR: " + hr;
        spo2Text.text = "SpO2: " + spo2 + "%";
        rrText.text = "RR: " + rr;
        bpText.text = "BP: " + bp;
        tempText.text = "Temp: " + temp.ToString("0.0") + "°C";
    }

    public void UpdateScore(int score)
    {
        scoreText.text = "Score: " + score;
    }

    public void SetTestVitals()
    {
        UpdateVitals(110, 88, 24, "125/80", 37.0f);
        UpdateScore(100);
    }

    public void SetImprovedVitals()
    {
        UpdateVitals(100, 94, 20, "120/78", 36.9f);
        UpdateScore(120);
    }
}