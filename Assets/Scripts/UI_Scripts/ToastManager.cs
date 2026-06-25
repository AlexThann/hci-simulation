using TMPro;
using UnityEngine;

public class ToastManager : MonoBehaviour
{
    public GameObject toastPanel;
    public TMP_Text toastText;

    public void ShowToast(string message, float duration = 1.5f)
    {
        toastPanel.SetActive(true);
        toastText.text = message;

        CancelInvoke(nameof(HideToast));
        Invoke(nameof(HideToast), duration);
    }

    

    public void HideToast()
    {
        toastPanel.SetActive(false);
    }
}