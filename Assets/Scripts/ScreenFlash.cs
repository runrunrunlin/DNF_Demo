using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class ScreenFlash : MonoBehaviour
{
    public static ScreenFlash Instance { get; private set; }

    private Image    flashImage;
    private Coroutine flashCoroutine;

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(this); return; }
        Instance = this;

        CreateFlashUI();
    }

    private void CreateFlashUI()
    {
        GameObject canvasGO = new GameObject("ScreenFlashCanvas");
        Canvas canvas       = canvasGO.AddComponent<Canvas>();
        canvas.renderMode   = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 999;
        canvasGO.AddComponent<CanvasScaler>();

        GameObject imageGO = new GameObject("FlashImage");
        imageGO.transform.SetParent(canvasGO.transform, false);

        flashImage             = imageGO.AddComponent<Image>();
        flashImage.color       = new Color(1f, 0f, 0f, 0f);
        flashImage.raycastTarget = false;

        RectTransform rt = imageGO.GetComponent<RectTransform>();
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;
    }

    public void Flash()
    {
        if (flashCoroutine != null) StopCoroutine(flashCoroutine);
        flashCoroutine = StartCoroutine(FlashRoutine());
    }

    private IEnumerator FlashRoutine()
    {
        flashImage.color = new Color(1f, 0f, 0f, 0.3f);

        float elapsed  = 0f;
        float duration = 0.3f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            flashImage.color = new Color(1f, 0f, 0f, Mathf.Lerp(0.3f, 0f, elapsed / duration));
            yield return null;
        }

        flashImage.color = new Color(1f, 0f, 0f, 0f);
        flashCoroutine   = null;
    }
}
