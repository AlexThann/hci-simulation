using TMPro;
using UnityEngine;

public class ToastManager : MonoBehaviour
{
    public TMP_Text feedbackText;

    public void ShowToast(string message)
    {
        feedbackText.text = message;
        Debug.Log(message);
    }
}