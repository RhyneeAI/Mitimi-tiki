using UnityEngine;
using UnityEngine.UI;

public class PanelManager : MonoBehaviour
{
    public GameObject[] subPanels;
    public GameObject backButton;
    public GameObject nextIcon;

    [Header("Manager")]
    public UIManager uiManager;

    private int currentIndex = 0;
    private bool isTransitioning = false;

    void Start()
    {
        ShowPanelInstant(currentIndex);
    }

    public void ShowPanel(int index)
    {
        if (isTransitioning) return;
        if (index < 0 || index >= subPanels.Length) return;
        if (index == currentIndex) return;

        bool moveNext = index > currentIndex;
        StartCoroutinePanelSwitch(currentIndex, index, moveNext);
    }

    public void ClosePanel()
    {
        if (uiManager != null)
        {
            AudioManager.Instance?.PlayButtonClose();
            uiManager.HidePanel(gameObject);
        }
    }

    public void NextPanel()
    {
        if (isTransitioning) return;

        AudioManager.Instance?.PlayButtonClick();
        int nextIndex = currentIndex + 1;
        if (nextIndex < subPanels.Length)
            ShowPanel(nextIndex);
    }

    public void PreviousPanel()
    {
        if (isTransitioning) return;

        AudioManager.Instance?.PlayButtonClick();
        int prevIndex = currentIndex - 1;
        if (prevIndex >= 0)
            ShowPanel(prevIndex);
    }

    private void ShowPanelInstant(int index)
    {
        for (int i = 0; i < subPanels.Length; i++)
            subPanels[i].SetActive(i == index);

        currentIndex = index;
        UpdateButtons();
    }

    private void StartCoroutinePanelSwitch(int fromIndex, int toIndex, bool moveNext)
    {
        StartCoroutine(SwitchPanelRoutine(fromIndex, toIndex, moveNext));
    }

    private System.Collections.IEnumerator SwitchPanelRoutine(int fromIndex, int toIndex, bool moveNext)
    {
        isTransitioning = true;

        GameObject fromPanel = subPanels[fromIndex];
        GameObject toPanel = subPanels[toIndex];

        PanelTransition fromTransition = fromPanel.GetComponent<PanelTransition>();
        PanelTransition toTransition = toPanel.GetComponent<PanelTransition>();
        if (toTransition != null)
        {
            toTransition.ResetToHome();
        }

        if (toPanel != null)
            toPanel.SetActive(true);

        if (moveNext)
        {
            if (fromTransition != null) fromTransition.SlideOutToLeft(true);
            else fromPanel.SetActive(false);

            if (toTransition != null) toTransition.SlideInFromRight();
        }
        else
        {
            if (fromTransition != null) fromTransition.SlideOutToRight(true);
            else fromPanel.SetActive(false);

            if (toTransition != null) toTransition.SlideInFromLeft();
        }

        float wait = 0.25f;
        if (toTransition != null) wait = toTransition.duration;
        else if (fromTransition != null) wait = fromTransition.duration;

        isTransitioning = true;
        yield return new WaitForSecondsRealtime(wait);
        isTransitioning = false;

        if (fromPanel != null)
            fromPanel.SetActive(false);

        currentIndex = toIndex;
        UpdateButtons();
        isTransitioning = false;
    }

    private void UpdateButtons()
    {
        if (backButton != null)
            backButton.SetActive(currentIndex > 0);

        if (nextIcon != null)
            nextIcon.SetActive(currentIndex < subPanels.Length - 1);
    }
}