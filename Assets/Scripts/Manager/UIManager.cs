using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class UIManager : MonoBehaviour
{
    [Header("Popup Animation")]
    public float popupDuration = 0.2f;
    public Vector3 popupStartScale = new Vector3(0.7f, 0.7f, 0.7f);
    public Vector3 popupEndScale = Vector3.one;

    public void ShowPanel(GameObject panel)
    {
        if (panel == null) return;

        AudioManager.Instance?.PlayModalOpen();

        panel.SetActive(true);

        PanelTransition transition = panel.GetComponent<PanelTransition>();
        if (transition != null)
        {
            transition.duration = popupDuration;
            transition.popupStartScale = popupStartScale;
            transition.popupEndScale = popupEndScale;
            transition.PopupIn();
            return;
        }

        StopAllCoroutines();
        StartCoroutine(PopupRoutine(panel));
    }

    public void HidePanel(GameObject panel)
    {
        Debug.Log("test");
        if (panel == null) return;
        AudioManager.Instance?.PlayButtonClose();

        PanelTransition transition = panel.GetComponent<PanelTransition>();
        if (transition != null)
        {
            transition.duration = popupDuration;
            transition.PopupOut(true);
            return;
        }

        panel.SetActive(false);
    }

    IEnumerator PopupRoutine(GameObject panel)
    {
        panel.SetActive(true);

        RectTransform rect = panel.GetComponent<RectTransform>();
        if (rect == null) yield break;

        rect.localScale = popupStartScale;

        float time = 0f;
        while (time < popupDuration)
        {
            time += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(time / popupDuration);
            t = 1f - Mathf.Pow(1f - t, 3f);
            rect.localScale = Vector3.Lerp(popupStartScale, popupEndScale, t);
            yield return null;
        }

        rect.localScale = popupEndScale;
    }

    public void LoadScene(string sceneName)
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(sceneName);
    }

    public void LoadSceneWithLoading(string targetScene)
    {
        LoadingContext.PrepareLoad(targetScene, false);
        LoadingContext.firebaseDone = true;
        LoadScene("Loading");
    }

    public void ReloadCurrentScene()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}