using System.Collections;
using UnityEngine;

[RequireComponent(typeof(RectTransform))]
[RequireComponent(typeof(CanvasGroup))]
public class PanelTransition : MonoBehaviour
{
    [Header("Common")]
    public float duration = 0.25f;
    public bool useUnscaledTime = true;

    [Header("Popup")]
    public Vector3 popupStartScale = new Vector3(0.7f, 0.7f, 0.7f);
    public Vector3 popupEndScale = Vector3.one;

    [Header("Slide")]
    public float slideOffset = 800f;

    private CanvasGroup canvasGroup;
    private RectTransform rectTransform;
    private Coroutine currentRoutine;

    private Vector2 homeAnchoredPosition;
    private Vector3 homeScale;

    void Awake()
    {
        CacheComponents();
        SaveHomeState();
    }

    void OnEnable()
    {
        CacheComponents();
    }

    private void CacheComponents()
    {
        if (rectTransform == null)
            rectTransform = GetComponent<RectTransform>();

        if (canvasGroup == null)
            canvasGroup = GetComponent<CanvasGroup>();
    }

    private void SaveHomeState()
    {
        if (rectTransform != null)
        {
            homeAnchoredPosition = rectTransform.anchoredPosition;
            homeScale = rectTransform.localScale;
        }
    }

    public void ResetToHome()
    {
        CacheComponents();

        if (rectTransform == null || canvasGroup == null)
        {
            Debug.LogError($"PanelTransition on {gameObject.name} is missing RectTransform or CanvasGroup.");
            return;
        }

        rectTransform.anchoredPosition = homeAnchoredPosition;
        rectTransform.localScale = homeScale;
        canvasGroup.alpha = 1f;
        canvasGroup.interactable = true;
        canvasGroup.blocksRaycasts = true;
    }

    public void PopupIn()
    {
        StartTransition(PopupRoutine());
    }

    public void PopupOut(bool deactivateOnFinish = true)
    {
        StartTransition(PopupOutRoutine(deactivateOnFinish));
    }

    public void SlideInFromRight()
    {
        StartTransition(SlideRoutine(homeAnchoredPosition + new Vector2(slideOffset, 0f), homeAnchoredPosition, true, false));
    }

    public void SlideInFromLeft()
    {
        StartTransition(SlideRoutine(homeAnchoredPosition + new Vector2(-slideOffset, 0f), homeAnchoredPosition, true, false));
    }

    public void SlideOutToLeft(bool deactivateOnFinish = true)
    {
        StartTransition(SlideRoutine(homeAnchoredPosition, homeAnchoredPosition + new Vector2(-slideOffset, 0f), false, deactivateOnFinish));
    }

    public void SlideOutToRight(bool deactivateOnFinish = true)
    {
        StartTransition(SlideRoutine(homeAnchoredPosition, homeAnchoredPosition + new Vector2(slideOffset, 0f), false, deactivateOnFinish));
    }

    private void StartTransition(IEnumerator routine)
    {
        CacheComponents();

        if (currentRoutine != null)
            StopCoroutine(currentRoutine);

        currentRoutine = StartCoroutine(routine);
    }

    private IEnumerator PopupRoutine()
    {
        CacheComponents();

        gameObject.SetActive(true);
        rectTransform.anchoredPosition = homeAnchoredPosition;
        rectTransform.localScale = popupStartScale;
        canvasGroup.alpha = 0f;

        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += useUnscaledTime ? Time.unscaledDeltaTime : Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            t = 1f - Mathf.Pow(1f - t, 3f);

            canvasGroup.alpha = Mathf.Lerp(0f, 1f, t);
            rectTransform.localScale = Vector3.Lerp(popupStartScale, popupEndScale, t);
            yield return null;
        }

        rectTransform.anchoredPosition = homeAnchoredPosition;
        rectTransform.localScale = popupEndScale;
        canvasGroup.alpha = 1f;
    }

    private IEnumerator PopupOutRoutine(bool deactivateOnFinish)
    {
        CacheComponents();

        gameObject.SetActive(true);

        Vector3 startScale = rectTransform.localScale;
        float startAlpha = canvasGroup.alpha;

        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += useUnscaledTime ? Time.unscaledDeltaTime : Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);

            canvasGroup.alpha = Mathf.Lerp(startAlpha, 0f, t);
            rectTransform.localScale = Vector3.Lerp(startScale, popupStartScale, t);
            yield return null;
        }

        rectTransform.anchoredPosition = homeAnchoredPosition;
        rectTransform.localScale = homeScale;
        canvasGroup.alpha = 0f;

        if (deactivateOnFinish)
            gameObject.SetActive(false);
    }

    private IEnumerator SlideRoutine(Vector2 startPos, Vector2 endPos, bool fadeIn, bool deactivateOnFinish)
    {
        CacheComponents();

        gameObject.SetActive(true);

        rectTransform.anchoredPosition = startPos;
        canvasGroup.alpha = fadeIn ? 0f : 1f;

        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += useUnscaledTime ? Time.unscaledDeltaTime : Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            t = 1f - Mathf.Pow(1f - t, 3f);

            rectTransform.anchoredPosition = Vector2.Lerp(startPos, endPos, t);
            canvasGroup.alpha = fadeIn ? Mathf.Lerp(0f, 1f, t) : Mathf.Lerp(1f, 0f, t);
            yield return null;
        }

        rectTransform.anchoredPosition = fadeIn ? homeAnchoredPosition : endPos;
        canvasGroup.alpha = fadeIn ? 1f : 0f;

        if (!fadeIn)
        {
            rectTransform.anchoredPosition = homeAnchoredPosition;

            if (deactivateOnFinish)
                gameObject.SetActive(false);
        }
    }
}