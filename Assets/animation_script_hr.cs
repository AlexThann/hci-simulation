using UnityEngine;
using UnityEngine.UI;

public class animation_script_hr : MonoBehaviour
{
    public RawImage rawImage;
    public float scrollSpeed = 0.5f;

    void Update()
    {
        if (rawImage != null)
        {
            Rect currentRect = rawImage.uvRect;
            currentRect.x += scrollSpeed * Time.deltaTime;
            rawImage.uvRect = currentRect;
        }
    }
}