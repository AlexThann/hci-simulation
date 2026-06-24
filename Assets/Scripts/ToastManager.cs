using TMPro;
using UnityEngine;

public class ToastManager : MonoBehaviour
{
    public GameObject toastPanel;
    public TMP_Text toastText;

    public void ShowToast(string message)
    {
        toastPanel.SetActive(true);
        toastText.text = message;

        CancelInvoke(nameof(HideToast));
        Invoke(nameof(HideToast), 3f);
    }

    public void HideToast()
    {
        toastPanel.SetActive(false);
    }
}